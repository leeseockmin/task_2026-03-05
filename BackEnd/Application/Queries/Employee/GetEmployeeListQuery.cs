using BackEnd.Application.DTOs.Employee;
using MediatR;

namespace BackEnd.Application.Queries.Employee
{
    public record GetEmployeeListQuery(int Page, int PageSize) : IRequest<EmployeeListResult>;

    public record EmployeeListResult(
        List<EmployeeDto> Items,
        int TotalCount,
        int Page,
        int PageSize
    );
}
