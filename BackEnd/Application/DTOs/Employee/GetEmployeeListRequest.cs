namespace BackEnd.Application.DTOs.Employee
{
    public record GetEmployeeListRequest
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
