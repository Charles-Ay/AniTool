using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Microsoft.Win32;
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
                    int latest = Convert.ToInt32(searcher.Results()[indexOfNovel].latest);
                    for (int i = 0; i < latest; ++i)
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
                    startChapterBox.Text = "1";
                    endChapterBox.Text = latest.ToString();
                    delayMsBox.Text = "1200";
                    maxPerMinuteBox.Text = "25";
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

        private async void ExportEpubBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(startChapterBox.Text, out int start) ||
                !int.TryParse(endChapterBox.Text, out int end))
            {
                status.Text = "Enter valid chapter numbers";
                return;
            }

            if (start < 1 || end < 1)
            {
                status.Text = "Chapter numbers must be 1 or higher";
                return;
            }

            if (start > end)
            {
                status.Text = "Start chapter must be <= end chapter";
                return;
            }

            int latest = Convert.ToInt32(searcher.Results()[indexOfNovel].latest);
            if (start > latest || end > latest)
            {
                status.Text = $"Chapters must be between 1 and {latest}";
                return;
            }

            string novelName = searcher.Results()[indexOfNovel].name;
            string safeName = MakeSafeFileName(novelName);
            string defaultName = $"{safeName} {start}-{end}.epub";

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "EPUB files (*.epub)|*.epub",
                FileName = defaultName,
                DefaultExt = ".epub"
            };

            if (dialog.ShowDialog() != true)
                return;

            exportEpubBtn.IsEnabled = false;
            status.Text = $"Downloading chapters {start}-{end}";

            try
            {
                int delayMs = ParseOrDefault(delayMsBox.Text, 1200);
                int maxPerMinute = ParseOrDefault(maxPerMinuteBox.Text, 25);
                if (delayMs < 0)
                    delayMs = 0;
                if (maxPerMinute < 1)
                    maxPerMinute = 1;

                var chapters = await Task.Run(() =>
                {
                    List<(int number, string text)> items = new();
                    Queue<DateTime> requestTimes = new();
                    for (int i = start; i <= end; i++)
                    {
                        int current = i;
                        Dispatcher.Invoke(() =>
                        {
                            status.Text = $"Downloading chapter {current} of {end}";
                        });

                        EnforceRateLimit(requestTimes, maxPerMinute);
                        if (delayMs > 0 && i != start)
                            Thread.Sleep(delayMs);

                        string text = FetchWithRetry(searcher.Results()[indexOfNovel], current);
                        requestTimes.Enqueue(DateTime.UtcNow);
                        items.Add((current, text));
                    }
                    return items;
                });

                status.Text = "Building EPUB";
                await Task.Run(() => BuildEpub(dialog.FileName, novelName, chapters));

                status.Text = $"Saved EPUB: {dialog.FileName}";
            }
            catch (Exception ex)
            {
                status.Text = $"Failed to export EPUB: {ex.Message}";
            }
            finally
            {
                exportEpubBtn.IsEnabled = true;
            }
        }

        private string FetchWithRetry(WeebLib.Utility.WeebLibUtil.SearchType info, int chapter)
        {
            const int maxAttempts = 5;
            const int baseDelayMs = 1200;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (attempt > 1)
                        Dispatcher.Invoke(() => status.Text = $"Retrying chapter {chapter} (attempt {attempt} of {maxAttempts})");

                    return fetcher.Fetch(info, chapter, 1, false);
                }
                catch (WebException ex) when (IsTooManyRequests(ex))
                {
                    int delay = baseDelayMs * attempt * attempt;
                    Thread.Sleep(delay);
                }
            }

            return fetcher.Fetch(info, chapter, 1, false);
        }

        private static int ParseOrDefault(string value, int fallback)
        {
            if (int.TryParse(value, out int parsed))
                return parsed;
            return fallback;
        }

        private static void EnforceRateLimit(Queue<DateTime> requestTimes, int maxPerMinute)
        {
            DateTime now = DateTime.UtcNow;
            while (requestTimes.Count > 0 && (now - requestTimes.Peek()).TotalSeconds >= 60)
                requestTimes.Dequeue();

            if (requestTimes.Count < maxPerMinute)
                return;

            DateTime oldest = requestTimes.Peek();
            int waitMs = (int)Math.Ceiling(60000 - (now - oldest).TotalMilliseconds);
            if (waitMs > 0)
                Thread.Sleep(waitMs);
        }

        private static bool IsTooManyRequests(WebException ex)
        {
            if (ex.Response is HttpWebResponse response)
                return (int)response.StatusCode == 429;

            return ex.Message.Contains("(429)", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("Too Many Requests", StringComparison.OrdinalIgnoreCase);
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        private static void BuildEpub(string outputPath, string novelTitle, List<(int number, string text)> chapters)
        {
            string tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "AniToolEpub", Guid.NewGuid().ToString("N"));
            string metaInf = System.IO.Path.Combine(tempRoot, "META-INF");
            string oebps = System.IO.Path.Combine(tempRoot, "OEBPS");
            Directory.CreateDirectory(metaInf);
            Directory.CreateDirectory(oebps);

            File.WriteAllText(System.IO.Path.Combine(tempRoot, "mimetype"), "application/epub+zip", Encoding.ASCII);

            string containerXml =
                "<?xml version=\"1.0\"?>\n" +
                "<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n" +
                "  <rootfiles>\n" +
                "    <rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/>\n" +
                "  </rootfiles>\n" +
                "</container>";
            File.WriteAllText(System.IO.Path.Combine(metaInf, "container.xml"), containerXml, Encoding.UTF8);

            List<string> chapterFiles = new();
            foreach (var chapter in chapters)
            {
                string fileName = $"chapter{chapter.number}.xhtml";
                string filePath = System.IO.Path.Combine(oebps, fileName);
                string html = BuildChapterHtml(novelTitle, chapter.number, chapter.text);
                File.WriteAllText(filePath, html, Encoding.UTF8);
                chapterFiles.Add(fileName);
            }

            string opf = BuildOpf(novelTitle, chapterFiles);
            File.WriteAllText(System.IO.Path.Combine(oebps, "content.opf"), opf, Encoding.UTF8);

            string ncx = BuildNcx(novelTitle, chapterFiles, chapters.Select(c => c.number).ToList());
            File.WriteAllText(System.IO.Path.Combine(oebps, "toc.ncx"), ncx, Encoding.UTF8);

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            using (ZipArchive archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                var mimetypeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
                using (var writer = new StreamWriter(mimetypeEntry.Open(), Encoding.ASCII))
                    writer.Write("application/epub+zip");

                foreach (string file in Directory.EnumerateFiles(tempRoot, "*", SearchOption.AllDirectories))
                {
                    string relative = System.IO.Path.GetRelativePath(tempRoot, file);
                    if (relative.Equals("mimetype", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string zipPath = relative.Replace('\\', '/');
                    archive.CreateEntryFromFile(file, zipPath, CompressionLevel.Optimal);
                }
            }

            Directory.Delete(tempRoot, true);
        }

        private static string BuildChapterHtml(string novelTitle, int chapterNumber, string text)
        {
            string body = BuildParagraphs(text);
            return
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<!DOCTYPE html>\n" +
                "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\n" +
                "<head>\n" +
                $"  <title>{WebUtility.HtmlEncode(novelTitle)} - Chapter {chapterNumber}</title>\n" +
                "  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\n" +
                "</head>\n" +
                "<body>\n" +
                $"  <h1>{WebUtility.HtmlEncode(novelTitle)} - Chapter {chapterNumber}</h1>\n" +
                $"  {body}\n" +
                "</body>\n" +
                "</html>";
        }

        private static string BuildParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "<p>(No content)</p>";

            string normalized = text.Replace("\r\n", "\n");
            string[] paragraphs = normalized.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder();

            foreach (string para in paragraphs)
            {
                string trimmed = para.Trim();
                if (trimmed.Length == 0)
                    continue;

                string encoded = WebUtility.HtmlEncode(trimmed).Replace("\n", "<br/>");
                builder.Append("<p>");
                builder.Append(encoded);
                builder.AppendLine("</p>");
            }

            return builder.ToString();
        }

        private static string BuildOpf(string novelTitle, List<string> chapterFiles)
        {
            StringBuilder manifest = new StringBuilder();
            StringBuilder spine = new StringBuilder();

            manifest.AppendLine("    <item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>");

            for (int i = 0; i < chapterFiles.Count; i++)
            {
                string id = $"chap{i + 1}";
                string href = chapterFiles[i];
                manifest.AppendLine($"    <item id=\"{id}\" href=\"{href}\" media-type=\"application/xhtml+xml\"/>");
                spine.AppendLine($"    <itemref idref=\"{id}\"/>");
            }

            return
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"2.0\" unique-identifier=\"bookid\">\n" +
                "  <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n" +
                $"    <dc:title>{WebUtility.HtmlEncode(novelTitle)}</dc:title>\n" +
                "    <dc:language>en</dc:language>\n" +
                "    <dc:identifier id=\"bookid\">urn:uuid:" + Guid.NewGuid().ToString() + "</dc:identifier>\n" +
                "  </metadata>\n" +
                "  <manifest>\n" +
                manifest +
                "  </manifest>\n" +
                "  <spine toc=\"ncx\">\n" +
                spine +
                "  </spine>\n" +
                "</package>";
        }

        private static string BuildNcx(string novelTitle, List<string> chapterFiles, List<int> chapterNumbers)
        {
            StringBuilder nav = new StringBuilder();
            for (int i = 0; i < chapterFiles.Count; i++)
            {
                nav.AppendLine("    <navPoint id=\"nav" + (i + 1) + "\" playOrder=\"" + (i + 1) + "\">");
                nav.AppendLine("      <navLabel><text>Chapter " + chapterNumbers[i] + "</text></navLabel>");
                nav.AppendLine("      <content src=\"" + chapterFiles[i] + "\"/>");
                nav.AppendLine("    </navPoint>");
            }

            return
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">\n" +
                "  <head>\n" +
                "    <meta name=\"dtb:uid\" content=\"urn:uuid:" + Guid.NewGuid().ToString() + "\"/>\n" +
                "    <meta name=\"dtb:depth\" content=\"1\"/>\n" +
                "    <meta name=\"dtb:totalPageCount\" content=\"0\"/>\n" +
                "    <meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n" +
                "  </head>\n" +
                "  <docTitle><text>" + WebUtility.HtmlEncode(novelTitle) + "</text></docTitle>\n" +
                "  <navMap>\n" +
                nav +
                "  </navMap>\n" +
                "</ncx>";
        }
    }
}
