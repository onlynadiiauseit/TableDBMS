using TableDBMS.Models;

namespace TableDBMS.Services
{
    public class TableUnionService
    {
        public Table Union(
            Table firstTable,
            Table secondTable,
            string resultTableName)
        {
            if (firstTable == null)
                throw new ArgumentNullException(nameof(firstTable));

            if (secondTable == null)
                throw new ArgumentNullException(nameof(secondTable));

            if (string.IsNullOrWhiteSpace(resultTableName))
            {
                throw new ArgumentException(
                    "Result table name cannot be empty.",
                    nameof(resultTableName)
                );
            }

            if (!firstTable.Schema.IsCompatibleWith(
                    secondTable.Schema))
            {
                throw new InvalidOperationException(
                    "Tables have incompatible schemas."
                );
            }

            TableSchema resultSchema = CloneSchema(
                firstTable.Schema
            );

            Table resultTable = new(
                resultTableName,
                resultSchema
            );

            foreach (Row row in firstTable.Rows)
            {
                AddUniqueRow(resultTable, row);
            }

            foreach (Row row in secondTable.Rows)
            {
                AddUniqueRow(resultTable, row);
            }

            return resultTable;
        }

        private static void AddUniqueRow(
            Table table,
            Row sourceRow)
        {
            bool alreadyExists = table.Rows.Any(
                existingRow =>
                    existingRow.Values.SequenceEqual(
                        sourceRow.Values
                    )
            );

            if (alreadyExists)
                return;

            Row copiedRow = new(
                sourceRow.Values.ToList()
            );

            table.AddRow(copiedRow);
        }

        private static TableSchema CloneSchema(
            TableSchema sourceSchema)
        {
            TableSchema result = new();

            foreach (Column column in sourceSchema.Columns)
            {
                result.AddColumn(
                    new Column(
                        column.Name,
                        column.DataTypeName
                    )
                );
            }

            return result;
        }
    }
}