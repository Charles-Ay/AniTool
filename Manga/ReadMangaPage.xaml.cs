using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeebLib.Manga.Parser;
using WeebLib.Manga.Retrieval;
using Path = System.IO.Path;

namespace AniTool.Manga
{
    /// <summary>
    /// Interaction logic for ReadMangaPage.xaml
    /// </summary>
    public partial class ReadMangaPage : Page
    {
        int chapter;
        int indexOfManga;
        MangaFetcher fetcher;
        MangaSearcher searcher;
        TextBlock status;

        public ReadMangaPage(ref MangaFetcher fetcher, ref MangaSearcher searcher, int indexOfManga, int chapter, ref TextBlock status)
        {
            InitializeComponent();
            this.fetcher = fetcher;
            this.searcher = searcher;
            this.indexOfManga = indexOfManga;
            this.chapter = chapter;
            this.status = status;
            Load();

        }

        /// <summary>
        /// Update status and load text for chapter into TextBlock
        /// </summary>
        private void Load()
        {
            status.Text = "Chapter: " + chapter;
            fetcher.Fetch(searcher.Results()[indexOfManga], chapter, 1, false);
            var dir = fetcher.GetWorkDir();
            string folderName = $"{searcher.Results()[indexOfManga].name} Chapter {chapter}";
            dir = Path.Combine(dir, folderName);

            int num = 0;
            StackPanel stack = new();
            stack.Orientation = Orientation.Vertical;
            scroller.Content = stack;
            foreach (var fileData in Directory.GetFiles(dir))
            {
                FileInfo file = new(fileData);
                Image image = new Image() { Width = 700, Height = 800, Name = $"image_{folderName.Replace(" ", "_")}_{num}", Cursor = Cursors.Hand };
                image.Source = GetImageFromStream(@$"{file.FullName}");
                stack.Children.Add(image);
                ++num;
            }

            // Image image = new Image() { Width = 100, Height = 100, Name = $"image{}", Cursor = Cursors.Hand };
        }

        /// <summary>
        /// Onlick for next chapter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Next_Btn(object sender, RoutedEventArgs e)
        {
            if (chapter < searcher.Results()[indexOfManga].latest)
            {
                scroller.ScrollToTop();
                ++chapter;
                Load();
            }
        }

        /// <summary>
        /// Onlick for pervious chapter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Btn(object sender, RoutedEventArgs e)
        {
            if (chapter > 1)
            {
                scroller.ScrollToTop();
                --chapter;
                Load();
            }
        }


        /// <summary>
        /// Get the image from the stream. Need this to prevents file from locking
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
