using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BimAiAssistant.Models;

namespace BimAiAssistant.UI
{
    public static class ClarificationDialog
    {
        private const int Pad    = 28;  // horizontal margin inside scroll panel
        private const int LblH  = 20;  // question label height
        private const int InpH  = 32;  // input control height
        private const int Gap   = 28;  // vertical gap between questions

        public static Dictionary<string, object> Show(List<ClarificationQuestion> questions)
        {
            var inputs = new List<(string id, Control control, string type)>();

            using (var form = new Form())
            {
                // ── Window ────────────────────────────────────────────────────
                form.Text            = "BIM AI Assistant";
                form.MinimumSize     = new Size(480, 360);
                form.Size            = new Size(560, 480);
                form.FormBorderStyle = FormBorderStyle.Sizable;
                form.StartPosition   = FormStartPosition.CenterScreen;
                form.MaximizeBox     = true;
                form.MinimizeBox     = false;
                form.BackColor       = Color.FromArgb(245, 246, 248);

                // ── Dark header (Dock=Top, always visible) ────────────────────
                var header = new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 56,
                    BackColor = Color.FromArgb(18, 18, 18)
                };
                var headerTitle = new Label
                {
                    Text      = "BIM AI Assistant",
                    Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Left      = 20,
                    Top       = 14,
                    AutoSize  = true
                };
                var headerSub = new Label
                {
                    Text      = "A few more details are needed",
                    Font      = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(170, 170, 170),
                    Left      = 22,
                    Top       = 36,
                    AutoSize  = true
                };
                header.Controls.Add(headerTitle);
                header.Controls.Add(headerSub);

                // ── Footer panel (Dock=Bottom) — buttons always visible ───────
                var footer = new Panel
                {
                    Dock      = DockStyle.Bottom,
                    Height    = 68,
                    BackColor = Color.FromArgb(245, 246, 248)
                };

                var footerDivider = new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 1,
                    BackColor = Color.FromArgb(220, 220, 220)
                };

                var cancelBtn = new Button
                {
                    Text      = "Cancel",
                    Width     = 90,
                    Height    = 36,
                    Top       = 16,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Cursor    = Cursors.Hand,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = DialogResult.Cancel
                };
                cancelBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                cancelBtn.FlatAppearance.BorderSize  = 1;

                var confirmBtn = new Button
                {
                    Text      = "✓  Confirm",
                    Width     = 120,
                    Height    = 36,
                    Top       = 16,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(18, 18, 18),
                    ForeColor = Color.White,
                    Cursor    = Cursors.Hand,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = DialogResult.OK
                };
                confirmBtn.FlatAppearance.BorderSize = 0;

                // Right-align footer buttons, update on resize
                footer.Resize += (s, e) =>
                {
                    confirmBtn.Left = footer.ClientSize.Width - Pad - 120;
                    cancelBtn.Left  = footer.ClientSize.Width - Pad - 120 - 8 - 90;
                };

                footer.Controls.Add(footerDivider);
                footer.Controls.AddRange(new Control[] { cancelBtn, confirmBtn });

                // ── Scroll panel (fills space between header and footer) ───────
                var scroll = new Panel
                {
                    Dock          = DockStyle.Fill,
                    AutoScroll    = true,
                    BackColor     = Color.FromArgb(245, 246, 248),
                    Padding       = new Padding(0)
                };

                // Inner panel holds all question rows; wider than scroll so padding works
                var inner = new Panel
                {
                    AutoSize     = false,
                    BackColor    = Color.FromArgb(245, 246, 248),
                    Left         = 0,
                    Top          = 0
                };

                // Build question rows inside inner
                int y = 24; // top padding inside scroll area

                foreach (var q in questions)
                {
                    // Question label
                    var lbl = new Label
                    {
                        Text      = q.Question,
                        Left      = Pad,
                        Top       = y,
                        Height    = LblH,
                        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(50, 50, 50),
                        AutoSize  = false,
                        Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    inner.Controls.Add(lbl);
                    y += LblH + 8;

                    string defaultStr = q.Default?.ToString() ?? "";
                    Control input;

                    if (q.Type == "choice" && q.Choices != null && q.Choices.Count > 0)
                    {
                        var combo = new ComboBox
                        {
                            Left          = Pad,
                            Top           = y,
                            Height        = InpH,
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Font          = new Font("Segoe UI", 10f),
                            BackColor     = Color.White,
                            FlatStyle     = FlatStyle.Flat,
                            Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                        };
                        foreach (string choice in q.Choices)
                            combo.Items.Add(choice);
                        int idx = combo.Items.IndexOf(defaultStr);
                        combo.SelectedIndex = idx >= 0 ? idx : 0;
                        input = combo;
                    }
                    else
                    {
                        var tb = new TextBox
                        {
                            Left        = Pad,
                            Top         = y,
                            Height      = InpH,
                            Text        = defaultStr,
                            Font        = new Font("Segoe UI", 10f),
                            BackColor   = Color.White,
                            ForeColor   = Color.FromArgb(20, 20, 20),
                            BorderStyle = BorderStyle.FixedSingle,
                            Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                        };
                        input = tb;
                    }

                    inner.Controls.Add(input);
                    inputs.Add((q.Id, input, q.Type ?? "text"));
                    y += InpH + Gap;
                }

                y += 16; // bottom padding
                inner.Height = y;

                // Keep inner width and all anchored controls in sync with scroll panel width
                scroll.Resize += (s, e) =>
                {
                    inner.Width = scroll.ClientSize.Width;
                    foreach (Control c in inner.Controls)
                    {
                        if ((c.Anchor & AnchorStyles.Right) != 0)
                            c.Width = inner.ClientSize.Width - Pad * 2;
                    }
                };

                scroll.Controls.Add(inner);

                // ── Assemble (order matters for docking) ──────────────────────
                // header Dock=Top, footer Dock=Bottom, scroll Dock=Fill
                form.Controls.Add(scroll);   // Fill — added first
                form.Controls.Add(footer);   // Bottom
                form.Controls.Add(header);   // Top — painted last, always on top

                form.AcceptButton = confirmBtn;
                form.CancelButton = cancelBtn;

                form.Load += (s, e) =>
                {
                    // Force initial layout
                    inner.Width = scroll.ClientSize.Width;
                    foreach (Control c in inner.Controls)
                    {
                        if ((c.Anchor & AnchorStyles.Right) != 0)
                            c.Width = inner.ClientSize.Width - Pad * 2;
                    }
                    confirmBtn.Left = footer.ClientSize.Width - Pad - 120;
                    cancelBtn.Left  = footer.ClientSize.Width - Pad - 120 - 8 - 90;
                };

                if (form.ShowDialog() != DialogResult.OK)
                    return null;

                // ── Collect answers ───────────────────────────────────────────
                var answers = new Dictionary<string, object>();
                foreach (var (id, control, type) in inputs)
                {
                    string raw = control is ComboBox cb
                        ? cb.SelectedItem?.ToString() ?? ""
                        : ((TextBox)control).Text.Trim();

                    if (type == "number" &&
                        double.TryParse(raw, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double d))
                        answers[id] = d;
                    else
                        answers[id] = raw;
                }
                return answers;
            }
        }
    }
}
