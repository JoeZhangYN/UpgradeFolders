namespace UpgradeFolders
{
    internal partial class Form1 : Form
    {
        public Form1()
        {
            AllowDrop = true;
            InitializeComponent();
        }

        private readonly FolderUpgrader upgrader = new();

        private void button1_Click(object sender, EventArgs e)
        {
            string rootFolderPath = textBox1.Text;

            try
            {
                upgrader.UpgradeFolders(rootFolderPath);
                TopMostMessageBox.Show("�ἶ���");
            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show("���������з�������: " + ex.Message);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            try
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    object? data = e.Data.GetData(DataFormats.FileDrop);
                    e.Effect =data is string[] paths && paths.Length > 0
                        ? Directory.Exists(paths[0]) ? DragDropEffects.Copy : DragDropEffects.None
                        : DragDropEffects.None;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception)
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e == null)
            {
                _ = MessageBox.Show("�Ϸ��¼�����Ϊ�ա�");
                return;
            }

            try
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    object? data = e.Data.GetData(DataFormats.FileDrop);
                    if (data is string[] paths && paths.Length > 0)
                    {
                        string folderPath = paths[0];
                        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                        {
                            textBox1.Text = folderPath;
                            textBox1.SelectionStart = textBox1.Text.Length;
                            textBox1.ScrollToCaret();
                        }
                        else
                        {
                            _ = MessageBox.Show("�ϷŵĲ�����Ч���ļ��С�");
                        }
                    }
                    else
                    {
                        _ = MessageBox.Show("û�л�ȡ����Ч���ļ�·����");
                    }
                }
                else
                {
                    _ = MessageBox.Show("���Ϸ��ļ��С�");
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"�����ϷŲ���ʱ��������{ex.Message}");
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                upgrader.SetThresholds(levenshteinThreshold: (100 - Convert.ToDouble(textBox2.Text.Trim())) * 0.01);
            }
            catch
            {
                _ = MessageBox.Show("�������ֵ����");
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
                upgrader.SetThresholds(minLength: Convert.ToInt32(textBox3.Text.Trim()));
            }
            catch
            {
                _ = MessageBox.Show("�������ֵ����");
                textBox3.Text = 4.ToString();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                upgrader.SetThresholds(jaccardThreshold: (100 - Convert.ToDouble(textBox4.Text.Trim())) * 0.01);
            }
            catch
            {
                _ = MessageBox.Show("�������ֵ����");
                textBox2.Text = 80.ToString();
            }
        }
    }
}