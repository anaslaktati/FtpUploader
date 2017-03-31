using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ftpUploader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            lblUpload.Text = "";
        }

        struct FtpSetting
        {
            public string Server { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string FileName { get; set; }
            public string Fullname { get; set; }
        }

        FtpSetting inputParameter;


        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string filename = inputParameter.FileName;
            string fullname = inputParameter.Fullname;
            string username = inputParameter.Username;
            string password = inputParameter.Password;
            string server = inputParameter.Server;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(string.Format("ftp://{0}/{1}", server, filename)));
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);
            Stream ftpStream = request.GetRequestStream();
            FileStream fs = File.OpenRead(fullname);
            byte[] buffer = new byte[1024];
            double total = (double)fs.Length ;
            int byteRead = 0;
            double read = 0;
            do
            {
                if (!backgroundWorker.CancellationPending)
                {
                    byteRead = fs.Read(buffer, 0, 1024);
                    ftpStream.Write(buffer, 0, byteRead);
                    read += (double)byteRead;
                    double pourcentage = read / total * 100;
                    backgroundWorker.ReportProgress((int)pourcentage);
                }

            } while (byteRead != 0);
            fs.Close();
            ftpStream.Close();

        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblUpload.Text = $"uploading {e.ProgressPercentage} %";
            progressBar.Value = e.ProgressPercentage;
            progressBar.Update();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblUpload.Text = "upload complete";
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd=new OpenFileDialog() { Multiselect=false,ValidateNames=true,Filter="All files|*.*" })
            {
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    inputParameter.Username = txtUserName.Text;
                    inputParameter.Password = txtPassword.Text;
                    inputParameter.Server = txtServer.Text;
                    inputParameter.FileName = fi.Name;
                    inputParameter.Fullname = fi.FullName;
                    backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
                    backgroundWorker.RunWorkerAsync();
                }
            }
        }
    }
}
