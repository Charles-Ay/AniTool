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

namespace AniTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        NovelFetcher fetcher;
        NovelSearcher searcher;
        public string? _status = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Status
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
            fetcher = new();
            fetcher.SetWorkDir(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\Novels", false);

            searcher = new();
            Closing += OnWindowClosing;

            //Create image folder
            if (!Directory.Exists(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages"))
                Directory.CreateDirectory(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");

            DataContext = this;
            Binding binding = new Binding("Status");
            statusText.SetBinding(TextBlock.TextProperty, binding);
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
        /// Onlick for search button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchNovelBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Content = new NovelSearchPage(ref fetcher, ref searcher, searchNovelBox.Text, ref NavigationService, ref statusText);
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
        /// Update status text
        /// </summary>
        /// <param name="text"></param>
        public void UpdateStatusText(string text)
        {
            Status = text;
        }

        /// <summary>
        /// Enter button for search box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchNovelBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) searchNovelBtn.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Button.MouseDownEvent });
        }

        /// <summary>
        /// Onclick for next page button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Next_Btn(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoForward)
                NavigationService.GoForward();
        }

        /// <summary>
        /// Onclick for previous page button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Btn(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
