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

        static void UpgradeFolders(string? folderPath)
        {
            // 获取当前目录下的所有子文件夹
            string?[] subDirectories = Directory.GetDirectories(folderPath);
            string? originalRootPath = folderPath; // 原始根路径

            // 如果当前目录下有且仅有一个子文件夹
            while (subDirectories.Length == 1)
            {
                string? subFolderPath = subDirectories[0];
                string subFolderName = new DirectoryInfo(subFolderPath).Name;
                string parentFolderName = new DirectoryInfo(folderPath).Name;

                // 检查子文件夹名称是否与父文件夹名称相同
                if (string.Equals(subFolderName, parentFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    // 递归进入子文件夹
                    folderPath = subFolderPath;
                    subDirectories = Directory.GetDirectories(folderPath);
                }
                else
                {
                    break;
                }
            }

            // 此时folderPath指向最深的符合条件的文件夹
            string? deepestFolderPath = folderPath;

            // 如果最深层文件夹路径与原始根路径相同，则不执行移动操作
            if (string.Equals(deepestFolderPath, originalRootPath, StringComparison.OrdinalIgnoreCase)) return;

            // 移动最深层文件夹中的所有文件到原始根目录
            foreach (var file in Directory.GetFiles(deepestFolderPath))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(originalRootPath, fileName);

                // 检查文件是否已经存在
                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                }
                else
                {
                    Console.WriteLine($"文件 {fileName} 已存在于目标目录，跳过移动。");
                }
            }

            // 移动最深层文件夹中的所有子文件夹到原始根目录
            foreach (var directory in Directory.GetDirectories(deepestFolderPath))
            {
                string destDirectory = Path.Combine(originalRootPath, new DirectoryInfo(directory).Name);

                // 检查目标子文件夹是否已经存在
                if (!Directory.Exists(destDirectory))
                {
                    Directory.Move(directory, destDirectory);
                }
                else
                {
                    Console.WriteLine($"文件夹 {new DirectoryInfo(directory).Name} 已存在于目标目录，跳过移动。");
                }
            }

            // 从最深层开始向上删除空文件夹
            while (folderPath != originalRootPath)
            {
                var parentPath = Directory.GetParent(folderPath)?.FullName;
                Directory.Delete(folderPath);
                folderPath = parentPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rootFolderPath = textBox1.Text; // 根目录路径

            // 获取当前目录下的所有子文件夹
            string?[] subDirectories = Directory.GetDirectories(rootFolderPath);
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
            // 检查拖入的数据是否为文件夹或文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    // 设置拖放效果为复制
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            // 获取拖入的数据
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                // 获取拖入的第一个文件夹路径（如果有多个，只获取第一个）
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    string folderPath = paths[0];
                    textBox1.Text = folderPath;
                }
            }
        }
    }
}
