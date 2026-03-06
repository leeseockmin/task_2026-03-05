# CQRS BackEnd 서버 실행 가이드

---

## 1. 필수 설치 항목

### .NET 8.0 SDK

```bash
# 설치 확인
dotnet --version
# 출력 예: 8.0.xxx
```

다운로드: https://dotnet.microsoft.com/download/dotnet/8.0

---

### MySQL 8.0+

다운로드: https://dev.mysql.com/downloads/mysql/

설치 후 아래 SQL로 DB를 생성합니다.

```sql
CREATE DATABASE accountdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

---

### MongoDB 7.0 이상 (권장: 8.x)

다운로드: https://www.mongodb.com/try/download/community

#### 설치 후 서비스 시작 (Windows)

```bash
# 서비스 시작
net start MongoDB

# 서비스 중지
net stop MongoDB

# 서비스 상태 확인
sc query MongoDB
```

#### 연결 확인

MongoDB Shell(mongosh)을 별도 설치한 경우:

```bash
mongosh
```

> **mongosh 다운로드 (선택)**: https://www.mongodb.com/try/download/shell

#### MongoDB Compass (GUI 뷰어, 선택 설치)

다운로드: https://www.mongodb.com/try/download/compass

1. MongoDB Compass 실행
2. `mongodb://localhost:27017` 로 연결
3. 좌측 데이터베이스 목록에서 **`LogDB`** 선택
4. 컬렉션 목록에서 `{yyyyMMdd}_Employee_Log` 선택 (예: `20260307_Employee_Log`)
5. Documents 탭에서 JSON 형태로 로그 확인 가능

---

### dotnet-ef CLI 도구

CLI 방식으로 Migration을 실행할 경우 필요합니다.

```bash
# 설치
dotnet tool install --global dotnet-ef

# 설치 확인
dotnet ef --version
```

---

### IDE

- **Visual Studio 2022** (워크로드: `ASP.NET 및 웹 개발`)
- **JetBrains Rider**

---

## 2. 설정 파일 생성

`BackEnd/appsettings.json` 파일은 gitignore 대상이므로 **직접 생성**해야 합니다.

```
BackEnd/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "WriteConnection": "Server=localhost;Database=accountdb;Uid=아이디;Pwd=비밀번호;CharSet=utf8mb4;",
    "ReadConnection": "Server=localhost;Database=accountdb;Uid=아이디;Pwd=비밀번호;CharSet=utf8mb4;",
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "LogDB"
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

| 항목 | 설명 |
|------|------|
| `ConnectionStrings.MongoDB` | MongoDB 접속 주소. 인증이 필요한 경우 `mongodb://아이디:비밀번호@localhost:27017` 형식 사용 |
| `MongoDB.DatabaseName` | 로그가 저장될 MongoDB 데이터베이스 이름 |

---

## 3. Migration 적용

서버는 **시작 시점에 미적용 Migration이 있으면 자동으로 실행을 중단(아래에 UpdateMigration 확인)**합니다.
최초 실행 또는 Migration이 변경된 경우 아래 두 가지 방법 중 하나로 적용하세요.

---

### 방법 A — CLI (dotnet ef)

```bash
# Data 프로젝트 디렉터리로 이동
cd Data

# Migration 파일이 없는 경우에만 실행 (최초 1회)
dotnet ef migrations add InitialCreate --context AccountDBContext --output-dir Migrations/AccountMigrations -- --environment Development

# DB에 Migration 적용
dotnet ef database update --context AccountDBContext -- --environment Development
```

---

### 방법 B — Visual Studio (Package Manager Console)

`도구 → NuGet 패키지 관리자 → 패키지 관리자 콘솔` 에서 실행합니다.

> **기본 프로젝트(Default project)를 `BackEnd`로 설정 후 실행하세요.**

**Migration 추가** (스키마 변경 후 실행, 최초 1회)

```powershell
Add-Migration InitialCreate -Context AccountDBContext -Project DB.Data -OutputDir "Migrations\AccountMigrations"
```

**DB 업데이트** (Migration을 실제 DB에 적용)

```powershell
Update-Database -Context AccountDBContext -Project DB.Data -StartupProject BackEnd
```

**특정 Migration으로 롤백**

```powershell
# 특정 시점으로 롤백
Update-Database InitialCreate -Context AccountDBContext -Project DB.Data -StartupProject BackEnd

# 모든 Migration 롤백 (DB 초기화)
Update-Database 0 -Context AccountDBContext -Project DB.Data -StartupProject BackEnd
```

**마지막 Migration 제거** (DB 적용 전에만 가능)

```powershell
Remove-Migration -Context AccountDBContext -Project DB.Data
```

**Migration 목록 확인**

```powershell
Get-Migration -Context AccountDBContext -Project DB.Data
```

**SQL 스크립트 추출** (배포 확인용)

```powershell
Script-Migration -From 0 -To InitialCreate -Context AccountDBContext -Project DB.Data
```

---

### Migration 오류가 발생하는 경우

서버 실행 시 아래 오류가 발생하면 Migration이 DB에 적용되지 않은 상태입니다.

```
미적용 마이그레이션이 있습니다. 서버를 시작하기 전에 마이그레이션을 적용하세요.
```

위 방법 A 또는 방법 B의 **DB 업데이트** 명령어를 실행하면 해결됩니다.

---

## 4. 서버 실행(CLI)

```bash
# NuGet 패키지 복원
dotnet restore

# 서버 실행
cd BackEnd
dotnet run
```

---

## 5. Swagger 확인

```
실행은 항상 http로 진행해야 됩니다.
http://localhost:{포트}/swagger
```

포트는 `BackEnd/Properties/launchSettings.json`의 `applicationUrl` 항목에서 확인합니다.

---

## 6. EmployeeTestApp (WPF 테스트 도구)

`EmployeeTestApp`은 직원 등록 API(`POST /api/Employee`)를 GUI로 테스트할 수 있는 Windows 전용 도구입니다.
서버가 실행 중인 상태에서 Visual Studio로 `EmployeeTestApp` 프로젝트를 실행합니다.

> **Windows 전용** — `net8.0-windows` 기반으로 macOS / Linux에서는 실행되지 않습니다.

---

### 서버 URL 설정

앱 상단의 **서버 URL** 입력란에 실행 중인 서버 주소를 입력합니다.

```
기본값: http://localhost:5136
```

> 로컬 HTTPS 인증서 검증은 자동으로 무시됩니다.

---

### 테스트 기능 (탭별)

#### Tab 1 — CSV 파일 업로드

`.csv` 파일을 선택해서 직원 데이터를 일괄 등록합니다.

**CSV 형식** (헤더 없이, 한 줄에 한 명)

```
이름,이메일,전화번호,입사일
홍길동,hong@example.com,01025862222,2018.03.07
김철수,kim@example.com,01012345678,2020.01.15
```

| 필드 | 설명 |
|------|------|
| 이름 | 필수 |
| 이메일 | 필수 |
| 전화번호 | 하이픈 포함/미포함 모두 허용 |
| 입사일 | `yyyy.MM.dd` / `yyyy-MM-dd` / `yyyy/MM/dd` 형식 허용 |

---

#### Tab 2 — JSON 파일 업로드

`.json` 파일을 선택해서 직원 데이터를 일괄 등록합니다.

**JSON 형식**

```json
[
  { "name": "홍길동", "email": "hong@example.com", "tel": "010-1111-2424", "joined": "2012-01-05" },
  { "name": "김철수", "email": "kim@example.com",  "tel": "010-2222-3333", "joined": "2020-03-15" }
]
```

| 필드 | 설명 |
|------|------|
| name | 필수 |
| email | 필수 |
| tel | 하이픈 포함/미포함 모두 허용 |
| joined | `yyyy-MM-dd` / `yyyy.MM.dd` / `yyyy/MM/dd` 형식 허용 |

> 단일 객체 `{ }` 형태도 지원합니다.

---

#### Tab 3 — CSV 직접 입력

CSV 텍스트를 직접 입력 후 전송합니다. 형식은 Tab 1과 동일합니다.
**초기화** 버튼을 누르면 예시 데이터로 초기화됩니다.

---

#### Tab 4 — JSON 직접 입력

JSON 텍스트를 직접 입력 후 전송합니다. 형식은 Tab 2와 동일합니다.
**초기화** 버튼을 누르면 예시 데이터로 초기화됩니다.

---

### 응답 로그

화면 하단의 **응답 로그** 영역에서 요청/응답 내용을 실시간으로 확인할 수 있습니다.

| 표시 정보 | 내용 |
|-----------|------|
| `▶ POST {url}` | 전송한 URL과 인원 수 |
| `Body` | 전송한 JSON 본문 (최대 300자 미리보기) |
| `Status` | HTTP 응답 코드 |
| `Response` | 서버 응답 본문 |
| `[!]` 접두사 | 오류 발생 항목 |

**로그 지우기** 버튼으로 로그를 초기화할 수 있습니다.

---

## 7. MongoDB 이벤트 로그

직원 등록(`POST /api/Employee`) 성공 시 MongoDB에 자동으로 로그가 기록됩니다.

### 컬렉션 명명 규칙

```
{yyyyMMdd}_{테이블명}_Log

예시)
20260307_Employee_Log   ← 2026년 3월 7일 Employee 테이블 로그
20260308_Employee_Log   ← 2026년 3월 8일 Employee 테이블 로그
```

날짜가 바뀌면 컬렉션이 자동으로 새로 생성됩니다. 별도 설정 불필요합니다.

---

### 저장되는 도큐먼트 구조

```json
{
  "_id": "ObjectId(...)",
  "Action": "CREATE",
  "TableName": "Employee",
  "Payload": {
    "items": [
      {
        "name": "홍길동",
        "email": "hong@example.com",
        "tel": "01012345678",
        "joined": "2024-03-07T00:00:00Z",
        "createdAt": "2026-03-07T09:00:00Z"
      }
    ]
  },
  "OccurredAt": "2026-03-07T09:00:00Z"
}
```

---

### MongoDB Compass로 확인하는 방법

1. MongoDB Compass 실행
2. `mongodb://localhost:27017` 로 연결
3. 좌측 데이터베이스 목록에서 `LogDB` 선택
4. 컬렉션 목록에서 `20260307_Employee_Log` 선택
5. Documents 탭에서 JSON 형태로 확인

---

### 새 테이블 로그 추가 방법

새로운 테이블의 이벤트 로그가 필요할 경우, 해당 CommandHandler에 `IMongoLogService`를 주입하고 아래와 같이 호출합니다.

```csharp
// 예: DepartmentCommandHandler
await _mongoLogService.LogAsync("Department", "CREATE", departments);
await _mongoLogService.LogAsync("Department", "UPDATE", departments);
await _mongoLogService.LogAsync("Department", "DELETE", departments);
```

별도 파일 추가 없이 `tableName` 파라미터만 바꾸면 컬렉션이 자동 생성됩니다.
