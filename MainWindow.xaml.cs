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
using WeebLib.Novel;
using WeebLib.Interfaces;

namespace AniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NovelFetcher fetcher;
        NovelSearcher searcher;
        public MainWindow()
        {
            InitializeComponent();
            fetcher = new();
            searcher = new();
        }

        private void searchNovelBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            searcher.Search(1, searchNovelBox.Text, NovelUtil.sourceToString(NovelUtil.NovelSources.FullNovel));
        }
    }
}
