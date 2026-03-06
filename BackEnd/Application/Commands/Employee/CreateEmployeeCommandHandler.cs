using BackEnd.Application.Constants;
using BackEnd.Application.Interfaces.Employee;
using BackEnd.Application.Utils;
using MediatR;
using EmployeeEntity = DB.Data.AccountDB.Employee;

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

            DateTime now = DateTime.UtcNow;
            var employees = new List<EmployeeEntity>();

            foreach (var req in request.CreateEmployees)
            {
                if (string.IsNullOrWhiteSpace(req.Name))
                {
                    _logger.LogError($"name이 비어있습니다.");
                    throw new ArgumentException("name은 필수 입력값입니다.");
                }

                if (req.Name.Length > EmployeeConstants.NameMaxLength)
                {
                    _logger.LogError($"name 길이 초과. 입력값 길이: {req.Name.Length}");
                    throw new ArgumentException($"name은 최대 {EmployeeConstants.NameMaxLength}자까지 허용됩니다.");
                }

                if (string.IsNullOrWhiteSpace(req.Email))
                {
                    _logger.LogError($"email이 비어있습니다.");
                    throw new ArgumentException("email은 필수 입력값입니다.");
                }

                if (req.Email.Length > EmployeeConstants.EmailMaxLength)
                {
                    _logger.LogError($"email 길이 초과. 입력값 길이: {req.Email.Length}");
                    throw new ArgumentException($"email은 최대 {EmployeeConstants.EmailMaxLength}자까지 허용됩니다.");
                }

                if (!EmployeeUtils.IsValidEmail(req.Email))
                {
                    _logger.LogError($"email 형식 오류. 입력값: {req.Email}");
                    throw new ArgumentException($"email 형식이 올바르지 않습니다. 입력값: '{req.Email}'");
                }

                var normalizedTel = EmployeeUtils.RemoveNonNumeric(req.Tel ?? string.Empty);
                if (normalizedTel.Length > EmployeeConstants.TelMaxLength)
                {
                    _logger.LogError($"tel 길이 초과. 입력값 길이: {normalizedTel.Length}");
                    throw new ArgumentException($"tel은 하이픈 제거 후 최대 {EmployeeConstants.TelMaxLength}자까지 허용됩니다.");
                }

                employees.Add(new EmployeeEntity
                {
                    name = req.Name,
                    email = req.Email,
                    tel = normalizedTel,
                    joined = req.Joined,
                    createdAt = now
				});
            }

            var insertedCount = await _commandRepository.BulkInsertAsync(employees);
            return insertedCount;
        }
    }
}
