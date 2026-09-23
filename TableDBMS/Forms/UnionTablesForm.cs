using TableDBMS.Models;

namespace TableDBMS.Forms
{
    public class UnionTablesForm : Form
    {
        private readonly Database _database;

        private readonly ComboBox _firstTableComboBox;
        private readonly ComboBox _secondTableComboBox;
        private readonly TextBox _resultNameTextBox;

        public Table? FirstTable { get; private set; }
        public Table? SecondTable { get; private set; }
        public string ResultTableName { get; private set; } = string.Empty;

        public UnionTablesForm(Database database)
        {
            _database = database;

            Text = "Об'єднання таблиць";
            Width = 500;
            Height = 330;

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            var firstLabel = new Label
            {
                Text = "Перша таблиця:",
                Left = 25,
                Top = 25,
                Width = 200
            };

            _firstTableComboBox = new ComboBox
            {
                Left = 25,
                Top = 50,
                Width = 420,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var secondLabel = new Label
            {
                Text = "Друга таблиця:",
                Left = 25,
                Top = 90,
                Width = 200
            };

            _secondTableComboBox = new ComboBox
            {
                Left = 25,
                Top = 115,
                Width = 420,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var resultLabel = new Label
            {
                Text = "Назва результуючої таблиці:",
                Left = 25,
                Top = 155,
                Width = 250
            };

            _resultNameTextBox = new TextBox
            {
                Left = 25,
                Top = 180,
                Width = 420,
                Text = "UnionResult"
            };

            var unionButton = new Button
            {
                Text = "Об'єднати",
                Left = 245,
                Top = 225,
                Width = 95,
                Height = 32
            };

            unionButton.Click += (_, _) =>
                ConfirmUnion();

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 350,
                Top = 225,
                Width = 95,
                Height = 32,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(firstLabel);
            Controls.Add(_firstTableComboBox);

            Controls.Add(secondLabel);
            Controls.Add(_secondTableComboBox);

            Controls.Add(resultLabel);
            Controls.Add(_resultNameTextBox);

            Controls.Add(unionButton);
            Controls.Add(cancelButton);

            CancelButton = cancelButton;

            LoadTables();
        }

        private void LoadTables()
        {
            _firstTableComboBox.Items.Clear();
            _secondTableComboBox.Items.Clear();

            foreach (Table table in _database.Tables)
            {
                _firstTableComboBox.Items.Add(table.Name);
                _secondTableComboBox.Items.Add(table.Name);
            }

            if (_firstTableComboBox.Items.Count > 0)
            {
                _firstTableComboBox.SelectedIndex = 0;
            }

            if (_secondTableComboBox.Items.Count > 1)
            {
                _secondTableComboBox.SelectedIndex = 1;
            }
            else if (_secondTableComboBox.Items.Count > 0)
            {
                _secondTableComboBox.SelectedIndex = 0;
            }
        }

        private void ConfirmUnion()
        {
            if (_firstTableComboBox.SelectedItem == null ||
                _secondTableComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Оберіть дві таблиці.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string firstName =
                _firstTableComboBox.SelectedItem.ToString()!;

            string secondName =
                _secondTableComboBox.SelectedItem.ToString()!;

            if (firstName.Equals(
                    secondName,
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Оберіть дві різні таблиці.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string resultName =
                _resultNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(resultName))
            {
                MessageBox.Show(
                    "Введіть назву результуючої таблиці.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (_database.ContainsTable(resultName))
            {
                MessageBox.Show(
                    $"Таблиця '{resultName}' вже існує.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            FirstTable =
                _database.GetTable(firstName);

            SecondTable =
                _database.GetTable(secondName);

            if (FirstTable == null ||
                SecondTable == null)
            {
                MessageBox.Show(
                    "Не вдалося знайти вибрані таблиці.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            ResultTableName = resultName;

            DialogResult = DialogResult.OK;

            Close();
        }
    }
}