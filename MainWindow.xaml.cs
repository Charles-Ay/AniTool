using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeebLib.Novel.Parser;
using WeebLib.Novel.Retrieval;
using WeebLib.Novel;
using WeebLib.Interfaces;
using System.Net;
using System.Security.Policy;
using System.Xml.Linq;

namespace AniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NovelFetcher fetcher;
        NovelSearcher searcher;
        public MainWindow()
        {
            InitializeComponent();
            fetcher = new();
            searcher = new();
        }

        private void searchNovelBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            searcher.Search(1, searchNovelBox.Text, NovelUtil.sourceToString(NovelUtil.NovelSources.FullNovel));
            
            WrapPanel mainStack = new();
            mainStack.Orientation = Orientation.Horizontal;
            novelScroll.Content = mainStack;
            int number = 1;
            searcher.getResults().ForEach((result) =>
            {
                string type = result.image.Split('.').Last();
                DownloadImage(number, type, result.image);
                
                StackPanel panel = new();
                panel.Orientation = Orientation.Vertical;
                BitmapImage bitImage = new BitmapImage(new Uri(@$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\{number}.{type}"));
                Image image = new Image() { Source = bitImage, Width = 1000, Height = 1300 };


                panel.Children.Add(image);
                //panel.Children.Add(new TextBlock() { Text = result.name });
                //panel.Children.Add(new TextBlock() { Text = result.link });
                mainStack.Children.Add(panel);
                ++number;
            });
        }
        
        private static void DownloadImage(int number, string type, string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), @$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\{number}.{type}");
            }
        }
    }
}
