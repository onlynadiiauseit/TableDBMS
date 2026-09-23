using TableDBMS.DataTypes;
using TableDBMS.Models;

namespace TableDBMS.Forms
{
    public class CreateTableForm : Form
    {
        private readonly TextBox _tableNameTextBox;
        private readonly DataGridView _columnsGrid;

        public Table? CreatedTable { get; private set; }

        public CreateTableForm()
        {
            Text = "Створення таблиці";

            Width = 650;
            Height = 500;

            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            var nameLabel = new Label
            {
                Text = "Назва таблиці:",
                Left = 20,
                Top = 20,
                Width = 150
            };

            _tableNameTextBox = new TextBox
            {
                Left = 20,
                Top = 45,
                Width = 590
            };

            var columnsLabel = new Label
            {
                Text = "Поля таблиці:",
                Left = 20,
                Top = 85,
                Width = 150
            };

            _columnsGrid = new DataGridView
            {
                Left = 20,
                Top = 110,
                Width = 590,
                Height = 260,

                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,

                RowHeadersVisible = false,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = false,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill
            };

            var nameColumn =
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Назва поля",
                    Name = "ColumnName"
                };

            var typeColumn =
                new DataGridViewComboBoxColumn
                {
                    HeaderText = "Тип даних",
                    Name = "DataType"
                };

            typeColumn.Items.AddRange(
                DataTypeFactory.GetAvailableTypes()
            );

            _columnsGrid.Columns.Add(nameColumn);
            _columnsGrid.Columns.Add(typeColumn);

            var addColumnButton = new Button
            {
                Text = "+ Додати поле",
                Left = 20,
                Top = 385,
                Width = 140,
                Height = 32
            };

            addColumnButton.Click += (_, _) =>
                AddColumnRow();

            var removeColumnButton = new Button
            {
                Text = "Видалити поле",
                Left = 170,
                Top = 385,
                Width = 140,
                Height = 32
            };

            removeColumnButton.Click += (_, _) =>
                RemoveSelectedColumn();

            var createButton = new Button
            {
                Text = "Створити",
                Left = 380,
                Top = 385,
                Width = 110,
                Height = 32
            };

            createButton.Click += (_, _) =>
                CreateTable();

            var cancelButton = new Button
            {
                Text = "Скасувати",
                Left = 500,
                Top = 385,
                Width = 110,
                Height = 32,

                DialogResult = DialogResult.Cancel
            };

            Controls.Add(nameLabel);
            Controls.Add(_tableNameTextBox);
            Controls.Add(columnsLabel);
            Controls.Add(_columnsGrid);

            Controls.Add(addColumnButton);
            Controls.Add(removeColumnButton);

            Controls.Add(createButton);
            Controls.Add(cancelButton);

            CancelButton = cancelButton;

            AddColumnRow();
        }

        private void AddColumnRow()
        {
            _columnsGrid.Rows.Add(
                string.Empty,
                "String"
            );
        }

        private void RemoveSelectedColumn()
        {
            if (_columnsGrid.SelectedRows.Count == 0)
                return;

            foreach (DataGridViewRow row
                     in _columnsGrid.SelectedRows)
            {
                _columnsGrid.Rows.Remove(row);
            }
        }

        private void CreateTable()
        {
            _columnsGrid.EndEdit();

            string tableName =
                _tableNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(tableName))
            {
                MessageBox.Show(
                    "Введіть назву таблиці.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (_columnsGrid.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Таблиця повинна мати хоча б одне поле.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            try
            {
                var schema = new TableSchema();

                foreach (DataGridViewRow row
                         in _columnsGrid.Rows)
                {
                    string columnName =
                        Convert.ToString(
                            row.Cells["ColumnName"].Value
                        )?.Trim()
                        ?? string.Empty;

                    string dataType =
                        Convert.ToString(
                            row.Cells["DataType"].Value
                        )?.Trim()
                        ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(
                            columnName))
                    {
                        throw new InvalidOperationException(
                            "Назва поля не може бути порожньою."
                        );
                    }

                    if (string.IsNullOrWhiteSpace(
                            dataType))
                    {
                        throw new InvalidOperationException(
                            $"Не обрано тип для поля " +
                            $"'{columnName}'."
                        );
                    }

                    schema.AddColumn(
                        new Column(
                            columnName,
                            dataType
                        )
                    );
                }

                CreatedTable =
                    new Table(
                        tableName,
                        schema
                    );

                DialogResult = DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Помилка створення таблиці",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
    }
}