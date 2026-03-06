using BackEnd.Application.Commands.Employee;
using BackEnd.Application.DTOs.Employee;
using BackEnd.Application.Interfaces.Employee;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EmployeeEntity = DB.Data.AccountDB.Employee;

namespace BackEnd.Tests.Commands
{
    /// <summary>
    /// CreateEmployeeCommandHandler 단위 테스트
    ///
    /// 입력 유효성 검사 로직(ArgumentException)은 핸들러 내부에 위치하므로
    /// 핸들러 레벨에서 직접 검증합니다.
    /// </summary>
    public class CreateEmployeeCommandHandlerTests
    {
        private readonly Mock<IEmployeeCommandRepository> _mockCommandRepository;
        private readonly Mock<ILogger<CreateEmployeeCommandHandler>> _mockLogger;
        private readonly CreateEmployeeCommandHandler _handler;

        public CreateEmployeeCommandHandlerTests()
        {
            _mockCommandRepository = new Mock<IEmployeeCommandRepository>();
            _mockLogger = new Mock<ILogger<CreateEmployeeCommandHandler>>();
            _handler = new CreateEmployeeCommandHandler(
                _mockCommandRepository.Object,
                _mockLogger.Object);
        }

        // =============================================
        // 성공 케이스
        // =============================================

        /// <summary>
        /// [성공] 유효한 단건 직원 요청 — BulkInsertAsync가 1회 호출되고 1을 반환한다.
        /// </summary>
        [Fact]
        public async Task Handle_ValidSingleEmployee_CallsBulkInsertAsyncOnceAndReturns1()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "hong@example.com", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            _mockCommandRepository
                .Setup(r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(1, result);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Once);
        }

        /// <summary>
        /// [성공] 유효한 복수 직원 요청 — BulkInsertAsync가 1회 호출되고 삽입된 행 수(N)를 반환한다.
        /// </summary>
        [Fact]
        public async Task Handle_ValidMultipleEmployees_CallsBulkInsertAsyncOnceAndReturnsN()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "hong@example.com", "01011111111", DateTime.UtcNow),
                new CreateEmployeeRequest("이순신", "lee@example.com",  "01022222222", DateTime.UtcNow),
                new CreateEmployeeRequest("강감찬", "kang@example.com", "01033333333", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            _mockCommandRepository
                .Setup(r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()))
                .ReturnsAsync(3);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(3, result);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Once);
        }

        /// <summary>
        /// [성공] Tel에 하이픈이 포함된 경우 — BulkInsertAsync 호출 전 하이픈이 제거된다.
        /// Moq Callback으로 실제 저장된 tel 값을 캡처하여 검증한다.
        /// </summary>
        [Fact]
        public async Task Handle_TelWithHyphens_RemovesHyphensBeforeBulkInsert()
        {
            // Arrange — 하이픈 포함 Tel 입력
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "hong@example.com", "010-1234-5678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            List<EmployeeEntity>? capturedEmployees = null;
            _mockCommandRepository
                .Setup(r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()))
                .Callback<List<EmployeeEntity>>(employees => capturedEmployees = employees)
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert — 하이픈이 제거된 값으로 저장되었는지 확인
            Assert.Equal(1, result);
            Assert.NotNull(capturedEmployees);
            Assert.Equal("01012345678", capturedEmployees![0].tel);
        }

        /// <summary>
        /// [성공] 빈 목록 전달 — BulkInsertAsync가 호출되고 0을 반환한다.
        /// </summary>
        [Fact]
        public async Task Handle_EmptyList_CallsBulkInsertAsyncAndReturns0()
        {
            // Arrange
            var command = new CreateEmployeeCommand(new List<CreateEmployeeRequest>());

            _mockCommandRepository
                .Setup(r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(0, result);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Once);
        }

        // =============================================
        // 실패 케이스 — Name 유효성
        // =============================================

        /// <summary>
        /// [실패] Name이 null인 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_NameIsNull_ThrowsArgumentException()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest(null!, "hong@example.com", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("name은 필수 입력값입니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Name이 빈 문자열인 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_NameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("", "hong@example.com", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("name은 필수 입력값입니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Name이 공백으로만 이루어진 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_NameIsWhitespace_ThrowsArgumentException()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("   ", "hong@example.com", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("name은 필수 입력값입니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Name이 100자를 초과하는 경우(101자) — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_NameExceeds100Characters_ThrowsArgumentException()
        {
            // Arrange — 101자 이름
            var longName = new string('가', 101);
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest(longName, "hong@example.com", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("name은 최대 100자까지 허용됩니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [성공] Name이 정확히 100자인 경우(경계값) — 예외 없이 정상 처리된다.
        /// </summary>
        [Fact]
        public async Task Handle_NameExactly100Characters_ProcessesSuccessfully()
        {
            // Arrange — 경계값: 정확히 100자는 허용되어야 합니다.
            var maxName = new string('가', 100);
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest(maxName, "hong@example.com", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            _mockCommandRepository
                .Setup(r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(1, result);
        }

        // =============================================
        // 실패 케이스 — Email 유효성
        // =============================================

        /// <summary>
        /// [실패] Email이 null인 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_EmailIsNull_ThrowsArgumentException()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", null!, "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("email은 필수 입력값입니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Email이 빈 문자열인 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_EmailIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("email은 필수 입력값입니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Email이 공백으로만 이루어진 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_EmailIsWhitespace_ThrowsArgumentException()
        {
            // Arrange
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "   ", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("email은 필수 입력값입니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Email이 255자를 초과하는 경우(256자) — 정규식 검사 이전에 ArgumentException이 발생한다.
        /// </summary>
        [Fact]
        public async Task Handle_EmailExceeds255Characters_ThrowsArgumentException()
        {
            // Arrange — 256자 이메일 (로컬 파트 250자 + "@x.com" 6자 = 256자)
            var localPart = new string('a', 250);
            var longEmail = $"{localPart}@x.com";
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", longEmail, "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("email은 최대 255자까지 허용됩니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Email에 @ 기호가 없는 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_EmailWithNoAtSign_ThrowsArgumentException()
        {
            // Arrange — @ 없음
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "not-an-email", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("email 형식이 올바르지 않습니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Email 도메인에 점(.)이 없는 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_EmailWithNoDotInDomain_ThrowsArgumentException()
        {
            // Arrange — 점 없음
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "user@nodot", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("email 형식이 올바르지 않습니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Email에 @ 기호가 중복된 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_EmailWithDoubleAtSign_ThrowsArgumentException()
        {
            // Arrange — @ 중복
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "@@double.com", "01012345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("email 형식이 올바르지 않습니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        // =============================================
        // 실패 케이스 — Tel 유효성
        // =============================================

        /// <summary>
        /// [실패] Tel이 하이픈 제거 후 20자를 초과하는 경우(21자) — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_TelExceeds20CharactersAfterHyphenRemoval_ThrowsArgumentException()
        {
            // Arrange — 하이픈 없이 21자
            var longTel = "0" + new string('1', 20);
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "hong@example.com", longTel, DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("tel은 하이픈 제거 후 최대 20자까지 허용됩니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [실패] Tel에 하이픈이 포함되어 있지만 제거 후에도 20자를 초과하는 경우 — ArgumentException이 발생한다.
        /// </summary>
        [Fact]
        public async Task Handle_TelWithHyphensExceeds20CharactersAfterRemoval_ThrowsArgumentException()
        {
            // Arrange — 하이픈 포함이지만 제거 후 21자 초과 ("0" + "1111" * 5 = 21자)
            var longTelWithHyphens = "0-" + string.Join("-", Enumerable.Repeat("1111", 5));
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "hong@example.com", longTelWithHyphens, DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }

        /// <summary>
        /// [성공] Tel이 정확히 20자인 경우(경계값) — 예외 없이 정상 처리된다.
        /// </summary>
        [Fact]
        public async Task Handle_TelExactly20Characters_ProcessesSuccessfully()
        {
            // Arrange — 경계값: 정확히 20자는 허용되어야 합니다.
            var maxTel = new string('1', 20);
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "hong@example.com", maxTel, DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            _mockCommandRepository
                .Setup(r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(1, result);
        }

        // =============================================
        // 복수 항목 중 일부 실패 케이스
        // =============================================

        /// <summary>
        /// [실패] 복수 요청 중 두 번째 항목의 Name이 비어있는 경우 — ArgumentException이 발생하고 BulkInsertAsync는 호출되지 않는다.
        /// </summary>
        [Fact]
        public async Task Handle_SecondItemHasEmptyName_ThrowsArgumentException()
        {
            // Arrange — 첫 번째 항목은 유효하고 두 번째 항목은 Name 누락
            var requests = new List<CreateEmployeeRequest>
            {
                new CreateEmployeeRequest("홍길동", "hong@example.com", "01012345678", DateTime.UtcNow),
                new CreateEmployeeRequest("",      "lee@example.com",  "01022345678", DateTime.UtcNow)
            };
            var command = new CreateEmployeeCommand(requests);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Contains("name은 필수 입력값입니다.", ex.Message);
            _mockCommandRepository.Verify(
                r => r.BulkInsertAsync(It.IsAny<List<EmployeeEntity>>()),
                Times.Never);
        }
    }
}
