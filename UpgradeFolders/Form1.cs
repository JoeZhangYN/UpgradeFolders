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

        static void UpgradeFolders(string? folderPath)
        {
            // ��ȡ��ǰĿ¼�µ��������ļ���
            string?[] subDirectories = Directory.GetDirectories(folderPath);
            string? originalRootPath = folderPath; // ԭʼ��·��

            // �����ǰĿ¼�����ҽ���һ�����ļ���
            while (subDirectories.Length == 1)
            {
                string? subFolderPath = subDirectories[0];
                string subFolderName = new DirectoryInfo(subFolderPath).Name;
                string parentFolderName = new DirectoryInfo(folderPath).Name;

                int maxDistance;

                var averageLength = subFolderName.Length + parentFolderName.Length;
                averageLength = (int)Math.Round(averageLength * 0.5, 0); // ȡƽ������

                if (averageLength <= 4) maxDistance = 0; // ��ƽ������С��4������ȫƥ��
                else maxDistance = Math.Max(0, (int)Math.Round(averageLength * Confidence, 0)); // ����ȡ Ĭ��80%���Ŷ� 

                // ������ļ��������Ƿ��븸�ļ���������ͬ
                if (ComputeLevenshteinDistance(subFolderName, parentFolderName) <= maxDistance)
                {
                    // �ݹ�������ļ���
                    folderPath = subFolderPath;
                    subDirectories = Directory.GetDirectories(folderPath);
                }
                else
                {
                    break;
                }
            }

            // ��ʱfolderPathָ������ķ����������ļ���
            string? deepestFolderPath = folderPath;

            // ���������ļ���·����ԭʼ��·����ͬ����ִ���ƶ�����
            if (string.Equals(deepestFolderPath, originalRootPath, StringComparison.OrdinalIgnoreCase)) return;

            // �ƶ�������ļ����е������ļ���ԭʼ��Ŀ¼
            foreach (var file in Directory.GetFiles(deepestFolderPath))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(originalRootPath, fileName);

                // ����ļ��Ƿ��Ѿ�����
                if (!File.Exists(destFile))
                {
                    File.Move(file, destFile);
                }
                else
                {
                    Console.WriteLine($"�ļ� {fileName} �Ѵ�����Ŀ��Ŀ¼�������ƶ���");
                }
            }

            // �ƶ�������ļ����е��������ļ��е�ԭʼ��Ŀ¼
            foreach (var directory in Directory.GetDirectories(deepestFolderPath))
            {
                string destDirectory = Path.Combine(originalRootPath, new DirectoryInfo(directory).Name);

                // ���Ŀ�����ļ����Ƿ��Ѿ�����
                if (!Directory.Exists(destDirectory))
                {
                    Directory.Move(directory, destDirectory);
                }
                else
                {
                    Console.WriteLine($"�ļ��� {new DirectoryInfo(directory).Name} �Ѵ�����Ŀ��Ŀ¼�������ƶ���");
                }
            }

            // ������㿪ʼ����ɾ�����ļ���
            while (folderPath != originalRootPath)
            {
                var parentPath = Directory.GetParent(folderPath)?.FullName;
                Directory.Delete(folderPath);
                folderPath = parentPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rootFolderPath = textBox1.Text; // ��Ŀ¼·��

            // ��ȡ��ǰĿ¼�µ��������ļ���
            string?[] subDirectories = Directory.GetDirectories(rootFolderPath);
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
            // �������������Ƿ�Ϊ�ļ��л��ļ�
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    // �����Ϸ�Ч��Ϊ����
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            // ��ȡ���������
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                // ��ȡ����ĵ�һ���ļ���·��������ж����ֻ��ȡ��һ����
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    string folderPath = paths[0];
                    textBox1.Text = folderPath;
                }
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
    }
}
