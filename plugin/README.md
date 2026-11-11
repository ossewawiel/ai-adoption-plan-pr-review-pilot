# The mini plugin

`PLAN.md` argues for one company plugin that every team's Claude Code loads. This folder is a small,
working piece of it, so the idea is in the repository and not only on paper.

| Path | What it is |
|---|---|
| `.claude-plugin/marketplace.json` | A one-plugin marketplace. This is what the company would host in its own repository. |
| `company-starter/` | The plugin itself. |
| `company-starter/skills/review-dotnet-change/SKILL.md` | One skill: the same review rubric the .NET tool uses. A test keeps the two in step. |
| `company-starter/hooks/` | One guard-rail hook. It blocks any tool call that would touch a `.env` file, a secrets folder, a certificate or key, or production settings. |
| `samples/managed-settings.sample.json` | Sample managed settings: how the company installs the plugin for everyone and locks it on. |
| `samples/metrics-export.sample.json` | A sample of the usage export, with made-up values. |

## Try it

In a Claude Code session, from the repository root:

```
/plugin marketplace add ./plugin
/plugin install company-starter@company-plugins
```

Then ask Claude to open a `.env` file. The hook blocks the call and tells the agent why. Ask it to
"review this branch" and the skill takes over. The hook needs Node.js on the path.

You can also check the structure without installing anything: `claude plugin validate ./plugin`.

## The hook, without Claude Code

Claude Code hands a hook the tool call as JSON on standard input. Exit code 2 blocks the call.

```
echo '{"tool_name":"Read","tool_input":{"file_path":".env"}}' | node plugin/company-starter/hooks/block-sensitive-files.js
```

The hook is a second lock, not the only one. The managed settings below deny the same reads at the
permission layer, which a user cannot override.

## The managed settings sample

An administrator delivers this file through the admin console, a device-management policy, or a file at
a system path (on Windows `C:\Program Files\ClaudeCode\managed-settings.json`). Users cannot override it.

| Key | What it does |
|---|---|
| `extraKnownMarketplaces` | Registers the company's marketplace on every machine. |
| `enabledPlugins` | Force-enables the starter plugin. A user cannot switch it off. |
| `strictKnownMarketplaces` | Allows plugins from the company's marketplace only. |
| `permissions.deny` | Blocks reads of secret files at the permission layer. |
| `permissions.disableBypassPermissionsMode` | Stops anyone from running without permission checks. |
| `allowManagedPermissionRulesOnly` | Makes these the only permission rules that apply. |
| `env` | Switches on the usage export and points it at the company's collector. |

I checked the key names against the Claude Code documentation on 11 November 2026. Two things I did
not confirm there and would check before a real roll-out: the exact shape of a `strictKnownMarketplaces`
entry, and the `env` block as the way to set the export variables. The repository name and the collector
address are placeholders.

## The metrics sample

Claude Code can export usage over OpenTelemetry. The sample shows one team's week and how it feeds the
scorecard in `PLAN.md`. The values are made up. The cost figures in a real export are estimates.

| Metric | Scorecard axis |
|---|---|
| `claude_code.session.count`, `claude_code.active_time.total` | Adoption |
| `claude_code.cost.usage` by `model` | Usage (the model mix) and Cost |
| `claude_code.cost.usage` by `plugin.name` and `skill.name` | Usage (which skills earn their keep) |
| `claude_code.token.usage` by `type` | Cost |
| `claude_code.commit.count`, `claude_code.pull_request.count` | Value, next to lead time from git and Jira |

Quality and Feeling do not come from this export. They come from the pipelines and the quarterly survey.

## What is not here

The real Plugin 1 also carries test skills, BA skills for Scrum and Jira, and an Issue lane. Those are
described in `PLAN.md` and not built.
