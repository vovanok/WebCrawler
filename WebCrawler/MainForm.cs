using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WebCrawlerLib;

namespace WebCrawler
{
    public partial class MainForm : Form
    {
        private static readonly List<string> Results = new List<string>();

        private static DataTable _dtGridDataTable;

        private static WebCrawlerMgr _webCrawlerMgr;

        public MainForm()
        {
            InitializeComponent();

            _dtGridDataTable = new DataTable();
            _dtGridDataTable.Columns.Add("URL");
            _dtGridDataTable.Columns.Add("Совпадения");

            dgvResults.DataSource = _dtGridDataTable;
            pgSettings.SelectedObject = GetDefaultSettings();
        }

        private static CrawlingSettings GetDefaultSettings()
        {
            return new CrawlingSettings(
                "http://www.livemaster.ru",
                true,
                @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)",
                3,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (pgSettings.SelectedObject == null || !(pgSettings.SelectedObject is CrawlingSettings))
                return;
            var settings = (CrawlingSettings) pgSettings.SelectedObject;
            var startUrl = settings.Url ?? string.Empty;
            var pattern = settings.Pattern ?? string.Empty;
            var maxDepth = settings.MaxDepth > 0 ? settings.MaxDepth : 1;
            var isOnlyStartUrlHost = settings.IsOnlyOnCurrentHost;

            var isUsingLogin = false;//settings.IsUseLogin;
            var loginUrl = string.Empty;//settings.LoginUrl ?? string.Empty;
            var loginElementName = string.Empty;//settings.LoginElemName ?? string.Empty;
            var passwordElementName = string.Empty;//settings.PasswordElemName ?? string.Empty;
            var login = string.Empty;//settings.Login ?? string.Empty;
            var password = string.Empty;//settings.Password ?? string.Empty;
            var additionalRequestString = string.Empty;//settings.AdditionalRequestString ?? string.Empty;

            _dtGridDataTable.Rows.Clear();
            Results.Clear();
            tMonitor.Stop();

            if (string.IsNullOrEmpty(startUrl) || string.IsNullOrEmpty(pattern))
            {
                MessageBox.Show(@"Не указан ""URL для поиска"" или ""Искомый шаблон""");
                return;
            }

            _webCrawlerMgr = !isUsingLogin 
                ? new WebCrawlerMgr(startUrl, maxDepth, new List<string> { pattern }, isOnlyStartUrlHost)
                : new WebCrawlerMgr(startUrl, maxDepth, new List<string> { pattern }, isOnlyStartUrlHost,
                    new UserAutentificationContext(loginUrl, loginElementName,
                        passwordElementName, login, password, string.Empty, additionalRequestString));

            _webCrawlerMgr.StartScan();
            //StartMonitor();
            tMonitor.Start();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var ofd = new SaveFileDialog
                          {
                              Title = @"Open Image Files",
                              Filter = @"TXT File|*.txt|XML File|*.xml",
                              InitialDirectory = @"C:\"
                          };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var fileName = ofd.FileName;
                if (File.Exists(fileName))
                {
                    if (MessageBox.Show(
                            @"Файл с таким именем уже существует. Хотите заменить его?",
                            @"Подтверждение сохранения",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                        File.Delete(fileName);
                    else
                        return;
                }

                switch (Path.GetExtension(fileName))
                {
                    case ".txt":
                        SaveAsTxt(fileName);
                        break;
                    default:
                        MessageBox.Show(@"Неизвестный формат для сохранения");
                        break;
                }
                MessageBox.Show(@"Сохранение успешно завершено");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_webCrawlerMgr != null) _webCrawlerMgr.StopScan();
            //if (_monitorThread != null) _monitorThread.Abort();
        }

        private static void SaveAsTxt(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            using (var fs = new FileStream(fileName, FileMode.Create))
                using (var sw = new StreamWriter(fs))
                    foreach (var rec in Results)
                        sw.WriteLine(rec);
        }

        private static void AddToGrid(List<object[]> rows)
        {
            if (rows == null) return;

            foreach (var row in rows)
                _dtGridDataTable.Rows.Add(row);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_webCrawlerMgr != null) _webCrawlerMgr.StopScan();
            tMonitor.Stop();
        }

        private void TickWaiter()
        {
            switch (lblTime.Text)
            {
                case "|": lblTime.Text = @"/"; break;
                case "/": lblTime.Text = @"-"; break;
                case "-": lblTime.Text = @"\"; break;
                case "\\": lblTime.Text = @"|"; break;
                default: lblTime.Text = @"|"; break;
            }
        }

        private void tMonitor_Tick(object sender, EventArgs e)
        {
            tMonitor.Stop();
            var isComplete = _webCrawlerMgr.IsComplete;

            var visitetUrls = _webCrawlerMgr.GetVisitedUrls();
            var results = visitetUrls.Where(g => g.MatchingResult.Count > 0).ToList();
            if (results.Count <= _dtGridDataTable.Rows.Count)
            {
                if (isComplete)
                {
                    MessageBox.Show(@"Сканирование завершено");
                    return;
                }
                tMonitor.Start();
            }
            var additionalResults = new List<object[]>();

            for (var curNum = _dtGridDataTable.Rows.Count; curNum < results.Count; curNum++)
            {
                additionalResults.Add(
                    new object[]
                        {
                            results[curNum].Url,
                            results[curNum].MatchingResult.Count > 0 
                                ? results[curNum].MatchingResult.Aggregate((g, h) => string.Format("{0}, {1}", g ?? string.Empty, h ?? string.Empty))
                                : string.Empty
                        });
                foreach (var match in results[curNum].MatchingResult)
                    if (!Results.Contains(match))
                        Results.Add(match);
            }

            AddToGrid(additionalResults);

            TickWaiter();

            if (isComplete)
                MessageBox.Show(@"Сканирование завершено");
            else
                tMonitor.Start();
        }
    }
}
