using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using System.Windows.Threading;

namespace Backupper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel MainWindowViewModel { get; }
        private readonly List<(TreeNode, string)> CopyQueue;

        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel = DataContext as MainWindowViewModel;
            CopyQueue = new List<(TreeNode, string)>();
        }

        private void PathBox_PathTextChanged(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.CanBackup = false;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region == Check ==

        /// <summary>
        /// ディレクトリを再帰的に走査し、ツリーにノードを追加します。
        /// </summary>
        /// <param name="path">ディレクトリのパス</param>
        /// <returns>子要素が追加されたノード</returns>
        private async Task<TreeNode> ScanFiles(string path) => await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (!MainWindowViewModel.CanCancel)
            {
                return null;
            }

            TreeNode node = null;

            try
            {
                node = new TreeNode(path,
                 Directory.EnumerateDirectories(path).Select(directory => ScanFiles(directory).Result).Concat(
                    Directory.EnumerateFiles(path).Select(child => new TreeNode(child))));
            }
            catch (Exception ee)
            {
                LogException(ee);
            }

            return node;
        });

        /// <summary>
        /// ノードを走査し、更新日時を対応するノードと比較します。
        /// </summary>
        /// <param name="node">比較対象のノード</param>
        /// <param name="nodes">比較対象のノードと同じ名前を持つノードが含まれるコレクション</param>
        /// <returns></returns>
        private async Task CheckUpdateTimeInNode(TreeNode node, TreeNodeCollection nodes) => await Task.Run(() =>
        {
            if (node == null || nodes == null)
            {
                return;
            }

            if (!MainWindowViewModel.CanCancel)
            {
                return;
            }

            try
            {
                MainWindowViewModel.Progress = MainWindowViewModel.None + MainWindowViewModel.Newer + MainWindowViewModel.Older + MainWindowViewModel.Missing;

                foreach (TreeNode item in nodes)
                {
                    if (node.Name.Equals(item.Name))
                    {
                        int dif = node.EditTime.CompareTo(item.EditTime);
                        node.Brush = dif < 0 ? Brushes.Yellow : dif > 0 ? Brushes.LightGreen : Brushes.White;
                        item.Brush = dif > 0 ? Brushes.Yellow : dif < 0 ? Brushes.LightGreen : Brushes.White;

                        if (dif == 0)
                        {
                            MainWindowViewModel.None++;
                        }
                        else if (dif > 0)
                        {
                            MainWindowViewModel.Newer++;
                            CopyQueue.Add((node, item.Text));
                        }
                        else
                        {
                            MainWindowViewModel.Older++;
                        }

                        foreach (TreeNode child in node.Nodes)
                        {
                            CheckUpdateTimeInNode(child, item.Nodes).Wait();
                        }

                        return;
                    }
                }

                void fillPink(TreeNode missingNode, string path)
                {
                    missingNode.Brush = Brushes.Pink;
                    MainWindowViewModel.Missing++;
                    string childPath = System.IO.Path.Combine(path, missingNode.Name);
                    CopyQueue.Add((missingNode, childPath));

                    foreach (TreeNode item in missingNode.Nodes)
                    {
                        fillPink(item, childPath);
                    }
                }

                fillPink(node, nodes.Parent?.Text);
            }
            catch (Exception ee)
            {
                LogException(ee);
            }
        });

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindowViewModel.CanCheck)
            {
                return;
            }

            string from = MainWindowViewModel.FromPathText;
            string to = MainWindowViewModel.ToPathText;

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                return;
            }

            bool isDirectory = Directory.Exists(from);

            if ((!File.Exists(from) && !isDirectory) || from.Equals(to))
            {
                return;
            }

            try
            {
                CopyQueue.Clear();

                MainWindowViewModel.TreeSource1.Clear();
                MainWindowViewModel.TreeSource2.Clear();
                MainWindowViewModel.ResetProperties2();
                MainWindowViewModel.CanBackup = false;
                MainWindowViewModel.CanCheck = false;
                MainWindowViewModel.CanCancel = true;
                MainWindowViewModel.IsIndeterminate = true;

                TreeNode root1 = isDirectory ? await ScanFiles(from) : new TreeNode(null, new TreeNode[] { new TreeNode(from) });
                TreeNode root2 = await ScanFiles(to);
                MainWindowViewModel.TreeSource1 = root1?.Nodes;
                MainWindowViewModel.TreeSource2 = root2?.Nodes;
                MainWindowViewModel.IsIndeterminate = false;

                if (root1 != null && root2 != null)
                {
                    MainWindowViewModel.MaxProgress = root1.CountAll;

                    foreach (TreeNode node in root1.Nodes)
                    {
                        await CheckUpdateTimeInNode(node, MainWindowViewModel.TreeSource2);
                    }

                    MainWindowViewModel.CanBackup = true;
                }
            }
            catch (Exception ee)
            {
                LogException(ee);
            }
            finally
            {
                MainWindowViewModel.CanCancel = false;
                MainWindowViewModel.ResetProperties1();
            }
        }

        #endregion

        #region == Backup ==

        /// <summary>
        /// チェックされたファイルをコピーし、必要であればディレクトリを作成します。
        /// </summary>
        /// <returns></returns>
        private async Task CopyFiles() => await Task.Run(() =>
        {
            if (CopyQueue == null)
            {
                return;
            }

            try
            {
                MainWindowViewModel.MaxProgress = CopyQueue.Count;

                foreach ((TreeNode, string) item in CopyQueue)
                {
                    if (!MainWindowViewModel.CanCancel)
                    {
                        return;
                    }

                    MainWindowViewModel.Progress++;

                    if (item.Item1.IsChecked == false)
                    {
                        continue;
                    }

                    string from = item.Item1.Text;
                    string to = item.Item2;

                    if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
                    {
                        if (File.Exists(from))
                        {
                            File.Copy(from, to, true);
                        }
                        else if (Directory.Exists(from))
                        {
                            Directory.CreateDirectory(to);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                LogException(ee);
            }
        });

        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MainWindowViewModel.CanBackup)
            {
                return;
            }

            try
            {
                MainWindowViewModel.CanCheck = false;
                MainWindowViewModel.CanCancel = true;

                await CopyFiles();
            }
            catch (Exception ee)
            {
                LogException(ee);
            }
            finally
            {
                MainWindowViewModel.CanBackup = false;
                MainWindowViewModel.CanCancel = false;
                MainWindowViewModel.ResetProperties1();
            }
        }

        #endregion

        private void CancelButton_Click(object sender = null, RoutedEventArgs e = null)
        {
            MainWindowViewModel.CanCancel = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel.CanBackup = false;
            MainWindowViewModel.CanCancel = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CancelButton_Click();
        }

        private void LogException(Exception e)
        {
            string message = e.Message;
            string trace = e.StackTrace;

            File.AppendAllText("exceptions.txt", $"{message}\n{trace}\n\n", Encoding.UTF8);

            MessageBox.Show(trace, message, MessageBoxButton.OK, MessageBoxImage.Warning);
            CancelButton_Click();
        }
    }
}
