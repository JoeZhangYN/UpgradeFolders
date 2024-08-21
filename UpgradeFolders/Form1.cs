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

                // ������ļ��������Ƿ��븸�ļ���������ͬ
                if (string.Equals(subFolderName, parentFolderName, StringComparison.OrdinalIgnoreCase))
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
    }
}
