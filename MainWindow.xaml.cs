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
            PageSetup();
            fetcher = new();
            fetcher.SetWorkDir(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\Novels", false);

            searcher = new();
            Closing += OnWindowClosing;

            if (!Directory.Exists(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages"))
                Directory.CreateDirectory(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\NovelImages");
        }

        private void PageSetup()
        {
            //TabItem tabItem = new();
            //tabItem.Header = new { Width = 20, Source = new BitmapImage(new Uri(@"C:\Users\charl\Documents\Programming\C#\WPF\AniTool\AniTool\resources\novel.png")) };

            //Grid mainNovelGrid = new();
            //mainNovelGrid.Name = "mainNovelGrid";
            //mainNovelGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            //mainNovelGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            //Grid novelSearchGrid = new();
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = new GridLength(20) });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = new GridLength(30) });
            //novelSearchGrid.ColumnDefinitions.Add(new() { Width = new GridLength(20) });

            //novelSearchGrid.RowDefinitions.Add(new() { Height = new GridLength(20) });
            //novelSearchGrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
            //novelSearchGrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
            //novelSearchGrid.RowDefinitions.Add(new() { Height = GridLength.Auto });
            //novelSearchGrid.RowDefinitions.Add(new() { Height = new GridLength(20) });

            ////inside 1 start
            //TextBlock novelBlock = new();
            //novelBlock.SetValue(Grid.ColumnProperty, 1);
            //novelBlock.SetValue(Grid.RowProperty, 1);
            //novelBlock.SetValue(TextBlock.FontSizeProperty, 36);
            //novelBlock.SetValue(Grid.ColumnSpanProperty, 2);
            //novelBlock.Text = "Light Novels";
            //novelBlock.Margin = new(0, 0, 0, 10);

            //TextBox searchNovelBox = new();
            //searchNovelBox.SetValue(Grid.ColumnProperty, 5);
            //searchNovelBox.SetValue(Grid.RowProperty, 1);
            //searchNovelBox.Height = 40;
            //searchNovelBox.Width = 150;
            //searchNovelBox.SetValue(Grid.ColumnSpanProperty, 2);
            //searchNovelBox.KeyDown += searchNovelBox_KeyDown;
            //searchNovelBox.Name = "searchNovelBox";

            //TextBlock searchBlock = new();
            //searchBlock.SetValue(Grid.ColumnProperty, 5);
            //searchBlock.SetValue(Grid.RowProperty, 1);
            //searchBlock.IsHitTestVisible = false;
            //searchBlock.Text = "Search Novel...";
            //searchBlock.VerticalAlignment = VerticalAlignment.Center;
            //searchBlock.HorizontalAlignment = HorizontalAlignment.Left;
            //searchBlock.Margin = new(10, 0, 0, 0);
            //searchBlock.Foreground = Brushes.DarkGray;

            //Style style = new();
            //style.TargetType = typeof(TextBlock);
            //style.Setters.Add(new Setter() { Property = VisibilityProperty, Value = Visibility.Collapsed });
            //style.Triggers.Add(new DataTrigger() { Binding = new Binding() { ElementName = "searchNovelBox}" } });


        }

        private void SearchNovelBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainFrame.Content = new NovelSearchPage(ref fetcher, ref searcher, ref statusText, searchNovelBox.Text, ref mainFrame);
        }

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

        private void searchNovelBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) searchNovelBtn.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Button.MouseDownEvent });
        }
    }
}
