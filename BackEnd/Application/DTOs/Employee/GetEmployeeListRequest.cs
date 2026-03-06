using BackEnd.Application.Constants;

namespace BackEnd.Application.DTOs.Employee
{
    public record GetEmployeeListRequest(
        int Page = EmployeeConstants.PageDefault,
        int PageSize = EmployeeConstants.PageSizeDefault
    );
}
