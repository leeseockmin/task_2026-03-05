namespace BackEnd.Application.DTOs.Employee
{
    public record UpdateEmployeeRequest(
        string Name,
        string Email,
        string Tel,
        DateTime Joined
    );
}
