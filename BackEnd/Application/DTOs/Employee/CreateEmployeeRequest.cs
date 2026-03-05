namespace BackEnd.Application.DTOs.Employee
{
    public record CreateEmployeeRequest(
        string Name,
        string Email,
        string Tel,
        DateTime Joined
    );
}
