namespace TableDBMS.Models
{
    public class TableSchema
    {
        public List<Column> Columns { get; set; } = new();

        public void AddColumn(Column column)
        {
            if (string.IsNullOrWhiteSpace(column.Name))
            {
                throw new ArgumentException(
                    "Column name cannot be empty."
                );
            }

            bool alreadyExists = Columns.Any(
                c => c.Name.Equals(
                    column.Name,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (alreadyExists)
            {
                throw new InvalidOperationException(
                    $"Column '{column.Name}' already exists."
                );
            }

            Columns.Add(column);
        }

        public void RemoveColumn(string columnName)
        {
            Column? column = Columns.FirstOrDefault(
                c => c.Name.Equals(
                    columnName,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (column == null)
            {
                throw new InvalidOperationException(
                    $"Column '{columnName}' was not found."
                );
            }

            Columns.Remove(column);
        }

        public bool IsCompatibleWith(TableSchema other)
        {
            if (Columns.Count != other.Columns.Count)
                return false;

            for (int i = 0; i < Columns.Count; i++)
            {
                Column first = Columns[i];
                Column second = other.Columns[i];

                if (!first.DataTypeName.Equals(
                        second.DataTypeName,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    return false;
                }
            }

            return true;
        }
    }
}