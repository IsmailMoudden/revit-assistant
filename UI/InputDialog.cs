using System.Windows.Forms;

namespace BimAiAssistant.UI
{
    /// <summary>
    /// Minimal single-line text input dialog.
    /// Returns null if the user cancels.
    /// </summary>
    public static class InputDialog
    {
        public static string Show(string title, string prompt)
        {
            using (var form = new Form())
            {
                form.Text = title;
                form.Width = 480;
                form.Height = 160;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label
                {
                    Text = prompt,
                    Left = 12,
                    Top = 16,
                    Width = 440
                };

                var textBox = new TextBox
                {
                    Left = 12,
                    Top = 40,
                    Width = 440
                };

                var okButton = new Button
                {
                    Text = "Send",
                    Left = 280,
                    Top = 80,
                    Width = 80,
                    DialogResult = DialogResult.OK
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    Left = 372,
                    Top = 80,
                    Width = 80,
                    DialogResult = DialogResult.Cancel
                };

                form.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                if (form.ShowDialog() != DialogResult.OK)
                    return null;

                string result = textBox.Text.Trim();
                return result.Length == 0 ? null : result;
            }
        }
    }
}
