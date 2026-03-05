using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace EmployeeTestApp.Services
{
    public class EmployeeCreateRequest
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Tel { get; set; } = "";
        public DateTime Joined { get; set; }
    }

    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Tel { get; set; } = "";
        public DateTime Joined { get; set; }
    }

    public class EmployeeListResult
    {
        public List<EmployeeDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class EmployeeApiService
    {
        private readonly HttpClient _http;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EmployeeApiService(string baseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        }

        public async Task<(bool Success, string Message)> CreateAsync(EmployeeCreateRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/employee", request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return (true, "등록 성공 (201 Created)");

            var body = await response.Content.ReadAsStringAsync();
            return (false, $"실패 ({(int)response.StatusCode}): {body}");
        }

        public async Task<EmployeeListResult> GetListAsync(int page = 1, int pageSize = 20)
        {
            var json = await _http.GetStringAsync($"api/employee?page={page}&pageSize={pageSize}");
            return JsonSerializer.Deserialize<EmployeeListResult>(json, _jsonOptions)
                   ?? new EmployeeListResult();
        }

        public async Task<EmployeeDto?> GetByNameAsync(string name)
        {
            var response = await _http.GetAsync($"api/employee/{Uri.EscapeDataString(name)}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmployeeDto>(json, _jsonOptions);
        }
    }
}
