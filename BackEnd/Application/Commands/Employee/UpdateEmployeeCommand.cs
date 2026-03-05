using MediatR;

namespace BackEnd.Application.Commands.Employee
{
    public record UpdateEmployeeCommand(
        int Id,
        string Name,
        string Email,
        string Tel,
        DateTime Joined
    ) : IRequest<int>;
}
