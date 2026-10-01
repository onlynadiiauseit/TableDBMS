using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5080");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

var dataDirectory = Path.Combine(
    app.Environment.ContentRootPath,
    "Data"
);

Directory.CreateDirectory(dataDirectory);

static string? GetSafeDatabaseName(string name)
{
    string safeName =
        Path.GetFileNameWithoutExtension(name).Trim();

    if (string.IsNullOrWhiteSpace(safeName))
        return null;

    foreach (char invalidChar in Path.GetInvalidFileNameChars())
    {
        safeName = safeName.Replace(invalidChar, '_');
    }

    return safeName;
}

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        service = "TableDBMS.Server",
        status = "ok"
    });
});

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        utcTime = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/databases", () =>
{
    var databases =
        Directory
            .EnumerateFiles(dataDirectory, "*.json")
            .Select(path => new
            {
                name = Path.GetFileNameWithoutExtension(path),
                updatedAt = File.GetLastWriteTimeUtc(path)
            })
            .OrderBy(database => database.name)
            .ToList();

    return Results.Ok(databases);
});

app.MapGet(
    "/api/databases/{name}",
    async (string name) =>
    {
        string? safeName =
            GetSafeDatabaseName(name);

        if (safeName == null)
        {
            return Results.BadRequest(
                new { error = "Invalid database name." }
            );
        }

        string filePath =
            Path.Combine(
                dataDirectory,
                $"{safeName}.json"
            );

        if (!File.Exists(filePath))
        {
            return Results.NotFound(
                new { error = "Database not found." }
            );
        }

        string json =
            await File.ReadAllTextAsync(filePath);

        return Results.Text(
            json,
            "application/json"
        );
    }
);

app.MapPut(
    "/api/databases/{name}",
    async (
        string name,
        HttpRequest request
    ) =>
    {
        string? safeName =
            GetSafeDatabaseName(name);

        if (safeName == null)
        {
            return Results.BadRequest(
                new { error = "Invalid database name." }
            );
        }

        using var reader =
            new StreamReader(request.Body);

        string json =
            await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            return Results.BadRequest(
                new { error = "Request body is empty." }
            );
        }

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return Results.BadRequest(
                new { error = "Invalid JSON." }
            );
        }

        string filePath =
            Path.Combine(
                dataDirectory,
                $"{safeName}.json"
            );

        await File.WriteAllTextAsync(
            filePath,
            json
        );

        return Results.Ok(new
        {
            name = safeName,
            saved = true,
            utcTime = DateTimeOffset.UtcNow
        });
    }
);

app.MapDelete(
    "/api/databases/{name}",
    (string name) =>
    {
        string? safeName =
            GetSafeDatabaseName(name);

        if (safeName == null)
        {
            return Results.BadRequest(
                new { error = "Invalid database name." }
            );
        }

        string filePath =
            Path.Combine(
                dataDirectory,
                $"{safeName}.json"
            );

        if (!File.Exists(filePath))
        {
            return Results.NotFound(
                new { error = "Database not found." }
            );
        }

        File.Delete(filePath);

        return Results.Ok(new
        {
            name = safeName,
            deleted = true
        });
    }
);

app.Run();