# /code-review — 코드 리뷰 요청

`senior-developer` 에이전트로서 아래 대상 코드에 대한 공식 코드 리뷰를 수행합니다.

## 리뷰 대상

$ARGUMENTS

인자가 없으면 현재 작업 중인 파일 또는 최근 변경된 코드를 리뷰 대상으로 합니다.

---

## 리뷰 진행 순서

### 1단계: 코드 파악
- 리뷰 대상 파일을 모두 읽습니다.
- 어떤 기능을 구현했는지 파악합니다.

### 2단계: 폴더 구조 검토
- [ ] 파일이 표준 폴더 경로에 위치하는지 확인
  - Command → `Application/Commands/{기능}/`
  - Query → `Application/Queries/{기능}/`
  - DTO → `Application/DTOs/{기능}/`
  - 인터페이스 → `Application/Interfaces/`
  - CommandRepository → `Infrastructure/Repositories/`
  - QueryRepository → `Infrastructure/Persistence/Read/`

### 3단계: CQRS 원칙 검토
- [ ] Command 핸들러에서 Dapper 미사용
- [ ] Query 핸들러에서 EF Core 쓰기 미사용
- [ ] 컨트롤러에서 MediatR `Send()`만 사용 (비즈니스 로직 없음)
- [ ] 엔티티 직접 반환 없음 → DTO 사용

### 4단계: C# 컨벤션 검토
- [ ] PascalCase: 클래스, 메서드, 프로퍼티
- [ ] _camelCase: private 필드
- [ ] Async 접미사: 모든 비동기 메서드
- [ ] record 타입: Command / Query DTO
- [ ] `.Result` / `.Wait()` 미사용
- [ ] `Console.WriteLine` 미사용

### 5단계: 로깅 검토
- [ ] 애플리케이션 로그 → `ILogger<T>` (Serilog 파일 로그)
- [ ] 서비스 이벤트 로그 → `IMongoDbServiceLogRepository` (MongoDB)
- [ ] 민감 정보(비밀번호, 토큰 등) 로그에 포함되지 않음

### 6단계: 데이터베이스 사용 검토
- [ ] MongoDB: 서비스 이벤트 외 비즈니스 데이터 저장 없음
- [ ] Redis: 세션 외 데이터 저장 없음, TTL 설정 여부
- [ ] Dapper 쿼리: 파라미터 바인딩 사용 (SQL Injection 방지)

### 7단계: 품질 검토
- [ ] 서비스를 `new`로 직접 생성하지 않음 (DI 사용)
- [ ] 공개 API 엔드포인트에 XML 주석 작성
- [ ] 단위 테스트 가능한 구조 (HTTP 컨텍스트 없이 테스트 가능)

---

## 리뷰 결과 출력 형식

```
## 코드 리뷰: {기능명 또는 파일명}

### ✅ 승인 항목
- ...

### ⚠️ 개선 권장 (비차단)
- {파일명}:{라인}: 설명

### ❌ 수정 필요 (차단)
- {파일명}:{라인}: 설명

### 최종 판정: [승인 / 의견 포함 승인 / 수정 요청]
```
