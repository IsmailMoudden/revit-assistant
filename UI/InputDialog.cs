using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BimAiAssistant.UI
{
    public static class InputDialog
    {
        private static readonly List<string> _history = new List<string>();

        public static string Show()
        {
            string submitted = null;

            using (var form = new Form())
            {
                // ── Window ─────────────────────────────────────────────────────
                form.Text            = "BIM AI Assistant";
                form.MinimumSize     = new Size(560, 460);
                form.Size            = new Size(640, 520);
                form.FormBorderStyle = FormBorderStyle.Sizable;          // resizable
                form.StartPosition   = FormStartPosition.CenterScreen;
                form.MaximizeBox     = true;
                form.MinimizeBox     = false;
                form.BackColor       = Color.FromArgb(245, 246, 248);    // light grey background

                // ── Header panel (dark bar at top) ────────────────────────────
                var header = new Panel
                {
                    Dock      = DockStyle.Top,
                    Height    = 56,
                    BackColor = Color.FromArgb(18, 18, 18)
                };

                var appTitle = new Label
                {
                    Text      = "BIM AI Assistant",
                    Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Left      = 20,
                    Top       = 14,
                    AutoSize  = true
                };
                var appSub = new Label
                {
                    Text      = "Describe your instruction below",
                    Font      = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(170, 170, 170),
                    Left      = 22,
                    Top       = 36,
                    AutoSize  = true
                };
                header.Controls.Add(appTitle);
                header.Controls.Add(appSub);

                // ── Body panel (fills remaining space, no WinForms padding — we use absolute offsets) ──
                var body = new Panel { Dock = DockStyle.Fill };

                // ── Instruction label ─────────────────────────────────────────
                // Top=20 gives clear breathing room below the dark header
                var instructionLabel = new Label
                {
                    Text      = "Instruction",
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Left      = 24,
                    Top       = 20,
                    AutoSize  = true
                };

                // ── Multi-line instruction textbox ─────────────────────────────
                const string placeholder = "e.g.  create a 6×4 frame with HEA240 columns on Level 2";
                var textBox = new TextBox
                {
                    Left        = 24,
                    Top         = 42,
                    Height      = 72,
                    Multiline   = true,
                    ScrollBars  = ScrollBars.Vertical,
                    Font        = new Font("Segoe UI", 10f),
                    ForeColor   = Color.FromArgb(140, 140, 140),
                    BackColor   = Color.White,
                    Text        = placeholder,
                    BorderStyle = BorderStyle.FixedSingle,
                    Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                body.Resize += (s, e) => textBox.Width = body.ClientSize.Width - 48;

                textBox.GotFocus += (s, e) =>
                {
                    if (textBox.ForeColor == Color.FromArgb(140, 140, 140))
                    { textBox.Text = ""; textBox.ForeColor = Color.FromArgb(20, 20, 20); }
                };
                textBox.LostFocus += (s, e) =>
                {
                    if (textBox.Text.Trim().Length == 0)
                    { textBox.Text = placeholder; textBox.ForeColor = Color.FromArgb(140, 140, 140); }
                };

                // ── Status label (sits between textbox and buttons) ───────────
                // textBox bottom = 42+72 = 114; status at 122 leaves 8px gap
                var statusLabel = new Label
                {
                    Text      = "",
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Left      = 24,
                    Top       = 122,
                    Height    = 18,
                    AutoSize  = false,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                body.Resize += (s, e) => statusLabel.Width = body.ClientSize.Width - 48 - 230;

                // ── Button row ────────────────────────────────────────────────
                // status label at 122 h=18 → buttons at 142
                var cancelButton = new Button
                {
                    Text      = "Cancel",
                    Width     = 90,
                    Height    = 34,
                    Top       = 140,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 60),
                    Cursor    = Cursors.Hand,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                cancelButton.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                cancelButton.FlatAppearance.BorderSize  = 1;
                cancelButton.DialogResult = DialogResult.Cancel;

                var runButton = new Button
                {
                    Text      = "⚡  Run AI",
                    Width     = 120,
                    Height    = 34,
                    Top       = 140,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(18, 18, 18),
                    ForeColor = Color.White,
                    Cursor    = Cursors.Hand,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                runButton.FlatAppearance.BorderSize = 0;

                // Right-align both buttons; updated on every resize
                body.Resize += (s, e) =>
                {
                    runButton.Left    = body.ClientSize.Width - 24 - 120;
                    cancelButton.Left = body.ClientSize.Width - 24 - 120 - 98;
                };

                // ── Divider ───────────────────────────────────────────────────
                // buttons bottom = 140+34 = 174; divider 12px below → 186
                var divider = new Panel
                {
                    Left      = 24,
                    Top       = 186,
                    Height    = 1,
                    BackColor = Color.FromArgb(220, 220, 220),
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                body.Resize += (s, e) => divider.Width = body.ClientSize.Width - 48;

                // ── History header row ────────────────────────────────────────
                var historyLabel = new Label
                {
                    Text      = "Conversation history",
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Left      = 24,
                    Top       = 198,
                    AutoSize  = true
                };

                var clearBtn = new Button
                {
                    Text      = "Clear",
                    Width     = 64,
                    Height    = 24,
                    Top       = 195,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 8f),
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Cursor    = Cursors.Hand,
                    Anchor    = AnchorStyles.Top | AnchorStyles.Right
                };
                clearBtn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
                clearBtn.FlatAppearance.BorderSize  = 1;
                body.Resize += (s, e) => clearBtn.Left = body.ClientSize.Width - 24 - 64;

                // ── History listbox ───────────────────────────────────────────
                // history header at 198 h~20 → listbox at 226
                var historyBox = new ListBox
                {
                    Left          = 24,
                    Top           = 226,
                    Font          = new Font("Segoe UI", 8.5f),
                    BackColor     = Color.White,
                    BorderStyle   = BorderStyle.FixedSingle,
                    SelectionMode = SelectionMode.None,
                    HorizontalScrollbar = true,
                    Anchor        = AnchorStyles.Top | AnchorStyles.Bottom |
                                    AnchorStyles.Left | AnchorStyles.Right
                };
                body.Resize += (s, e) =>
                {
                    historyBox.Width  = body.ClientSize.Width - 48;
                    historyBox.Height = Math.Max(40, body.ClientSize.Height - 226 - 12);
                };
                RefreshHistory(historyBox);

                // ── Wire up handlers ──────────────────────────────────────────
                clearBtn.Click += (s, e) =>
                {
                    RunAiCommand.ClearHistory();
                    _history.Clear();
                    RefreshHistory(historyBox);
                };

                runButton.Click += (s, e) =>
                {
                    string instruction = textBox.ForeColor == Color.FromArgb(140, 140, 140)
                        ? ""
                        : textBox.Text.Trim();

                    if (instruction.Length == 0)
                    {
                        statusLabel.ForeColor = Color.Crimson;
                        statusLabel.Text      = "Please enter an instruction.";
                        return;
                    }

                    runButton.Text    = "Running...";
                    runButton.Enabled = false;
                    statusLabel.ForeColor = Color.FromArgb(120, 120, 120);
                    statusLabel.Text      = "Contacting AI backend...";
                    form.Update();

                    submitted         = instruction;
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                // ── Assemble ──────────────────────────────────────────────────
                body.Controls.AddRange(new Control[]
                {
                    instructionLabel, textBox,
                    statusLabel, cancelButton, runButton,
                    divider, historyLabel, clearBtn, historyBox
                });

                form.Controls.Add(body);
                form.Controls.Add(header);   // add after body so it paints on top

                form.AcceptButton = runButton;
                form.CancelButton = cancelButton;

                // Trigger initial layout to set all widths / positions
                form.Load += (s, e) =>
                {
                    int w = body.ClientSize.Width;
                    int h = body.ClientSize.Height;
                    textBox.Width      = w - 48;
                    statusLabel.Width  = w - 48 - 230;
                    divider.Width      = w - 48;
                    runButton.Left     = w - 24 - 120;
                    cancelButton.Left  = w - 24 - 120 - 98;
                    clearBtn.Left      = w - 24 - 64;
                    historyBox.Width   = w - 48;
                    historyBox.Height  = Math.Max(40, h - 226 - 12);
                };

                form.ShowDialog();
            }

            return submitted;
        }

        public static void RecordAction(string instruction, string summary)
        {
            string entry = $"[{DateTime.Now:HH:mm}]  {summary}  —  {instruction}";
            _history.Insert(0, entry);
            if (_history.Count > 20) _history.RemoveAt(_history.Count - 1);
        }

        private static void RefreshHistory(ListBox box)
        {
            box.Items.Clear();
            if (_history.Count == 0)
            { box.Items.Add("No actions yet this session."); return; }
            foreach (string e in _history)
                box.Items.Add(e);
        }
    }
}
