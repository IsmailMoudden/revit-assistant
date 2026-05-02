using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BimAiAssistant.Models;

namespace BimAiAssistant.UI
{
    /// <summary>
    /// Dynamically builds a WinForms dialog from a list of clarification questions.
    /// Supports question types: "number", "text", "choice" (dropdown).
    /// Returns null if the user cancels.
    /// </summary>
    public static class ClarificationDialog
    {
        private const int LabelH   = 18;
        private const int InputH   = 26;
        private const int Margin   = 12;
        private const int Pad      = 8;
        private const int FormW    = 460;

        public static Dictionary<string, object> Show(List<ClarificationQuestion> questions)
        {
            // Collect one Control per question so we can read values on submit
            var inputs = new List<(string id, Control control)>();

            int y = Margin + 30; // top offset — leaves room for the header label

            using (var form = new Form())
            {
                form.Text            = "BIM AI — More information needed";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition   = FormStartPosition.CenterScreen;
                form.MaximizeBox     = false;
                form.MinimizeBox     = false;
                form.BackColor       = Color.White;
                form.Width           = FormW;

                // ── Header ────────────────────────────────────────────────────
                var header = new Label
                {
                    Text      = "The AI needs a few more details:",
                    Left      = Margin,
                    Top       = Margin,
                    Width     = FormW - Margin * 2,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(40, 40, 40)
                };
                form.Controls.Add(header);

                // ── One row per question ───────────────────────────────────────
                foreach (var q in questions)
                {
                    // Question label
                    var lbl = new Label
                    {
                        Text      = q.Question,
                        Left      = Margin,
                        Top       = y,
                        Width     = FormW - Margin * 2,
                        Height    = LabelH,
                        Font      = new Font("Segoe UI", 9f),
                        ForeColor = Color.FromArgb(60, 60, 60)
                    };
                    form.Controls.Add(lbl);
                    y += LabelH + Pad / 2;

                    string defaultStr = q.Default?.ToString() ?? "";

                    Control input;

                    if (q.Type == "choice" && q.Choices != null && q.Choices.Count > 0)
                    {
                        var combo = new ComboBox
                        {
                            Left          = Margin,
                            Top           = y,
                            Width         = FormW - Margin * 2,
                            Height        = InputH,
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Font          = new Font("Segoe UI", 9f)
                        };
                        foreach (string choice in q.Choices)
                            combo.Items.Add(choice);

                        // Pre-select default
                        int idx = combo.Items.IndexOf(defaultStr);
                        combo.SelectedIndex = idx >= 0 ? idx : 0;

                        input = combo;
                    }
                    else
                    {
                        var tb = new TextBox
                        {
                            Left  = Margin,
                            Top   = y,
                            Width = FormW - Margin * 2,
                            Height = InputH,
                            Text  = defaultStr,
                            Font  = new Font("Segoe UI", 9f)
                        };
                        input = tb;
                    }

                    form.Controls.Add(input);
                    inputs.Add((q.Id, input));
                    y += InputH + Margin;
                }

                // ── Buttons ───────────────────────────────────────────────────
                var confirmBtn = new Button
                {
                    Text      = "Confirm",
                    Left      = FormW - Margin - 200 - Pad - 90,
                    Top       = y,
                    Width     = 90,
                    Height    = 32,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    DialogResult = DialogResult.OK
                };
                confirmBtn.FlatAppearance.BorderSize = 0;

                var cancelBtn = new Button
                {
                    Text         = "Cancel",
                    Left         = FormW - Margin - 90,
                    Top          = y,
                    Width        = 90,
                    Height       = 32,
                    FlatStyle    = FlatStyle.Flat,
                    Font         = new Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel
                };

                form.Controls.AddRange(new Control[] { confirmBtn, cancelBtn });
                form.Height       = y + 32 + Margin * 3;
                form.AcceptButton = confirmBtn;
                form.CancelButton = cancelBtn;

                if (form.ShowDialog() != DialogResult.OK)
                    return null;

                // ── Collect answers ───────────────────────────────────────────
                var answers = new Dictionary<string, object>();

                foreach (var (id, control) in inputs)
                {
                    string raw = control is ComboBox cb
                        ? cb.SelectedItem?.ToString() ?? ""
                        : ((TextBox)control).Text.Trim();

                    // Find matching question to know expected type
                    ClarificationQuestion q = questions.Find(x => x.Id == id);

                    if (q?.Type == "number")
                    {
                        if (double.TryParse(raw, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out double d))
                            answers[id] = d;
                        else
                            answers[id] = raw; // pass as string — backend will handle
                    }
                    else
                    {
                        answers[id] = raw;
                    }
                }

                return answers;
            }
        }
    }
}
