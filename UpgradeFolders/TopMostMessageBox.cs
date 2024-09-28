namespace UpgradeFolders;

internal class TopMostMessageBox : Form
{
    private TopMostMessageBox(string message)
    {
        Text = "消息";
        TopMost = true;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;

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

        Controls.Add(label);
        Controls.Add(okButton);
        ClientSize = new Size(
            Math.Max(label.Right + 10, okButton.Right + 10),
            okButton.Bottom + 10
        );
        AcceptButton = okButton;

        // 添加按钮点击事件
        okButton.Click += (sender, e) => Close();

        // 设置窗体关闭时释放资源
        FormClosed += (sender, e) => Dispose();
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
            using Form mainForm = Application.OpenForms[0] ?? new Form();
            if (mainForm.InvokeRequired)
                mainForm.Invoke(new Action(() => messageBox.ShowDialog()));
            else
                _ = messageBox.ShowDialog();
        }
        else
        {
            _ = messageBox.ShowDialog();
        }
    }
}