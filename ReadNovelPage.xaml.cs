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

        private void LoadText()
        {
            status.Text = "Chapter: " + chapter;
            novelText.Text = text;
        }
        
        private void Next_Btn(object sender, RoutedEventArgs e)
        {
            if (chapter < searcher.Results()[indexOfNovel].latest)
            {
                ++chapter;
                text = fetcher.Fetch(searcher.Results()[indexOfNovel], chapter, 1, false);
                LoadText();
            }
        }

        private void Back_Btn(object sender, RoutedEventArgs e)
        {
            if (chapter > 1)
            {
                --chapter;
                text = fetcher.Fetch(searcher.Results()[indexOfNovel], chapter, 1, false);
                LoadText();
            }
        }
    }
}
