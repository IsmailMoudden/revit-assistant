using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BimAiAssistant.UI
{
    /// <summary>
    /// Main BIM AI dialog.
    /// Shows instruction input, loading state, last action result, and a simple history list.
    /// Returns null if the user cancels without submitting.
    /// </summary>
    public static class InputDialog
    {
        // In-process history — persists across invocations within the same Revit session
        private static readonly List<string> _history = new List<string>();

        public static string Show()
        {
            string submitted = null;

            using (var form = new Form())
            {
                // ── Window ────────────────────────────────────────────────────
                form.Text            = "BIM AI Assistant";
                form.Width           = 520;
                form.Height          = 340;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition   = FormStartPosition.CenterScreen;
                form.MaximizeBox     = false;
                form.MinimizeBox     = false;
                form.BackColor       = Color.White;

                // ── "Describe what you want to create" label ──────────────────
                var titleLabel = new Label
                {
                    Text      = "Describe what you want to create",
                    Left      = 16,
                    Top       = 16,
                    Width     = 472,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(40, 40, 40)
                };

                // ── Instruction textbox with placeholder ──────────────────────
                var textBox = new TextBox
                {
                    Left      = 16,
                    Top       = 40,
                    Width     = 472,
                    Height    = 24,
                    Font      = new Font("Segoe UI", 9f),
                    ForeColor = Color.Gray,
                    Text      = "e.g. add 3 windows evenly spaced"
                };

                // Placeholder behaviour
                textBox.GotFocus += (s, e) =>
                {
                    if (textBox.ForeColor == Color.Gray)
                    {
                        textBox.Text      = "";
                        textBox.ForeColor = Color.Black;
                    }
                };
                textBox.LostFocus += (s, e) =>
                {
                    if (textBox.Text.Trim().Length == 0)
                    {
                        textBox.Text      = "e.g. add 3 windows evenly spaced";
                        textBox.ForeColor = Color.Gray;
                    }
                };

                // ── Run AI button ─────────────────────────────────────────────
                var runButton = new Button
                {
                    Text      = "Run AI",
                    Left      = 352,
                    Top       = 76,
                    Width     = 136,
                    Height    = 32,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                runButton.FlatAppearance.BorderSize = 0;

                // ── Cancel button ─────────────────────────────────────────────
                var cancelButton = new Button
                {
                    Text         = "Cancel",
                    Left         = 256,
                    Top          = 76,
                    Width        = 88,
                    Height       = 32,
                    FlatStyle    = FlatStyle.Flat,
                    Font         = new Font("Segoe UI", 9f),
                    DialogResult = DialogResult.Cancel,
                    Cursor       = Cursors.Hand
                };

                // ── Status label (loading / result) ───────────────────────────
                var statusLabel = new Label
                {
                    Left      = 16,
                    Top       = 80,
                    Width     = 230,
                    Height    = 24,
                    Text      = "",
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    ForeColor = Color.Gray
                };

                // ── History label ─────────────────────────────────────────────
                var historyTitle = new Label
                {
                    Text      = "Conversation history:",
                    Left      = 16,
                    Top       = 118,
                    Width     = 360,
                    Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 100, 100)
                };

                var clearBtn = new Button
                {
                    Text      = "Clear",
                    Left      = 388,
                    Top       = 114,
                    Width     = 100,
                    Height    = 22,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 7.5f),
                    Cursor    = Cursors.Hand
                };

                var historyBox = new ListBox
                {
                    Left          = 16,
                    Top           = 140,
                    Width         = 472,
                    Height        = 120,
                    Font          = new Font("Segoe UI", 8.5f),
                    BorderStyle   = BorderStyle.FixedSingle,
                    SelectionMode = SelectionMode.None
                };
                RefreshHistory(historyBox);

                clearBtn.Click += (s, e) =>
                {
                    RunAiCommand.ClearHistory();
                    _history.Clear();
                    RefreshHistory(historyBox);
                };

                // ── Run click handler ─────────────────────────────────────────
                runButton.Click += (s, e) =>
                {
                    string instruction = textBox.ForeColor == Color.Gray
                        ? ""
                        : textBox.Text.Trim();

                    if (instruction.Length == 0)
                    {
                        statusLabel.ForeColor = Color.Crimson;
                        statusLabel.Text      = "Please enter an instruction.";
                        return;
                    }

                    // Loading state
                    runButton.Text    = "Running...";
                    runButton.Enabled = false;
                    statusLabel.ForeColor = Color.Gray;
                    statusLabel.Text  = "Contacting AI backend...";
                    form.Update();

                    submitted = instruction;
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                form.Controls.AddRange(new Control[]
                {
                    titleLabel, textBox, runButton, cancelButton,
                    statusLabel, historyTitle, clearBtn, historyBox
                });

                form.AcceptButton = runButton;
                form.CancelButton = cancelButton;

                form.ShowDialog();
            }

            return submitted;
        }

        /// <summary>
        /// Call this after a successful action to record it in history.
        /// </summary>
        public static void RecordAction(string instruction, string actionType)
        {
            string entry = $"[{DateTime.Now:HH:mm}]  {actionType}  —  {instruction}";
            _history.Insert(0, entry);
            if (_history.Count > 10) _history.RemoveAt(_history.Count - 1);
        }

        private static void RefreshHistory(ListBox box)
        {
            box.Items.Clear();
            if (_history.Count == 0)
            {
                box.Items.Add("No actions yet this session.");
                return;
            }
            foreach (string entry in _history)
                box.Items.Add(entry);
        }
    }
}
