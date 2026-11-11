> **Illustrated edition:** [docs/plan.html](docs/plan.html) · [PDF](docs/plan.pdf). Same plan, with pictures.

# AI in D&E: make it work, then make it better

## Where I'm coming from

For eight months I have run this play at my current employer, replacing a legacy C++ desktop client with a web application. About six weeks went into preparation: proofs of concept, design guidelines, and a showcase that won the go-ahead. Since then eight contributors have made 3,437 commits. Commits per month grew from 85 in May to 1,165 in October, rising every month as we added rules and checks around the AI. The work runs in four agent "lanes", with 18 quality gates between a change and the main branch.

The honest caveat: that was eight people on new code, in Kotlin. You have 60 developers on existing .NET systems. So I tested the lesson against outside evidence.

## What the evidence says

- **AI amplifies what is already there.** [DORA 2025](https://services.google.com/fh/files/misc/2025_state_of_ai_assisted_software_development.pdf), from nearly 5,000 respondents: AI magnifies a strong organisation's strengths and a struggling one's dysfunctions. Throughput improves; instability still rises.
- **Feeling fast is not being fast.** In [METR's early-2025 trial](https://arxiv.org/abs/2507.09089), experienced developers took 19% longer with AI and believed they were 20% faster. METR now calls the result out of date; the gap between felt and measured speed is the lesson.
- **Those who got far built the rails first.** [Spotify](https://engineering.atspotify.com/2025/11/spotifys-background-coding-agent-part-1) has merged more than 1,500 agent-written pull requests, starting with migrations. At [Stripe](https://stripe.dev/blog/minions-stripes-one-shot-end-to-end-coding-agents-part-2), over 1,300 a week are agent-written, each reviewed by a person.

## The stance: one company plugin

A small AI platform team owns one Claude Code **plugin**: a package of rules, skills (ready-made recipes for a task) and automatic checks. Every team's AI tool loads it at start-up, and central settings lock it on. Think of the toolbox bolted to every bakkie: same tools, same safety gear, whoever is driving. The standard is enforced by the tool, not by a memo.

**North star: agents write; people own the specs, the architecture and the review.** Accountability stays with a person at every step, and the evidence says speed without gates costs you stability.

## 1a. Roadmap

| When | What ships |
|---|---|
| Month 3 | **Plugin 1, the strong starter,** on a pilot group: review and test skills, BA skills for Scrum and Jira, guard rails, the metrics export, and an *Issue lane* (an agent implements a small fix, a person approves it) |
| Month 6–9 | Plugin 1 on every team. **Plugin 2:** design-first pipelines for small UI and API work |
| Month 12 | **Plugin 3** on pilot teams: the full *Feature lane*. One C++ to C# migration stream starts |
| Month 18 | **The promise:** small fixes, tests and migrations are agent-written and human-approved on every team |
| Beyond | **The prize:** spec to pull request, agent-driven by default. No date on this one |

**The first 90 days**

- **Weeks 1–2.** Baseline from git, Jira and the pipelines. First survey. The security pack goes in.
- **Weeks 3–6.** A pilot group of 6 to 8: one product team with its QA and BA. Plugin 1 in daily use. First workshop.
- **Weeks 7–12.** The Issue lane runs. First scorecard against the baseline. Second survey. Decide: widen, adjust or stop.

**Why start at review and testing.** Copilot already helps there, so the first step is small. Gates must exist before speed arrives. And the pilot in this repository is a review tool: a working piece of Plugin 1.

**Build, buy or SaaS.** Buy the model and the agent runtime. Build only the thin layer that holds our own standards: the plugin. Take SaaS only when the vendor passes the same security pack. Three questions settle each case: does it set us apart, does data leave South Africa, and what does it cost to leave?

## 1b. Rolling out Claude Code under strict security

**What I put in front of security.** One pack:

1. A picture of exactly what leaves a developer's machine, and where it goes.
2. The vendor's retention terms, in writing.
3. Our answer to POPIA section 72. Personal information may not go to a third party in a foreign country unless a listed ground applies, and the Act counts location information as personal. Plan on the model running outside South Africa. So **no personal information reaches a prompt**: a redaction step and a blocking hook see to that. The pilot shows both.
4. The managed settings: what the agent may read and run, the plugin lock, the usage export.
5. The pilot's scope, and how we switch it off.

**Risks I expect:** personal data or secrets in a prompt; instructions hidden in repository content; an agent with more permissions than it needs; runaway cost; tools nobody approved. Each gets a named control in the pack.

**Usage standards.** Five rules:

1. No personal information or secrets in a prompt.
2. A person reviews and owns every change. An agent never approves its own work.
3. Agent-written code passes the same pipeline gates as any other code.
4. Approved tools, models and plugins only.
5. Say when an agent wrote it.

**Making them stick.** Four mechanisms, strongest first: managed settings a user cannot override, hooks that block an action, gates that fail a build, and review by a person. A missing rule is fixed with a rule and a check, not a reminder.

**Rollout and stepping out.** The pilot group learns on its own tickets, not on slides. Each later team gets a champion trained by the pilot group. I step out when D&E people run the plugin releases, every team has a champion, and someone else publishes the scorecard.

**Bring the people along.** Weekly, a short bulletin of what shipped: drafted by AI from the merge history, sent by a person, in the team's own South African voice. It teases the work, never the person. Monthly, a workshop: one thing that worked, one that did not. Quarterly, a survey on confidence, friction and trust.

**Copilot.** It stays until Claude Code is approved. The pilot group runs both for one cycle and reports the difference. Teams move at licence renewal, so nobody pays twice for long.

**Design.** Plugin 2 carries design skills: one library of screen building blocks and brand rules, plus a critic step, so AI-built screens look like one product.

## 1c. Measuring adoption, value, quality and cost

| Axis | What we track |
|---|---|
| Adoption | Active users each week; teams on the plugin |
| Usage | Lane runs; the mix of small, middle and large models |
| Value | Lead time for a change; review turnaround |
| Quality | Change failure rate; escaped defects; gate pass rate; share of agent pull requests accepted |
| Cost | Spend per developer; cost per merged change |
| Feeling | The quarterly survey |

Borrowed from DORA and SPACE, plus what only AI work needs. Each team sees its own card. No league table; this is not the Currie Cup.

**Baseline and return.** Four to six weeks of history before the pilot sets the baseline. The pilot group is compared with teams that have not started. Return is hours saved at loaded cost, less the licences and the platform team. I report measured speed, not felt speed.

**Helping a team that struggles.** A red robot on a team's card means "send help", not "send blame". Low use with low trust gets pairing time. High use with a rising failure rate gets tighter gates. High cost per change gets a look at the model mix.

**Tiered models are the cost lever.** On [Anthropic's price list](https://platform.claude.com/docs/en/about-claude/pricing) (11 November 2026), Opus 5.5 costs twice what Sonnet 5.5 does and about forty times Haiku 5.5 on short prompts. The plugin pins a model to each job: small for checks, middle for daily work, large to plan and judge. My project runs 7 small, 9 middle and 4 large agents.

**Assumptions.** Security approval lands by about week 3; if later, the dates slide and the order holds, and the pilot group applies the tool-neutral standards with Copilot until then. Tests and pipelines are in fair shape; if not, test coverage comes before any lane. The company holds customer location data, so POPIA applies; compliance confirms any other regulator, and security helps choose the route. The milestones run to month 12, as asked; month 18 is where they lead.
