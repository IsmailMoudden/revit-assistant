using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BimAiAssistant.UI
{
    public static class WarningsDialog
    {
        public static void Show(List<string> warnings)
        {
            if (warnings == null || warnings.Count == 0) return;

            using (var form = new Form())
            {
                form.Text            = "BIM AI Assistant";
                form.MinimumSize     = new Size(480, 320);
                form.Size            = new Size(560, 420);
                form.FormBorderStyle = FormBorderStyle.Sizable;
                form.StartPosition   = FormStartPosition.CenterScreen;
                form.MaximizeBox     = true;
                form.MinimizeBox     = false;
                form.BackColor       = Color.FromArgb(245, 246, 248);

                // ── Dark header ───────────────────────────────────────────────
                var header = new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 56,
                    BackColor = Color.FromArgb(18, 18, 18)
                };
                header.Controls.Add(new Label
                {
                    Text      = "BIM AI Assistant",
                    Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Left = 20, Top = 14, AutoSize = true
                });
                header.Controls.Add(new Label
                {
                    Text      = "Section substitutions",
                    Font      = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(170, 170, 170),
                    Left = 22, Top = 36, AutoSize = true
                });

                // ── Footer ────────────────────────────────────────────────────
                var footer = new Panel
                {
                    Dock      = DockStyle.Bottom,
                    Height    = 68,
                    BackColor = Color.FromArgb(245, 246, 248)
                };
                footer.Controls.Add(new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 1,
                    BackColor = Color.FromArgb(220, 220, 220)
                });

                var okBtn = new Button
                {
                    Text         = "OK",
                    Width        = 110,
                    Height       = 36,
                    Top          = 16,
                    FlatStyle    = FlatStyle.Flat,
                    Font         = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    BackColor    = Color.FromArgb(18, 18, 18),
                    ForeColor    = Color.White,
                    Cursor       = Cursors.Hand,
                    Anchor       = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = DialogResult.OK
                };
                okBtn.FlatAppearance.BorderSize = 0;
                footer.Resize += (s, e) => okBtn.Left = footer.ClientSize.Width - 28 - 110;
                footer.Controls.Add(okBtn);

                // ── Scrollable warning list ───────────────────────────────────
                var body = new Panel
                {
                    Dock      = DockStyle.Fill,
                    BackColor = Color.FromArgb(245, 246, 248)
                };

                var list = new RichTextBox
                {
                    Dock       = DockStyle.Fill,
                    ReadOnly   = true,
                    BackColor  = Color.FromArgb(245, 246, 248),
                    ForeColor  = Color.FromArgb(40, 40, 40),
                    Font       = new Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.None,
                    ScrollBars  = RichTextBoxScrollBars.Vertical,
                    Padding    = new Padding(28, 20, 28, 12),
                    WordWrap   = true,
                    TabStop    = false
                };

                // Build text: amber bullet + message per warning
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < warnings.Count; i++)
                {
                    sb.Append("⚠  ");
                    sb.Append(warnings[i]);
                    if (i < warnings.Count - 1)
                        sb.Append("\n\n");
                }
                list.Text = sb.ToString();

                body.Controls.Add(list);

                // ── Assemble ──────────────────────────────────────────────────
                form.Controls.Add(body);
                form.Controls.Add(footer);
                form.Controls.Add(header);

                form.AcceptButton = okBtn;
                form.Load += (s, e) => okBtn.Left = footer.ClientSize.Width - 28 - 110;

                form.ShowDialog();
            }
        }
    }
}
