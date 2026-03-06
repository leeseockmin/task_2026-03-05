# /new-feature — 신규 기능 구현

`developer-a` 또는 `developer-b` 에이전트로서 아래 기능을 CQRS 패턴에 맞게 구현합니다.
Command(쓰기)와 Query(읽기) **모두** 작성합니다.

## 구현할 기능

$ARGUMENTS

---

## 구현 전 확인 사항

1. `.claude/skills/team-rules.md` 파일을 읽고 **팀 개발 룰** 숙지
2. 기존에 동일한 엔티티/인터페이스가 있는지 확인
3. 폴더 구조 표준 경로 확인

---

## 구현 단계

### [Command — 쓰기]
1. `Domain/Entities/{엔티티}.cs` — 엔티티 생성 또는 확인
2. `Application/Commands/{기능}/Create{기능}Command.cs` — Command DTO (`record`)
3. `Application/Commands/{기능}/Create{기능}CommandHandler.cs` — 핸들러
4. `Application/Commands/{기능}/Update{기능}Command.cs` — 수정 Command DTO
5. `Application/Commands/{기능}/Update{기능}CommandHandler.cs` — 수정 핸들러
6. `Application/Commands/{기능}/Delete{기능}Command.cs` — 삭제 Command DTO
7. `Application/Commands/{기능}/Delete{기능}CommandHandler.cs` — 삭제 핸들러
8. `Application/Interfaces/I{기능}CommandRepository.cs` — 인터페이스
9. `Infrastructure/Persistence/Write/Configurations/{엔티티}Configuration.cs` — EF Core 설정
10. `Infrastructure/Repositories/{기능}CommandRepository.cs` — EF Core 구현체

### [Query — 읽기]
11. `Application/DTOs/{기능}/{기능}Dto.cs` — 응답 DTO (`record`)
12. `Application/Queries/{기능}/Get{기능}ByIdQuery.cs` — 단건 Query DTO
13. `Application/Queries/{기능}/Get{기능}ByIdQueryHandler.cs` — 단건 핸들러
14. `Application/Queries/{기능}/Get{기능}ListQuery.cs` — 목록 Query DTO
15. `Application/Queries/{기능}/Get{기능}ListQueryHandler.cs` — 목록 핸들러
16. `Application/Interfaces/I{기능}QueryRepository.cs` — 인터페이스
17. `Infrastructure/Persistence/Read/{기능}QueryRepository.cs` — Dapper 구현체

### [Controller]
18. `Controllers/{기능}Controller.cs` — GET/POST/PUT/DELETE 엔드포인트, XML 주석 필수

### [DI 등록]
19. `Program.cs` — 새 Repository 및 Handler DI 등록 안내

---

## 로깅 적용 기준

| 로그 종류 | 방법 | 시점 |
|-----------|------|------|
| 애플리케이션 로그 | `ILogger<T>` (Serilog → 파일) | 핸들러 시작/완료/오류 |
| 서비스 이벤트 로그 | `IMongoDbServiceLogRepository` (MongoDB) | 생성/수정/삭제 완료 후 |

---

## 완료 후 처리

1. 생성한 파일 목록과 경로를 요약 출력
2. EF Core 마이그레이션 명령어 안내:
   ```bash
   dotnet ef migrations add Add{기능}
   dotnet ef database update
   ```
3. `/code-review`로 시니어 개발자에게 리뷰 요청
