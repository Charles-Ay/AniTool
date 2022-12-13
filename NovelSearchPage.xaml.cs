using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using WeebLib.Interfaces;
using WeebLib.Novel.Parser;
using WeebLib.Novel.Retrieval;
using WeebLib.WeeLibExceptions;

namespace AniTool
{
    /// <summary>
    /// Interaction logic for NovelSearchPage.xaml
    /// </summary>
    public partial class NovelSearchPage : Page
    {
        NovelFetcher fetcher;
        NovelSearcher searcher;
        //store novels we got for later use
        Dictionary<int, string> searchResults;
        string search;
        TextBlock statusText;
        Frame mainFrame;

        public NovelSearchPage(ref NovelFetcher fetcher, ref NovelSearcher searcher, ref TextBlock statusText, string search, ref Frame mainFrame)
        {
            InitializeComponent();
            this.fetcher = fetcher;
            this.searcher = searcher;
            searchResults = new();
            this.search = search;
            this.statusText = statusText;
            this.mainFrame = mainFrame;

            SearchResults();
        }

        private void SearchResults()
        {
            statusText.Text = "Searching for " + search + "...";
            try
            {
                searcher.Search(1, search);
            }
            catch (WeebLibException ex)
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
                if (number % 2 == 0)
                {
                    panel.Children.Add(new TextBlock() { Text = result.name + "  ", Foreground = Brushes.Blue, TextWrapping = TextWrapping.Wrap });
                }
                else
                {
                    panel.Children.Add(new TextBlock() { Text = result.name + "  ", Foreground = Brushes.Orange, TextWrapping = TextWrapping.Wrap });
                }
                searchResults.Add(number, result.name);

                mainStack.Children.Add(panel);
                ++number;
            });
            statusText.Text = $"{number + 1} results found";
        }

        private void NovelImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image src = (Image)sender;
            int number = int.Parse(src.Name.Substring(5));
            //call a fetch on a huge number that we wont hit so that we can get all the chapeters avalible
            fetcher.Fetch(searcher.getResults()[number], 1, 20000, false);
            LoadingNovel(number);
        }

        private void LoadingNovel(int number)
        {
            mainFrame.Content = new ChapterSelectionPage(ref fetcher, ref searcher, number, ref statusText, ref mainFrame);
        }

        private static void DownloadImage(int number, string type, string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), @$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages\{number}.{type}");
            }
        }

    }
}
