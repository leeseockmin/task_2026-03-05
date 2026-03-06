using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace EmployeeTestApp
{
    public partial class MainWindow : Window
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // API 요청 모델 (Controller의 CreateEmployeeRequest 와 동일 구조)
        // ─────────────────────────────────────────────────────────────────────────────
        private class EmployeeRequest
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = "";

            [JsonPropertyName("email")]
            public string Email { get; set; } = "";

            [JsonPropertyName("tel")]
            public string Tel { get; set; } = "";

            // DateTime 그대로 직렬화하면 DateTimeKind.Unspecified 로 전송되어
            // MySQL 드라이버에서 오류 발생 → "yyyy-MM-dd" 문자열로 명시적으로 변환
            [JsonPropertyName("joined")]
            public string Joined { get; set; } = "";
        }

        // JSON 파일/바디 역직렬화용 (joined 를 string 으로 받아서 유연하게 파싱)
        private class JsonEmployeeItem
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("tel")]
            public string? Tel { get; set; }

            [JsonPropertyName("joined")]
            public string? Joined { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // HttpClient (HTTPS 로컬 개발 환경 — 인증서 검증 무시)
        // ─────────────────────────────────────────────────────────────────────────────
        private static readonly HttpClient _http = new(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            });

        private static readonly JsonSerializerOptions _serializeOpts = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions _deserializeOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // CSV 날짜 형식: 2018.03.07
        private static readonly string[] CsvDateFormats = { "yyyy.MM.dd", "yyyy-MM-dd", "yyyy/MM/dd" };

        // JSON 날짜 형식: 2012-01-05
        private static readonly string[] JsonDateFormats = { "yyyy-MM-dd", "yyyy.MM.dd", "yyyy/MM/dd" };

        // ─────────────────────────────────────────────────────────────────────────────

        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
            UpdateParseInfo(TxtCsvBodyParseInfo, TxtCsvBody.Text, ParseCsv);
            UpdateParseInfo(TxtJsonBodyParseInfo, TxtJsonBody.Text, ParseJson);
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // Tab 1 — CSV 파일 업로드
        // ═════════════════════════════════════════════════════════════════════════════

        private void BtnSelectCsvFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "CSV 파일 선택",
                Filter = "CSV 파일 (*.csv)|*.csv|텍스트 파일 (*.txt)|*.txt|모든 파일 (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true) return;

            TxtCsvFilePath.Text = dlg.FileName;
            var content = ReadAllTextAutoDetect(dlg.FileName);
            TxtCsvFileContent.Text = content;

            var parsed = ParseCsv(content);
            TxtCsvFileParseInfo.Text = parsed.Count > 0
                ? $"파싱 결과: {parsed.Count}명"
                : "파싱 실패 — 형식을 확인하세요.";
            TxtCsvFileParseInfo.Foreground = parsed.Count > 0
                ? new SolidColorBrush(Color.FromRgb(0x38, 0x8E, 0x3C))
                : new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F));
        }

        private async void BtnSendCsvFile_Click(object sender, RoutedEventArgs e)
        {
            var content = TxtCsvFileContent.Text.Trim();
            if (string.IsNullOrWhiteSpace(content) || content.StartsWith("파일을"))
            {
                AppendLog("[오류] 먼저 CSV 파일을 선택하세요.", isError: true);
                return;
            }

            var requests = ParseCsv(content);
            if (requests.Count == 0)
            {
                AppendLog("[오류] 파싱된 직원이 없습니다. CSV 형식을 확인하세요.", isError: true);
                return;
            }

            await PostEmployeesAsync(requests, $"CSV 파일 ({Path.GetFileName(TxtCsvFilePath.Text)})");
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // Tab 2 — JSON 파일 업로드
        // ═════════════════════════════════════════════════════════════════════════════

        private void BtnSelectJsonFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "JSON 파일 선택",
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true) return;

            TxtJsonFilePath.Text = dlg.FileName;
            var content = File.ReadAllText(dlg.FileName, Encoding.UTF8);
            TxtJsonFileContent.Text = content;

            var parsed = ParseJson(content);
            TxtJsonFileParseInfo.Text = parsed.Count > 0
                ? $"파싱 결과: {parsed.Count}명"
                : "파싱 실패 — 형식을 확인하세요.";
            TxtJsonFileParseInfo.Foreground = parsed.Count > 0
                ? new SolidColorBrush(Color.FromRgb(0x38, 0x8E, 0x3C))
                : new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F));
        }

        private async void BtnSendJsonFile_Click(object sender, RoutedEventArgs e)
        {
            var content = TxtJsonFileContent.Text.Trim();
            if (string.IsNullOrWhiteSpace(content) || content.StartsWith("파일을"))
            {
                AppendLog("[오류] 먼저 JSON 파일을 선택하세요.", isError: true);
                return;
            }

            var requests = ParseJson(content);
            if (requests.Count == 0)
            {
                AppendLog("[오류] 파싱된 직원이 없습니다. JSON 형식을 확인하세요.", isError: true);
                return;
            }

            await PostEmployeesAsync(requests, $"JSON 파일 ({Path.GetFileName(TxtJsonFilePath.Text)})");
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // Tab 3 — CSV 직접 입력
        // ═════════════════════════════════════════════════════════════════════════════

        private void TxtCsvBody_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TxtCsvBodyParseInfo is null) return;
            UpdateParseInfo(TxtCsvBodyParseInfo, TxtCsvBody.Text, ParseCsv);
        }

        private void BtnResetCsvBody_Click(object sender, RoutedEventArgs e)
            => TxtCsvBody.Text = "홍길동,hong@example.com,01025862222,2018.03.07\n김철수,kim@example.com,01012345678,2020.01.15";

        private async void BtnSendCsvBody_Click(object sender, RoutedEventArgs e)
        {
            var requests = ParseCsv(TxtCsvBody.Text);
            if (requests.Count == 0)
            {
                AppendLog("[오류] 파싱된 직원이 없습니다. CSV 형식을 확인하세요.", isError: true);
                return;
            }
            await PostEmployeesAsync(requests, "CSV Body");
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // Tab 4 — JSON 직접 입력
        // ═════════════════════════════════════════════════════════════════════════════

        private void TxtJsonBody_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TxtJsonBodyParseInfo is null) return;
            UpdateParseInfo(TxtJsonBodyParseInfo, TxtJsonBody.Text, ParseJson);
        }

        private void BtnResetJsonBody_Click(object sender, RoutedEventArgs e)
            => TxtJsonBody.Text = "[\n  { \"name\": \"홍길동\", \"email\": \"hong@example.com\", \"tel\": \"010-1111-2424\", \"joined\": \"2012-01-05\" },\n  { \"name\": \"김철수\", \"email\": \"kim@example.com\",  \"tel\": \"010-2222-3333\", \"joined\": \"2020-03-15\" }\n]";

        private async void BtnSendJsonBody_Click(object sender, RoutedEventArgs e)
        {
            var requests = ParseJson(TxtJsonBody.Text);
            if (requests.Count == 0)
            {
                AppendLog("[오류] 파싱된 직원이 없습니다. JSON 형식을 확인하세요.", isError: true);
                return;
            }
            await PostEmployeesAsync(requests, "JSON Body");
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // 공통 — 파싱
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// CSV 텍스트를 파싱합니다.
        /// 형식: 이름,이메일,전화번호,입사일(yyyy.MM.dd)
        /// </summary>
        private static List<EmployeeRequest> ParseCsv(string content)
        {
            var result = new List<EmployeeRequest>();

            foreach (var rawLine in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                var name = parts[0].Trim();
                var email = parts[1].Trim();
                var tel = parts[2].Trim();
                var joinedStr = parts[3].Trim();

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email)) continue;

                if (!DateTime.TryParseExact(joinedStr, CsvDateFormats,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var joined))
                    continue;

                result.Add(new EmployeeRequest
                {
                    Name = name,
                    Email = email,
                    Tel = tel,
                    Joined = joined.ToString("yyyy-MM-dd")
                });
            }

            return result;
        }

        /// <summary>
        /// JSON 텍스트를 파싱합니다.
        /// 형식: [{"name":"...","email":"...","tel":"...","joined":"yyyy-MM-dd"}]
        /// 단일 객체 {}도 지원합니다.
        /// </summary>
        private static List<EmployeeRequest> ParseJson(string content)
        {
            var result = new List<EmployeeRequest>();
            content = content.Trim();
            if (string.IsNullOrEmpty(content)) return result;

            try
            {
                // 단일 객체인 경우 배열로 감싸서 처리
                if (content.StartsWith("{"))
                    content = $"[{content}]";

                var items = JsonSerializer.Deserialize<List<JsonEmployeeItem>>(content, _deserializeOpts);
                if (items is null) return result;

                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Email))
                        continue;

                    if (!DateTime.TryParseExact(item.Joined ?? "", JsonDateFormats,
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var joined))
                        continue;

                    result.Add(new EmployeeRequest
                    {
                        Name = item.Name,
                        Email = item.Email,
                        Tel = item.Tel ?? "",
                        Joined = joined.ToString("yyyy-MM-dd")
                    });
                }
            }
            catch
            {
                // 파싱 실패 시 빈 목록 반환
            }

            return result;
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // 공통 — HTTP 전송
        // ═════════════════════════════════════════════════════════════════════════════

        private async Task PostEmployeesAsync(List<EmployeeRequest> requests, string source)
        {
            var baseUrl = TxtBaseUrl.Text.TrimEnd('/');
            var url = $"{baseUrl}/api/Employee";

            var bodyJson = JsonSerializer.Serialize(requests, _serializeOpts);
            var httpContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            AppendLog($"[{DateTime.Now:HH:mm:ss}] ▶ POST {url}  ({source}, {requests.Count}명)");
            AppendLog($"  Body: {bodyJson[..Math.Min(bodyJson.Length, 300)]}{(bodyJson.Length > 300 ? "..." : "")}");

            try
            {
                var response = await _http.PostAsync(url, httpContent);
                var responseBody = await response.Content.ReadAsStringAsync();
                var isSuccess = response.IsSuccessStatusCode;

                AppendLog($"  Status: {(int)response.StatusCode} {response.StatusCode}", isError: !isSuccess);

                if (!string.IsNullOrWhiteSpace(responseBody))
                    AppendLog($"  Response: {responseBody}", isError: !isSuccess);

                AppendLog(isSuccess
                    ? $"  완료: {requests.Count}명 등록 성공"
                    : $"  실패: 서버 응답을 확인하세요.",
                    isError: !isSuccess);
            }
            catch (HttpRequestException ex)
            {
                AppendLog($"  [연결 오류] {ex.Message}", isError: true);
                AppendLog($"  서버가 실행 중인지, URL이 올바른지 확인하세요.", isError: true);
            }
            catch (Exception ex)
            {
                AppendLog($"  [예외] {ex.Message}", isError: true);
            }

            AppendLog("");
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // 공통 — 인코딩 자동 감지
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// BOM 확인 후 UTF-8 시도, 실패 시 EUC-KR(CP949)로 폴백하여 파일을 읽습니다.
        /// </summary>
        private static string ReadAllTextAutoDetect(string path)
        {
            var bom = new byte[3];
            using (var fs = File.OpenRead(path))
            {
                fs.Read(bom, 0, 3);
            }

            // UTF-8 BOM(EF BB BF)이 있으면 UTF-8로 읽기
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }

            // UTF-8 유효성 검사 (BOM 없는 UTF-8)
            try
            {
                return File.ReadAllText(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            }
            catch (DecoderFallbackException)
            {
                // UTF-8 디코딩 실패 → EUC-KR(CP949) 폴백
                return File.ReadAllText(path, Encoding.GetEncoding(949));
            }
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // 공통 — UI 헬퍼
        // ═════════════════════════════════════════════════════════════════════════════

        private static void UpdateParseInfo(
            System.Windows.Controls.TextBlock label,
            string text,
            Func<string, List<EmployeeRequest>> parser)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                label.Text = "";
                return;
            }

            try
            {
                var count = parser(text).Count;
                label.Text = count > 0 ? $"파싱 결과: {count}명" : "파싱 실패 — 형식을 확인하세요.";
                label.Foreground = count > 0
                    ? new SolidColorBrush(Color.FromRgb(0x38, 0x8E, 0x3C))
                    : new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F));
            }
            catch
            {
                label.Text = "파싱 실패";
                label.Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F));
            }
        }

        private void AppendLog(string message, bool isError = false)
        {
            var prefix = isError ? "[!] " : "";
            TxtLog.AppendText($"{prefix}{message}\n");
            TxtLog.ScrollToEnd();
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
            => TxtLog.Clear();
    }
}
