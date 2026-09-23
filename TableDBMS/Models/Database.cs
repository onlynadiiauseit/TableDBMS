namespace TableDBMS.Models
{
    public class Database
    {
        public string Name { get; set; } = "NewDatabase";

        public List<Table> Tables { get; set; } = new();

        public Database()
        {
        }

        public Database(string name)
        {
            Name = name;
        }

        public void AddTable(Table table)
        {
            if (string.IsNullOrWhiteSpace(table.Name))
            {
                throw new ArgumentException(
                    "Table name cannot be empty."
                );
            }

            if (ContainsTable(table.Name))
            {
                throw new InvalidOperationException(
                    $"Table '{table.Name}' already exists."
                );
            }

            Tables.Add(table);
        }

        public void RemoveTable(string tableName)
        {
            Table? table = GetTable(tableName);

            if (table == null)
            {
                throw new InvalidOperationException(
                    $"Table '{tableName}' was not found."
                );
            }

            Tables.Remove(table);
        }

        public Table? GetTable(string tableName)
        {
            return Tables.FirstOrDefault(
                t => t.Name.Equals(
                    tableName,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        public bool ContainsTable(string tableName)
        {
            return GetTable(tableName) != null;
        }
    }
}