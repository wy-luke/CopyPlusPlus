﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Windows;
using System.Windows.Input;
using ToggleSwitch;
using WK.Libraries.SharpClipboardNS;

namespace CopyPlusPlus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Is the translate API being changed or not
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static bool changeStatus = false;
#pragma warning restore CA2211 // Non-constant fields should not be visible
        private bool switch1Check;
        private bool switch2Check;
        private bool switch3Check;
        private bool switch4Check;

        public SharpClipboard clipboard;

        public MainWindow()
        {
            InitializeComponent();

            InitializeClipboardMonitor();

            switch1Check = Properties.Settings.Default.Switch1Check;
            switch2Check = Properties.Settings.Default.Switch2Check;
            switch3Check = Properties.Settings.Default.Switch3Check;
            switch4Check = Properties.Settings.Default.Switch4Check;
            if (switch1Check == true)
            {
                switch1.IsChecked = true;
            }
            if (switch2Check == true)
            {
                switch2.IsChecked = true;
            }
            if (switch3Check == true)
            {
                switch3.IsChecked = true;
            }
            if (switch4Check == true)
            {
                switch4.IsChecked = true;
            }
        }

        //Initializes a new instance of SharpClipboard
        public void InitializeClipboardMonitor()
        {
            clipboard = new SharpClipboard();
            //Attach your code to the ClipboardChanged event to listen to cuts/copies
            clipboard.ClipboardChanged += ClipboardChanged;
            //disable issueing ClipboardChanged event when start
            clipboard.ObserveLastEntry = false;
            //disable monitoring files
            clipboard.ObservableFormats.Files = false;
            //disable monitoring images
            clipboard.ObservableFormats.Images = false;
        }

        private void ClipboardChanged(Object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            // Is the content copied of text type?
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                // Get the cut/copied text.
                string text = e.Content.ToString();

                if (switch1Check == true || switch2Check == true)
                {
                    for (int counter = 0; counter < text.Length - 1; counter++)
                    {
                        if (switch1Check == true)
                        {
                            if (text[counter + 1].ToString() == "\r")
                            {
                                if (text[counter].ToString() == ".")
                                {
                                    continue;
                                }
                                if (text[counter].ToString() == "。")
                                {
                                    continue;
                                }
                                text = text.Remove(counter + 1, 2);
                            }
                        }

                        if (switch2Check == true)
                        {
                            if (text[counter].ToString() == " ")
                            {
                                text = text.Remove(counter, 1);
                            }
                        }
                    }
                }

                if (switch3Check == true)
                {
                    if (changeStatus == false)
                    {
                        string appId = Properties.Settings.Default.AppID;
                        string secretKey = Properties.Settings.Default.SecretKey;
                        if (appId == "none" || secretKey == "none")
                        {
                            //MessageBox.Show("请先设置翻译接口", "Copy++");
                            Show_InputAPIWindow();
                        }
                        else
                        {
                            text = BaiduTrans(appId, secretKey, text);

                            if (switch4Check == true)
                            {
                                //MessageBox.Show(text);
                                TranslateResult translateResult = new TranslateResult();
                                translateResult.textBox.Text = text;

                                //translateResult.WindowStartupLocation = WindowStartupLocation.Manual;
                                //translateResult.Left = System.Windows.Forms.Control.MousePosition.X;
                                //translateResult.Top = System.Windows.Forms.Control.MousePosition.Y;
                                translateResult.Show();

                                //var left = translateResult.Left;
                                //var top = translateResult.Top;
                            }
                        }
                    }
                }

                //Prevent loop
                clipboard.StopMonitoring();
                Clipboard.SetText(text);
                Clipboard.Flush();
                InitializeClipboardMonitor();
            }

            // If the cut/copied content is complex, use 'Other'.
            else if (e.ContentType == SharpClipboard.ContentTypes.Other)
            {
                //do nothing

                // Do something with 'clipboard.ClipboardObject' or 'e.Content' here...
            }
        }

        private void Todolist_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("欢迎赞助我，加快开发进度！");
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/CopyPlusPlus/CopyPlusPlus");
        }

        private static string BaiduTrans(string appId, string secretKey, string q = "apple")
        {
            //q为原文

            // 源语言
            string from = "en";
            // 目标语言
            string to = "zh";

            // 改成您的APP ID
            //appId = NoAPI.baidu_id;
            // 改成您的密钥
            //secretKey = NoAPI.baidu_secretKey;

            Random rd = new Random();
            string salt = rd.Next(100000).ToString();
            string sign = EncryptString(appId + q + salt + secretKey);
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(q);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            //read json(retString) as a object
            var result = JsonSerializer.Deserialize<Rootobject>(retString);

            return result.trans_result[0].dst;
        }

        // 计算MD5值
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }

        //打开翻译按钮
        private void TranslateSwitch_Check(object sender, RoutedEventArgs e)
        {
            string appId = Properties.Settings.Default.AppID;
            string secretKey = Properties.Settings.Default.SecretKey;
            if (appId == "none" || secretKey == "none")
            {
                //MessageBox.Show("请先设置翻译接口", "Copy++");
                Show_InputAPIWindow();
            }
            switch3Check = true;
        }

        //点击"翻译"文字
        private void TranslateText_Clicked(object sender, MouseButtonEventArgs e)
        {
            Show_InputAPIWindow(false);
        }

        private void Show_InputAPIWindow(bool showMessage = true)
        {
            KeyInput keyinput = new KeyInput
            {
                Owner = this
            };

            keyinput.Show();

            if (showMessage == true)
            {
                MessageBox.Show(keyinput, "请先设置翻译接口", "Copy++");
            }
            changeStatus = true;
        }

        private void SwitchUncheck(object sender, RoutedEventArgs e)
        {
            HorizontalToggleSwitch switchButton = sender as HorizontalToggleSwitch;
            string switchName = switchButton.Name;
            if (switchName == "switch1")
            {
                switch1Check = false;
            }
            if (switchName == "switch2")
            {
                switch2Check = false;
            }
            if (switchName == "switch3")
            {
                switch3Check = false;
            }
            if (switchName == "switch4")
            {
                switch4Check = false;
            }
        }

        private void SwitchCheck(object sender, RoutedEventArgs e)
        {
            HorizontalToggleSwitch switchButton = sender as HorizontalToggleSwitch;
            string switchName = switchButton.Name;
            if (switchName == "switch1")
            {
                switch1Check = true;
            }
            if (switchName == "switch2")
            {
                switch2Check = true;
            }
            if (switchName == "switch3")
            {
                switch3Check = true;
            }
            if (switchName == "switch4")
            {
                switch4Check = true;
            }
        }

        public static void Switch3Uncheck()
        {
            //switch3.IsChecked = false;
        }

        private void ChangeSwitch()
        {
            if (switch1Check == true)
            {
                switch1.IsChecked = true;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //记录每个Switch的状态,以便下次打开恢复
            Properties.Settings.Default.Switch1Check = switch1Check;
            Properties.Settings.Default.Switch2Check = switch2Check;
            Properties.Settings.Default.Switch3Check = switch3Check;
            Properties.Settings.Default.Switch4Check = switch4Check;

            //判断Swith3状态,避免bug
            if (Properties.Settings.Default.AppID == "none" || Properties.Settings.Default.SecretKey == "none")
            {
                Properties.Settings.Default.Switch3Check = false;
            }

            Properties.Settings.Default.Save();
        }
    }
}