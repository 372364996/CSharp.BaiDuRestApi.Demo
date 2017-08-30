using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace voice
{
    public partial class Form1 : Form
    {
        string token ="*****";//第一次运行时候获取到的token
        string apiKey = "****";//对应百度云界面基本信息的API Key
        string secretKey = "****";//对应百度云界面基本信息的Secret Key
        string cuid = "464313464";//这个随便写  不过尽量写唯一的，比如自己创建个guid，或者你手机号码什么的都可以
        string getTokenURL = "";
        string serverURL = "http://vop.baidu.com/server_api";

        public Form1()
        {
            InitializeComponent();
            cuid = Guid.NewGuid().ToString();
            //第一次运行，将注释去掉，获取token，保存起来，有效期为1个月
            //getToken();
        }
        private void getToken()
        {
            getTokenURL = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials" +
                               "&client_id=" + apiKey + "&client_secret=" + secretKey;
            token = GetValue("access_token");
        }

        private string GetValue(string key)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(getTokenURL);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader reader1 = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string ssss = reader1.ReadToEnd().Replace("\"", "").Replace("{", "").Replace("}", "").Replace("\n", "");
            string[] indexs = ssss.Split(',');
            foreach (string index in indexs)
            {
                string[] _indexs = index.Split(':');
                if (_indexs[0] == key)
                    return _indexs[1];
            }
            return "";
        }
        private string Post(string audioFilePath)
        {
            serverURL += "?lan=zh&cuid=kwwwvagaa&token=" + token;
            FileStream fs = new FileStream(audioFilePath, FileMode.Open);
            byte[] voice = new byte[fs.Length];
            fs.Read(voice, 0, voice.Length);
            fs.Close();
            fs.Dispose();

            HttpWebRequest request = null;

            Uri uri = new Uri(serverURL);
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Timeout = 100000;
            request.Method = "POST";
            request.ContentType = "audio/wav; rate=8000";
            request.ContentLength = voice.Length;
            try
            {
                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(voice, 0, voice.Length);
                    writeStream.Close();
                    writeStream.Dispose();
                }
            }
            catch
            {
                return null;
            }
            string result = string.Empty;
            string result_final = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string line = string.Empty;
                        StringBuilder sb = new StringBuilder();
                        while (!readStream.EndOfStream)
                        {
                            line = readStream.ReadLine();
                            sb.Append(line);
                            sb.Append("\r");
                        }
                        readStream.Close();
                        readStream.Dispose();
                        result = sb.ToString();
                        string[] indexs = result.Split(',');
                        foreach (string index in indexs)
                        {
                            string[] _indexs = index.Split('"');
                            if (_indexs[2] == ":[")
                                result_final = _indexs[3];
                        }
                    }
                    responseStream.Close();
                    responseStream.Dispose();
                }
                response.Close();
            }
            return result_final;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = Post(Application.StartupPath + "\\3.wav");
        }
    }
}