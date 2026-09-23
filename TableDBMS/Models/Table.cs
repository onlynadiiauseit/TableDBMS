namespace TableDBMS.Models
{
    public class Table
    {
        public string Name { get; set; } = string.Empty;

        public TableSchema Schema { get; set; } = new();

        public List<Row> Rows { get; set; } = new();

        public Table()
        {
        }

        public Table(string name, TableSchema schema)
        {
            Name = name;
            Schema = schema;
        }

        public void AddRow(Row row)
        {
            ValidateRow(row);
            Rows.Add(row);
        }

        public void UpdateRow(int index, Row newRow)
        {
            if (index < 0 || index >= Rows.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                );
            }

            ValidateRow(newRow);

            Rows[index] = newRow;
        }

        public void DeleteRow(int index)
        {
            if (index < 0 || index >= Rows.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                );
            }

            Rows.RemoveAt(index);
        }

        public void AddColumn(
            Column column,
            string? defaultValue)
        {
            if (column == null)
            {
                throw new ArgumentNullException(
                    nameof(column)
                );
            }

            if (Rows.Count > 0 &&
                !column.IsValid(defaultValue))
            {
                throw new InvalidOperationException(
                    $"Значення '{defaultValue}' не відповідає " +
                    $"типу '{column.DataTypeName}'."
                );
            }

            Schema.AddColumn(column);

            foreach (Row row in Rows)
            {
                row.Values.Add(defaultValue);
            }
        }

        public void RemoveColumn(int index)
        {
            if (index < 0 ||
                index >= Schema.Columns.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                );
            }

            if (Schema.Columns.Count <= 1)
            {
                throw new InvalidOperationException(
                    "Таблиця повинна містити хоча б одне поле."
                );
            }

            string columnName =
                Schema.Columns[index].Name;

            Schema.RemoveColumn(columnName);

            foreach (Row row in Rows)
            {
                if (index < row.Values.Count)
                {
                    row.Values.RemoveAt(index);
                }
            }
        }

        public void RenameColumn(
            int index,
            string newName)
        {
            if (index < 0 ||
                index >= Schema.Columns.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                );
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException(
                    "Назва поля не може бути порожньою.",
                    nameof(newName)
                );
            }

            newName = newName.Trim();

            bool alreadyExists =
                Schema.Columns
                    .Where((_, i) => i != index)
                    .Any(
                        column =>
                            column.Name.Equals(
                                newName,
                                StringComparison.OrdinalIgnoreCase
                            )
                    );

            if (alreadyExists)
            {
                throw new InvalidOperationException(
                    $"Поле '{newName}' вже існує."
                );
            }

            Schema.Columns[index].Name =
                newName;
        }

        public void ChangeColumnType(
            int index,
            string newTypeName)
        {
            if (index < 0 ||
                index >= Schema.Columns.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index)
                );
            }

            if (string.IsNullOrWhiteSpace(
                    newTypeName))
            {
                throw new ArgumentException(
                    "Тип даних не може бути порожнім.",
                    nameof(newTypeName)
                );
            }

            Column currentColumn =
                Schema.Columns[index];

            var testColumn =
                new Column(
                    currentColumn.Name,
                    newTypeName
                );

            foreach (Row row in Rows)
            {
                if (index >= row.Values.Count)
                {
                    throw new InvalidOperationException(
                        "Структура рядка не відповідає структурі таблиці."
                    );
                }

                string? value =
                    row.Values[index];

                if (!testColumn.IsValid(value))
                {
                    throw new InvalidOperationException(
                        $"Значення '{value}' у полі " +
                        $"'{currentColumn.Name}' неможливо " +
                        $"перетворити на тип '{newTypeName}'."
                    );
                }
            }

            currentColumn.DataTypeName =
                newTypeName;
        }

        public void Clear()
        {
            Rows.Clear();
        }

        private void ValidateRow(Row row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(
                    nameof(row)
                );
            }

            if (row.Values.Count !=
                Schema.Columns.Count)
            {
                throw new InvalidOperationException(
                    "Кількість значень у рядку " +
                    "не відповідає кількості полів таблиці."
                );
            }

            for (int i = 0;
                 i < Schema.Columns.Count;
                 i++)
            {
                Column column =
                    Schema.Columns[i];

                string? value =
                    row.Values[i];

                if (!column.IsValid(value))
                {
                    throw new InvalidOperationException(
                        $"Значення '{value}' не відповідає " +
                        $"типу '{column.DataTypeName}' " +
                        $"для поля '{column.Name}'."
                    );
                }
            }
        }
    }
}