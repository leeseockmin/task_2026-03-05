using BackEnd.Application.Interfaces.Employee;
using DB.Data.AccountDB;
using MediatR;

namespace BackEnd.Application.Commands.Employee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, int>
    {
        private readonly IEmployeeCommandRepository _commandRepository;
        private readonly ILogger<CreateEmployeeCommandHandler> _logger;

        public CreateEmployeeCommandHandler(
            IEmployeeCommandRepository commandRepository,
            ILogger<CreateEmployeeCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _logger = logger;
        }

        public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("직원 생성 커맨드. Name: {Name}", request.Name);

            var employee = new DB.Data.AccountDB.Employee
			{
                name = request.Name,
                email = request.Email,
                tel = request.Tel,
                joined = request.Joined,
                createdAt = DateTime.UtcNow
            };

            return await _commandRepository.InsertAsync(employee);
        }
    }
}
