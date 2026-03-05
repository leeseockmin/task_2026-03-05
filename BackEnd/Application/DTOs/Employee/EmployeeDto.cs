namespace BackEnd.Application.DTOs.Employee
{
    public record EmployeeDto(
        int employeeId,
        string name,
        string email,
        string tel,
        DateTime joined
    );
}
