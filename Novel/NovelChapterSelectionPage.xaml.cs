using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace AniTool
{
    /// <summary>
    /// Interaction logic for ChapterSelectionPage.xaml
    /// </summary>
    public partial class NovelChapterSelectionPage : Page
    {
        NovelFetcher fetcher;
        NovelSearcher searcher;
        int indexOfNovel;
        new Frame NavigationService;
        Image image;
        TextBlock status;

        public NovelChapterSelectionPage(ref NovelFetcher fetcher, ref NovelSearcher searcher, int indexOfNovel, ref Frame NavigationService, Image image, ref TextBlock status)
        {
            InitializeComponent();
            this.fetcher = fetcher;
            this.searcher = searcher;
            this.indexOfNovel = indexOfNovel;
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
            novelScroll.Content = mainStack;
            status.Text = "Loading chapters";

            //Worker to retrieve the chapters in the background
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < searcher.Results()[indexOfNovel].latest; ++i)
                    {
                        if (i % 2 == 0)
                        {
                            var text = new TextBlock() { Text = $"Chapter {i + 1}", Margin = new Thickness(10, 10, 10, 10), FontSize = 20, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Blue, Cursor = Cursors.Hand };
                            text.PreviewMouseDown += NovelChapter_MouseDown;
                            mainStack.Children.Add(text);
                        }
                        else
                        {
                            var text = new TextBlock() { Text = $"Chapter {i + 1}", Margin = new Thickness(10, 10, 10, 10), FontSize = 20, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Orange, Cursor = Cursors.Hand };
                            text.PreviewMouseDown += NovelChapter_MouseDown;
                            mainStack.Children.Add(text);

                        }
                    }
                    novelImage.Source = image.Source;
                    status.Text = $"{searcher.Results()[indexOfNovel].name} - chapters loaded";
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
        private void NovelChapter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            int number = int.Parse(textBlock.Text.Substring(8));

            string text = fetcher.Fetch(searcher.Results()[indexOfNovel], number, 1, false);
            LoadChapter(text, number);
        }

        /// <summary>
        /// Load the selected chapter
        /// </summary>
        /// <param name="text">text for the chapter</param>
        /// <param name="chapter">Chapter number</param>
        private void LoadChapter(string text, int chapter)
        {
            NavigationService.Content = new ReadNovelPage(ref fetcher, ref searcher, indexOfNovel, text, chapter, ref status);
        }
    }
}
