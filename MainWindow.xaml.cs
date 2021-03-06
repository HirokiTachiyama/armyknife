﻿using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ArmyKnife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // 全体
        private string configFile = "../../../config.json";
        private Config conf;
        private DispatcherTimer timer;

        // DokuWikiタブ 関連変数
        private string DW_status = "DokuWiki";
        DokuwikiPageManager DW_dpm;
        DW_TreeViewItem DW_TreeViewRoot;
        // private var _DW_TreeViewRoot = new ObservableCollection();


        // LogViewerタブ 関連変数
        private string LV_logFile;
        private string LV_formatFile;
        private string LV_status = "";

        // Redmineタブ 関連変数
        private string RM_status = "Redmine";


        public class Product
        {
            public string Name { get; set; }
            public int Price { get; set; }
            public double Tax { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();


            // Config読み込み
            string configFileStr = System.IO.File.ReadAllText(configFile);
            conf = JsonSerializer.Deserialize<Config>(configFileStr);

            timer = new DispatcherTimer();

            // デフォルトでチェックしておく
            //Radio_LogKind_FormPDFMaker.Checked = true;

            // DokuWiki設定
            // PrepareDokuWiki();

            // Timer_AK 始動
            this.timer.Start();

            // ログの表示
            ShowLog("../../logs/FormPDFMaker_Excel.log");

            // 駒音用の音源ファイルの指定
            mediaPlayer.URL = "./shogi1.mp3";

        }

        private void PrepareDokuWiki()
        {
            // DokuWikiへのログイン
            DW_dpm = new DokuwikiPageManager(conf.todoUrl, conf.todoUser, conf.todoPass);

            DW_UrlLabel.Content = "URL : " + conf.todoUrl + conf.todoPage;
            DW_UserPassLabel.Content = "User : " + conf.todoUser + ", Pass : ************";

            // DokuWikiのテキスト読み込み
            DW_PageContentTextBox.Text = DW_dpm.GetPageStr(conf.todoPage);
            DW_status = $"{conf.todoUrl}/{conf.todoPage} loaded.";

            DW_TreeViewRoot = new DW_TreeViewItem("root");

           

            //DW_TreeViewRoot.DW_TreeViewItems = new List<DW_TreeViewItem>
            //DW_TreeViewRoot.DW_TreeViewItems.Add(new DW_TreeViewItem("item1"));

            //var 


            //var dto1 = new Dto("Name1");
            //dto1.Dtos.Add(new Dto("Name1-1"));
            //dto1.Dtos.Add(new Dto("Name1-2"));
            //_dtos.Add(dto1);

            //CTreeView.ItemsSource = _dtos;


        }

        private void ShowLog(String _fileName)
        {
            // クリア
            //logListBox.Items.Clear();
            //string[] logs = System.IO.File.ReadAllLines(_fileName);

            //foreach (string log in logs)
            //{
            //    var item = new ListViewItem();
            //    item.Text = log;
            //    logListBox.Items.Add(item);
            //}
        }

        private void DW_LoadButton_Clicked(object sender, RoutedEventArgs e)
        {
            DW_PageContentTextBox.Text = DW_dpm.GetPageStr(conf.todoPage);
            DW_status = "Loaded.";
            UpdateStatusLabel();
        }

        private void DW_SaveButton_Clicked(object sender, RoutedEventArgs e)
        {
            bool ret = DW_dpm.PutPage(DW_PageContentTextBox.Text, conf.todoPage);

            if (ret)
            {
                DW_status = "put success.";
            }
            else
            {
                DW_status = "put failed.";
            }
            UpdateStatusLabel();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.AK_TabControl.SelectedIndex)
            {
                case 0: // DokuWiki
                    DW_TabItem_Selected();
                    break;
                case 1: // Syogi
                    SY_TabItem_Selected();
                    break;
                case 2: // LogViewer
                    LV_TabItem_Selected();
                    break;
                case 3: // RedMine
                    RM_TabItem_Selected();
                    break;
                default:
                    break;
            }

        }

        private void LV_TabItem_Selected()
        {
            Console.WriteLine("LogViewer");
            UpdateStatusLabel();
        }

        private void SY_TabItem_Selected()
        {
            Console.WriteLine("Syogi");
            UpdateStatusLabel();
        }

        private void DW_TabItem_Selected()
        {
            Console.WriteLine("DokuWiki");
            UpdateStatusLabel();
        }

        private void RM_TabItem_Selected()
        {
            Console.WriteLine("Redmine");

            UpdateStatusLabel();
        }


        // ステータステキストのアップデート
        private void UpdateStatusLabel()
        {
            // 現在のタブに応じて分岐
            switch (AK_TabControl.SelectedIndex)
            {
                case 0: // DokuWiki
                    AK_StatusLabel.Content = DW_status;
                    break;
                case 1: // Syogi
                    AK_StatusLabel.Content = SY_status;
                    break;
                case 2: // LogViewer
                    AK_StatusLabel.Content = LV_status;
                    break;
                case 3: // Redmine
                    AK_StatusLabel.Content = RM_status;
                    break;
                default:
                    break;
            }

        }

        private void testTB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void shogibutton_ClickAsync(object sender, RoutedEventArgs e)
        {

            string text;
            WebClient wc = new WebClient();
            try
            {
                text = wc.DownloadString(shogitb.Text);
                testTB.Text = text.Substring(text.IndexOf("var data")/*, text.IndexOf("</script>")*/).ToString();
                
//                testTB.Text = text[text.IndexOf("var data")..].ToString();

            }
            catch (WebException we)
            {
                testTB.Text = we.ToString();
            }
            _ = Task.Run(async () =>
                {
                    try
                    {

                        // 株価を取得したいサイトのURL
                        var code = "eff8410f11971491eef56d0d2f506c2114ee5fd1";
                        var urlstring = $"https://shogidb2.com/games/{code}";

                        // 指定したサイトのHTMLをストリームで取得する
                        var doc = default(IHtmlDocument);
                        using (var client = new HttpClient())
                        using (var stream = await client.GetStreamAsync(new Uri(urlstring)))
                        {
                            // AngleSharp.Html.Parser.HtmlParserオブジェクトにHTMLをパースさせる
                            var parser = new HtmlParser();
                            //doc = await parser.ParseDocumentAsync(stream);


                            var document = await parser.ParseDocumentAsync(wc.DownloadString(shogitb.Text));
                            Debug.WriteLine(document.ToString());
                            Console.WriteLine(document.ToString());

                        }

                        // クエリーセレクタを指定し株価部分を取得する
                        var priceElement = doc.QuerySelector("#main td[class=stoksPrice]");
                        var hoge = doc.QuerySelector("<script>");

                        Debug.WriteLine(hoge.TextContent.ToString());

                        // 取得した株価がstring型なのでint型にパースする
                        //int.TryParse(priceElement.TextContent, NumberStyles.AllowThousands, null, out var price);
                        //Debug.WriteLine("コクヨ(7984.T)の株価: {0}円", price);

                    }catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                });

//            testMethodAsync();
            
        }

        private async Task testMethodAsync()
        {
            // 株価を取得したいサイトのURL
            var code = "eff8410f11971491eef56d0d2f506c2114ee5fd1";
            var urlstring = $"https://shogidb2.com/games/{code}";

            // 指定したサイトのHTMLをストリームで取得する
            var doc = default(IHtmlDocument);
            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(new Uri(urlstring)))
            {
                // AngleSharp.Html.Parser.HtmlParserオブジェクトにHTMLをパースさせる
                var parser = new HtmlParser();
                doc = await parser.ParseDocumentAsync(stream);
            }

            // クエリーセレクタを指定し株価部分を取得する
            var priceElement = doc.QuerySelector("#main td[class=stoksPrice]");

            var hoge = doc.QuerySelector("<script>");

            Debug.WriteLine(hoge.TextContent.ToString());

            // 取得した株価がstring型なのでint型にパースする
            int.TryParse(priceElement.TextContent, NumberStyles.AllowThousands, null, out var price);

            Debug.WriteLine("コクヨ(7984.T)の株価: {0}円", price);

                

        }

 
    }
}
