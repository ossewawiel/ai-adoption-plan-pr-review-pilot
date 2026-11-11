# AI adoption plan and a local PR review assistant

A take-home assignment in two parts.

| Part | Where | What it is |
|---|---|---|
| 1. The plan | [`PLAN.md`](PLAN.md) | A three-page AI adoption plan for a 60-developer .NET group. Illustrated edition: [`docs/plan.html`](docs/plan.html) · [`docs/plan.pdf`](docs/plan.pdf) |
| 2. The pilot | [`src/`](src), [`tests/`](tests), [`samples/`](samples) | A small .NET 8 / C# 12 tool that reviews a change set and writes a Markdown review |
| Design notes | [`SOLUTION.md`](SOLUTION.md) | Design, trade-offs, the Claude adapter, a file-by-file map, and what I chose not to build |
| The plugin | [`plugin/`](plugin) | A mini version of the company plugin the plan argues for |
| How AI was used | [`PROMPTS.md`](PROMPTS.md), [`docs/process/`](docs/process) | The prompts I used, and the question-and-answer brief that came before the build |

## Run the pilot

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). Check with `dotnet --list-sdks`.

```
dotnet run --project src/ReviewAssistant.Cli -- samples/changeset.json
```

That reviews the sample change set and writes `review.md` in the current folder. It runs fully
offline: the default provider is simulated and calls no model and no network.

```
Review written to ...\review.md
Provider: simulated (pattern rules, no model was called)
Items: 4 suggestions, 8 risks
```

A copy of that output is in [`samples/sample-review.md`](samples/sample-review.md).

### Options

```
review <change-set.json> [--out <file.md>] [--provider <name>] [--model <name>] [--show-prompt]
```

| Option | What it does |
|---|---|
| `--out review.md` | Where the review is written |
| `--provider simulated` | The default. Pattern rules, no network |
| `--provider claude-code` | Opt-in. Runs Claude Code headless and sends the **redacted** change set to Claude |
| `--model <name>` | Pins the model for the `claude-code` provider |
| `--show-prompt` | Prints the exact text a provider is sent, so you can see what was redacted |

### The opt-in Claude Code provider

```
dotnet run --project src/ReviewAssistant.Cli -- samples/changeset.json --provider claude-code
```

This needs [Claude Code](https://code.claude.com/docs) installed and signed in. It is the only thing
in the repository that uses the network, and it only runs when you ask for it. One real run on the
sample is saved in [`samples/sample-review.claude-code.md`](samples/sample-review.claude-code.md).

## Run the tests

```
dotnet test
```

36 tests: redaction, diff reading, parsing, rendering, the rubric, and end-to-end runs on the sample.
The first build downloads the xUnit test packages from NuGet. The tool itself has no package
dependencies.

## Review your own change

The input is a small JSON file. Each file's diff is an array of lines, so you can read and edit it:

```json
{
  "repository": "Sample.FleetApi",
  "branch": "feature/trip-average-speed",
  "title": "Add average speed to the trip summary",
  "files": [
    { "path": "src/Trips/TripSummary.cs", "diff": ["@@ -1,3 +1,3 @@", " unchanged line", "-old line", "+new line"] }
  ]
}
```

The `diff` lines are what `git diff` prints for that file, from the `@@` line down.

## Good to know

- The sample change set contains a password, an ID number and a pair of coordinates. They are fake
  and they are there on purpose, to show the redaction step working.
- `global.json` asks for a .NET 8 SDK. If you only have a newer SDK, delete `global.json`; the
  projects still target `net8.0`, so you also need the .NET 8 runtime.
- Nothing here posts to GitHub or any other service.
