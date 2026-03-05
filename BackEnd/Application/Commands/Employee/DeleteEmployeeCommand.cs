using MediatR;

namespace BackEnd.Application.Commands.Employee
{
    public record DeleteEmployeeCommand(int Id) : IRequest<int>;
}
