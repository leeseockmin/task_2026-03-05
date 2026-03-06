# CQRS BackEnd 프로젝트

ASP.NET Core 8.0 기반 백엔드 API 서버입니다.
CQRS 패턴을 적용하며, MySQL(주 DB)을 사용합니다.

---

## 기술 스택

| 기술 | 버전 | 용도 |
|------|------|------|
| .NET | 8.0 | 런타임 |
| ASP.NET Core | 8.0 | Web API 프레임워크 |
| MySQL | 8.0+ | 주 데이터베이스 (Read / Write 분리) |
| Entity Framework Core | 8.x | 스키마 정의 및 마이그레이션 전용 |
| Dapper | 2.x | 모든 SQL 실행 (읽기 및 쓰기) |
| MediatR | 14.x | CQRS 핸들러 디스패치 |
| Serilog | 4.x | 애플리케이션 로그 → 파일 |

---

## 프로젝트 구조

```
CQRS.sln
├── BackEnd/                                    ← API 프로젝트 (런타임)
│   ├── Application/
│   │   ├── Commands/
│   │   │   └── Employee/
│   │   │       ├── CreateEmployeeCommand.cs
│   │   │       └── CreateEmployeeCommandHandler.cs
│   │   ├── Queries/
│   │   │   └── Employee/
│   │   │       ├── GetEmployeeListQuery.cs
│   │   │       ├── GetEmployeeListQueryHandler.cs
│   │   │       ├── GetEmployeeByNameQuery.cs
│   │   │       └── GetEmployeeByNameQueryHandler.cs
│   │   ├── DTOs/
│   │   │   ├── Common/
│   │   │   │   └── ErrorResponse.cs
│   │   │   └── Employee/
│   │   │       ├── EmployeeDto.cs
│   │   │       ├── CreateEmployeeRequest.cs
│   │   │       └── GetEmployeeListRequest.cs
│   │   └── Interfaces/
│   │       ├── Employee/
│   │       │   ├── IEmployeeCommandRepository.cs
│   │       │   └── IEmployeeQueryRepository.cs
│   │       └── RepositoryServiceRegistration.cs
│   ├── Infrastructure/
│   │   ├── DataBase/
│   │   │   └── DataBaseManager.cs              ← 연결 관리, 트랜잭션
│   │   ├── Persistence/
│   │   │   ├── Write/
│   │   │   │   └── EmployeeCommandRepository.cs
│   │   │   └── Read/
│   │   │       └── EmployeeQueryRepository.cs
│   ├── Controllers/
│   │   └── EmployeeController.cs
│   ├── Program.cs
│   └── appsettings.json                        ← gitignore 대상 (직접 생성 필요)
│
└── Data/                                       ← 마이그레이션 전용 프로젝트
    ├── AccountDB/
    │   ├── AccountDBContext.cs
    │   └── Employee.cs
    ├── Migrations/
    │   └── AccountMigrations/                  ← EF Core 자동 생성
    └── IModelCreateEntity.cs
```

---

## 아키텍처

### DB 접근 구조

| 레이어 | 역할 |
|--------|------|
| `EF Core (AccountDBContext)` | 스키마 정의 및 마이그레이션 전용. SaveChanges 직접 호출 금지 |
| `DataBaseManager` | DbContextFactory에서 `DbConnection`을 꺼내 Dapper에 전달 |
| `Dapper` | 모든 실제 SQL 실행 (읽기 및 쓰기) |

### CQRS 원칙

| 구분 | Repository | 역할 |
|------|-----------|------|
| Command (쓰기) | `IEmployeeCommandRepository` → Dapper | 생성 |
| Query (읽기) | `IEmployeeQueryRepository` → Dapper | 조회, 목록 |

### DataBaseManager 주요 메서드

| 메서드 | 설명 |
|--------|------|
| `ExecuteAsync(DBType, Func<DbConnection, Task<T>>)` | 단일 DB 실행 (반환값 있음) |
| `ExecuteAsync(DBType, Func<DbConnection, Task>)` | 단일 DB 실행 (반환값 없음) |
| `ExecuteAsync(DBType, DBType, Func<...>)` | 두 DB 동시 실행 |
| `ExecuteTransactionAsync(DBType, Func<DbConnection, Task<bool>>)` | 단일 DB 트랜잭션 (true 반환 시 커밋) |
| `ExecuteTransactionAsync(DBType, DBType, ...)` | 두 DB 트랜잭션 (true 반환 시 커밋) |

---

## API 엔드포인트

### Employee

| 메서드 | 경로 | 설명 |
|--------|------|------|
| `GET` | `/api/Employee` | 직원 목록 조회 (페이징) |
| `GET` | `/api/Employee/{name}` | 이름으로 직원 조회 |
| `POST` | `/api/Employee` | 직원 등록 |

#### GET /api/Employee (쿼리 파라미터)

| 파라미터 | 타입 | 기본값 | 설명 |
|----------|------|--------|------|
| `page` | int | 1 | 페이지 번호 |
| `pageSize` | int | 20 | 페이지당 항목 수 |

---

## 필수 설치 항목

### 1. .NET 8.0 SDK
```bash
dotnet --version
# 출력 예: 8.0.xxx
```
다운로드: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. IDE
- **Visual Studio 2022** (워크로드: `ASP.NET 및 웹 개발`)
- **JetBrains Rider**

---

## 로컬 실행 방법

### 1. 저장소 클론
```bash
git clone {저장소 URL}
cd CQRS
```

### 2. NuGet 패키지 복원
```bash
dotnet restore
```

### 3. appsettings.json 생성
`BackEnd/appsettings.json` 파일을 직접 생성합니다 (gitignore 대상).

```json
{
  "ConnectionStrings": {
    "WriteConnection": "Server=localhost;Database=accountdb;User=backend_user;Password=비밀번호;CharSet=utf8mb4;",
    "ReadConnection": "Server=localhost;Database=accountdb;User=backend_user;Password=비밀번호;CharSet=utf8mb4;"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

> Read/Write DB를 분리 운영하는 경우 `ReadConnection`에 Replica 서버 주소를 입력합니다.


### 4. Swagger 확인
```
https://localhost:{포트}/swagger
```

## 로깅

| 로그 종류 | 기술 | 저장 위치 |
|-----------|------|-----------|
| 애플리케이션 로그 | Serilog | `BackEnd/Logs/app-날짜.log` |

**로그 보존 정책:** 30일 (일별 롤링)

---

## 개발 규칙

### 코딩 컨벤션

| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스, 메서드, 프로퍼티 | PascalCase | `EmployeeService`, `GetListAsync` |
| private 필드 | `_camelCase` | `_queryRepository` |
| 지역 변수, 파라미터 | camelCase | `employeeId`, `request` |
| 인터페이스 | `I` + PascalCase | `IEmployeeQueryRepository` |
| 비동기 메서드 | `Async` 접미사 필수 | `CreateEmployeeAsync` |
| Command / Query / DTO | `record` 타입 | `public record CreateEmployeeCommand(...)` |
| Entity 컬럼 프로퍼티 | camelCase (예외) | `employeeId`, `createdAt` |

### 절대 금지 사항

| 금지 | 이유 / 대안 |
|------|-------------|
| `Console.WriteLine` | `ILogger<T>` (Serilog) 사용 |
| `.Result` / `.Wait()` | 데드락 위험 → `await` 사용 |
| 컨트롤러에서 비즈니스 로직 | MediatR `Send()` 만 사용 |
| 엔티티 직접 반환 | DTO 반환 필수 |
| `new` 로 서비스 직접 생성 | DI 컨테이너 사용 |
| 에러 반환 전 로그 누락 | `_logger.LogWarning/LogError` 호출 후 반환 |
| 원시 타입 직접 요청/반환 | 요청·응답은 반드시 `record` 객체 사용 |

### 에러 응답 로그 레벨

| 응답 코드 | 로그 레벨 |
|-----------|-----------|
| 400 BadRequest | `LogError` |
| 404 NotFound | `LogError` |
| 401 Unauthorized | `LogError` |
| 500 내부 오류 | `LogError` |

---

## 팀 역할 및 Claude 에이전트

| 역할 | 에이전트 (`@`으로 호출) | 담당 |
|------|------------------------|------|
| 시니어 개발자 | `@senior-developer` | 설계, 신기술 도입, 코드 리뷰 |
| 개발자 A | `@developer-a` | Command + Query 신규 기능, 유지보수 |
| 개발자 B | `@developer-b` | Command + Query 신규 기능, 유지보수 |
| QA 엔지니어 | `@qa-engineer` | 단위/통합 테스트, 버그 보고 |

### 스킬 (`/스킬명`으로 호출)

| 스킬 | 설명 |
|------|------|
| `/code-review` | 시니어 개발자 코드 리뷰 |
| `/new-feature` | 개발자 A/B — CQRS 신규 기능 구현 |
| `/maintenance` | 개발자 A/B — 버그 수정, 리팩토링 |
| `/test` | QA — 테스트 작성 및 실행 |
| `/cqrs-scaffold` | 시니어 — CQRS 전체 구조 스캐폴딩 |

### 브랜치 전략

```
main            — 운영 배포 (시니어 개발자 승인 필수)
└── develop     — 통합 개발
    ├── feature/{기능명}   — 신규 기능
    ├── fix/{버그명}       — 버그 수정
    └── refactor/{대상}   — 리팩토링
```

---

## 참고 문서

| 문서 | URL |
|------|-----|
| ASP.NET Core 공식 문서 | https://learn.microsoft.com/ko-kr/aspnet/core/ |
| Entity Framework Core | https://learn.microsoft.com/ko-kr/ef/core/ |
| Dapper GitHub | https://github.com/DapperLib/Dapper |
| MediatR GitHub | https://github.com/jbogard/MediatR |
| Serilog 공식 사이트 | https://serilog.net/ |
| C# 코딩 컨벤션 | https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/coding-style/coding-conventions |
