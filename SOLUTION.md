# Solution notes

How the pilot is designed, what I traded away, how Claude plugs in, a map of every file, and what I
chose not to build.

## The four-hour core, and what I added

The brief suggests about four hours. This repository is more than that, on purpose, and I would
rather say so than hide it.

| The core the brief asks for | What I added, and why |
|---|---|
| `PLAN.md`, the three-page plan | **The illustrated edition** (`docs/plan.html`, `docs/plan.pdf`): the plan argues that documents for people should look like it. |
| The .NET tool with a simulated provider, and its tests | **A real Claude Code provider**, off by default: it proves the extension point is real and not a diagram. |
| `README.md` and this file | **A mini plugin** (`plugin/`): the plan's main idea is a company plugin, so a small piece of it runs. |
| | **`PROMPTS.md` and `docs/process/`**: I was asked to show how I used AI. |

Delete the right-hand column and the four-hour version is what remains.

## The design on one page

```
changeset.json
     │
 1 read ──▶ 2 redact ──▶ 3 build prompt ──▶ 4 provider ──▶ 5 parse ──▶ 6 render ──▶ review.md
                                               ▲
                                    the extension point:
                              simulated (default) │ claude-code (opt-in)
```

`ReviewPipeline.RunAsync` is those six lines, in that order. Each step is an interface in
`Abstractions.cs` with one small implementation. The decisions that matter:

1. **One interface per step, not one big interface.** The brief asks for one extension point, the
   provider. I gave each step a seam because the security story lives in the steps *around* the
   provider: what goes in (redaction, prompt) and what is trusted coming out (parsing).
2. **Redaction comes before the prompt exists, and the compiler helps.** The redactor returns a
   `RedactedChangeSet`. The prompt builder and the provider only accept that type, so raw input
   cannot reach a provider by accident. A test checks that the planted password, ID number and
   coordinates appear in neither the prompt nor the review.
3. **A provider returns text, not objects.** Every provider replies in the same JSON, and one parser
   reads it. So the simulated provider exercises the same parse-and-render path a real model does,
   and adding a provider means writing one method.
4. **The parser does not trust the reply.** A model can wrap JSON in chat, use a severity we do not
   know, or point at a file that is not in the change. The parser strips the chat, falls back to
   medium severity and low confidence, drops made-up file references, and fails loudly on nonsense.
5. **The simulated provider is honest about what it is.** It is eight pattern rules and a comparison
   of public members, not a model. The review header says "no model was called". It exists so the
   tool runs offline and the pipeline can be tested, not to pretend to be intelligent.
6. **The output keeps the brief's three headings** and adds what a reviewer needs to triage: severity,
   confidence, category, and a file and line.
7. **No dependencies.** The tool uses only the .NET base library; xUnit is the single package, for
   tests. There is no dependency-injection container: `ReviewPipeline.WithDefaults` wires six objects
   by hand, which is easier to read and to explain.

## Trade-offs

| Choice | What it buys | What it costs |
|---|---|---|
| Input is JSON with each diff as an array of lines | One format, easy to read and hand-edit | A developer cannot pipe `git diff` straight in. A second `IChangeSetReader` would fix that. |
| Redaction by regular expression | Small, fast, testable, no dependencies | It catches common shapes only. It will miss a name in a comment and may over-redact a 13-digit number. |
| Pattern rules in the simulated provider | Deterministic, offline, good enough to demo and test | It only finds what its rules describe. It does not reason. |
| The reply is JSON in plain text | Works with any provider | The model can get the format wrong. The parser guards it; Claude Code's `--json-schema` would be stricter. |
| Shell out to Claude Code, not an API SDK | No API key in this tool; the company's managed settings apply | Needs Claude Code installed; starting a process is clumsier than an HTTP call. |
| Providers in their own project | `Core` contains no code that can start a process or open a socket | One more project file. |
| Review is advice only | Safe to trial; nothing is blocked or approved | It is not a gate yet. See "not built". |

## The Claude / Claude Code adapter

### How it plugs in

A provider is one interface with one method:

```csharp
public interface IReviewProvider
{
    string Name { get; }
    Task<string> GenerateAsync(ReviewRequest request, CancellationToken cancellationToken);
}
```

To add one: write a class that implements it, and add one line to the `switch` in `Program.cs`.
Nothing else changes. `PipelineTests` proves it with a stand-in provider.

The adapter is built: `ClaudeCodeReviewProvider`. It starts the Claude Code command-line tool in
headless mode, writes the prompt to its standard input, and reads the answer from the `result` field
of its JSON output. The flags it uses:

| Flag | Why |
|---|---|
| `-p` | Print mode: one question, one answer, no interactive session |
| `--output-format json` | A machine-readable answer |
| `--tools ""` | No tools at all. The model can read the prompt and reply; it cannot open files, run commands or browse |
| `--max-turns 1` | One turn, so it cannot loop |
| `--model <name>` | Optional: pins the model, for cost and repeatability |

I ran it once on the sample; the result is `samples/sample-review.claude-code.md`.

### How I would enable it safely under strict security

1. **It stays off until security approves the route.** The simulated provider is the default. The
   real one needs an explicit flag, and the tool prints a notice when it is used.
2. **Nothing personal or secret reaches the prompt.** Redaction always runs first (decision 2 above).
   This is also the answer to POPIA section 72, which restricts sending personal information to a
   third party in a foreign country: plan on the model running outside South Africa, and send it
   no personal information.
3. **No key in the tool.** The adapter uses Claude Code's own sign-in. There is no secret to store,
   rotate or leak in this code base.
4. **The company's managed settings apply to the call**: permissions, the plugin lock and the usage
   export, as in `plugin/samples/managed-settings.sample.json`. This is why I did not use Claude
   Code's `--bare` mode: it is the documented mode for scripts, but it needs an API key and skips
   plugins and hooks.
5. **The model gets no tools and one turn.** Even if the code under review contains text such as
   "ignore your rules", there is nothing for the model to *do* with it. The prompt also marks the
   change set as data, and a person reads the result.
6. **Choose the route with security.** Two I checked in Anthropic's and Microsoft's documentation:
   - *Anthropic direct with zero data retention.* Available to qualified Claude for Enterprise
     accounts, enabled per organisation by Anthropic. It does not apply through cloud providers.
   - *Microsoft Foundry.* Claude Code supports it (`CLAUDE_CODE_USE_FOUNDRY=1`), with sign-in through
     Entra ID. On the Azure-hosted version prompts stay within Azure. The data zones on offer are
     Global and US, so the POPIA answer in point 2 still stands.
7. **Pin the model and watch the cost.** `--model` in the adapter, and Claude Code's OpenTelemetry
   export for spend per team (`plugin/samples/metrics-export.sample.json`).
8. **Pilot before rollout.** One team, advice only. The review never approves, blocks or posts.

What I would still add before production use: a locked-down Claude Code profile for the headless
call (today it also loads the developer's own settings), `--json-schema` for a stricter reply, a
record of each call for audit (a hash of the prompt, the model, the cost), and a spend cap.

## File-by-file map

For the interview: one line on what each file is for, and the one thing worth saying about it.

### `src/ReviewAssistant.Core` — the pipeline, no network code

| File | Lines | What it does | Worth saying |
|---|---:|---|---|
| `Abstractions.cs` | 59 | The six step interfaces and the small records passed between them | The whole design is on this page. `IReviewProvider` is the extension point. |
| `ChangeSet.cs` | 22 | `ChangeSet`, `ChangedFile`, `RedactedChangeSet` | `RedactedChangeSet` is how the compiler keeps raw input away from providers. |
| `Review.cs` | 29 | `Review`, `ReviewItem`, and the `Section`, `Severity`, `Confidence` enums | One item type for all three headings keeps the parser and renderer short. |
| `ReviewPipeline.cs` | 43 | Runs the six steps in order | Read this file first. `WithDefaults` is the hand-wiring. |
| `JsonChangeSetReader.cs` | 59 | Step 1: reads and validates the input file | Fails with a clear message, never a stack trace. |
| `PatternRedactor.cs` | 67 | Step 2: replaces secrets and personal information with markers | A table of rules. Keeps the key name (`Password=`) so the reviewer still sees what was there. |
| `ReviewPromptBuilder.cs` | 70 | Step 3: the rubric, the reply format, the redacted change set | The rubric is plain text a person can challenge. It marks the change set as data. |
| `JsonReviewParser.cs` | 86 | Step 5: turns a reply into a `Review` | It distrusts the reply. Unknown confidence becomes *low*, not high. |
| `MarkdownReviewRenderer.cs` | 75 | Step 6: writes the Markdown file | The header says which provider ran and what was redacted. |
| `DiffLines.cs` | 57 | Reads added and removed lines from a unified diff, with line numbers | This is where `file:line` in the review comes from. |

### `src/ReviewAssistant.Providers` — step 4, the swappable part

| File | Lines | What it does | Worth saying |
|---|---:|---|---|
| `SimulatedReviewProvider.cs` | 144 | The default: pattern rules over the added lines, replies in the agreed JSON | Honest fake. Same output contract as a model, so everything after it is really tested. |
| `ClaudeCodeReviewProvider.cs` | 110 | The opt-in: runs Claude Code headless | The only code that can cause network traffic. No tools, one turn, no API key. |

### `src/ReviewAssistant.Cli`

| File | Lines | What it does | Worth saying |
|---|---:|---|---|
| `Program.cs` | 89 | Reads the arguments, picks the provider, runs the pipeline, writes the file | The `switch` is the one place a provider is chosen. Exit codes: 0 ok, 1 failed, 2 wrong usage. |

### `tests/ReviewAssistant.Tests` — 36 tests

| File | What it proves |
|---|---|
| `PipelineTests.cs` | End to end on the sample; sensitive values reach neither prompt nor review; another provider plugs in; nonsense replies fail loudly |
| `PatternRedactorTests.cs` | Each kind of sensitive value is replaced and counted; ordinary code is left alone |
| `JsonReviewParserTests.cs` | Good replies parse; chat and code fences are ignored; made-up files and unknown words are handled |
| `MarkdownReviewRendererTests.cs` | The three headings, the header facts, the item format, severity order |
| `DiffLinesTests.cs` | Line numbers are right |
| `ClaudeCodeReviewProviderTests.cs` | Reading Claude Code's output; a missing install gives a clear error. Never starts Claude Code |
| `RubricTests.cs` | The plugin's skill and the code's rubric say the same thing; the prompt marks the change set as data |

### Everything else

| Path | What it is |
|---|---|
| `samples/changeset.json` | A three-file change with problems planted in it, including fake sensitive values |
| `samples/sample-review.md`, `samples/sample-review.claude-code.md` | What the two providers produce for it |
| `plugin/` | The mini plugin; see `plugin/README.md` |
| `Directory.Build.props`, `global.json` | C# 12, nullable checks, warnings as errors; ask for a .NET 8 SDK |

## Not built, and why

| Not built | Why |
|---|---|
| Posting the review to GitHub | The brief says local only. |
| Reading `git diff` output directly | One input format was enough for a pilot. The reader interface is where it goes. |
| A build gate (`--fail-on high`) | The pilot is advice. A gate is a policy decision for after the pilot, not before it. |
| Real data-loss prevention | Regular expressions show the control and its place in the flow. A real roll-out would use the company's approved scanner behind `IRedactor`. |
| Semantic analysis with Roslyn | It would make the simulated provider smarter, but the simulated provider is not the product. |
| A config file for rules and policies | More schema than a four-file pilot deserves. The rules are short tables in code. |
| A telemetry step in the tool | Usage data comes from Claude Code's own export; the sample is in `plugin/samples/`. |
| Model tiering in the tool | It is a paragraph in the plan. With one job, the tool has nothing to tier. |
| An Anthropic API or Microsoft Foundry adapter | One real adapter proves the seam. Each further route is a security decision first. |
| Retries, streaming and a spend cap in the Claude Code provider | Needed for production, noise in a pilot. Listed above under "what I would still add". |
| The rest of Plugin 1 (test skills, BA skills, the Issue lane) | Described in the plan. One skill and one hook show the mechanism. |
| A dependency-injection container and a logging framework | Six objects wired by hand are easier to read. |

## What next

If the pilot earns a second week: read `git diff` directly, run it on a real team's pull requests
with the simulated provider to tune the rubric, then switch on the Claude Code provider for that team
once security has approved a route, and compare the two reviews side by side.
