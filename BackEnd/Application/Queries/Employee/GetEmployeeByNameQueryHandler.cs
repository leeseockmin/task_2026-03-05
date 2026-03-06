using BackEnd.Application.DTOs.Employee;
using BackEnd.Application.Interfaces.Employee;
using MediatR;

namespace BackEnd.Application.Queries.Employee
{
    public class GetEmployeeByNameQueryHandler : IRequestHandler<GetEmployeeByNameQuery, EmployeeDto?>
    {
        private readonly IEmployeeQueryRepository _queryRepository;
        private readonly ILogger<GetEmployeeByNameQueryHandler> _logger;

        public GetEmployeeByNameQueryHandler(
            IEmployeeQueryRepository queryRepository,
            ILogger<GetEmployeeByNameQueryHandler> logger)
        {
            _queryRepository = queryRepository;
            _logger = logger;
        }

        public async Task<EmployeeDto?> Handle(GetEmployeeByNameQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"직원 이름 조회 쿼리. Name: {request.Name}");
            return await _queryRepository.GetByNameAsync(request.Name);
        }
    }
}
