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
using WeebLib.Manga.Parser;
using WeebLib.Manga.Retrieval;
using System.ComponentModel;
using System.IO;
using System.Net;
using WeebLib.WeeLibExceptions;
using Path = System.IO.Path;

namespace AniTool.Manga
{
    /// <summary>
    /// Interaction logic for MangaSearchPage.xaml
    /// </summary>
    public partial class MangaSearchPage : Page
    {
        MangaFetcher fetcher;
        MangaSearcher searcher;
        //store mangas we got for later use
        Dictionary<int, string> searchResults;
        string search;
        new Frame NavigationService;
        TextBlock status;

        public MangaSearchPage(ref MangaFetcher fetcher, ref MangaSearcher searcher, string search, ref Frame NavigationService, ref TextBlock status)
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

        /// <summary>
        /// Clear the images in the folder
        /// </summary>
        private void ClearImagesAndCreateDirectory()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string dir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string resourceDir = Path.Combine(dir, "resources");

            System.IO.DirectoryInfo di = new(Path.Combine(resourceDir, "MangaImages"));
            if (!di.Exists)
            {
                di.Create();
            }
            else
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        /// <summary>
        /// Search for the manga
        /// </summary>
        private void SearchResults()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string dir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string resourceDir = Path.Combine(dir, "resources");

            status.Text = $"Searching for " + search + "...";

            //Worker thread to search for mangas
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
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
                catch (WeebLibException)
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
                    mangaScroll.Content = mainStack;
                    int number = 0;
                    ClearImagesAndCreateDirectory();
                    searcher.Results().ForEach((result) =>
                    {
                        string type = result.image.Split('.').Last();
                        DownloadImage(number, type, result.image);

                        StackPanel panel = new();
                        panel.Orientation = Orientation.Vertical;
                        panel.Margin = new Thickness(0, 0, 0, 30);
                        Image image = new Image() { Width = 100, Height = 100, Name = $"image{number}", Cursor = Cursors.Hand };
                        string imagePath = Path.Combine(resourceDir, "MangaImages");
                        image.Source = GetImageFromStream(@$"{imagePath}\{number}.{type}");


                        image.Stretch = Stretch.Uniform;
                        image.MouseDown += MangaImage_MouseDown;

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

                        //worker.ReportProgress((number / searcher.Results().Count) * 100);

                        ++number;
                    });
                    status.Text = $"{number + 1} results found";
                });
            };
            worker.RunWorkerAsync();
            //Update message
            worker.ProgressChanged += (sender, e) =>
            {
                // Update the WPF user interface with the progress
                if (e.ProgressPercentage > 30) status.Text = $"Searching for {search}.";
                else if (e.ProgressPercentage > 40) status.Text = $"Searching for {search}..";
                else if (e.ProgressPercentage > 80) status.Text = $"Searching for {search}...";
            };
        }

        /// <summary>
        /// Get the image clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MangaImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image src = (Image)sender;
            int number = int.Parse(src.Name.Substring(5));

            //call a fetch on a huge number that we wont hit so that we can get all the chapters avalible
            fetcher.Fetch(searcher.Results()[number], 1, 20000, false);
            LoadingManga(number, src);
        }

        /// <summary>
        /// Load the manga
        /// </summary>
        /// <param name="number"></param>
        /// <param name="image"></param>
        private void LoadingManga(int number, Image image)
        {
            NavigationService.Content = new MangaChapterSelectionPage(ref fetcher, ref searcher, number, ref NavigationService, ref image, ref status);
        }

        /// <summary>
        /// Download the image
        /// </summary>
        /// <param name="number"></param>
        /// <param name="type"></param>
        /// <param name="url"></param>
        private void DownloadImage(int number, string type, string url)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string dir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string resourceDir = Path.Combine(dir, "resources");
            
            using (WebClient client = new())
            {
                client.DownloadFile(new Uri(url), @$"{resourceDir}\MangaImages\{number}.{type}");
            }
        }

        /// <summary>
        /// Get the image from the stream. Need this to prevents file from locking
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private BitmapImage GetImageFromStream(string path)
        {
            try
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
            catch (Exception ex)
            {
                string workingDirectory = Environment.CurrentDirectory;
                string dir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
                string resourceDir = Path.Combine(dir, "resources");
                
                return GetImageFromStream(@$"{resourceDir}\manga.webp");
            }
        }
    }
}
