namespace UpgradeFolders
{
    internal class TopMostMessageBox : Form
    {
        public TopMostMessageBox(string message)
        {
            Text = "消息";
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            Label label = new()
            {
                Text = message,
                AutoSize = true,
                Location = new Point(10, 10)
            };

            Button okButton = new()
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

            // 添加按钮点击事件
            okButton.Click += (sender, e) => this.Close();

            // 设置窗体关闭时释放资源
            this.FormClosed += (sender, e) => this.Dispose();
        }

        public static void Show(object? sender, string message)
        {
            ShowMessageInternal(message);
        }

        public static void Show(string message)
        {
            ShowMessageInternal(message);
        }

        private static void ShowMessageInternal(string message)
        {
            using TopMostMessageBox messageBox = new(message);

            if (Application.OpenForms.Count > 0)
            {
                using Form mainForm = Application.OpenForms[0] ?? new();
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => messageBox.ShowDialog()));
                }
                else
                {
                    _=messageBox.ShowDialog();
                }
            }
            else
            {
                _=messageBox.ShowDialog();
            }
        }
    }
}
