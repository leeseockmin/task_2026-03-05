using BackEnd.Application.DTOs.Employee;
using MediatR;

namespace BackEnd.Application.Queries.Employee
{
    public record GetEmployeeByNameQuery(string Name) : IRequest<EmployeeDto?>;
}
