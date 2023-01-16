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
using System.Reflection;
using Path = System.IO.Path;
using System.DirectoryServices.ActiveDirectory;
using System.Diagnostics;

namespace AniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        NovelFetcher novelFetcher;
        NovelSearcher novelSearcher;

        MangaFetcher mangaFetcher;
        MangaSearcher mangaSearcher;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        public string? _status = "";
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

        private ImageSource novelImgBinding;
        public ImageSource NovelImgBinding
        {
            get
            {
                return novelImgBinding;
            }
        }

        private ImageSource mangaImgBinding;
        public ImageSource MangaImgBinding
        {
            get
            {
                return mangaImgBinding;
            }
        }

        private ImageSource searchImgBinding;
        public ImageSource SearchImgBinding
        {
            get
            {
                return searchImgBinding;
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
            Trace.WriteLine(novelImg.Source);
            string workingDirectory = Environment.CurrentDirectory;
            string dir = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            string resourceDir = Path.Combine(dir, "resources");

            novelFetcher = new();
            novelFetcher.SetWorkDir(Path.Combine(resourceDir, "Novels"), false);
            novelSearcher = new();

            mangaFetcher = new();
            mangaFetcher.SetWorkDir(Path.Combine(resourceDir, "Manga"), false);
            mangaSearcher = new();

            //Closing += OnWindowClosing;

            //Create image folder
            if (!Directory.Exists(Path.Combine(resourceDir, "NovelImages")))
                Directory.CreateDirectory(Path.Combine(resourceDir, "NovelImages"));

            DataContext = this;

            //Set all the bindings
            //Status binding
            Binding binding = new Binding("Status");
            novelStatusText.SetBinding(TextBlock.TextProperty, binding);
            mangaStatusText.SetBinding(TextBlock.TextProperty, binding);

            novelImgBinding = new BitmapImage(new Uri(Path.Combine(resourceDir, "novel.png"), UriKind.RelativeOrAbsolute));
            mangaImgBinding = new BitmapImage(new Uri(Path.Combine(resourceDir, "manga.png"), UriKind.RelativeOrAbsolute));
            searchImgBinding = new BitmapImage(new Uri(Path.Combine(resourceDir, "search.png"), UriKind.RelativeOrAbsolute));
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
//        private static void OnWindowClosing(object sender, CancelEventArgs e)
//        {
//#if DEBUG
//#else
//            System.IO.DirectoryInfo di = new (@$"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");
//            foreach (FileInfo file in di.GetFiles())
//            {
//                file.Delete();
//            }
//#endif
//        }

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
