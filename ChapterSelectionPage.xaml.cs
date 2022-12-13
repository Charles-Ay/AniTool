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

namespace AniTool
{
    /// <summary>
    /// Interaction logic for ChapterSelectionPage.xaml
    /// </summary>
    public partial class ChapterSelectionPage : Page
    {
        NovelFetcher fetcher;
        NovelSearcher searcher;
        int indexOfNovel;
        TextBlock statusText;
        Frame mainFrame;

        public ChapterSelectionPage(ref NovelFetcher fetcher, ref NovelSearcher searcher, int indexOfNovel, ref TextBlock statusText, ref Frame mainFrame)
        {
            InitializeComponent();
            this.fetcher = fetcher;
            this.searcher = searcher;
            this.indexOfNovel = indexOfNovel;
            this.statusText = statusText;
            this.mainFrame = mainFrame;
            LoadChapterSelection();
        }

        public void LoadChapterSelection()
        {
            WrapPanel mainStack = new();
            mainStack.Orientation = Orientation.Horizontal;
            novelScroll.Content = mainStack;
            for (int i = 0; i < searcher.getResults()[indexOfNovel].latest; ++i)
            {
                if (i % 2 == 0)
                {
                    mainStack.Children.Add(new TextBlock() { Text = $"Chapter {i + 1}", Margin = new Thickness(10, 10, 10, 10), FontSize = 20, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Blue });
                }
                else
                {
                    mainStack.Children.Add(new TextBlock() { Text = $"Chapter {i + 1}", Margin = new Thickness(10, 10, 10, 10), FontSize = 20, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Orange });

                }
            }
        }
    }
}
