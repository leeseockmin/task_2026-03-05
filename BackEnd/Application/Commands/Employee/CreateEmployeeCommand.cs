using MediatR;

namespace BackEnd.Application.Commands.Employee
{
    public record CreateEmployeeCommand(
        string Name,
        string Email,
        string Tel,
        DateTime Joined
    ) : IRequest<int>;
}
