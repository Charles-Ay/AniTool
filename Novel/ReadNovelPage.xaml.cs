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
    /// Interaction logic for ReadNovelPage.xaml
    /// </summary>
    public partial class ReadNovelPage : Page
    {
        int chapter;
        string text;
        int indexOfNovel;
        NovelFetcher fetcher;
        NovelSearcher searcher;
        TextBlock status;
        public ReadNovelPage(ref NovelFetcher fetcher, ref NovelSearcher searcher, int indexOfNovel, string text, int chapter, ref TextBlock status)
        {
            InitializeComponent();
            this.fetcher = fetcher;
            this.searcher = searcher;
            this.indexOfNovel = indexOfNovel;
            this.chapter = chapter;
            this.text = text;
            this.status = status;
            LoadText();
        }

        /// <summary>
        /// Update status and load text for chapter into TextBlock
        /// </summary>
        private void LoadText()
        {
            status.Text = "Chapter: " + chapter;
            novelText.Text = text;
        }
        
        /// <summary>
        /// Onlick for next chapter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Next_Btn(object sender, RoutedEventArgs e)
        {
            if (chapter < searcher.Results()[indexOfNovel].latest)
            {
                scroller.ScrollToTop();
                ++chapter;
                text = fetcher.Fetch(searcher.Results()[indexOfNovel], chapter, 1, false);
                LoadText();
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
                text = fetcher.Fetch(searcher.Results()[indexOfNovel], chapter, 1, false);
                LoadText();
            }
        }
    }
}
