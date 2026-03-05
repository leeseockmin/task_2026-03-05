using BackEnd.Application.Interfaces.Employee;
using MediatR;

namespace BackEnd.Application.Commands.Employee
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, int>
    {
        private readonly IEmployeeCommandRepository _commandRepository;
        private readonly ILogger<DeleteEmployeeCommandHandler> _logger;

        public DeleteEmployeeCommandHandler(
            IEmployeeCommandRepository commandRepository,
            ILogger<DeleteEmployeeCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _logger = logger;
        }

        public async Task<int> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("직원 삭제 커맨드. Id: {Id}", request.Id);

            return await _commandRepository.DeleteAsync(request.Id);
        }
    }
}
