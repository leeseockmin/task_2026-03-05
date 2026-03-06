# 팀 개발 룰 (전체 공통)

> 이 파일은 개발자A·B 에이전트 및 모든 스킬의 공통 룰 참조 파일입니다.
> 작업 시작 전 반드시 이 파일 전체를 읽고 숙지하세요.

---

## 1. DB 접근 아키텍처

| 레이어 | 역할 |
|--------|------|
| `EF Core ({DB스키마명}DBContext)` | 스키마 정의 및 마이그레이션 전용. `SaveChanges` 직접 호출 금지 |
| `DataBaseManager` | DbContextFactory에서 `DbConnection`을 꺼내 Dapper에 전달 |
| `Dapper` | 모든 실제 SQL 실행 (읽기 및 쓰기 모두) |

> **레이어 의존성 예외:** `DB.Data` 프로젝트의 Entity 클래스(`DB.Data.AccountDB.*`)는 Dapper SQL 생성의 편의를 위해 Application 인터페이스 및 핸들러에서 직접 참조 허용.

**DataBaseManager 주요 메서드:**

| 메서드 | 설명 |
|--------|------|
| `ExecuteAsync(DBType, Func<DbConnection, Task<T>>)` | 단일 DB, 반환값 있음 |
| `ExecuteAsync(DBType, Func<DbConnection, Task>)` | 단일 DB, 반환값 없음 |
| `ExecuteAsync(DBType, DBType, Func<...>)` | 두 DB 동시 실행 |
| `ExecuteTransactionAsync(DBType, Func<DbConnection, Task<bool>>)` | 단일 DB 트랜잭션 (true 반환 시 커밋) |
| `ExecuteTransactionAsync(DBType, DBType, ...)` | 두 DB 트랜잭션 |

**현재 등록된 DBContext:**

| Context 이름 | DB 스키마 | 포함 테이블 | 비고 |
|-------------|---------|-----------|------|
| `AccountDBContext` | `accountdb` | `Employee` | 현재 사용 중 |

---

## 2. 프로젝트 구조 표준

```
BackEnd/
├── Application/
│   ├── Commands/{기능}/
│   │   ├── {동작}{기능}Command.cs
│   │   └── {동작}{기능}CommandHandler.cs
│   ├── Queries/{기능}/
│   │   ├── Get{기능}Query.cs
│   │   └── Get{기능}QueryHandler.cs
│   ├── DTOs/
│   │   ├── Common/ErrorResponse.cs
│   │   └── {기능}/{기능}Dto.cs
│   ├── Interfaces/{기능}/
│   │   ├── I{기능}CommandRepository.cs
│   │   └── I{기능}QueryRepository.cs
│   └── Utils/{기능}Utils.cs
├── Infrastructure/
│   ├── Repositories/
│   │   └── {기능}CommandRepository.cs    ← Command (Dapper 쓰기)
│   ├── Persistence/Read/
│   │   └── {기능}QueryRepository.cs      ← Query (Dapper 읽기)
│   └── DataBase/DataBaseManager.cs
└── Controllers/{기능}Controller.cs

Data/                                      ← 마이그레이션 전용 프로젝트
├── {스키마명}DB/
│   ├── {스키마명}DBContext.cs
│   └── {엔티티}.cs
└── Migrations/{스키마명}Migrations/
```

---

## 3. C# 코딩 컨벤션

| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스, 메서드, 프로퍼티 | PascalCase | `EmployeeService`, `GetListAsync` |
| private 필드 | `_camelCase` | `_queryRepository` |
| 지역 변수, 파라미터 | camelCase | `employeeId`, `request` |
| 인터페이스 | `I` + PascalCase | `IEmployeeQueryRepository` |
| 비동기 메서드 | `Async` 접미사 필수 | `CreateEmployeeAsync` |
| Command / Query / DTO | `record` 타입 | `public record CreateEmployeeCommand(...)` |
| Entity 컬럼 프로퍼티 | camelCase (예외) | `employeeId`, `createdAt` |
| DBContext 이름 | `{DB스키마명}DBContext` | `AccountDBContext` |
| 날짜/시간 | `DateTime` 사용 | `DateOnly` / `TimeOnly` 사용 금지 |
| 조건문 중괄호 | 한 줄이라도 `{}` 필수 | 생략 금지 |
| SQL 문자열 | `@"..."` 또는 `$@"..."` | `"""..."""` raw string literal 금지 |
| Query 파라미터 | 컬럼명은 `nameof(Entity.Property)` 사용, 비컬럼 파라미터는 `@파라미터명` 사용 | `WHERE {nameof(Employee.employeeId)} = @EmployeeId` |

**Query 파라미터 예시:**

```csharp
// ✅ 컬럼명: nameof(Entity.Property) 사용
$@"SELECT *
   FROM Employee
   WHERE {nameof(Employee.employeeId)} = @EmployeeId
     AND {nameof(Employee.name)} LIKE @Name"

// ✅ 비컬럼 파라미터 (페이징 등): @파라미터명 그대로 사용
$@"SELECT *
   FROM Employee
   ORDER BY {nameof(Employee.createdAt)} DESC
   LIMIT @PageSize OFFSET @Offset"

// ❌ 문자열 하드코딩
"WHERE employeeId = @EmployeeId"
```

---

## 4. 에러 로그 규칙

에러 응답 반환 전 **반드시** `_logger.LogError` 호출.

| 상황 | 로그 레벨 |
|------|-----------|
| 400 BadRequest | `LogError` |
| 404 NotFound | `LogError` |
| 401 Unauthorized | `LogError` |
| 403 Forbidden | `LogError` |
| 500 내부 오류 | `LogError` |
| Exception / throw 발생 | `LogError` (throw 직전에 반드시 기록) |

```csharp
// ✅ 올바른 예시 — $"" 보간 문자열 사용
if (result is null)
{
    _logger.LogError($"직원 조회 결과 없음. Name: {name}");
    return NotFound(new ErrorResponse($"이름 '{name}'에 해당하는 직원이 없습니다."));
}

// ✅ throw 직전
if (req.Name.Length > 100)
{
    _logger.LogError($"name 길이 초과. 입력값 길이: {req.Name.Length}");
    throw new ArgumentException("name은 최대 100자까지 허용됩니다.");
}

// ✅ catch 블록 — ex.Message 포함
catch (Exception ex)
{
    _logger.LogError($"직원 일괄 등록 유효성 검사 실패. Message: {ex.Message}");
    return BadRequest(new ErrorResponse(ex.Message));
}

// ❌ 구조적 로깅 형식 (사용 금지)
_logger.LogError("name 길이 초과. Length: {Length}", req.Name.Length);
_logger.LogError(ex, "DB 실행 오류. DBType: {DBType}", dbType);
```

---

## 5. 요청/응답 패킷 규칙

요청과 응답은 반드시 **record 객체** 사용. 원시 타입 직접 반환/수신 금지.

```csharp
// ✅ 올바른 예시
public async Task<IActionResult> GetListAsync([FromQuery] GetEmployeeListRequest request) { }
return Ok(new EmployeeListResult(...));
return BadRequest(new ErrorResponse("메시지"));

// ❌ 잘못된 예시
return BadRequest("메시지");
return Ok(userId);
public async Task<IActionResult> GetListAsync([FromQuery] int page) { }
```

**공통 에러 응답:** `Application/DTOs/Common/ErrorResponse.cs`
```csharp
public record ErrorResponse(string Message);
```

---

## 6. 절대 금지 사항

| 금지 | 이유 / 대안 |
|------|-------------|
| `.Result` / `.Wait()` | 데드락 위험 → `await` 사용 |
| `Console.WriteLine` | `ILogger<T>` 사용 |
| EF Core `SaveChanges` 직접 호출 | Dapper로 모든 SQL 실행 |
| 엔티티 직접 반환 | DTO 반환 필수 |
| `new` 로 서비스 직접 생성 | DI 컨테이너 사용 |
| 에러 반환 전 로그 누락 | `_logger.LogError` 호출 후 반환 |
| 구조적 로깅 형식 | `$""` 보간 문자열 사용 (`"... {Value}", value` 형식 금지) |
| 원시 타입 직접 반환/수신 | record 객체 사용 |
| `"""..."""` raw string literal | `@"..."` 또는 `$@"..."` 사용 |
| `DateOnly` / `TimeOnly` | `DateTime` 사용 |
| 조건문 `{}` 생략 | 한 줄이라도 반드시 `{}` 사용 |
| `IReadOnlyList<T>` 사용 | `List<T>` 사용 (단순 객체만 사용) |
| `.AsReadOnly()` 호출 | 불필요한 래핑 금지, `List<T>` 그대로 사용 |
| MongoDB에 비즈니스 데이터 저장 | 서비스 이벤트 로그 전용 |
| Redis에 세션 외 데이터 저장 | 세션 전용 |

---

## 7. 자가 점검 체크리스트

- [ ] 폴더 경로 표준 준수 (CommandRepo → `Repositories/`, QueryRepo → `Persistence/Read/`)
- [ ] DBContext 이름 `{DB스키마명}DBContext` 형식
- [ ] Entity 컬럼 프로퍼티 camelCase 사용
- [ ] Async 접미사 모든 비동기 메서드에 적용
- [ ] EF Core `SaveChanges` 미사용 (Dapper로 모든 SQL 실행)
- [ ] Dapper 쿼리 파라미터 바인딩 사용 (SQL Injection 방지)
- [ ] SQL 컬럼명은 `nameof(Entity.Property)` 사용 (문자열 하드코딩 금지)
- [ ] 컨트롤러에 비즈니스 로직 없음 (MediatR Send만)
- [ ] 에러 반환 전 `_logger.LogError` 호출 (400~500 전체, throw 직전 포함)
- [ ] 에러 응답은 `ErrorResponse` 객체로 반환
- [ ] 요청 파라미터는 객체(`Request` record)로 수신
- [ ] Entity 직접 반환 없음 (DTO 반환)
- [ ] `.Result` / `.Wait()` 미사용
- [ ] `Console.WriteLine` 미사용
- [ ] `DateTime` 사용 (`DateOnly` / `TimeOnly` 금지)
- [ ] 조건문 한 줄이라도 `{}` 사용
- [ ] SQL 문자열 `@"..."` 또는 `$@"..."` 사용 (`"""..."""` 금지)
- [ ] record 타입 사용 (Command / Query / DTO)
- [ ] `IReadOnlyList<T>` 미사용 (`List<T>` 사용)
- [ ] `.AsReadOnly()` 미사용
