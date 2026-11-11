namespace ReviewAssistant.Core;

/// <summary>The tool's only input: a small set of changed files to review.</summary>
public sealed record ChangeSet(
    string Repository,
    string Branch,
    string Title,
    IReadOnlyList<ChangedFile> Files);

/// <summary>One changed file and its unified diff (the text `git diff` prints).</summary>
public sealed record ChangedFile(string Path, string Diff);

/// <summary>
/// A change set that has been through the redactor.
/// Everything after the redaction step asks for this type, not <see cref="ChangeSet"/>,
/// so the compiler stops raw input from reaching a prompt or a provider by accident.
/// </summary>
/// <param name="ChangeSet">The change set with sensitive values replaced by markers.</param>
/// <param name="Redactions">How many values were replaced, per kind (for example "secret": 1).</param>
public sealed record RedactedChangeSet(
    ChangeSet ChangeSet,
    IReadOnlyDictionary<string, int> Redactions);
