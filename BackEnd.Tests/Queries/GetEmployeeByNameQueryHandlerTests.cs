using BackEnd.Application.DTOs.Employee;
using BackEnd.Application.Interfaces.Employee;
using BackEnd.Application.Queries.Employee;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackEnd.Tests.Queries
{
    /// <summary>
    /// GetEmployeeByNameQueryHandler 단위 테스트
    /// </summary>
    public class GetEmployeeByNameQueryHandlerTests
    {
        private readonly Mock<IEmployeeQueryRepository> _mockQueryRepository;
        private readonly Mock<ILogger<GetEmployeeByNameQueryHandler>> _mockLogger;
        private readonly GetEmployeeByNameQueryHandler _handler;

        public GetEmployeeByNameQueryHandlerTests()
        {
            _mockQueryRepository = new Mock<IEmployeeQueryRepository>();
            _mockLogger = new Mock<ILogger<GetEmployeeByNameQueryHandler>>();
            _handler = new GetEmployeeByNameQueryHandler(
                _mockQueryRepository.Object,
                _mockLogger.Object);
        }

        // =============================================
        // 성공 케이스
        // =============================================

        /// <summary>
        /// [성공] 존재하는 직원 이름 조회 — 모든 필드가 채워진 EmployeeDto를 반환한다.
        /// </summary>
        [Fact]
        public async Task Handle_ExistingName_ReturnsEmployeeDto()
        {
            // Arrange
            var name = "홍길동";
            var expectedDto = new EmployeeDto(
                EmployeeId: 1,
                Name: "홍길동",
                Email: "hong@example.com",
                Tel: "01012345678",
                Joined: new DateTime(2024, 1, 1));

            var query = new GetEmployeeByNameQuery(name);

            _mockQueryRepository
                .Setup(r => r.GetByNameAsync(name))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.EmployeeId);
            Assert.Equal("홍길동", result.Name);
            Assert.Equal("hong@example.com", result.Email);
            Assert.Equal("01012345678", result.Tel);
        }

        /// <summary>
        /// [성공] 존재하지 않는 직원 이름 조회 — 예외 없이 null을 반환한다.
        /// </summary>
        [Fact]
        public async Task Handle_NonExistentName_ReturnsNull()
        {
            // Arrange
            var name = "존재하지않는사람";
            var query = new GetEmployeeByNameQuery(name);

            _mockQueryRepository
                .Setup(r => r.GetByNameAsync(name))
                .ReturnsAsync((EmployeeDto?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// [성공] 이름 조회 시 파라미터 전달 검증 — 요청한 이름과 동일한 값이 GetByNameAsync에 전달된다.
        /// </summary>
        [Fact]
        public async Task Handle_NameQuery_PassesSameNameToGetByNameAsync()
        {
            // Arrange
            var name = "이순신";
            var query = new GetEmployeeByNameQuery(name);

            _mockQueryRepository
                .Setup(r => r.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((EmployeeDto?)null);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert — 요청한 이름이 Repository에 정확히 전달되어야 합니다.
            _mockQueryRepository.Verify(
                r => r.GetByNameAsync(name),
                Times.Once);
        }

        /// <summary>
        /// [성공] 조회 성공 시 호출 횟수 검증 — GetByNameAsync가 정확히 1회만 호출된다.
        /// </summary>
        [Fact]
        public async Task Handle_SuccessfulQuery_CallsGetByNameAsyncOnce()
        {
            // Arrange
            var name = "강감찬";
            var query = new GetEmployeeByNameQuery(name);

            _mockQueryRepository
                .Setup(r => r.GetByNameAsync(name))
                .ReturnsAsync(new EmployeeDto(
                    EmployeeId: 5,
                    Name: "강감찬",
                    Email: "kang@example.com",
                    Tel: "01099999999",
                    Joined: new DateTime(2023, 6, 15)));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockQueryRepository.Verify(
                r => r.GetByNameAsync(It.IsAny<string>()),
                Times.Once);
        }

        /// <summary>
        /// [성공] 반환된 EmployeeDto의 필드 정확성 검증 — EmployeeId, Name, Email, Tel, Joined 모든 필드가 정확히 반환된다.
        /// </summary>
        [Fact]
        public async Task Handle_ReturnedEmployeeDto_AllFieldsReturnedAccurately()
        {
            // Arrange — 반환된 DTO의 모든 필드가 그대로 전달되는지 확인
            var joined = new DateTime(2025, 3, 1);
            var expectedDto = new EmployeeDto(
                EmployeeId: 42,
                Name: "테스트직원",
                Email: "test@company.co.kr",
                Tel: "01055556666",
                Joined: joined);

            var query = new GetEmployeeByNameQuery("테스트직원");

            _mockQueryRepository
                .Setup(r => r.GetByNameAsync("테스트직원"))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(42, result!.EmployeeId);
            Assert.Equal("테스트직원", result.Name);
            Assert.Equal("test@company.co.kr", result.Email);
            Assert.Equal("01055556666", result.Tel);
            Assert.Equal(joined, result.Joined);
        }
    }
}
