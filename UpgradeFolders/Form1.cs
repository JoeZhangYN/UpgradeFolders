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

        private static double JaccardThreshold = 0.7; // Jaccard相似度阈值
        private static double LevenshteinThreshold = 0.8; // Levenshtein相似度阈值
        private static int MinLength = 4;

        public void UpgradeFolders(string originalRootPath)
        {
            if (string.IsNullOrEmpty(originalRootPath) || !Directory.Exists(originalRootPath))
            {
                return;
            }

            bool upgraded = UpgradeSingleFolder(originalRootPath);

            if (upgraded)
            {
                // 如果发生了提级，递归处理父文件夹
                string parentPath = Path.GetDirectoryName(originalRootPath) ?? "";
                if (!string.IsNullOrEmpty(parentPath))
                {
                    UpgradeFolders(parentPath);
                }
            }
            else
            {
                // 如果当前文件夹没有提级，处理所有子文件夹
                foreach (var subDirectory in Directory.GetDirectories(originalRootPath))
                {
                    UpgradeFolders(subDirectory);
                }
            }
        }

        private bool UpgradeSingleFolder(string folderPath)
        {
            string[] subDirectories = Directory.GetDirectories(folderPath);

            if (subDirectories.Length != 1)
            {
                return false;
            }

            string subFolderPath = subDirectories[0];
            string parentFolderName = Path.GetFileName(folderPath);
            string subFolderName = Path.GetFileName(subFolderPath);

            if (IsSimilarEnough(parentFolderName, subFolderName))
            {
                MoveContentsToParent(subFolderPath, folderPath);
                DeleteEmptyFolders(subFolderPath, folderPath);
                return true;
            }

            return false;
        }

        private void MoveContentsToParent(string sourceFolder, string parentFolder)
        {
            // 移动文件
            foreach (var file in Directory.GetFiles(sourceFolder))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(parentFolder, fileName);
                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                }
            }

            // 移动文件夹
            foreach (var directory in Directory.GetDirectories(sourceFolder))
            {
                string dirName = Path.GetFileName(directory);
                string destDirectory = Path.Combine(parentFolder, dirName);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.Move(directory, destDirectory);
                }
            }
        }

        private void DeleteEmptyFolders(string currentFolder, string stopFolder)
        {
            while (!string.Equals(currentFolder, stopFolder, StringComparison.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(currentFolder))
                {
                    break;
                }

                if (Directory.GetFileSystemEntries(currentFolder).Length == 0)
                {
                    var parentDirectory = Directory.GetParent(currentFolder);
                    if (parentDirectory == null)
                    {
                        break;
                    }

                    string parentFolder = parentDirectory.FullName;

                    try
                    {
                        Directory.Delete(currentFolder);
                        currentFolder = parentFolder;
                    }
                    catch
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool IsSimilarEnough(string str1, string str2)
        {
            // 分割字符串为单独的部分
            var parts1 = SplitIntoParts(str1);
            var parts2 = SplitIntoParts(str2);

            // 计算Jaccard相似度
            double jaccardSimilarity = CalculateJaccardSimilarity(parts1, parts2);

            if (jaccardSimilarity >= JaccardThreshold && str1.Length > 5 && str2.Length > 5)
            {
                return true;
            }
            else
            {
                // 当有一个长度小于等于最小长度时，完全匹配才移动
                if (str1.Length <= MinLength || str2.Length<= MinLength) return str1 == str2;
                // 计算Levenshtein相似度
                double levenshteinSimilarity = CalculateLevenshteinSimilarity(string.Join(" ", parts1), string.Join(" ", parts2));
                return levenshteinSimilarity >= LevenshteinThreshold;
            }
        }

        private List<string> SplitIntoParts(string input)
        {
            // 使用正则表达式分割字符串
            var parts = System.Text.RegularExpressions.Regex.Split(input, @"(\[.*?\]|\S+)")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToLower()) // 转换为小写
                .ToList();

            // 移除任何纯数字的部分
            parts = parts.Where(p => !System.Text.RegularExpressions.Regex.IsMatch(p, @"^\d+$")).ToList();

            return parts;
        }

        private double CalculateJaccardSimilarity(List<string> list1, List<string> list2)
        {
            var set1 = new HashSet<string>(list1);
            var set2 = new HashSet<string>(list2);

            int intersectionCount = set1.Intersect(set2).Count();
            int unionCount = set1.Union(set2).Count();

            return (double)intersectionCount / unionCount;
        }

        private double CalculateLevenshteinSimilarity(string s1, string s2)
        {
            int distance = ComputeLevenshteinDistance(s1, s2);
            int maxLength = Math.Max(s1.Length, s2.Length);
            return 1 - ((double)distance / maxLength);
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

        private void button1_Click(object sender, EventArgs e)
        {
            string rootFolderPath = textBox1.Text; // 根目录路径

            // 获取当前目录下的所有子文件夹
            string[] subDirectories = Directory.GetDirectories(rootFolderPath);

            try
            {
                UpgradeFolders(rootFolderPath);
                TopMostMessageBox.Show("提级完成");
            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show("操作过程中发生错误: " + ex.Message);
            }
        }

        public class TopMostMessageBox : Form
        {
            public TopMostMessageBox(string message)
            {
                this.Text = "消息";
                this.TopMost = true;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MinimizeBox = false;
                this.MaximizeBox = false;

                Label label = new Label
                {
                    Text = message,
                    AutoSize = true,
                    Location = new Point(10, 10)
                };

                Button okButton = new Button
                {
                    Text = "确定",
                    DialogResult = DialogResult.OK,
                    Location = new Point(10, label.Bottom + 10)
                };

                this.Controls.Add(label);
                this.Controls.Add(okButton);
                this.ClientSize = new Size(
                    Math.Max(label.Right + 10, okButton.Right + 10),
                    okButton.Bottom + 10
                );

                this.AcceptButton = okButton;
            }

            public static void Show(string message)
            {
                using (var box = new TopMostMessageBox(message))
                {
                    box.ShowDialog();
                }
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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                LevenshteinThreshold = (100 - Convert.ToDouble(textBox2.Text.Trim())) * 0.01;
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
                MinLength = Convert.ToInt32(textBox3.Text.Trim());
            }
            catch
            {
                MessageBox.Show("输入的数值有误");
                textBox3.Text = 4.ToString();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                JaccardThreshold = (100 - Convert.ToDouble(textBox4.Text.Trim())) * 0.01;
            }
            catch
            {
                MessageBox.Show("输入的数值有误");
                textBox2.Text = 80.ToString();
            }
        }
    }
}
