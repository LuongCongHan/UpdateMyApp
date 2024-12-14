using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdateMyApp
{
    public partial class frmUpdateInfo : Form
    {
        public frmUpdateInfo()
        {
            InitializeComponent();
            lbLinkUpdate.LinkClicked += LbLinkUpdate_LinkClicked;
            lbLinkUpdate.MaximumSize = new Size(this.Size.Width - lbLinkUpdate.Location.X - 30, 0);
            lbLinkUpdate.AutoSize = true;
            try
            {
                // Để không bị lỗi: WebException: The request was aborted: Could not create SSL/TLS secure channel.
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)192 |
                                                        (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            }
            catch (NotSupportedException)
            { }

        }


        private void frmUpdateInfo_Load(object sender, EventArgs e)
        {
            timerCheckUpdate.Start();
        }


        public long? _totalDownloadSize = 0;
        private async void timerCheckUpdate_Tick(object sender, EventArgs e)
        {
            timerCheckUpdate.Stop();
            lbLinkUpdate.Visible = false;
            lbSize.Visible = false;
            picLoad.Visible = true;
            btnUpdate.Visible = false;
            lbLinkUpdate.LinkArea = new LinkArea(0, 0);
            //timerCheckUpdate.Stop();
            //Check update
            try
            {
                bool isUpdate = await CheckUpdateAsync();
                if (isUpdate)
                {
                    //Convert Bytes to Megabytes (Hệ nhị phân)
                    lbLinkUpdate.Text = string.Format("{0} {1} is now available. Would you like to download it now?", this.ProductName, _updateVersion.Version);
                    lbLinkUpdate.LinkArea = new LinkArea(0, this.ProductName.Length + _updateVersion.Version.Length + 1);
                    lbSize.Text = "Download size: ...";

                    lbLinkUpdate.Visible = true;
                    lbSize.Visible = true;
                    picLoad.Visible = false;


                    //Sẽ có trường hợp không bật 1.1.1.1 thì tải không được ở đây- có thể do mạng -sau 3s nhảy xuống catch 
                    //3s đã đặt timeout cho _httpClient
                    using (HttpResponseMessage headResponse = await _httpClient.GetAsync(_updateVersion.UrlDownload, HttpCompletionOption.ResponseHeadersRead))
                    {
                        _totalDownloadSize = headResponse.Content.Headers.ContentLength;
                    }

                    //Giải quyết vấn đề bộ nhớ tăng
                    double mb = (double)(_totalDownloadSize / Math.Pow(2, 20));
                    lbSize.Text = string.Format("Download size: {0} MB", mb.ToString("F2"));
                    lbSize.Visible = true;
                    btnUpdate.Visible = true;
                }
                else
                {
                    //  DialogResult = DialogResult.No;
                    picLoad.Visible = false;
                    lbLinkUpdate.Visible = true;
                    btnUpdate.Visible = false;
                    lbSize.Visible = false;
                    lbLinkUpdate.Text = "There is no new version available.";
                }
            }
            catch
            {
                // DialogResult = DialogResult.No;
                //await Task.Delay(500);
                picLoad.Visible = false;
                lbSize.Visible = false;
                lbLinkUpdate.LinkArea = new LinkArea(0, 0);
                lbLinkUpdate.Visible = true;
                lbLinkUpdate.Text = "Failed to download version information.";
            }

        }
        private void LbLinkUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Specify that the link was visited.
            this.lbLinkUpdate.LinkVisited = true;

            // Navigate to a URL.
            if (!string.IsNullOrEmpty(_updateVersion.UrlInfo))
                System.Diagnostics.Process.Start(_updateVersion.UrlInfo);
        }
        HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(3000) };
        public UpdateVersion _updateVersion = new UpdateVersion();
        public async Task<bool> CheckUpdateAsync()
        {

            // string updateJson = await _httpClient.GetStringAsync("https://raw.githubusercontent.com/LuongCongHan/TepVip6/master/WindowsFormsApp40/update.json");
            string updateJson = await _httpClient.GetStringAsync("https://raw.githubusercontent.com/LuongCongHan/UpdateMyApp/refs/heads/master/UpdateMyApp/update.json");
            //Chuyển đổi Version về dạng dữ liệu đối tượng
            _updateVersion = JsonConvert.DeserializeObject<UpdateVersion>(updateJson);
            //    _updateVersion.Url = "https://github.com/LuongCongHan/MyAppUpdate/releases/download/File/AllFile.zip";
            // _updateVersion.Url = "https://github.com/LuongCongHan/TestUpdateUngDung/releases/download/newVd/LenhDk.zip";
            var currentVersion = new Version(this.ProductVersion);
            var jsonVersion = new Version(_updateVersion.Version);
            //So sánh Version
            int result = currentVersion.CompareTo(jsonVersion); //Icomparea
            if (result < 0)
                return true;
            else
                return false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //  DialogResult = DialogResult.No;
            this.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
