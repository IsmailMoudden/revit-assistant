using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BimAiAssistant.Models;

namespace BimAiAssistant.UI
{
    public static class ClarificationDialog
    {
        private const int FormW   = 520;
        private const int Pad     = 24;   // horizontal margin
        private const int RowGap  = 20;   // vertical gap between questions

        public static Dictionary<string, object> Show(List<ClarificationQuestion> questions)
        {
            var inputs = new List<(string id, Control control, string type)>();

            using (var form = new Form())
            {
                // ── Window ────────────────────────────────────────────────────
                form.Text            = "BIM AI Assistant";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition   = FormStartPosition.CenterScreen;
                form.MaximizeBox     = false;
                form.MinimizeBox     = false;
                form.BackColor       = Color.FromArgb(245, 246, 248);
                form.Width           = FormW;

                // ── Dark header ───────────────────────────────────────────────
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

                // ── Body (dynamic height, added after controls so we know total height) ─
                // We'll position everything absolutely, starting below the header (56px).
                // Controls are added directly to form for simplicity with FixedDialog.

                int y = 56 + 20; // below header + top padding

                // ── One row per question ──────────────────────────────────────
                foreach (var q in questions)
                {
                    var lbl = new Label
                    {
                        Text      = q.Question,
                        Left      = Pad,
                        Top       = y,
                        Width     = FormW - Pad * 2 - 16,
                        Height    = 18,
                        Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(60, 60, 60),
                        AutoSize  = false
                    };
                    form.Controls.Add(lbl);
                    y += 22;

                    string defaultStr = q.Default?.ToString() ?? "";
                    Control input;

                    if (q.Type == "choice" && q.Choices != null && q.Choices.Count > 0)
                    {
                        var combo = new ComboBox
                        {
                            Left          = Pad,
                            Top           = y,
                            Width         = FormW - Pad * 2 - 16,
                            Height        = 28,
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Font          = new Font("Segoe UI", 9.5f),
                            BackColor     = Color.White,
                            FlatStyle     = FlatStyle.Flat
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
                            Width       = FormW - Pad * 2 - 16,
                            Height      = 28,
                            Text        = defaultStr,
                            Font        = new Font("Segoe UI", 9.5f),
                            BackColor   = Color.White,
                            ForeColor   = Color.FromArgb(20, 20, 20),
                            BorderStyle = BorderStyle.FixedSingle
                        };
                        input = tb;
                    }

                    form.Controls.Add(input);
                    inputs.Add((q.Id, input, q.Type ?? "text"));
                    y += 28 + RowGap;
                }

                // ── Divider ───────────────────────────────────────────────────
                var divider = new Panel
                {
                    Left      = Pad,
                    Top       = y,
                    Width     = FormW - Pad * 2 - 16,
                    Height    = 1,
                    BackColor = Color.FromArgb(220, 220, 220)
                };
                form.Controls.Add(divider);
                y += 16;

                // ── Buttons ───────────────────────────────────────────────────
                var cancelBtn = new Button
                {
                    Text      = "Cancel",
                    Left      = FormW - Pad - 16 - 90 - 8 - 120,
                    Top       = y,
                    Width     = 90,
                    Height    = 34,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Cursor    = Cursors.Hand,
                    DialogResult = DialogResult.Cancel
                };
                cancelBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                cancelBtn.FlatAppearance.BorderSize  = 1;

                var confirmBtn = new Button
                {
                    Text      = "✓  Confirm",
                    Left      = FormW - Pad - 16 - 120,
                    Top       = y,
                    Width     = 120,
                    Height    = 34,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(18, 18, 18),
                    ForeColor = Color.White,
                    Cursor    = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                confirmBtn.FlatAppearance.BorderSize = 0;

                form.Controls.AddRange(new Control[] { divider, cancelBtn, confirmBtn });
                y += 34 + 20;

                form.Height       = y;
                form.AcceptButton = confirmBtn;
                form.CancelButton = cancelBtn;

                // Header is added last so it paints on top
                form.Controls.Add(header);

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
