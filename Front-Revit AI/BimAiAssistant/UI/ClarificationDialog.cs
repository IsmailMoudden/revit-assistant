using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BimAiAssistant.Models;

namespace BimAiAssistant.UI
{
    public static class ClarificationDialog
    {
        private const int Pad   = 28;  // horizontal margin
        private const int InpH  = 32;  // input control height
        private const int Gap   = 32;  // vertical gap between questions

        public static Dictionary<string, object> Show(List<ClarificationQuestion> questions)
        {
            var inputs = new List<(string id, Control control, string type)>();

            using (var form = new Form())
            {
                // ── Window ────────────────────────────────────────────────────
                form.Text            = "BIM AI Assistant";
                form.MinimumSize     = new Size(480, 380);
                form.Size            = new Size(560, 500);
                form.FormBorderStyle = FormBorderStyle.Sizable;
                form.StartPosition   = FormStartPosition.CenterScreen;
                form.MaximizeBox     = true;
                form.MinimizeBox     = false;
                form.BackColor       = Color.FromArgb(245, 246, 248);

                // ── Dark header (Dock=Top) ────────────────────────────────────
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
                    Text      = "A few more details are needed",
                    Font      = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(170, 170, 170),
                    Left = 22, Top = 36, AutoSize = true
                });

                // ── Footer panel (Dock=Bottom) ────────────────────────────────
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

                var cancelBtn = new Button
                {
                    Text         = "Cancel",
                    Width        = 96,
                    Height       = 36,
                    Top          = 16,
                    FlatStyle    = FlatStyle.Flat,
                    Font         = new Font("Segoe UI", 9f),
                    BackColor    = Color.White,
                    ForeColor    = Color.FromArgb(60, 60, 60),
                    Cursor       = Cursors.Hand,
                    Anchor       = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = DialogResult.Cancel
                };
                cancelBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                cancelBtn.FlatAppearance.BorderSize  = 1;

                var confirmBtn = new Button
                {
                    Text         = "Confirm",
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
                confirmBtn.FlatAppearance.BorderSize = 0;

                var defaultsBtn = new Button
                {
                    Text      = "Use defaults",
                    Width     = 110,
                    Height    = 36,
                    Top       = 16,
                    Left      = Pad,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Cursor    = Cursors.Hand,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left
                };
                defaultsBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                defaultsBtn.FlatAppearance.BorderSize  = 1;

                footer.Resize += (s, e) =>
                {
                    confirmBtn.Left = footer.ClientSize.Width - Pad - 110;
                    cancelBtn.Left  = footer.ClientSize.Width - Pad - 110 - 10 - 96;
                };
                footer.Controls.AddRange(new Control[] { defaultsBtn, cancelBtn, confirmBtn });

                // "Use defaults" resets every input to its question's default value then confirms
                defaultsBtn.Click += (s, e) =>
                {
                    foreach (var (id, control, type) in inputs)
                    {
                        ClarificationQuestion q = questions.Find(x => x.Id == id);
                        string def = q?.Default?.ToString() ?? "";
                        if (control is ComboBox cb)
                        {
                            int idx = cb.Items.IndexOf(def);
                            cb.SelectedIndex = idx >= 0 ? idx : 0;
                        }
                        else if (control is TextBox tb)
                        {
                            tb.Text = def;
                        }
                    }
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                // ── Scrollable body ───────────────────────────────────────────
                var scroll = new Panel
                {
                    Dock       = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor  = Color.FromArgb(245, 246, 248)
                };

                // inner holds question rows; resizes with scroll panel
                var inner = new Panel
                {
                    Left      = 0,
                    Top       = 0,
                    BackColor = Color.FromArgb(245, 246, 248)
                };

                int y = 28; // top padding

                foreach (var q in questions)
                {
                    // Label — auto-height so long questions never get clipped
                    var lbl = new Label
                    {
                        Text      = q.Question,
                        Left      = Pad,
                        Top       = y,
                        Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(40, 40, 40),
                        AutoSize  = false,
                        // Width set in resize handler; Height measured below
                    };
                    inner.Controls.Add(lbl);

                    // Measure label height for the current width so wrapping is correct.
                    // We'll update Width + reflow in the resize handler; for now use a
                    // generous single-line height — the resize will recalculate.
                    int lblMeasuredH = 22;
                    lbl.Height = lblMeasuredH;
                    y += lblMeasuredH + 8;

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
                        input = new TextBox
                        {
                            Left        = Pad,
                            Top         = y,
                            Height      = InpH,
                            Text        = defaultStr,
                            Font        = new Font("Segoe UI", 10f),
                            BackColor   = Color.White,
                            ForeColor   = Color.FromArgb(20, 20, 20),
                            BorderStyle = BorderStyle.FixedSingle
                        };
                    }

                    inner.Controls.Add(input);
                    inputs.Add((q.Id, input, q.Type ?? "text"));
                    y += InpH + Gap;
                }

                y += 12; // bottom padding
                inner.Height = y;

                void RelayoutInner()
                {
                    int availW = scroll.ClientSize.Width - Pad * 2;
                    if (availW < 10) return;

                    int newY = 28;
                    for (int i = 0; i < inner.Controls.Count; i += 2)
                    {
                        if (i + 1 >= inner.Controls.Count) break;
                        var lbl   = inner.Controls[i] as Label;
                        var input = inner.Controls[i + 1];
                        if (lbl == null || input == null) break;

                        lbl.Width = availW;
                        lbl.Top   = newY;

                        Size sz = TextRenderer.MeasureText(
                            lbl.Text, lbl.Font,
                            new Size(availW, int.MaxValue),
                            TextFormatFlags.WordBreak);
                        lbl.Height = sz.Height + 4;
                        newY += lbl.Height + 8;

                        input.Width = availW;
                        input.Top   = newY;
                        newY += input.Height + Gap;
                    }

                    newY += 12;
                    inner.Height = newY;
                    inner.Width  = scroll.ClientSize.Width;
                }

                scroll.Resize += (s, e) => RelayoutInner();
                scroll.Controls.Add(inner);

                // ── Assemble (dock order matters) ─────────────────────────────
                form.Controls.Add(scroll);   // Fill — first
                form.Controls.Add(footer);   // Bottom
                form.Controls.Add(header);   // Top — painted on top

                form.AcceptButton = confirmBtn;
                form.CancelButton = cancelBtn;

                form.Load += (s, e) =>
                {
                    RelayoutInner();
                    confirmBtn.Left = footer.ClientSize.Width - Pad - 110;
                    cancelBtn.Left  = footer.ClientSize.Width - Pad - 110 - 10 - 96;
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
                        double.TryParse(raw,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double d))
                        answers[id] = d;
                    else
                        answers[id] = raw;
                }
                return answers;
            }
        }
    }
}
