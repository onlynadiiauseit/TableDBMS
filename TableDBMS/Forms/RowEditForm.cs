using TableDBMS.Models;

namespace TableDBMS.Forms
{
    public class RowEditForm : Form
    {
        private readonly Table _table;
        private readonly List<TextBox> _textBoxes = new();

        public Row? ResultRow { get; private set; }

        public RowEditForm(
            Table table,
            Row? existingRow = null)
        {
            _table = table;

            Text = existingRow == null
                ? "Додати рядок"
                : "Редагувати рядок";

            Width = 500;
            Height = Math.Min(
                250 + table.Schema.Columns.Count * 60,
                700
            );

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            BuildInterface(existingRow);
        }

        private void BuildInterface(
            Row? existingRow)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            Controls.Add(panel);

            int top = 15;

            for (int i = 0;
                 i < _table.Schema.Columns.Count;
                 i++)
            {
                Column column =
                    _table.Schema.Columns[i];

                var label = new Label
                {
                    Left = 20,
                    Top = top,
                    Width = 420,

                    Text =
                        $"{column.Name} " +
                        $"({column.DataTypeName})"
                };

                var textBox = new TextBox
                {
                    Left = 20,
                    Top = top + 22,
                    Width = 420
                };

                if (existingRow != null &&
                    i < existingRow.Values.Count)
                {
                    textBox.Text =
                        existingRow.Values[i] ?? "";
                }

                _textBoxes.Add(textBox);

                panel.Controls.Add(label);
                panel.Controls.Add(textBox);

                top += 60;
            }

            var saveButton = new Button
            {
                Text = "Зберегти",
                Left = 250,
                Top = top + 10,
                Width = 90,
                Height = 32
            };

            saveButton.Click += (_, _) =>
                SaveRow();

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 350,
                Top = top + 10,
                Width = 90,
                Height = 32,

                DialogResult =
                    DialogResult.Cancel
            };

            panel.Controls.Add(saveButton);
            panel.Controls.Add(cancelButton);

            CancelButton = cancelButton;
        }

        private void SaveRow()
        {
            var values =
                _textBoxes
                    .Select(
                        textBox =>
                            (string?)textBox.Text
                    )
                    .ToList();

            for (int i = 0;
                 i < _table.Schema.Columns.Count;
                 i++)
            {
                Column column =
                    _table.Schema.Columns[i];

                string? value = values[i];

                if (!column.IsValid(value))
                {
                    MessageBox.Show(
                        $"Значення '{value}' не відповідає " +
                        $"типу {column.DataTypeName} " +
                        $"для поля '{column.Name}'.",
                        "Некоректне значення",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    _textBoxes[i].Focus();

                    return;
                }
            }

            ResultRow = new Row(values);

            DialogResult = DialogResult.OK;

            Close();
        }
    }
}