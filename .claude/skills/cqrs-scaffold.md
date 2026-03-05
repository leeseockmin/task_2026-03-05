# /cqrs-scaffold — CQRS 전체 구조 스캐폴딩

`senior-developer` 에이전트로서 지정한 기능의 CQRS 전체 파일 구조를 한 번에 생성합니다.
Command(쓰기) + Query(읽기) + 컨트롤러 + 인터페이스를 모두 포함합니다.

## 스캐폴딩 기능명

$ARGUMENTS

---

## 생성할 파일 목록

`{기능}`을 인자로 받은 기능명으로 치환하여 아래 파일들을 순서대로 생성합니다.

### [Domain]
- `BackEnd/Domain/Entities/{기능}.cs`

### [Application — DTO]
- `BackEnd/Application/DTOs/{기능}/{기능}Dto.cs`
- `BackEnd/Application/DTOs/{기능}/{기능}SummaryDto.cs`

### [Application — Command]
- `BackEnd/Application/Commands/{기능}/Create{기능}Command.cs`
- `BackEnd/Application/Commands/{기능}/Create{기능}CommandHandler.cs`
- `BackEnd/Application/Commands/{기능}/Update{기능}Command.cs`
- `BackEnd/Application/Commands/{기능}/Update{기능}CommandHandler.cs`
- `BackEnd/Application/Commands/{기능}/Delete{기능}Command.cs`
- `BackEnd/Application/Commands/{기능}/Delete{기능}CommandHandler.cs`

### [Application — Query]
- `BackEnd/Application/Queries/{기능}/Get{기능}ByIdQuery.cs`
- `BackEnd/Application/Queries/{기능}/Get{기능}ByIdQueryHandler.cs`
- `BackEnd/Application/Queries/{기능}/Get{기능}ListQuery.cs`
- `BackEnd/Application/Queries/{기능}/Get{기능}ListQueryHandler.cs`

### [Application — Interface]
- `BackEnd/Application/Interfaces/I{기능}CommandRepository.cs`
- `BackEnd/Application/Interfaces/I{기능}QueryRepository.cs`

### [Infrastructure — Write (EF Core)]
- `BackEnd/Infrastructure/Persistence/Write/Configurations/{기능}Configuration.cs`
- `BackEnd/Infrastructure/Repositories/{기능}CommandRepository.cs`

### [Infrastructure — Read (Dapper)]
- `BackEnd/Infrastructure/Persistence/Read/{기능}QueryRepository.cs`

### [Controller]
- `BackEnd/Controllers/{기능}Controller.cs`
  - `GET    /api/{기능}` — 목록 조회
  - `GET    /api/{기능}/{id}` — 단건 조회
  - `POST   /api/{기능}` — 생성
  - `PUT    /api/{기능}/{id}` — 수정
  - `DELETE /api/{기능}/{id}` — 삭제

---

## 각 파일 코드 기준

- 엔티티: private setter + 팩토리 메서드 (`Create`) + 업데이트 메서드 (`Update`)
- Command DTO: `record` 타입, `IRequest<T>` 구현
- Query DTO: `record` 타입, `IRequest<T>` 구현
- 핸들러: 생성자 DI, `ILogger<T>` + `IMongoDbServiceLogRepository` 주입
- CommandRepository: EF Core `AppDbContext` 사용
- QueryRepository: Dapper `IDbConnection` 사용, 파라미터 바인딩 필수
- 컨트롤러: MediatR `Send()` 만 사용, XML 주석 필수

---

## 완료 후 안내

스캐폴딩 완료 후 아래 내용을 출력합니다:

1. 생성된 전체 파일 목록
2. `Program.cs` DI 등록 코드 예시
3. EF Core 마이그레이션 명령어:
   ```bash
   dotnet ef migrations add Add{기능}
   dotnet ef database update
   ```
4. 개발자A·B가 TODO 항목을 채워 구현을 완성하도록 안내
5. 완성 후 `/code-review`로 시니어 개발자에게 리뷰 요청 안내
