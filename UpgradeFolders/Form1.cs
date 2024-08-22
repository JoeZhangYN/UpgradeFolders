namespace UpgradeFolders
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            // 设置允许拖放
            AllowDrop = true;

            InitializeComponent();
        }

        private static double Confidence = 0.2;
        private static int MinLength = 4;

        public void UpgradeFolders(string originalRootPath)
        {
            if (string.IsNullOrEmpty(originalRootPath) || !Directory.Exists(originalRootPath))
            {
                // MessageBox.Show("无效的根目录路径。");
                return;
            }

            string deepestFolderPath = FindDeepestSimilarFolder(originalRootPath);

            if (string.Equals(deepestFolderPath, originalRootPath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("没有找到需要处理的子文件夹。");
                return;
            }

            MoveContents(deepestFolderPath, originalRootPath);
            DeleteEmptyFolders(deepestFolderPath, originalRootPath);
        }

        private string FindDeepestSimilarFolder(string folderPath)
        {
            while (true)
            {
                string[] subDirectories = Directory.GetDirectories(folderPath);
                if (subDirectories.Length != 1)
                    break;

                string subFolderPath = subDirectories[0];
                string subFolderName = Path.GetFileName(subFolderPath);
                string parentFolderName = Path.GetFileName(folderPath);

                int averageLength = (subFolderName.Length + parentFolderName.Length) / 2;
                int maxDistance = averageLength <= MinLength ? 0 : Math.Max(0, (int)(averageLength * Confidence));

                if (ComputeLevenshteinDistance(subFolderName, parentFolderName) <= maxDistance)
                {
                    folderPath = subFolderPath;
                }
                else
                {
                    break;
                }
            }
            return folderPath;
        }

        private void MoveContents(string sourceFolder, string destinationFolder)
        {
            // 移动文件
            foreach (var file in Directory.GetFiles(sourceFolder))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destinationFolder, fileName);
                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                    // MessageBox.Show($"已移动文件: {fileName}");
                }
                else
                {
                    // MessageBox.Show($"文件已存在，跳过: {fileName}");
                }
            }

            // 移动文件夹
            foreach (var directory in Directory.GetDirectories(sourceFolder))
            {
                string dirName = Path.GetFileName(directory);
                string destDirectory = Path.Combine(destinationFolder, dirName);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.Move(directory, destDirectory);
                    // MessageBox.Show($"已移动文件夹: {dirName}");
                }
                else
                {
                    // MessageBox.Show($"文件夹已存在，跳过: {dirName}");
                }
            }
        }

        private void DeleteEmptyFolders(string currentFolder, string stopFolder)
        {
            if (string.IsNullOrEmpty(currentFolder) || string.IsNullOrEmpty(stopFolder))
            {
                MessageBox.Show("无效的文件夹路径。");
                return;
            }

            while (!string.Equals(currentFolder, stopFolder, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (!Directory.Exists(currentFolder))
                    {
                        // MessageBox.Show($"文件夹不存在: {currentFolder}");
                        break;
                    }

                    if (Directory.GetFileSystemEntries(currentFolder).Length == 0)
                    {
                        var parentDirectory = Directory.GetParent(currentFolder);
                        if (parentDirectory == null)
                        {
                            // MessageBox.Show($"无法获取父文件夹: {currentFolder}");
                            break;
                        }

                        string parentFolder = parentDirectory.FullName;

                        try
                        {
                            Directory.Delete(currentFolder);
                            // MessageBox.Show($"已删除空文件夹: {currentFolder}");
                        }
                        catch (IOException)
                        {
                            // MessageBox.Show($"删除文件夹时出错: {currentFolder}. 错误: {ex.Message}");
                            break;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // MessageBox.Show($"没有权限删除文件夹: {currentFolder}. 错误: {ex.Message}");
                            break;
                        }

                        currentFolder = parentFolder;
                    }
                    else
                    {
                        // MessageBox.Show($"文件夹不为空，停止删除: {currentFolder}");
                        break;
                    }
                }
                catch (Exception)
                {
                    // MessageBox.Show($"处理文件夹时发生未预期的错误: {currentFolder}. 错误: {ex.Message}");
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rootFolderPath = textBox1.Text; // 根目录路径

            // 获取当前目录下的所有子文件夹
            string[] subDirectories = Directory.GetDirectories(rootFolderPath);
            var errorDir = "";

            try
            {

                foreach (var subDirectory in subDirectories)
                {
                    errorDir = subDirectory;
                    // 递归处理每个子文件夹
                    UpgradeFolders(subDirectory);
                }

                MessageBox.Show("提级完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(errorDir + "操作过程中发生错误: " + ex.Message);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                return; // 如果事件参数为空，直接返回
            }

            try
            {
                // 检查拖入的数据是否为文件或文件夹
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    var data = e.Data.GetData(DataFormats.FileDrop);
                    if (data is string[] paths && paths.Length > 0)
                    {
                        // 检查第一个路径是否为文件夹
                        if (Directory.Exists(paths[0]))
                        {
                            // 设置拖放效果为复制
                            e.Effect = DragDropEffects.Copy;
                        }
                        else
                        {
                            // 如果不是文件夹，设置为不允许
                            e.Effect = DragDropEffects.None;
                        }
                    }
                    else
                    {
                        e.Effect = DragDropEffects.None;
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception)
            {
                // 发生任何异常时，设置为不允许拖放
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                MessageBox.Show("拖放事件数据为空。");
                return;
            }

            try
            {
                // 检查是否存在文件拖放数据
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    // 尝试获取拖放的文件路径
                    var data = e.Data.GetData(DataFormats.FileDrop);
                    if (data is string[] paths && paths.Length > 0)
                    {
                        string folderPath = paths[0];
                        // 检查路径是否为有效目录
                        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                        {
                            textBox1.Text = folderPath;
                        }
                        else
                        {
                            MessageBox.Show("拖放的不是有效的文件夹。");
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有获取到有效的文件路径。");
                    }
                }
                else
                {
                    MessageBox.Show("请拖放文件夹。");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理拖放操作时发生错误：{ex.Message}");
            }
        }

        /// <summary>
        ///     通过矩阵计算两个字符串之间的Levenshtein距离
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int m = source.Length;
            int n = target.Length;
            int[,] d = new int[m + 1, n + 1];

            // 初始化矩阵
            for (int i = 0; i <= m; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j <= n; j++)
            {
                d[0, j] = j;
            }

            // 对比Levenshtein距离
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    int cost = (source[i - 1] == target[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[m, n];
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Confidence = (100 - Convert.ToDouble(textBox2.Text.Trim())) * 0.01;
            }
            catch
            {
                MessageBox.Show("输入的数值有误");
                textBox2.Text = 80.ToString();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                MinLength = Convert.ToInt32(textBox2.Text.Trim());
            }
            catch
            {
                MessageBox.Show("输入的数值有误");
                textBox3.Text = 4.ToString();
            }
        }
    }
}
