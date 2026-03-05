using BackEnd.Application.Interfaces.Employee;
using DB.Data.AccountDB;
using MediatR;

namespace BackEnd.Application.Commands.Employee
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, int>
    {
        private readonly IEmployeeCommandRepository _commandRepository;
        private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

        public UpdateEmployeeCommandHandler(
            IEmployeeCommandRepository commandRepository,
            ILogger<UpdateEmployeeCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _logger = logger;
        }

        public async Task<int> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("직원 수정 커맨드. Id: {Id}", request.Id);

            var employee = new DB.Data.AccountDB.Employee
            {
                employeeId = request.Id,
                name = request.Name,
                email = request.Email,
                tel = request.Tel,
                joined = request.Joined,
                updatedAt = DateTime.UtcNow
            };

            return await _commandRepository.UpdateAsync(employee);
        }
    }
}
