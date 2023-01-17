using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AniTool.Manga
{
    /// <summary>
    /// Interaction logic for MangaChapterSelectionPage.xaml
    /// </summary>
    public partial class MangaChapterSelectionPage : Page
    {
        MangaFetcher fetcher;
        MangaSearcher searcher;
        int indexOfManga;
        new Frame NavigationService;
        Image image;
        TextBlock status;

        public MangaChapterSelectionPage(ref MangaFetcher fetcher, ref MangaSearcher searcher, int indexOfManga, ref Frame NavigationService, ref Image image, ref TextBlock status)
        {
            InitializeComponent();
            this.fetcher = fetcher;
            this.searcher = searcher;
            this.indexOfManga = indexOfManga;
            this.NavigationService = NavigationService;
            this.image = image;
            this.status = status;
            LoadChapterSelection();
        }

        /// <summary>
        /// Loads the chapter selection page
        /// </summary>
        public void LoadChapterSelection()
        {
            WrapPanel mainStack = new();
            mainStack.Orientation = Orientation.Horizontal;
            mangaScroll.Content = mainStack;
            status.Text = "Loading chapters";

            //Worker to retrieve the chapters in the background
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < searcher.Results()[indexOfManga].latest; ++i)
                    {
                        if (i % 2 == 0)
                        {
                            var text = new TextBlock() { Text = $"Chapter {i + 1}", Margin = new Thickness(10, 10, 10, 10), FontSize = 20, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Blue, Cursor = Cursors.Hand };
                            text.PreviewMouseDown += MangaChapter_MouseDown;
                            mainStack.Children.Add(text);
                        }
                        else
                        {
                            var text = new TextBlock() { Text = $"Chapter {i + 1}", Margin = new Thickness(10, 10, 10, 10), FontSize = 20, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Orange, Cursor = Cursors.Hand };
                            text.PreviewMouseDown += MangaChapter_MouseDown;
                            mainStack.Children.Add(text);

                        }
                    }
                    mangaImage.Source = image.Source;
                    status.Text = $"{searcher.Results()[indexOfManga].name} - chapters loaded";
                });
            };

            //Update message
            worker.ProgressChanged += (sender, e) =>
            {
                // Update the WPF user interface with the progress
                if (e.ProgressPercentage > 30) status.Text = "Loading chapters.";
                else if (e.ProgressPercentage > 60) status.Text = "Loading chapters..";
                else if (e.ProgressPercentage > 80) status.Text = "Loading chapters...";
            };

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Event handler for when a chapter is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MangaChapter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            int number = int.Parse(textBlock.Text.Substring(8));

            fetcher.Fetch(searcher.Results()[indexOfManga], number, 1, false);
            LoadChapter(number);
        }

        /// <summary>
        /// Load the selected chapter
        /// </summary>
        /// <param name="text">text for the chapter</param>
        /// <param name="chapter">Chapter number</param>
        private void LoadChapter(int chapter)
        {
            NavigationService.Content = new ReadMangaPage(ref fetcher, ref searcher, indexOfManga, chapter, ref status);
        }
    }
}
