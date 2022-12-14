using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
        Frame NavigationService;
        TextBlock status;

        public NovelSearchPage(ref NovelFetcher fetcher, ref NovelSearcher searcher, string search, ref Frame NavigationService, ref TextBlock status)
        {
            InitializeComponent();
            this.fetcher = fetcher;
            this.searcher = searcher;
            searchResults = new();
            this.search = search;
            this.NavigationService = NavigationService;
            this.status = status;

            SearchResults();
        }
        
        private void ClearImages()
        {
            System.IO.DirectoryInfo di = new(@$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        private void SearchResults()
        {
            status.Text = $"Searching for " + search + "...";

            //Worker thread to search for novels
            BackgroundWorker worker = new BackgroundWorker();   
            worker.DoWork += (sender, e) =>
            {
                try
                {
                    if (!searcher.Search(1, search))
                    {
                        Dispatcher.Invoke((Action)delegate
                        {
                            status.Text = "No results found";
                        });
                        return;
                    }
                }
                catch (WeebLibException ex)
                {
                    Dispatcher.Invoke((Action)delegate
                    {
                        status.Text = "No results found";
                    });
                    return;
                }
                Dispatcher.Invoke((Action)delegate
                {
                    WrapPanel mainStack = new();
                    mainStack.Orientation = Orientation.Horizontal;
                    novelScroll.Content = mainStack;
                    int number = 0;
                    ClearImages();
                    searcher.Results().ForEach((result) =>
                    {
                        string type = result.image.Split('.').Last();
                        DownloadImage(number, type, result.image);

                        StackPanel panel = new();
                        panel.Orientation = Orientation.Vertical;
                        panel.Margin = new Thickness(0, 0, 0, 30);
                        Image image = new Image() { Width = 100, Height = 100, Name = $"image{number}", Cursor = Cursors.Hand };
                        image.Source = GetImageFromStream(@$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages\{number}.{type}");


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
                    status.Text = $"{number + 1} results found";
                });
            };
            worker.RunWorkerAsync();
        }

        private void NovelImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image src = (Image)sender;
            int number = int.Parse(src.Name.Substring(5));
            
            //call a fetch on a huge number that we wont hit so that we can get all the chapters avalible
            fetcher.Fetch(searcher.Results()[number], 1, 20000, false);
            LoadingNovel(number, src);
        }

        private void LoadingNovel(int number, Image image)
        {
            NavigationService.Content = new ChapterSelectionPage(ref fetcher, ref searcher, number, ref NavigationService, image, ref status);
        }

        private void DownloadImage(int number, string type, string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), @$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages\{number}.{type}");
            }
        }
        
        //prevents file locking
        private BitmapImage GetImageFromStream(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
        }
    }
}
