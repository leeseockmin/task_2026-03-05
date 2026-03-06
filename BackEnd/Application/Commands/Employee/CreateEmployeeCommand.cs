using BackEnd.Application.DTOs.Employee;
using MediatR;

namespace BackEnd.Application.Commands.Employee
{
    public record CreateEmployeeCommand(
        List<CreateEmployeeRequest> CreateEmployees
    ) : IRequest<int>;
}
