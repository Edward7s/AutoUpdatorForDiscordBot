using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace AutoUpdator
{

    public partial class Form1 : Form
    {
        public JObject JObject { get; set; }
        public String[] Arguments { get; set; }
        public string ExeDir { get; set; }
        public bool Update { get; set; } = false;

        public Form1()
        {
            InitializeComponent();
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            if (!File.Exists(@"C:\Program Files\WinRAR\unrar.exe"))
            {
                this.Opacity = 1;
                this.ShowInTaskbar = true;
                MessageBox.Show("                < < < [ WINRAR WAS NOT FOUND. ] > > >\n" + @"< C:\Program Files\WinRAR\unrar.exe > can not be found." + "\n                                 Please Install WinRar.", "WINRAR WAS NOT FOUND.");
                Process.GetCurrentProcess().Kill();
            }
            Text.Text = "-------------------------------[ Initializing Updator ]-------------------------------";

            RegistryKey startup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            ExeDir = startup.GetValue("VRCDiscordBotNotifier").ToString().Replace("VRCDiscordBotNotifier.exe", "");


            Arguments = Environment.GetCommandLineArgs();
            if (Arguments.Length != 2)
            {
                this.Opacity = 1;
                this.ShowInTaskbar = true;
                Text.ForeColor = Color.Red;
                Write("The Arguments Is Missing Please Do Not Open The Exe By Yourself.");
                return;
            }

            string stringJson = CheckVersion("https://api.github.com/repos/Edward7s/VRCDiscordBotNotifier/releases/latest");
            if (stringJson == string.Empty)
            {
                Write("Request Failed. Something Went Wrong Please Restart The App!!!");
                Text.ForeColor = Color.Red;
                return;
            }
            Write("Request Worked.");
            JObject = JObject.Parse(stringJson);
            Write(new StringBuilder().AppendFormat("Newest Version: {0}", JObject["tag_name"]));
            Write(new StringBuilder().AppendFormat("Download URl: {0}", JObject["assets"][0]["browser_download_url"]));
            Write(new StringBuilder().AppendFormat("ChangeLog: {0}", JObject["body"]));

            if (Arguments[1] != JObject["tag_name"].ToString())
            {
                Title.Text = "Current Version Is Outdated.";
                Write("Current Version Is Outdated.");
                Update = true;
                this.Opacity = 1;
                this.ShowInTaskbar = true;
            }
            else
            {
                Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    Process.GetCurrentProcess().Kill();
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Update) return;
            Title.Text = "Updating In Proggress.";
            var pros = Process.GetProcessesByName("VRCDiscordBotNotifier");
            if (pros.Length != 0)
                for (int i = 0; i < pros.Length; i++)
                    pros[i].Kill();

            string rarPath = ExeDir + @"DiscordBot.win-x86.rar";
            using (WebClient wc = new WebClient())
                wc.DownloadFile(JObject["assets"][0]["browser_download_url"].ToString(), rarPath);
            Process winRar = new Process();
            winRar.StartInfo.FileName = @"C:\Program Files\WinRAR\unrar.exe";
            winRar.StartInfo.CreateNoWindow = true;
            winRar.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            winRar.EnableRaisingEvents = true;
            winRar.StartInfo.Arguments = String.Format("x -p- {0} {1}", rarPath, ExeDir);
            winRar.Start();
            winRar.WaitForExit();
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(200);
                    if (!Directory.Exists(ExeDir + @"win-x86")) continue;
                    Thread.Sleep(2000);
                    if (File.Exists(ExeDir + @"VRCDiscordBotNotifier.exe"))
                        File.Delete(ExeDir + @"VRCDiscordBotNotifier.exe");
                    Thread.Sleep(1000);
                    var files = new DirectoryInfo(ExeDir + @"win-x86\").GetFiles();
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (!files[i].Name.EndsWith(".exe")) continue;
                        files[i].MoveTo(ExeDir + @"\VRCDiscordBotNotifier.exe");
                        while (true)
                        {
                            Thread.Sleep(200);
                            if (!File.Exists(ExeDir + @"VRCDiscordBotNotifier.exe")) continue;
                            Process.Start(new ProcessStartInfo() { FileName = ExeDir + @"VRCDiscordBotNotifier.exe" });
                            break;
                        }
                    }
                    break;
                }
            });
            Title.ForeColor = Color.Green;
            Title.Text = "App Will Close In 5 Seconds.";
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                Process.GetCurrentProcess().Kill();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Title.Text = "App Will Close In 5 Seconds.";
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                Process.GetCurrentProcess().Kill();
            });
        }
        private string CheckVersion(string url)
        {
            Write(new StringBuilder().AppendFormat("Requesting On: {0}.", url));
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "Get";
                request.UserAgent = "Opera";
                request.Accept = "application/vnd.github+json";
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "Gzip, deflate, br");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Write(new StringBuilder().AppendFormat("Request Finished With Status: {0}.", response.StatusCode));
                using (var streamReader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                    return streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Write(ex);
            }
            return string.Empty;
        }
        private void Write(object obj) =>
            Text.Text += "\n" + obj.ToString();
    }
}