namespace UpgradeFolders
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            // ���������Ϸ�
            AllowDrop = true;

            InitializeComponent();
        }

        private static double Confidence = 0.2;
        private static int MinLength = 4;

        public void UpgradeFolders(string originalRootPath)
        {
            if (string.IsNullOrEmpty(originalRootPath) || !Directory.Exists(originalRootPath))
            {
                // MessageBox.Show("��Ч�ĸ�Ŀ¼·����");
                return;
            }

            string deepestFolderPath = FindDeepestSimilarFolder(originalRootPath);

            if (string.Equals(deepestFolderPath, originalRootPath, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("û���ҵ���Ҫ��������ļ��С�");
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
            // �ƶ��ļ�
            foreach (var file in Directory.GetFiles(sourceFolder))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destinationFolder, fileName);
                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                    // MessageBox.Show($"���ƶ��ļ�: {fileName}");
                }
                else
                {
                    // MessageBox.Show($"�ļ��Ѵ��ڣ�����: {fileName}");
                }
            }

            // �ƶ��ļ���
            foreach (var directory in Directory.GetDirectories(sourceFolder))
            {
                string dirName = Path.GetFileName(directory);
                string destDirectory = Path.Combine(destinationFolder, dirName);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.Move(directory, destDirectory);
                    // MessageBox.Show($"���ƶ��ļ���: {dirName}");
                }
                else
                {
                    // MessageBox.Show($"�ļ����Ѵ��ڣ�����: {dirName}");
                }
            }
        }

        private void DeleteEmptyFolders(string currentFolder, string stopFolder)
        {
            if (string.IsNullOrEmpty(currentFolder) || string.IsNullOrEmpty(stopFolder))
            {
                MessageBox.Show("��Ч���ļ���·����");
                return;
            }

            while (!string.Equals(currentFolder, stopFolder, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (!Directory.Exists(currentFolder))
                    {
                        // MessageBox.Show($"�ļ��в�����: {currentFolder}");
                        break;
                    }

                    if (Directory.GetFileSystemEntries(currentFolder).Length == 0)
                    {
                        var parentDirectory = Directory.GetParent(currentFolder);
                        if (parentDirectory == null)
                        {
                            // MessageBox.Show($"�޷���ȡ���ļ���: {currentFolder}");
                            break;
                        }

                        string parentFolder = parentDirectory.FullName;

                        try
                        {
                            Directory.Delete(currentFolder);
                            // MessageBox.Show($"��ɾ�����ļ���: {currentFolder}");
                        }
                        catch (IOException)
                        {
                            // MessageBox.Show($"ɾ���ļ���ʱ����: {currentFolder}. ����: {ex.Message}");
                            break;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // MessageBox.Show($"û��Ȩ��ɾ���ļ���: {currentFolder}. ����: {ex.Message}");
                            break;
                        }

                        currentFolder = parentFolder;
                    }
                    else
                    {
                        // MessageBox.Show($"�ļ��в�Ϊ�գ�ֹͣɾ��: {currentFolder}");
                        break;
                    }
                }
                catch (Exception)
                {
                    // MessageBox.Show($"�����ļ���ʱ����δԤ�ڵĴ���: {currentFolder}. ����: {ex.Message}");
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rootFolderPath = textBox1.Text; // ��Ŀ¼·��

            // ��ȡ��ǰĿ¼�µ��������ļ���
            string[] subDirectories = Directory.GetDirectories(rootFolderPath);
            var errorDir = "";

            try
            {

                foreach (var subDirectory in subDirectories)
                {
                    errorDir = subDirectory;
                    // �ݹ鴦��ÿ�����ļ���
                    UpgradeFolders(subDirectory);
                }

                MessageBox.Show("�ἶ���");
            }
            catch (Exception ex)
            {
                MessageBox.Show(errorDir + "���������з�������: " + ex.Message);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                return; // ����¼�����Ϊ�գ�ֱ�ӷ���
            }

            try
            {
                // �������������Ƿ�Ϊ�ļ����ļ���
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    var data = e.Data.GetData(DataFormats.FileDrop);
                    if (data is string[] paths && paths.Length > 0)
                    {
                        // ����һ��·���Ƿ�Ϊ�ļ���
                        if (Directory.Exists(paths[0]))
                        {
                            // �����Ϸ�Ч��Ϊ����
                            e.Effect = DragDropEffects.Copy;
                        }
                        else
                        {
                            // ��������ļ��У�����Ϊ������
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
                // �����κ��쳣ʱ������Ϊ�������Ϸ�
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                MessageBox.Show("�Ϸ��¼�����Ϊ�ա�");
                return;
            }

            try
            {
                // ����Ƿ�����ļ��Ϸ�����
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    // ���Ի�ȡ�Ϸŵ��ļ�·��
                    var data = e.Data.GetData(DataFormats.FileDrop);
                    if (data is string[] paths && paths.Length > 0)
                    {
                        string folderPath = paths[0];
                        // ���·���Ƿ�Ϊ��ЧĿ¼
                        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                        {
                            textBox1.Text = folderPath;
                        }
                        else
                        {
                            MessageBox.Show("�ϷŵĲ�����Ч���ļ��С�");
                        }
                    }
                    else
                    {
                        MessageBox.Show("û�л�ȡ����Ч���ļ�·����");
                    }
                }
                else
                {
                    MessageBox.Show("���Ϸ��ļ��С�");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�����ϷŲ���ʱ��������{ex.Message}");
            }
        }

        /// <summary>
        ///     ͨ��������������ַ���֮���Levenshtein����
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int m = source.Length;
            int n = target.Length;
            int[,] d = new int[m + 1, n + 1];

            // ��ʼ������
            for (int i = 0; i <= m; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j <= n; j++)
            {
                d[0, j] = j;
            }

            // �Ա�Levenshtein����
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
                MessageBox.Show("�������ֵ����");
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
                MessageBox.Show("�������ֵ����");
                textBox3.Text = 4.ToString();
            }
        }
    }
}
