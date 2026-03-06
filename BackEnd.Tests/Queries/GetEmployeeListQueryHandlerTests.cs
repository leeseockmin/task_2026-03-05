using BackEnd.Application.DTOs.Employee;
using BackEnd.Application.Interfaces.Employee;
using BackEnd.Application.Queries.Employee;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackEnd.Tests.Queries
{
    /// <summary>
    /// GetEmployeeListQueryHandler 단위 테스트
    ///
    /// [참고] Page/PageSize 범위 검증(0 이하 값)은 컨트롤러(EmployeeController)에서
    ///        처리하므로 핸들러 레벨에서는 별도 테스트가 불필요합니다.
    ///        해당 검증은 컨트롤러 통합 테스트 범위에 해당합니다.
    /// </summary>
    public class GetEmployeeListQueryHandlerTests
    {
        private readonly Mock<IEmployeeQueryRepository> _mockQueryRepository;
        private readonly Mock<ILogger<GetEmployeeListQueryHandler>> _mockLogger;
        private readonly GetEmployeeListQueryHandler _handler;

        public GetEmployeeListQueryHandlerTests()
        {
            _mockQueryRepository = new Mock<IEmployeeQueryRepository>();
            _mockLogger = new Mock<ILogger<GetEmployeeListQueryHandler>>();
            _handler = new GetEmployeeListQueryHandler(
                _mockQueryRepository.Object,
                _mockLogger.Object);
        }

        // =============================================
        // 성공 케이스
        // =============================================

        /// <summary>
        /// [성공] 유효한 페이지 조회 요청 — 직원 목록과 올바른 총 건수를 반환한다.
        /// </summary>
        [Fact]
        public async Task Handle_ValidQuery_ReturnsEmployeeListWithTotalCount()
        {
            // Arrange
            var expectedItems = new List<EmployeeDto>
            {
                new EmployeeDto(1, "홍길동", "hong@example.com", "01012345678", new DateTime(2024, 1, 1)),
                new EmployeeDto(2, "이순신", "lee@example.com",  "01022345678", new DateTime(2024, 3, 1))
            };
            var expectedTotalCount = 2;
            var query = new GetEmployeeListQuery(Page: 1, PageSize: 10);

            _mockQueryRepository
                .Setup(r => r.GetListAsync(1, 10))
                .ReturnsAsync(expectedItems);

            _mockQueryRepository
                .Setup(r => r.GetTotalCountAsync())
                .ReturnsAsync(expectedTotalCount);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(expectedTotalCount, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal("홍길동", result.Items[0].Name);
            Assert.Equal("이순신", result.Items[1].Name);
        }

        /// <summary>
        /// [성공] 데이터가 없는 경우 — 빈 목록과 총 건수 0을 반환한다.
        /// </summary>
        [Fact]
        public async Task Handle_NoData_ReturnsEmptyListAndZeroTotalCount()
        {
            // Arrange
            var query = new GetEmployeeListQuery(Page: 1, PageSize: 10);

            _mockQueryRepository
                .Setup(r => r.GetListAsync(1, 10))
                .ReturnsAsync(new List<EmployeeDto>());

            _mockQueryRepository
                .Setup(r => r.GetTotalCountAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
        }

        /// <summary>
        /// [성공] 2페이지 요청 — GetListAsync에 올바른 Page와 PageSize 파라미터가 전달된다.
        /// </summary>
        [Fact]
        public async Task Handle_Page2Request_PassesCorrectPageParametersToGetListAsync()
        {
            // Arrange
            var expectedItems = new List<EmployeeDto>
            {
                new EmployeeDto(11, "가나다", "abc@example.com", "01099999999", new DateTime(2025, 1, 1))
            };
            var query = new GetEmployeeListQuery(Page: 2, PageSize: 10);

            _mockQueryRepository
                .Setup(r => r.GetListAsync(2, 10))
                .ReturnsAsync(expectedItems);

            _mockQueryRepository
                .Setup(r => r.GetTotalCountAsync())
                .ReturnsAsync(11);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(11, result.TotalCount);
            Assert.Equal(2, result.Page);

            // GetListAsync가 올바른 페이지 파라미터로 호출되었는지 검증
            _mockQueryRepository.Verify(
                r => r.GetListAsync(2, 10),
                Times.Once);
        }

        /// <summary>
        /// [성공] 단일 조회 요청 — GetListAsync와 GetTotalCountAsync가 각각 정확히 1회씩 호출된다.
        /// </summary>
        [Fact]
        public async Task Handle_QueryRequest_CallsGetListAsyncAndGetTotalCountAsyncOnceEach()
        {
            // Arrange
            var query = new GetEmployeeListQuery(Page: 1, PageSize: 5);

            _mockQueryRepository
                .Setup(r => r.GetListAsync(1, 5))
                .ReturnsAsync(new List<EmployeeDto>());

            _mockQueryRepository
                .Setup(r => r.GetTotalCountAsync())
                .ReturnsAsync(0);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert — 두 Repository 메서드가 각 1회씩 호출되어야 합니다.
            _mockQueryRepository.Verify(
                r => r.GetListAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);

            _mockQueryRepository.Verify(
                r => r.GetTotalCountAsync(),
                Times.Once);
        }

        /// <summary>
        /// [성공] 조회 결과의 Page와 PageSize — 요청값과 동일한 값이 반환된다.
        /// </summary>
        [Fact]
        public async Task Handle_QueryResult_ReturnsPageAndPageSizeSameAsRequest()
        {
            // Arrange — 결과 객체에 요청 Page/PageSize가 그대로 담기는지 확인
            var query = new GetEmployeeListQuery(Page: 3, PageSize: 25);

            _mockQueryRepository
                .Setup(r => r.GetListAsync(3, 25))
                .ReturnsAsync(new List<EmployeeDto>());

            _mockQueryRepository
                .Setup(r => r.GetTotalCountAsync())
                .ReturnsAsync(100);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Page);
            Assert.Equal(25, result.PageSize);
        }
    }
}
