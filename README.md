# BackEnd 프로젝트

ASP.NET Core 8.0 기반 백엔드 API 서버입니다.
CQRS 패턴을 적용하며, MySQL(주 DB), MongoDB(서비스 이벤트 로그), Redis(세션)를 사용합니다.

---

## 기술 스택

| 기술 | 버전 | 용도 |
|------|------|------|
| .NET | 8.0 | 런타임 |
| ASP.NET Core | 8.0 | Web API 프레임워크 |
| MySQL | 8.0+ | 주 데이터베이스 |
| MongoDB | 7.0+ | 서비스 이벤트 로그 저장 |
| Redis | 7.0+ | 사용자 세션 관리 |
| Entity Framework Core | 8.x | ORM — Command(쓰기) 측 |
| Dapper | 2.x | 마이크로 ORM — Query(읽기) 측 |
| MediatR | 12.x | CQRS 핸들러 디스패치 |
| **Serilog** | 4.x | 애플리케이션 로그 → 파일 |
| xUnit | 2.x | 단위 테스트 |
| Moq | 4.x | 테스트 모킹 |

---

## 필수 설치 항목

### 1. .NET 8.0 SDK
- **다운로드:** https://dotnet.microsoft.com/download/dotnet/8.0
- **설치 확인:**
  ```bash
  dotnet --version
  # 출력 예: 8.0.xxx
  ```

### 2. MySQL 8.0
- **다운로드:** https://dev.mysql.com/downloads/mysql/
- **MySQL Installer (Windows):** https://dev.mysql.com/downloads/installer/
- **설치 확인:**
  ```bash
  mysql --version
  ```
- **초기 DB 설정:**
  ```sql
  CREATE DATABASE BackEndDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
  CREATE USER 'backend_user'@'localhost' IDENTIFIED BY '비밀번호';
  GRANT ALL PRIVILEGES ON BackEndDb.* TO 'backend_user'@'localhost';
  FLUSH PRIVILEGES;
  ```

### 3. MongoDB 7.0
- **다운로드:** https://www.mongodb.com/try/download/community
- **설치 가이드 (Windows):** https://www.mongodb.com/docs/manual/tutorial/install-mongodb-on-windows/
- **설치 확인:**
  ```bash
  mongod --version
  ```
- **MongoDB Compass (GUI, 권장):** https://www.mongodb.com/try/download/compass

### 4. Redis 7.0
- **Windows (WSL2 방식, 권장):** https://redis.io/docs/install/install-redis/install-redis-on-windows/
- **Windows (MSI):** https://github.com/microsoftarchive/redis/releases
- **설치 확인:**
  ```bash
  redis-cli ping
  # 출력: PONG
  ```

### 5. Visual Studio 2022 또는 JetBrains Rider
- **Visual Studio 2022 Community:** https://visualstudio.microsoft.com/ko/downloads/
  - 설치 워크로드: `ASP.NET 및 웹 개발` 선택
- **JetBrains Rider:** https://www.jetbrains.com/rider/

### 6. EF Core CLI 도구
```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
```

### 7. NuGet 패키지 (자동 복원)
```bash
cd BackEnd
dotnet restore
```

**주요 NuGet 패키지 목록:**

| 패키지 | 용도 |
|--------|------|
| `Serilog.AspNetCore` | Serilog ASP.NET Core 통합 |
| `Serilog.Sinks.File` | 파일 로그 출력 |
| `Serilog.Sinks.Console` | 콘솔 로그 출력 |
| `MediatR` | CQRS 핸들러 디스패치 |
| `Microsoft.EntityFrameworkCore` | ORM (Command 측) |
| `Pomelo.EntityFrameworkCore.MySql` | MySQL EF Core 드라이버 |
| `Dapper` | 마이크로 ORM (Query 측) |
| `MongoDB.Driver` | MongoDB .NET 드라이버 |
| `StackExchange.Redis` | Redis .NET 클라이언트 |
| `xunit` | 단위 테스트 |
| `Moq` | 테스트 모킹 |

---

## 프로젝트 구조

```
Project/
├── BackEnd/
│   ├── Application/
│   │   ├── Commands/                 # CQRS — Command(쓰기) 측
│   │   │   └── {기능}/
│   │   │       ├── Create{기능}Command.cs
│   │   │       ├── Create{기능}CommandHandler.cs
│   │   │       ├── Update{기능}Command.cs
│   │   │       ├── Update{기능}CommandHandler.cs
│   │   │       ├── Delete{기능}Command.cs
│   │   │       └── Delete{기능}CommandHandler.cs
│   │   ├── Queries/                  # CQRS — Query(읽기) 측
│   │   │   └── {기능}/
│   │   │       ├── Get{기능}ByIdQuery.cs
│   │   │       ├── Get{기능}ByIdQueryHandler.cs
│   │   │       ├── Get{기능}ListQuery.cs
│   │   │       └── Get{기능}ListQueryHandler.cs
│   │   ├── DTOs/                     # 응답 DTO
│   │   │   └── {기능}/
│   │   │       ├── {기능}Dto.cs
│   │   │       └── {기능}SummaryDto.cs
│   │   └── Interfaces/               # Repository 인터페이스
│   │       ├── I{기능}CommandRepository.cs
│   │       └── I{기능}QueryRepository.cs
│   ├── Domain/
│   │   ├── Entities/                 # 도메인 엔티티
│   │   └── Events/                   # 도메인 이벤트
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Write/                # EF Core (Command — 쓰기)
│   │   │   │   ├── AppDbContext.cs
│   │   │   │   └── Configurations/
│   │   │   └── Read/                 # Dapper (Query — 읽기)
│   │   │       └── {기능}QueryRepository.cs
│   │   ├── Logging/                  # MongoDB 서비스 이벤트 로그
│   │   │   └── MongoDbServiceLogRepository.cs
│   │   ├── Session/                  # Redis 세션
│   │   │   └── RedisSessionService.cs
│   │   └── Repositories/             # CommandRepository 구현체
│   ├── Controllers/
│   ├── Logs/                         # Serilog 파일 로그 출력 경로
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
├── BackEnd.Tests/
│   ├── Unit/
│   │   ├── Commands/
│   │   └── Queries/
│   └── Integration/
│       └── Controllers/
└── README.md
```

---

## 로깅 전략

| 로그 종류 | 기술 | 저장 위치 | 대상 |
|-----------|------|-----------|------|
| 애플리케이션 로그 | **Serilog** | 파일 (`Logs/app-날짜.log`) | 에러, 경고, 디버그, 앱 시작/종료 |
| 서비스 이벤트 로그 | **MongoDB** | `ServiceLogs` 컬렉션 | 사용자 행위, 비즈니스 이벤트, 감사 로그 |

**MongoDB 서비스 이벤트 로그 기록 대상 예시:**
- 사용자 로그인 / 로그아웃
- 데이터 생성 / 수정 / 삭제 완료
- 외부 API 호출 이벤트
- 권한 위반 시도

---

## 설정 파일 구성

`appsettings.Development.json` 예시:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=BackEndDb;User=backend_user;Password=비밀번호;",
    "MongoDB": "mongodb://localhost:27017",
    "Redis": "localhost:6379"
  },
  "MongoDB": {
    "DatabaseName": "BackEndDb",
    "ServiceLogCollection": "ServiceLogs"
  },
  "Redis": {
    "SessionTTLMinutes": 30
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

---

## 로컬 실행 방법

### 1. 저장소 클론
```bash
git clone {저장소 URL}
cd Project
```

### 2. 로컬 서비스 시작
```bash
# MongoDB 시작
mongod --dbpath C:\data\db

# Redis 시작
redis-server
```

### 3. appsettings.Development.json 설정
위 설정 구성 예시를 참고하여 연결 문자열 수정

### 4. EF Core 마이그레이션 적용
```bash
cd BackEnd
dotnet ef database update
```

### 5. 프로젝트 실행
```bash
dotnet run --project BackEnd
```

### 6. API 문서 확인 (Swagger)
브라우저: `https://localhost:{포트}/swagger`

---

## 개발 작업 방식

### CQRS 패턴 원칙

| 구분 | 기술 | 역할 |
|------|------|------|
| Command (쓰기) | EF Core (ORM) | 생성, 수정, 삭제 |
| Query (읽기) | Dapper | 조회, 검색, 목록 |

**핵심 규칙:**
- Command 핸들러 → **EF Core만** 사용
- Query 핸들러 → **Dapper만** 사용
- MongoDB → **서비스 이벤트 로그 전용**
- Redis → **세션 전용** (TTL 30분)
- 애플리케이션 로그 → **Serilog → 파일**

### 브랜치 전략

```
main            — 운영 배포 (시니어 개발자 승인 필수)
└── develop     — 통합 개발
    ├── feature/{기능명}   — 신규 기능
    ├── fix/{버그명}       — 버그 수정
    └── refactor/{대상}   — 리팩토링
```

### 팀 작업 흐름

```
1. develop 에서 feature 브랜치 생성
2. 개발자A 또는 개발자B — Command + Query 구현
3. /code-review → 시니어 개발자 리뷰 요청
4. QA — /test 실행 (단위 + 통합 테스트)
5. 시니어 개발자 승인 후 develop 머지
6. develop → main 배포
```

---

## 팀 역할 및 Claude 에이전트

### 에이전트 (`@에이전트명`으로 호출)

| 역할 | 에이전트 | 담당 업무 |
|------|----------|-----------|
| 시니어 개발자 | `@senior-developer` | 프로젝트 초기 설계, 신기술 도입, 팀 룰 수립, 코드 리뷰 |
| 개발자A | `@developer-a` | Command + Query 신규 기능, 코드 유지보수 |
| 개발자B | `@developer-b` | Command + Query 신규 기능, 코드 유지보수 |
| QA 엔지니어 | `@qa-engineer` | 단위/통합 테스트, 버그 보고 |

> 개발자A와 개발자B는 시니어 개발자가 정의한 룰을 반드시 참조하여 개발합니다.

### 스킬 (`.claude/skills/`)

| 스킬 파일 | 설명 |
|-----------|------|
| `code-review.md` | 시니어 개발자의 코드 리뷰 수행 |
| `new-feature.md` | 개발자A/B — Command + Query 신규 기능 구현 |
| `maintenance.md` | 개발자A/B — 유지보수 (버그 수정, 리팩토링, 성능 개선) |
| `test.md` | QA — 테스트 작성 및 실행 |
| `cqrs-scaffold.md` | 시니어 — CQRS 전체 구조 스캐폴딩 |

---

## C# 코딩 컨벤션

참고: https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/coding-style/coding-conventions

| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스, 메서드, 프로퍼티 | PascalCase | `UserService`, `GetUserAsync` |
| private 필드 | `_camelCase` | `_userRepository` |
| 지역 변수, 파라미터 | camelCase | `userId`, `request` |
| 인터페이스 | `I` + PascalCase | `IUserRepository` |
| 비동기 메서드 | `Async` 접미사 필수 | `CreateUserAsync` |
| Command / Query DTO | `record` 타입 | `public record CreateUserCommand(...)` |

**절대 금지:**

| 금지 사항 | 대안 |
|-----------|------|
| `Console.WriteLine` | `ILogger<T>` (Serilog) 사용 |
| `.Result` / `.Wait()` | `await` 사용 |
| 컨트롤러에서 비즈니스 로직 | MediatR `Send()` 사용 |
| 엔티티 직접 반환 | DTO 반환 |
| `new` 로 서비스 생성 | DI 컨테이너 사용 |

---

## 자주 사용하는 명령어

### EF Core 마이그레이션
```bash
dotnet ef migrations add {마이그레이션명} --project BackEnd
dotnet ef database update --project BackEnd
dotnet ef migrations remove --project BackEnd
dotnet ef migrations list --project BackEnd
```

### 테스트 실행
```bash
dotnet test
dotnet test --filter "FullyQualifiedName~{테스트클래스명}"
dotnet test --collect:"XPlat Code Coverage"
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
| Serilog.AspNetCore | https://github.com/serilog/serilog-aspnetcore |
| MongoDB .NET Driver | https://www.mongodb.com/docs/drivers/csharp/current/ |
| StackExchange.Redis | https://stackexchange.github.io/StackExchange.Redis/ |
| C# 코딩 컨벤션 | https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/coding-style/coding-conventions |
| xUnit 문서 | https://xunit.net/ |
| Moq 문서 | https://github.com/devlooped/moq |
