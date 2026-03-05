using BackEnd.Application.Interfaces.Employee;
using MediatR;

namespace BackEnd.Application.Queries.Employee
{
    public class GetEmployeeListQueryHandler : IRequestHandler<GetEmployeeListQuery, EmployeeListResult>
    {
        private readonly IEmployeeQueryRepository _queryRepository;
        private readonly ILogger<GetEmployeeListQueryHandler> _logger;

        public GetEmployeeListQueryHandler(
            IEmployeeQueryRepository queryRepository,
            ILogger<GetEmployeeListQueryHandler> logger)
        {
            _queryRepository = queryRepository;
            _logger = logger;
        }

        public async Task<EmployeeListResult> Handle(GetEmployeeListQuery request, CancellationToken cancellationToken)
        {

            var items = await _queryRepository.GetListAsync(request.Page, request.PageSize);
            var totalCount = await _queryRepository.GetTotalCountAsync();


            return new EmployeeListResult(items, totalCount, request.Page, request.PageSize);
        }
    }
}
