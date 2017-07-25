using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace SynHelper
{
    public partial class frmSyn : DevComponents.DotNetBar.Office2007Form
    {
        List<cCard> lstCard = new List<cCard>();
        CookieContainer cc;
        string strIP;
        public frmSyn()
        {
            InitializeComponent();
            this.Text = "管家婆同步助手";
            this.StartPosition = FormStartPosition.CenterScreen;
            strIP = "127.0.0.1:8000";
            //strIP = "39.108.61.82";

            login();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://" + strIP + "/api/vipcards/");
            request.CookieContainer = cc;
            request.Method = "POST";
            request.ContentType = "application/json;characterSet:UTF-8";
            request.KeepAlive = false;
            request.Timeout = 2 * 60 * 1000;
            using (var sm = new StreamWriter(request.GetRequestStream()))
            {
                //string strJson = "[{\"sample_time\": \"2013-05-25 14:21:38+03:00\", \"value\": 20},{\"sample_time\": \"2033-05-25 14:21:38+03:00\", \"value\": 40}]";
                string str = JsonConvert.SerializeObject(lstCard);
                sm.Write(str);
                //sm.Write(strJson);
                sm.Flush();
            }
            string strContent = "";
            DateTime dt1 = DateTime.Now;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream smResponse = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(smResponse, Encoding.UTF8))
                    {
                        Char[] readBuff = new Char[256];
                        int count = sr.Read(readBuff, 0, 256);
                        while (count > 0)
                        {
                            String outputData = new String(readBuff, 0, count);
                            strContent += outputData;
                            count = sr.Read(readBuff, 0, 256);
                        }
                    }
                }
            }
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            MessageBox.Show(strContent + " 耗时：" + ts.TotalSeconds + "s");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://" + strIP + "/api/vipcards/");
            request.CookieContainer = cc;
            request.Method = "DELETE";
            request.ContentType = "application/json;characterSet:UTF-8";
            request.KeepAlive = false;
            //using (var sm = new StreamWriter(request.GetRequestStream()))
            //{
            //    //string strJson = "[{\"sample_time\": \"2013-05-25 14:21:38+03:00\", \"value\": 20},{\"sample_time\": \"2033-05-25 14:21:38+03:00\", \"value\": 40}]";
            //    string str = JsonConvert.SerializeObject(lstCard);
            //    sm.Write(str);
            //    //sm.Write(strJson);
            //    sm.Flush();
            //}
            string strContent = "";
            DateTime dt1 = DateTime.Now;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream smResponse = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(smResponse, Encoding.UTF8))
                    {
                        Char[] readBuff = new Char[256];
                        int count = sr.Read(readBuff, 0, 256);
                        while (count > 0)
                        {
                            String outputData = new String(readBuff, 0, count);
                            strContent += outputData;
                            count = sr.Read(readBuff, 0, 256);
                        }
                    }
                }
            }
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            MessageBox.Show(strContent + " 耗时：" + ts.TotalSeconds + "s");
        }

        private void btnGetDB_Click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection("Server=.;DataBase=test;integrated security=SSPI;");
            conn.Open();
            SqlDataAdapter myda = new SqlDataAdapter("select * from VipCards", conn);
            DataSet myds = new DataSet();
            myda.Fill(myds);
            DataTable dt = myds.Tables[0];
            dgv.DataSource = dt;
            conn.Close();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //var m = i % 2;
                var m = i;
                string strDeleted = (string)dt.Rows[m]["Deleted"];
                if (strDeleted == "1")
                {
                    continue;
                }
                cCard card = new cCard();
                card.card = (string)dt.Rows[m]["CardNo"];
                card.name = (string)dt.Rows[m]["HolderName"];
                card.tel = (string)dt.Rows[m]["HolderTel"];
                card.openid = "";
                card.integral = Convert.ToInt32(dt.Rows[m]["UndoIntegral"]);
                card.money = Convert.ToInt32(dt.Rows[m]["SaveMoney"]);
                card.gender = ((string)dt.Rows[m]["Sex"]) == "男" ? "male" : "female";
                card.address = (string)dt.Rows[m]["HolderAdd"];
                string strBirth = (string)dt.Rows[m]["Birthday"];
                if (strBirth != "")
                {
                    card.birth = (Convert.ToDateTime(dt.Rows[m]["Birthday"])).ToString("yyyy-MM-dd");
                }
                else
                {
                    card.birth = null;
                }
                
                card.weixin = "";
                card.des = (string)dt.Rows[m]["Comment"];
                string strAddTime = (string)dt.Rows[m]["HandOutDate"];
                if (strAddTime != "")
                {
                    card.add_time = (Convert.ToDateTime(dt.Rows[m]["HandOutDate"])).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    card.add_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                
                lstCard.Add(card);
            }
        }

        private void login()
        {
            cc = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://" + strIP + "/api/login/");
            request.CookieContainer = cc;
            request.Method = "POST";
            request.ContentType = "application/json;characterSet:UTF-8";
            request.KeepAlive = false;
            using (var sm = new StreamWriter(request.GetRequestStream()))
            {
                string str = JsonConvert.SerializeObject(new
                {
                    name = "szh",
                    password = "zhhszhx1216"
                });
                sm.Write(str);
                sm.Flush();
            }
            string strContent = "";
            DateTime dt1 = DateTime.Now;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream smResponse = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(smResponse, Encoding.UTF8))
                    {
                        Char[] readBuff = new Char[256];
                        int count = sr.Read(readBuff, 0, 256);
                        while (count > 0)
                        {
                            String outputData = new String(readBuff, 0, count);
                            strContent += outputData;
                            count = sr.Read(readBuff, 0, 256);
                        }
                    }
                }
            }
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            //MessageBox.Show(strContent + " 耗时：" + ts.TotalSeconds + "s");
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            login();
        }
    }
}
