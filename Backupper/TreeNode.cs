using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Input;

namespace Backupper
{
    public class TreeNode : SharedWPF.ViewModelBase
    {
        #region == IsChecked ==

        private bool? _IsChecked = true;
        public bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    RaisePropertyChanged(nameof(IsChecked));
                }
            }
        }

        #endregion
        public ICommand CheckCommand { get; private set; }
        public BitmapSource Icon
        {
            get
            {
                BitmapSource source = null;
                //System.Drawing.Icon.ExtractAssociatedIcon(path).ToImageSource()
                SHFILEINFO shinfo = new SHFILEINFO();
                if (SHGetFileInfo(Text, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON) != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
                {
                    Application.Current.Dispatcher.Invoke(() => source = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                }
                return source;
            }
        }
        public string Name => Path.GetFileName(Text);
        #region == Brush ==

        private Brush _Brush = Brushes.White;
        public Brush Brush
        {
            get => _Brush;
            set
            {
                if (_Brush != value)
                {
                    _Brush = value;
                    RaisePropertyChanged(nameof(Brush));
                }
            }
        }

        #endregion

        public string Text { get; set; }
        public DateTime EditTime => Directory.Exists(Text) ? Directory.GetLastWriteTime(Text) : File.Exists(Text) ? File.GetLastWriteTime(Text) : new DateTime();
        public string EditTimeText => EditTime.ToString("G");
        public TreeNode Parent { get; internal set; }
        public TreeNodeCollection Nodes { get; }
        public int Count => Nodes.Count;
        public int CountAll
        {
            get
            {
                int result = 0;
                void count(TreeNode node)
                {
                    result += node.Count;
                    foreach (TreeNode item in node.Nodes)
                    {
                        count(item);
                    }
                }
                count(this);
                return result;
            }
        }

        public TreeNode(string path, IEnumerable<TreeNode> nodes = null)
        {
            IsChecked = true;
            CheckCommand = CreateCommand(CheckBox_Click);
            Brush = Brushes.White;

            Text = string.Empty;
            Parent = null;
            Nodes = new TreeNodeCollection(this);

            if (!string.IsNullOrWhiteSpace(path))
            {
                Text = path;
            }
            if (nodes != null)
            {
                Nodes.AddRange(nodes);
            }
        }

        public void CheckBox_Click(object value)
        {
            void up(TreeNode parent)
            {
                if (parent != null)
                {
                    int checkedCount = 0;
                    foreach (TreeNode node in parent.Nodes)
                    {
                        if (node.IsChecked == null)
                        {
                            checkedCount++;
                        }
                        else {
                            checkedCount += (node.IsChecked ?? false) ? 2 : 0;
                        }
                    }
                    parent.IsChecked = checkedCount == parent.Count * 2 ? true : checkedCount == 0 ? false : (bool?)null;
                    up(parent.Parent);
                }
            }
            up(Parent);

            void down(TreeNode node)
            {
                if (node != null)
                {
                    foreach (TreeNode child in node.Nodes)
                    {
                        child.IsChecked = node.IsChecked;
                        down(child);
                    }
                }
            }
            down(this);
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
    }

    public class TreeNodeCollection : ObservableCollection<TreeNode>
    {
        public TreeNode Parent { get; }

        public TreeNodeCollection(TreeNode parent = null)
        {
            Parent = parent;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (TreeNode node in e.NewItems)
                        {
                            node.Parent = Parent;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (TreeNode node in e.OldItems)
                        {
                            node.Parent = null;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (Items != null)
                    {
                        foreach (TreeNode node in Items)
                        {
                            node.Parent = null;
                        }
                    }
                    break;
                default:
                    break;
            }

            base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
            {
                Add(node);
            }
        }
    }

}
