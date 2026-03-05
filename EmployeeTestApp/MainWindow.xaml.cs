using EmployeeTestApp.Services;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace EmployeeTestApp
{
    public partial class MainWindow : Window
    {
        private EmployeeApiService? _api;

        // 미리보기용 목록 (CSV/JSON 탭 공용)
        private List<EmployeeCreateRequest> _csvRows = new();
        private List<EmployeeCreateRequest> _jsonRows = new();

        public MainWindow()
        {
            InitializeComponent();
            DpJoined.SelectedDate = DateTime.Today;
        }

        // ─── 공통 헬퍼 ────────────────────────────────────────────────

        private EmployeeApiService GetApi()
        {
            var url = TxtBaseUrl.Text.Trim();
            if (_api is null || _api.GetType().Name != url)
                _api = new EmployeeApiService(url);
            return _api;
        }

        private void Log(string message)
        {
            TxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            TxtLog.ScrollToEnd();
        }

        // ─── 연결 확인 ─────────────────────────────────────────────────

        private async void BtnPing_Click(object sender, RoutedEventArgs e)
        {
            TxtPingResult.Text = "확인 중...";
            try
            {
                var api = new EmployeeApiService(TxtBaseUrl.Text.Trim());
                await api.GetListAsync(1, 1);
                TxtPingResult.Text = "✔ 연결됨";
                TxtPingResult.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                TxtPingResult.Text = "✘ 실패";
                TxtPingResult.Foreground = System.Windows.Media.Brushes.Red;
                Log($"연결 실패: {ex.Message}");
            }
        }

        // ─── 직접 입력 ─────────────────────────────────────────────────

        private async void BtnDirectCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                string.IsNullOrWhiteSpace(TxtEmail.Text) ||
                string.IsNullOrWhiteSpace(TxtTel.Text) ||
                DpJoined.SelectedDate is null)
            {
                MessageBox.Show("모든 필드를 입력해주세요.", "유효성 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var req = new EmployeeCreateRequest
            {
                Name = TxtName.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                Tel = TxtTel.Text.Trim(),
                Joined = DpJoined.SelectedDate.Value
            };

            try
            {
                var (success, msg) = await GetApi().CreateAsync(req);
                Log($"직접입력 [{req.Name}] → {msg}");
                if (success)
                {
                    TxtName.Clear(); TxtEmail.Clear(); TxtTel.Clear();
                    DpJoined.SelectedDate = DateTime.Today;
                }
            }
            catch (Exception ex)
            {
                Log($"오류: {ex.Message}");
            }
        }

        // ─── CSV 파일 ──────────────────────────────────────────────────

        private void BtnSelectCsv_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "CSV 파일 (*.csv)|*.csv|모든 파일|*.*" };
            if (dlg.ShowDialog() != true) return;

            TxtCsvPath.Text = dlg.FileName;
            _csvRows.Clear();

            var lines = File.ReadAllLines(dlg.FileName, Encoding.UTF8);
            // 첫 행은 헤더 → 건너뜀
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                if (!DateTime.TryParse(parts[3].Trim(), out var joined)) continue;

                _csvRows.Add(new EmployeeCreateRequest
                {
                    Name = parts[0].Trim(),
                    Email = parts[1].Trim(),
                    Tel = parts[2].Trim(),
                    Joined = joined
                });
            }

            GridCsvPreview.ItemsSource = null;
            GridCsvPreview.ItemsSource = _csvRows;
            Log($"CSV 로드 완료: {_csvRows.Count}건");
        }

        private async void BtnSendCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_csvRows.Count == 0)
            {
                MessageBox.Show("전송할 데이터가 없습니다. CSV 파일을 먼저 선택해주세요.");
                return;
            }

            await SendRowsAsync(_csvRows, "CSV");
        }

        // ─── JSON 파일 ─────────────────────────────────────────────────

        private void BtnSelectJson_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON 파일 (*.json)|*.json|모든 파일|*.*" };
            if (dlg.ShowDialog() != true) return;

            TxtJsonPath.Text = dlg.FileName;
            _jsonRows.Clear();

            try
            {
                var json = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<List<EmployeeCreateRequest>>(json, opts);
                if (parsed != null) _jsonRows.AddRange(parsed);
            }
            catch (Exception ex)
            {
                Log($"JSON 파싱 오류: {ex.Message}");
                return;
            }

            GridJsonPreview.ItemsSource = null;
            GridJsonPreview.ItemsSource = _jsonRows;
            Log($"JSON 로드 완료: {_jsonRows.Count}건");
        }

        private async void BtnSendJson_Click(object sender, RoutedEventArgs e)
        {
            if (_jsonRows.Count == 0)
            {
                MessageBox.Show("전송할 데이터가 없습니다. JSON 파일을 먼저 선택해주세요.");
                return;
            }

            await SendRowsAsync(_jsonRows, "JSON");
        }

        // ─── 공통 전송 로직 ────────────────────────────────────────────

        private async Task SendRowsAsync(List<EmployeeCreateRequest> rows, string source)
        {
            int success = 0, fail = 0;
            foreach (var row in rows)
            {
                try
                {
                    var (ok, msg) = await GetApi().CreateAsync(row);
                    Log($"[{source}] [{row.Name}] → {msg}");
                    if (ok) success++; else fail++;
                }
                catch (Exception ex)
                {
                    Log($"[{source}] [{row.Name}] 오류: {ex.Message}");
                    fail++;
                }
            }
            Log($"[{source}] 전송 완료 — 성공: {success}, 실패: {fail}");
        }

        // ─── 조회 ──────────────────────────────────────────────────────

        private async void BtnGetList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await GetApi().GetListAsync();
                GridResult.ItemsSource = result.Items;
                Log($"목록 조회 완료: {result.Items.Count}/{result.TotalCount}건 (Page {result.Page})");
            }
            catch (Exception ex)
            {
                Log($"목록 조회 오류: {ex.Message}");
            }
        }

        private async void BtnGetByName_Click(object sender, RoutedEventArgs e)
        {
            var name = TxtSearchName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("이름을 입력해주세요.");
                return;
            }

            try
            {
                var emp = await GetApi().GetByNameAsync(name);
                if (emp is null)
                {
                    Log($"이름 조회 [{name}] → 결과 없음 (404)");
                    GridResult.ItemsSource = null;
                }
                else
                {
                    GridResult.ItemsSource = new List<EmployeeDto> { emp };
                    Log($"이름 조회 [{name}] → {emp.Email} / {emp.Tel}");
                }
            }
            catch (Exception ex)
            {
                Log($"이름 조회 오류: {ex.Message}");
            }
        }
    }
}
