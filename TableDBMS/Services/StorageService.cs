using System.Text.Json;
using TableDBMS.Models;

namespace TableDBMS.Services
{
    public class StorageService
    {
        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public void SaveDatabase(Database database, string filePath)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(
                    "File path cannot be empty.",
                    nameof(filePath)
                );

            string json = JsonSerializer.Serialize(database, _options);

            File.WriteAllText(filePath, json);
        }

        public Database LoadDatabase(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(
                    "File path cannot be empty.",
                    nameof(filePath)
                );

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Database file was not found.",
                    filePath
                );
            }

            string json = File.ReadAllText(filePath);

            Database? database =
                JsonSerializer.Deserialize<Database>(json, _options);

            if (database == null)
            {
                throw new InvalidOperationException(
                    "Failed to load database."
                );
            }

            return database;
        }
    }
}