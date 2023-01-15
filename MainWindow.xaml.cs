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
using WeebLib.WeeLibExceptions;
using WeebLib.Novel;
using WeebLib.Interfaces;
using System.Net;
using System.Security.Policy;
using System.Xml.Linq;
using System.ComponentModel;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using AniTool.Manga;
using WeebLib.Manga.Parser;
using WeebLib.Manga.Retrieval;

namespace AniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        NovelFetcher novelFetcher;
        NovelSearcher novelSearcher;
        
        public string? _status = "";

        MangaFetcher mangaFetcher;
        MangaSearcher mangaSearcher;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? novelStatus
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public string? mangaStatus
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
            novelFetcher = new();
            novelFetcher.SetWorkDir(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\Novels", false);
            novelSearcher = new();

            mangaFetcher = new();
            mangaFetcher.SetWorkDir(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\Manga", false);
            mangaSearcher = new();

            Closing += OnWindowClosing;

            //Create image folder
            if (!Directory.Exists(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages"))
                Directory.CreateDirectory(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");

            DataContext = this;
            Binding binding = new Binding("Status");
            novelStatusText.SetBinding(TextBlock.TextProperty, binding);
        }

        /// <summary>
        /// Used to check if status text has changed
        /// </summary>
        /// <param name="propertyName"></param>
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Onlick for novel search button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchNovelBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(novelsTab.IsSelected)
                NovelNavigationService.Content = new NovelSearchPage(ref novelFetcher, ref novelSearcher, searchNovelBox.Text, ref NovelNavigationService, ref novelStatusText); ;
        }

        /// <summary>
        /// Onlick for search button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchMangaBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mangaTab.IsSelected)
                MangaNavigationService.Content = new MangaSearchPage(ref mangaFetcher, ref mangaSearcher, searchMangaBox.Text, ref MangaNavigationService, ref mangaStatusText);
        }

        /// <summary>
        /// Clean up folders on close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnWindowClosing(object sender, CancelEventArgs e)
        {
#if DEBUG
#else
            System.IO.DirectoryInfo di = new (@$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
#endif
        }

        /// <summary>
        /// Enter button for search box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchNovelBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && novelsTab.IsSelected) searchNovelBtn.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Button.MouseDownEvent });
        }

        /// <summary>
        /// Enter button for search box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchMangaBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && mangaTab.IsSelected) searchMangaBtn.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Button.MouseDownEvent });
        }

        /// <summary>
        /// Onclick for next page button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NovelNext_Btn(object sender, RoutedEventArgs e)
        {
            if (NovelNavigationService.CanGoForward)
                NovelNavigationService.GoForward();
        }

        /// <summary>
        /// Onclick for previous page button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NovelBack_Btn(object sender, RoutedEventArgs e)
        {
            if (NovelNavigationService.CanGoBack)
                NovelNavigationService.GoBack();
        }

        /// <summary>
        /// Onclick for next page button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MangaNext_Btn(object sender, RoutedEventArgs e)
        {
            if (MangaNavigationService.CanGoForward)
                MangaNavigationService.GoForward();
        }

        /// <summary>
        /// Onclick for previous page button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MangaBack_Btn(object sender, RoutedEventArgs e)
        {
            if (MangaNavigationService.CanGoBack)
                MangaNavigationService.GoBack();
        }
    }
}
