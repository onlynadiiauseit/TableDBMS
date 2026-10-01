using System.Net.Http.Json;
using System.Text.Json;
using TableDBMS.Models;

namespace TableDBMS.Services
{
    public sealed class RemoteStorageService
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RemoteStorageService(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException(
                    "Server URL cannot be empty.",
                    nameof(baseUrl)
                );
            }

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(
                    baseUrl.TrimEnd('/') + "/"
                ),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<bool> CheckConnectionAsync()
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    "api/health"
                );

            return response.IsSuccessStatusCode;
        }

        public async Task SaveDatabaseAsync(
            Database database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(
                    nameof(database)
                );
            }

            string name =
                Uri.EscapeDataString(database.Name);

            using HttpResponseMessage response =
                await _httpClient.PutAsJsonAsync(
                    $"api/databases/{name}",
                    database,
                    _jsonOptions
                );

            response.EnsureSuccessStatusCode();
        }

        public async Task<Database> LoadDatabaseAsync(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Database name cannot be empty.",
                    nameof(name)
                );
            }

            string encodedName =
                Uri.EscapeDataString(name);

            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"api/databases/{encodedName}"
                );

            response.EnsureSuccessStatusCode();

            Database? database =
                await response.Content
                    .ReadFromJsonAsync<Database>(
                        _jsonOptions
                    );

            if (database == null)
            {
                throw new InvalidOperationException(
                    "Server returned an empty database."
                );
            }

            return database;
        }

        public async Task<List<string>>
            GetDatabaseNamesAsync()
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    "api/databases"
                );

            response.EnsureSuccessStatusCode();

            List<RemoteDatabaseInfo>? databases =
                await response.Content
                    .ReadFromJsonAsync<
                        List<RemoteDatabaseInfo>
                    >(_jsonOptions);

            if (databases == null)
            {
                return new List<string>();
            }

            return databases
                .Select(database => database.Name)
                .OrderBy(name => name)
                .ToList();
        }

        private sealed class RemoteDatabaseInfo
        {
            public string Name { get; set; } =
                string.Empty;
        }
    }
}