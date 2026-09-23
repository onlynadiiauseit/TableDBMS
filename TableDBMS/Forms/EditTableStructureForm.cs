using TableDBMS.DataTypes;
using TableDBMS.Models;

namespace TableDBMS.Forms
{
    public class EditTableStructureForm : Form
    {
        private readonly Table _workingTable;
        private readonly DataGridView _grid;

        public Table? ResultTable { get; private set; }

        public EditTableStructureForm(Table sourceTable)
        {
            _workingTable =
                CloneTable(sourceTable);

            Text =
                $"Структура таблиці: {sourceTable.Name}";

            Width = 760;
            Height = 520;

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            _grid = new DataGridView
            {
                Left = 20,
                Top = 20,
                Width = 700,
                Height = 330,

                ReadOnly = true,

                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,

                RowHeadersVisible = false,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = false,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill
            };

            _grid.Columns.Add(
                "ColumnName",
                "Назва поля"
            );

            _grid.Columns.Add(
                "DataType",
                "Тип даних"
            );

            var addButton = new Button
            {
                Text = "Додати поле",
                Left = 20,
                Top = 370,
                Width = 120,
                Height = 32
            };

            addButton.Click += (_, _) =>
                AddColumn();

            var renameButton = new Button
            {
                Text = "Перейменувати",
                Left = 150,
                Top = 370,
                Width = 130,
                Height = 32
            };

            renameButton.Click += (_, _) =>
                RenameColumn();

            var typeButton = new Button
            {
                Text = "Змінити тип",
                Left = 290,
                Top = 370,
                Width = 120,
                Height = 32
            };

            typeButton.Click += (_, _) =>
                ChangeColumnType();

            var deleteButton = new Button
            {
                Text = "Видалити поле",
                Left = 420,
                Top = 370,
                Width = 130,
                Height = 32
            };

            deleteButton.Click += (_, _) =>
                DeleteColumn();

            var saveButton = new Button
            {
                Text = "Зберегти",
                Left = 510,
                Top = 420,
                Width = 100,
                Height = 32
            };

            saveButton.Click += (_, _) =>
            {
                ResultTable = _workingTable;

                DialogResult =
                    DialogResult.OK;

                Close();
            };

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 620,
                Top = 420,
                Width = 100,
                Height = 32,

                DialogResult =
                    DialogResult.Cancel
            };

            Controls.Add(_grid);

            Controls.Add(addButton);
            Controls.Add(renameButton);
            Controls.Add(typeButton);
            Controls.Add(deleteButton);

            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            CancelButton = cancelButton;

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();

            foreach (Column column
                     in _workingTable.Schema.Columns)
            {
                _grid.Rows.Add(
                    column.Name,
                    column.DataTypeName
                );
            }
        }

        private int GetSelectedIndex()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Оберіть поле.",
                    "Поле не обрано",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return -1;
            }

            return _grid.SelectedRows[0].Index;
        }

        private void AddColumn()
        {
            using var dialog = new Form
            {
                Text = "Додати поле",
                Width = 430,
                Height = 270,

                StartPosition =
                    FormStartPosition.CenterParent,

                FormBorderStyle =
                    FormBorderStyle.FixedDialog,

                MaximizeBox = false,
                MinimizeBox = false
            };

            var nameLabel = new Label
            {
                Text = "Назва поля:",
                Left = 20,
                Top = 20,
                Width = 150
            };

            var nameTextBox = new TextBox
            {
                Left = 20,
                Top = 45,
                Width = 370
            };

            var typeLabel = new Label
            {
                Text = "Тип даних:",
                Left = 20,
                Top = 80,
                Width = 150
            };

            var typeComboBox =
                new ComboBox
                {
                    Left = 20,
                    Top = 105,
                    Width = 370,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList
                };

            typeComboBox.Items.AddRange(
                DataTypeFactory.GetAvailableTypes()
            );

            typeComboBox.SelectedItem =
                "String";

            var defaultLabel = new Label
            {
                Text = _workingTable.Rows.Count > 0
                    ? "Значення для існуючих рядків:"
                    : "Початкове значення:",
                Left = 20,
                Top = 140,
                Width = 300
            };

            var defaultTextBox = new TextBox
            {
                Left = 20,
                Top = 165,
                Width = 370
            };

            var okButton = new Button
            {
                Text = "Додати",
                Left = 210,
                Top = 200,
                Width = 85,
                Height = 30,

                DialogResult =
                    DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 305,
                Top = 200,
                Width = 85,
                Height = 30,

                DialogResult =
                    DialogResult.Cancel
            };

            dialog.Controls.Add(nameLabel);
            dialog.Controls.Add(nameTextBox);

            dialog.Controls.Add(typeLabel);
            dialog.Controls.Add(typeComboBox);

            dialog.Controls.Add(defaultLabel);
            dialog.Controls.Add(defaultTextBox);

            dialog.Controls.Add(okButton);
            dialog.Controls.Add(cancelButton);

            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                string name =
                    nameTextBox.Text.Trim();

                string type =
                    Convert.ToString(
                        typeComboBox.SelectedItem
                    ) ?? "";

                string defaultValue =
                    defaultTextBox.Text;

                _workingTable.AddColumn(
                    new Column(
                        name,
                        type
                    ),
                    defaultValue
                );

                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void RenameColumn()
        {
            int index =
                GetSelectedIndex();

            if (index < 0)
                return;

            string currentName =
                _workingTable
                    .Schema
                    .Columns[index]
                    .Name;

            string? newName =
                PromptText(
                    "Перейменування поля",
                    "Нова назва поля:",
                    currentName
                );

            if (string.IsNullOrWhiteSpace(
                    newName))
            {
                return;
            }

            try
            {
                _workingTable.RenameColumn(
                    index,
                    newName
                );

                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void ChangeColumnType()
        {
            int index =
                GetSelectedIndex();

            if (index < 0)
                return;

            Column column =
                _workingTable
                    .Schema
                    .Columns[index];

            using var dialog = new Form
            {
                Text =
                    $"Тип поля: {column.Name}",

                Width = 400,
                Height = 180,

                StartPosition =
                    FormStartPosition.CenterParent,

                FormBorderStyle =
                    FormBorderStyle.FixedDialog,

                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = "Новий тип:",
                Left = 20,
                Top = 20,
                Width = 150
            };

            var comboBox = new ComboBox
            {
                Left = 20,
                Top = 50,
                Width = 340,

                DropDownStyle =
                    ComboBoxStyle.DropDownList
            };

            comboBox.Items.AddRange(
                DataTypeFactory.GetAvailableTypes()
            );

            comboBox.SelectedItem =
                column.DataTypeName;

            var okButton = new Button
            {
                Text = "OK",
                Left = 180,
                Top = 90,
                Width = 80,

                DialogResult =
                    DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 270,
                Top = 90,
                Width = 90,

                DialogResult =
                    DialogResult.Cancel
            };

            dialog.Controls.Add(label);
            dialog.Controls.Add(comboBox);

            dialog.Controls.Add(okButton);
            dialog.Controls.Add(cancelButton);

            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            string newType =
                Convert.ToString(
                    comboBox.SelectedItem
                ) ?? "";

            try
            {
                _workingTable.ChangeColumnType(
                    index,
                    newType
                );

                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Неможливо змінити тип",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private void DeleteColumn()
        {
            int index =
                GetSelectedIndex();

            if (index < 0)
                return;

            string name =
                _workingTable
                    .Schema
                    .Columns[index]
                    .Name;

            DialogResult answer =
                MessageBox.Show(
                    $"Видалити поле '{name}'?\n\n" +
                    "Дані цього поля будуть видалені " +
                    "з усіх рядків.",
                    "Підтвердження",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (answer !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                _workingTable.RemoveColumn(
                    index
                );

                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private static string? PromptText(
            string title,
            string message,
            string initialValue)
        {
            using var dialog = new Form
            {
                Text = title,
                Width = 420,
                Height = 180,

                StartPosition =
                    FormStartPosition.CenterParent,

                FormBorderStyle =
                    FormBorderStyle.FixedDialog,

                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = message,
                Left = 20,
                Top = 20,
                Width = 360
            };

            var textBox = new TextBox
            {
                Text = initialValue,
                Left = 20,
                Top = 50,
                Width = 360
            };

            var okButton = new Button
            {
                Text = "OK",
                Left = 210,
                Top = 90,
                Width = 80,

                DialogResult =
                    DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 300,
                Top = 90,
                Width = 80,

                DialogResult =
                    DialogResult.Cancel
            };

            dialog.Controls.Add(label);
            dialog.Controls.Add(textBox);
            dialog.Controls.Add(okButton);
            dialog.Controls.Add(cancelButton);

            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            return dialog.ShowDialog() ==
                   DialogResult.OK
                ? textBox.Text
                : null;
        }

        private static Table CloneTable(
            Table source)
        {
            var schema =
                new TableSchema();

            foreach (Column column
                     in source.Schema.Columns)
            {
                schema.AddColumn(
                    new Column(
                        column.Name,
                        column.DataTypeName
                    )
                );
            }

            var result =
                new Table(
                    source.Name,
                    schema
                );

            foreach (Row row in source.Rows)
            {
                result.Rows.Add(
                    new Row(
                        row.Values.ToList()
                    )
                );
            }

            return result;
        }
    }
}