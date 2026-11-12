# How I used AI, and the prompts I used

I built this assignment with Claude Code, on purpose and in the open. The plan argues that agents
write and people own the specs, the architecture and the review, so that is how I worked. This file
is the record: the key prompts in order, with one line on why each one was there.

Names of the hiring company, the recruiter, my employer's product and my private folders are
replaced with [placeholders]. My typing is left as it was.

## The method in four steps

1. **Interrogate before building.** Research the problem, then make the AI ask me the questions that
   change the answer, until one shape is left.
2. **Decide.** Every decision in the brief is mine. The AI lays out the options; I pick.
3. **Build with check-ins.** One build prompt with hard rules, and stops where I review the work.
4. **Verify.** Every quoted figure checked against its primary source. Tests and the tool run for
   real. I read every file before I defend it.

## Step 1 and 2: the interrogation

Before any code, I ran a planning session with a custom Claude Code command of mine that does
"deep interrogation": it reads the material, pulls primary sources, sketches candidate options, and
asks questions as cards I answer on a page. It ran for five turns and 25 questions. Options were
eliminated by my answers, not by the AI.

I did not keep the exact wording of that session's opening prompt, so I am not reconstructing it
here. What it produced is in the repository in full: [`docs/process/interrogation-brief.html`](docs/process/interrogation-brief.html),
a scrubbed copy of the brief with every question, the answer I picked, and my own notes.

Three of my answers, as examples of the kind of steering involved:

| Question | My answer | Why it mattered |
|---|---|---|
| Win outright or pass safely? | "my own experience already leading and fast tracking a team and building up the harnessing and ai structure will take the strong here. i can be opinianated on it" | It set the plan's shape: one bold stance on a safe base. |
| How deep does the provider abstraction go? | "i am out of touch with .net and C#. part of this is to show that in the end the language and libraries used do not matter. the solid engineering princeples do." | It chose the layered pipeline, and it is why the code is small and explained. |
| Which month-18 state does the plan promise? | "last option is what we aim for but we have to be realistic about it ... so bounded lanes." | It turned an ambitious north star into a promise I can sign. |

One place the AI pushed back and I accepted it: I wanted to say HTML documents are "already widely
adopted by strong AI-first companies". It found one engineer's argument and no adoption data, so the
plan shows the practice and does not claim the fact.

## Step 3: the build

### Prompt 1: the build brief

Why: one prompt that hands over the spec, states the rules that must hold throughout, and names the
two points where I want to see the work before it goes further.

```text
Build my take-home assignment in this folder. The assignment is AI_Lead_Assignment.pdf.

Start by reading two things, in full:
1. The interrogation brief at [link to the brief]
   (read it with the Artifact tool). Its Recommendation section is the build spec.
   The 25 answered questions are my decisions. Don't reopen them.
2. The project memory for this folder.

Then build the repository the Recommendation describes: PLAN.md, SOLUTION.md,
README.md, PROMPTS.md, the .NET 8 / C# 12 review assistant with its tests and a
sample change set, the mini plugin, the illustrated edition of the plan (HTML and
PDF), and a scrubbed copy of the brief. Follow its order of work and its cut order.

Rules that hold throughout:
- The repo will be public. Never name my employer's product, its owner, or the
  hiring company. Use the anonymised wording and numbers from the brief. No private
  paths, email content or people's names anywhere.
- PLAN.md is for a Head of D&E who is smart but not an AI specialist: three pages
  at most, relaxed South African English, illustrative, light on jargon. Seasoning,
  not the meal.
- The brief lists figures to check before quoting. Check each against its primary
  source. If you can't confirm one, leave it out and tell me. No invented numbers.
- The tool runs locally with no network by default. The simulated provider is the
  default; the Claude Code provider is opt-in.
- I have to explain this code in an interview and I'm not a C# specialist. Keep it
  small and readable, and put a file-by-file map and a "not built, and why" list in
  SOLUTION.md.
- Run the tool and the tests yourself and show me the review file it produces.
- git init and commit locally in small steps. Don't create a remote or push until
  I say so.

Check in with me twice: when the pilot runs green with the simulated provider, and
when the PLAN.md draft is ready to read. Otherwise keep going.

Finish with a short outline for the 5 to 10 minute video: the demo, the
architecture and the extension point, the plan's key points, and what I chose not
to build.

Before anything else, run dotnet --version and tell me what it prints.
```

### Prompt 2: after the first check-in

Why: the pilot ran green and the review file looked right, so the plan could start.

```text
carry on with PLAN.md
```

### Prompt 3: a fact only I could supply

Why: the AI had flagged two figures in my own story it could not confirm. I confirmed one and added
context it could not have known.

```text
the 6 weeks is confirmed but mention the prior year and a lave using it in personal capacity to
improve productivity and learn and gain experience the hard way. carry on
```

### Prompt 4: the second fact only I could supply

Why: the plan said "8 contributors" from a git count, and the AI kept flagging that it could not
tell whether that meant eight people. Who they are turned out to be the better story.

```text
the eight contributers. you can mention 1 is junior that needed upskilling and guidence regarding
ai where the harnes as plugin was used to get the ball rolling. the 4 senior devs got on the
programm quickly understood the harness and helped eventually to shape it further. i am the fith.
then 2 non developers the product manager or operations director and the team manager both now
contribute via the harness not having to know or understand the code architecture or harness
itself.
```

And a follow-up, once I had read the result:

```text
just mention me as the project manager leading the operation as part of the 8, also doing regular
stake holder engagement meetings with the executive team
```

## Step 4: the verification prompts

The build prompt says "check each figure against its primary source". Claude did that by briefing
three research sub-agents. This is one of those briefs, shortened. The pattern is the useful part:
name the primary source, demand a quote and a status, forbid guessing.

```text
I am writing a public document that may quote research figures. Each figure must be
confirmed against its PRIMARY source (the report or paper itself, not press coverage).
For each item give: the figure or wording, the exact URL of the primary source where
you saw it, a short verbatim quote, and a status: CONFIRMED-PRIMARY, COVERAGE-ONLY
(found only in secondary coverage — name it), or NOT FOUND. Never guess, never fill in
from memory. If a PDF cannot be opened, say so.

1. DORA 2025 "State of AI-assisted Software Development" report: number of respondents;
   the "AI is an amplifier" statement; findings on throughput vs. instability ...
3. METR 2025 randomised trial: the 19% slowdown, number of developers and tasks ...
   Also check whether METR published any later follow-up that changes the result.
```

What the checks changed:

| Check | Result | What I did |
|---|---|---|
| METR's 19% slowdown | Confirmed, but METR now marks the result "out of date" | The plan says so, and keeps only the felt-versus-measured lesson |
| A vendor framework said to recommend quarterly surveys | The framework document does not say it | The plan keeps the survey as my practice, with no citation |
| Stripe's agent pull requests | "Over 1,300" a week, not "about 1,300" | Wording fixed |
| A managed-settings key | It sits under `permissions`, not at the top level | Sample settings fixed |
| Three open-source review tools I meant to name | Not verified | Left out |

## The prompt inside the tool

The review assistant has a prompt of its own: the rubric and reply format in
[`src/ReviewAssistant.Core/ReviewPromptBuilder.cs`](src/ReviewAssistant.Core/ReviewPromptBuilder.cs).
It is short on purpose, so a team can read it and argue with it. The same rubric is the skill in
[`plugin/company-starter/skills/review-dotnet-change/SKILL.md`](plugin/company-starter/skills/review-dotnet-change/SKILL.md),
and a test fails if the two drift apart.

## What the AI did, and what I did

| The AI | Me |
|---|---|
| Research, with sources | Chose the stance and every one of the 25 decisions |
| Wrote the C#, the tests, the plugin and the first drafts of every document | Set the rules, the scope and the cut order |
| Ran the build, the tests and the tool, and checked the page count of the plan | Reviewed at each check-in and sent work back |
| Flagged what it could not confirm | Supplied and confirmed the facts about my own work |

The git history shows the same thing in small steps. The commits carry a co-author line where the AI
wrote the change.
