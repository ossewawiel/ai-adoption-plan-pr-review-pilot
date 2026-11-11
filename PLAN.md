> 📎 **Illustrated edition:** [docs/plan.html](docs/plan.html) · [PDF](docs/plan.pdf). Same plan, with pictures.

# AI in D&E: make it work, then make it better

## 🛠️ Where I'm coming from

I've spent the past eight months doing exactly this at my current employer, replacing an old C++ desktop client with a web application. The first six weeks or so held no product code: proofs of concept, design guidelines, and a showcase that won the go-ahead. Then the numbers moved, and they climbed every month as we put more rules and checks around the AI.

| 8 | 3,437 | 85 → 1,165 | 4 lanes · 18 gates |
|:---:|:---:|:---:|:---:|
| contributors | commits | commits a month, May to October | between an agent's change and the main branch |

> **The honest caveat.** That was eight people on brand-new code, in Kotlin. You have 60 developers on .NET systems with history. So I checked the lesson against outside evidence.

## 🔎 What the evidence says

- **AI is an amplifier.** [DORA 2025](https://services.google.com/fh/files/misc/2025_state_of_ai_assisted_software_development.pdf) asked nearly 5,000 people. AI magnifies what a strong organisation does well and what a struggling one does badly. Throughput goes up; so does instability.
- **Feeling fast isn't being fast.** In [METR's early-2025 trial](https://arxiv.org/abs/2507.09089), experienced developers reckoned AI had made them 20% faster. The stopwatch said they took 19% longer. METR now calls that result out of date, but the lesson holds: measure, don't guess.
- **The ones who got far built the rails first.** [Spotify](https://engineering.atspotify.com/2025/11/spotifys-background-coding-agent-part-1) has merged more than 1,500 agent-written pull requests, starting with migrations. At [Stripe](https://stripe.dev/blog/minions-stripes-one-shot-end-to-end-coding-agents-part-2), over 1,300 a week are agent-written, each reviewed by a person.

## 🧰 The stance: one company plugin

Picture a developer's first morning. They install one thing, and from then on their AI assistant knows our standards, refuses to open the secrets file, and reports what it costs. That one thing is a Claude Code **plugin**: rules, skills (ready-made recipes for a task) and automatic checks, owned by a small AI platform team and locked on by central settings. It's the toolbox bolted to every bakkie: same tools, same safety gear, whoever's driving. The standard is enforced by the tool, not by a memo.

> [!IMPORTANT]
> **North star: agents write. People own the specs, the architecture and the review.** A person stays accountable at every step, and speed without gates costs you stability.

## 🗺️ 1a. Roadmap, and the first 90 days up close

| When | What ships |
|---|---|
| 🌱 Month 3 | **Plugin 1, the strong starter,** on a pilot group: review and test skills, BA skills for Scrum and Jira, guard rails, the metrics export, and an *Issue lane* (an agent makes a small fix, a person approves it) |
| 🌿 Month 6–9 | Plugin 1 on every team. **Plugin 2:** design-first pipelines for small UI and API work |
| 🌳 Month 12 | **Plugin 3** on pilot teams: the full *Feature lane*. One C++ to C# migration stream starts |
| 🤝 Month 18 | **The promise:** small fixes, tests and migrations are agent-written and human-approved on every team |
| 🏆 Beyond | **The prize:** spec to pull request, agent-driven by default. No date on this one |

| Weeks 1–2 · measure | Weeks 3–6 · start small | Weeks 7–12 · prove it |
|---|---|---|
| Baseline from git, Jira and the pipelines. First survey. The security pack goes in. | A pilot group of 6 to 8: one product team with its QA and BA. Plugin 1 in daily use. First workshop. | The Issue lane runs. First scorecard against the baseline. Second survey. Then we decide: widen, adjust or stop. |

**Why start at review and testing?** Copilot already helps there, so it's a small first step. Gates must exist before the speed arrives: you get the fire right before the meat goes on. And the pilot in this repo *is* a review tool, a working piece of Plugin 1.

**Build, buy or SaaS?** Buy the model and the agent runtime. Build only the thin layer that holds *our* standards: the plugin. Take SaaS only when the vendor passes the same security pack. Three questions settle it: does it set us apart, does data leave South Africa, and what does it cost to leave?

## 🔐 1b. Rolling out Claude Code under strict security

Security's first question will be *"what leaves the building?"*, so the pack starts there:

1. A picture of exactly what leaves a developer's machine, and where it goes.
2. The vendor's retention terms, in writing.
3. Our answer to POPIA section 72. Personal information may not go to a third party in a foreign country unless a listed ground applies, and location information counts as personal. Plan on the model running outside South Africa, so **no personal information reaches a prompt**. The pilot shows how.
4. The managed settings: what the agent may read and run, the plugin lock, the usage export.
5. The pilot's scope, and how we switch it off.

| Risk I expect | What stops it |
|---|---|
| Personal data or secrets in a prompt | A redaction step, and a hook that blocks the read |
| Instructions hidden in repo content | Code is treated as data; a person reviews every change |
| An agent with too many permissions | Managed settings: a short allow-list, no bypass mode |
| Runaway cost | A model pinned to each job; spend on the scorecard |
| Tools nobody approved | Only the approved plugin source is allowed |

**The house rules.** ① No personal information or secrets in a prompt. ② A person reviews and owns every change; an agent never approves its own work. ③ Agent code passes the same pipeline gates as everyone else's. ④ Approved tools, models and plugins only. ⑤ Say when an agent wrote it.

**Making them stick.** Strongest first: 🔒 managed settings nobody can override → 🪝 hooks that block → 🚧 gates that fail a build → 👀 review by a person. A missing rule gets a rule and a check, not a reminder email.

**Rollout and stepping out.** The pilot group learns on its own tickets, not on slides. Each later team gets a champion the pilot group trained. I'm done when: ✅ D&E people run the plugin releases ✅ every team has a champion ✅ someone else publishes the scorecard.

**Bring the people along.** 📰 *Weekly*, a short bulletin: AI drafts it from the merge history, a person presses send, and it sounds like us. It teases the work, never the person. 🛠️ *Monthly*, a workshop: one thing that worked, one that flopped. 🌡️ *Quarterly*, a survey on confidence, friction and trust.

**Copilot.** It stays until Claude Code is approved. The pilot group runs both for one cycle and reports the difference. Teams move over at licence renewal, so nobody pays twice for long.

**Design.** Plugin 2 carries design skills: one library of screen building blocks and brand rules, plus a critic step, so AI-built screens look like one product.

## 📊 1c. Measuring adoption, value, quality and cost

| Axis | What we track |
|---|---|
| 👥 Adoption | Active users each week; teams on the plugin |
| ⚙️ Usage | Lane runs; the mix of small, middle and large models |
| 🚀 Value | Lead time for a change; review turnaround |
| 🛡️ Quality | Change failure rate; escaped defects; gate pass rate; share of agent pull requests accepted |
| 💰 Cost | Spend per developer; cost per merged change |
| 🌡️ Feeling | The quarterly survey |

Borrowed from DORA and SPACE, plus what only AI work needs. Each team sees its own card. No league table; this isn't the Currie Cup.

**Baseline and return.** Four to six weeks of history set the baseline, and the pilot group is compared with teams that haven't started. Then: `return = hours saved × loaded cost − licences − platform team`. I report measured speed, not felt speed.

**Helping a team that struggles.** 🚦 A red robot on a team's card means "send help", not "send blame". Low use with low trust gets pairing time. High use with a rising failure rate gets tighter gates. High cost per change gets a look at the model mix.

**Tiered models are the cost lever.** Price follows model size: on short prompts Sonnet 5.5 costs about 20× Haiku 5.5, and Opus 5.5 about 40× ([Anthropic's price list](https://platform.claude.com/docs/en/about-claude/pricing), 11 November 2026). So the plugin pins a size to each job: small runs checks, middle does daily work, large plans and judges. My project runs 7 small, 9 middle and 4 large agents.

**What I'm assuming.** Security approval lands by about week 3; if later, the dates slide and the pilot group applies the same standards with Copilot. Tests and pipelines are in fair shape; if not, tests come first. The company holds customer location data, so POPIA applies; compliance confirms any other regulator. The milestones run to month 12; month 18 is where they lead.
