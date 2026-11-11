using System.Text.Json;

namespace ReviewAssistant.Core;

/// <summary>
/// Step 1. Reads a change set from a JSON file. See samples/changeset.json for the shape.
/// Each file's diff is an array of lines, so a person can read and edit the JSON.
/// </summary>
public sealed class JsonChangeSetReader : IChangeSetReader
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    // The shape of the file on disk. Everything is nullable because the file may be incomplete.
    private sealed record ChangeSetFile(string? Repository, string? Branch, string? Title, List<FileEntry>? Files);
    private sealed record FileEntry(string? Path, List<string>? Diff);

    public ChangeSet Read(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidChangeSetException($"Change set not found: {path}");
        }

        ChangeSetFile? file;
        try
        {
            file = JsonSerializer.Deserialize<ChangeSetFile>(File.ReadAllText(path), Options);
        }
        catch (JsonException error)
        {
            throw new InvalidChangeSetException($"Change set is not valid JSON: {error.Message}");
        }

        if (file?.Files is null || file.Files.Count == 0)
        {
            throw new InvalidChangeSetException("Change set has no files.");
        }

        var files = new List<ChangedFile>();
        foreach (var entry in file.Files)
        {
            if (string.IsNullOrWhiteSpace(entry.Path) || entry.Diff is null)
            {
                throw new InvalidChangeSetException("Every file needs a \"path\" and a \"diff\".");
            }

            files.Add(new ChangedFile(entry.Path, string.Join('\n', entry.Diff)));
        }

        return new ChangeSet(
            file.Repository ?? "unknown repository",
            file.Branch ?? "unknown branch",
            file.Title ?? "",
            files);
    }
}

/// <summary>The input file is missing or has the wrong shape. The message is safe to show the user.</summary>
public sealed class InvalidChangeSetException(string message) : Exception(message);
