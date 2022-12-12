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
using WeebLib.WeeLibExceptions;
using WeebLib.Novel;
using WeebLib.Interfaces;
using System.Net;
using System.Security.Policy;
using System.Xml.Linq;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;

namespace AniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NovelFetcher fetcher;
        NovelSearcher searcher;
        //store novels we got for later use
        Dictionary<int, string> searchResults;

        public MainWindow()
        {
            InitializeComponent();
            fetcher = new();
            fetcher.SetWorkDir(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\Novels", false);

            searcher = new();
            searchResults = new();
            Closing += OnWindowClosing;

            if (!Directory.Exists(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages"))
                Directory.CreateDirectory(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");
        }

        private void SearchNovelBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                searcher.Search(1, searchNovelBox.Text);
            }
            catch(WeebLibException ex)
            {
                MessageBox.Show(ex.nonStackMessage);
                return;
            }
            
            WrapPanel mainStack = new();
            mainStack.Orientation = Orientation.Horizontal;
            novelScroll.Content = mainStack;
            int number = 0;
            searcher.getResults().ForEach((result) =>
            {
                string type = result.image.Split('.').Last();
                DownloadImage(number, type, result.image);
                
                StackPanel panel = new();
                panel.Orientation = Orientation.Vertical;
                panel.Margin = new Thickness(0, 0, 0, 30);

                BitmapImage bitImage = new BitmapImage(new Uri(@$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages\{number}.{type}"));
                Image image = new Image() { Source = bitImage, Width = 100, Height = 100, Name = $"image{number}", Cursor = Cursors.Hand };
                image.Stretch = Stretch.Uniform;
                image.MouseDown += NovelImage_MouseDown;

                panel.Children.Add(image);
                if(number %2 == 0)
                {
                    panel.Children.Add(new TextBlock() { Text = result.name + "  ", Foreground = Brushes.Blue });
                }
                else
                {
                    panel.Children.Add(new TextBlock() { Text = result.name + "  ", Foreground = Brushes.Orange });
                }
                searchResults.Add(number, result.name);
                
                mainStack.Children.Add(panel);
                ++number;
            });
        }

        private void NovelImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image src = (Image)sender;
            int number = int.Parse(src.Name.Substring(5));
            string text = fetcher.Fetch(searcher.getResults()[number], 1, 10, false);
        }

        private static void LoadingNovel()
        {

        }

        private static void DownloadImage(int number, string type, string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), @$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages\{number}.{type}");
            }
        }

        private static void OnWindowClosing(object sender, CancelEventArgs e)
        {
#if DEBUG
#else
            System.IO.DirectoryInfo di = new (@$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
#endif
        }

        private void searchNovelBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) searchNovelBtn.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Button.MouseDownEvent });
        }
    }
}
