using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PathViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string[] Path_Directories = Environment.GetEnvironmentVariable("path").Split(";", StringSplitOptions.RemoveEmptyEntries);
        private static readonly string[] Path_Extensions = Environment.GetEnvironmentVariable("pathext").Split(";", StringSplitOptions.RemoveEmptyEntries);
        private CancellationTokenSource _cancelTokenSrc;

        public MainWindow()
        {
            InitializeComponent();
            ContentRendered += (_, _) => InitTree();
        }
        
        public async void InitTree()
        {
            ScanPathDirectories();

            Label_PleaseWaitHint.Visibility = Visibility.Hidden;
            App_FilterTextBox.IsEnabled = true;
            await Task.Yield();
        }

        private void ScanPathDirectories()
        {
            ImageSource? DirImage = Application.Current.Resources["Directory_DrawingImage"] as ImageSource;
            SolidColorBrush? DirFore = Application.Current.Resources["DirItemForeground"] as SolidColorBrush;

            foreach (string Dir in Path_Directories)
            {
                CustomTreeViewItem DirItem = new CustomTreeViewItem(Dir)
                {
                    Image = DirImage,
                    Foreground = DirFore
                };

                foreach (string ext in Path_Extensions)
                    foreach (string file in Directory.EnumerateFiles(Dir, "*" + ext))
                    {
                        CustomTreeViewItem FileItem = new CustomTreeViewItem(Path.GetFileName(file));
                        FileItem.Image = GetExecutableIcon(file);
                        FileItem.MouseDoubleClick += (_, _) => OpenFolderAndSelectFile(file);

                        DirItem.Items.Add(FileItem);
                        //Debug.WriteLine(string.Format("Added \"{0}\"", file));
                    }

                if (DirItem.Items.Count != 0)
                {
                    App_TreeView.Items.Add(DirItem);
                    DirItem.UpdateName();
                }
            }

            //Debug.WriteLine("Scanning end");
        }

        private void FilterTreeView(string FilterWord)
        {
            // Async shet
            _cancelTokenSrc?.Cancel();
            _cancelTokenSrc = new CancellationTokenSource();
            CancellationToken token = _cancelTokenSrc.Token;

            // Show all
            if (string.IsNullOrWhiteSpace(FilterWord))
            {
                foreach (CustomTreeViewItem DirItem in App_TreeView.Items)
                {
                    ChangeItemState(DirItem, true);
                    foreach (CustomTreeViewItem FileItem in DirItem.Items)
                        ChangeItemState(FileItem, true);
                    DirItem.UpdateName();
                }
            }

            // Filter
            foreach (CustomTreeViewItem DirItem in App_TreeView.Items)
            {
                int MatchedItemsCount = 0;
                foreach (CustomTreeViewItem FileItem in DirItem.Items)
                {
                    token.ThrowIfCancellationRequested();
                    bool Match =
                        NativeMethods.PathMatchSpecExW(FileItem.Text, FilterWord, NativeMethods.MatchPatternFlags.Normal) == 0
                        | FileItem.Text.StartsWith(FilterWord);

                    ChangeItemState(FileItem, Match);
                    if (Match)
                        MatchedItemsCount++;
                }

                ChangeItemState(DirItem, MatchedItemsCount != 0);
                DirItem.UpdateName(MatchedItemsCount);
            }
        }

        private static void ChangeItemState(CustomTreeViewItem Item, bool State)
        {
            Item.Visibility = State ? Visibility.Visible : Visibility.Hidden;
            Item.Height = State ? double.NaN : 0;
        }

        private async void FilterTextBox_Changed(object sender, TextChangedEventArgs e)
        {
            try
            {
                Label_FilterHint.Visibility = string.IsNullOrEmpty(App_FilterTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
                FilterTreeView(App_FilterTextBox.Text);
            }
            finally
            {
                await Task.Yield();
            }
        }

        /// <summary>
        /// Opens explorer with folder opened where is file contained and highlights it
        /// </summary>
        public static void OpenFolderAndSelectFile(string filePath)
        {
            IntPtr pidl = NativeMethods.ILCreateFromPathW(filePath ?? throw new ArgumentNullException("filePath"));
            NativeMethods.SHOpenFolderAndSelectItems(pidl, 0, IntPtr.Zero, 0);
            NativeMethods.ILFree(pidl);
        }

        /// <summary>
        /// Extarcts icon for specified file
        /// </summary>
        public static ImageSource GetExecutableIcon(string path)
        {
            // SHGFI_USEFILEATTRIBUTES takes the file name and attributes into account if it doesn't exist
            uint flags = NativeMethods.SHGFI_ICON | NativeMethods.SHGFI_USEFILEATTRIBUTES;
            uint attributes = NativeMethods.FILE_ATTRIBUTE_NORMAL;

            NativeMethods.SHFILEINFO shfi;
            if (NativeMethods.SHGetFileInfo(path, attributes, out shfi, (uint)Marshal.SizeOf(typeof(NativeMethods.SHFILEINFO)), flags) != 0)
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon( shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            return null;
        }

        private static class NativeMethods
        {
            [DllImport("Shlwapi.dll", SetLastError = false)]
            public static extern int PathMatchSpecExW([MarshalAs(UnmanagedType.LPWStr)] string file, [MarshalAs(UnmanagedType.LPWStr)] string spec, MatchPatternFlags flags);

            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr ILCreateFromPathW(string pszPath);

            [DllImport("shell32.dll")]
            public static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, int cild, IntPtr apidl, int dwFlags);

            [DllImport("shell32.dll")]
            public static extern void ILFree(IntPtr pidl);

            [DllImport("shell32")]
            public static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);
            
            [Flags]
            public enum MatchPatternFlags : uint
            {
                Normal = 0x00000000,   // PMSF_NORMAL
                Multiple = 0x00000001,   // PMSF_MULTIPLE
                DontStripSpaces = 0x00010000    // PMSF_DONT_STRIP_SPACES
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            }

            public const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
            public const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
            public const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
            public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            public const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
            public const uint FILE_ATTRIBUTE_DEVICE = 0x00000040;
            public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
            public const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
            public const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
            public const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
            public const uint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
            public const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
            public const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
            public const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;
            public const uint FILE_ATTRIBUTE_VIRTUAL = 0x00010000;

            public const uint SHGFI_ICON = 0x000000100;     // get icon
            public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
        }
    }
}
