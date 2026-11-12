> 📎 **Illustrated edition:** [docs/plan.html](docs/plan.html) · [PDF](docs/plan.pdf). Same plan, with pictures.

# AI in D&E: make it work, then make it better

## Where I'm coming from

For a year and a half I used AI coding tools in my own time, and I learned the hard way what works and what does not. For the past eight months I have been doing it at my current employer, where we are busy replacing an old C++ desktop client with a web application. In the first six weeks we wrote no product code. We built proofs of concept, design guidelines and a showcase that convinced management to go ahead. After that the numbers started to climb, every month, as we put more rules and checks around the AI.

| 8 | 3,437 | 85 → 1,165 | 4 lanes · 18 gates |
|:---:|:---:|:---:|:---:|
| contributors | commits | commits a month, May to October | between an agent's change and the main branch |

**Who the eight are.** I am the project manager and I lead the work, with regular stakeholder meetings with the executive team. Four senior developers got on the programme quickly and now help to shape the plugin that carries those rules. One junior needed upskilling, and the plugin got the ball rolling there. The other two are not developers, a product lead and the team manager, and they contribute through the plugin without having to know the code.

> **To be honest about it.** That was eight people on new code, in Kotlin. You have 60 developers on existing .NET systems. So I did not want to trust only my own experience, and I went to look at the hard evidence.

## What the evidence says

- AI makes a team more of what it already is. [DORA 2025](https://services.google.com/fh/files/misc/2025_state_of_ai_assisted_software_development.pdf) asked nearly 5,000 people. Strong organisations get stronger with AI and struggling ones see their problems grow. Teams deliver more, but delivery also becomes less stable.
- Feeling faster is not the same as being faster. In [METR's trial](https://arxiv.org/abs/2507.09089) early in 2025, experienced developers believed AI made them 20% faster, but the measurements showed they took 19% longer. METR says the result is out of date by now. The lesson I take from it is that we must measure.
- The companies that got far built the rails first. [Spotify](https://engineering.atspotify.com/2025/11/spotifys-background-coding-agent-part-1) has already merged more than 1,500 pull requests written by agents, and it started with migrations. At [Stripe](https://stripe.dev/blog/minions-stripes-one-shot-end-to-end-coding-agents-part-2) it is more than 1,300 a week, and a person reviews every one.

## The stance: one company plugin

Think of a new developer on the first morning. They install one thing, and from then on their AI assistant already knows our standards, will not open the secrets file, and reports what it costs. That one thing is a Claude Code **plugin**: a package of rules, skills (ready-made recipes) and automatic checks. A small AI platform team owns it, and central settings keep it switched on. It is like the toolbox that is bolted to every bakkie, with the same tools and safety gear no matter who drives. In this way the tool itself enforces the standard, and we do not have to rely on a memo.

> [!IMPORTANT]
> **North star: agents write the code, and people own the specs, the architecture and the review.** A person stays accountable at every step, and the evidence shows that speed without gates costs stability.

## 1a. Roadmap, and the first 90 days up close

| When | What ships |
|---|---|
| 🌱 Month 3 | **Plugin 1, a strong starter,** for a pilot group: review and test skills, BA skills for Scrum and Jira, guard rails, the metrics export, and an *Issue lane* (an agent makes a small fix and a person approves it) |
| 🌿 Month 6–9 | Plugin 1 on every team. **Plugin 2:** pipelines for small UI and API work where the design comes first, with design skills (shared building blocks, brand rules, a critic step) so that screens built by AI still look like one product |
| 🌳 Month 12 | **Plugin 3** on pilot teams, with the full *Feature lane*. One C++ to C# migration stream starts |
| 🤝 Month 18 | **The promise:** on every team, small fixes, tests and migrations are written by agents and approved by people |
| 🏆 Beyond | **The ultimate prize:** from spec to pull request, done by agents by default. At this stage I am not putting a date on it |

| Weeks 1–2 · measure | Weeks 3–6 · start small | Weeks 7–12 · prove it |
|---|---|---|
| We take the baseline from git, Jira and the pipelines, run the first survey and hand in the security pack. | A pilot group of 6 to 8 starts: one product team with its QA and BA. They use Plugin 1 every day, and we hold the first workshop. | The Issue lane runs. We publish the first scorecard against the baseline, run the second survey, and then decide to widen, adjust or stop. |

**Why start at review and testing?** Copilot already helps there, so it is a small first step. The gates must also be in place before the speed arrives. It is like a braai, where you get the fire right before the meat goes on. And the pilot in this repo is a review tool, so it is already a working piece of Plugin 1.

**Build, buy or SaaS?** We buy the model and the agent runtime. We build only the thin layer that holds our own standards, and that is the plugin. We take SaaS only when the vendor passes the same security pack. Three questions decide it: does it set us apart, does data leave South Africa, and what will it cost us to leave again?

## 1b. Rolling out Claude Code under strict security

The first thing security will ask is what leaves the building, so the pack starts there:

1. A picture of exactly what leaves a developer's machine, and where it goes.
2. The vendor's retention terms, in writing.
3. Our answer to POPIA section 72. Personal information may not go to a third party in a foreign country unless one of the listed grounds applies, and location information counts as personal. We must plan for the model running outside South Africa, so **no personal information may reach a prompt**. The pilot shows how.
4. The managed settings: what the agent may read and run, the plugin lock, and the usage export.
5. The scope of the pilot, and how we switch it off.

**The risks I expect** are personal data or secrets in a prompt, instructions hidden in repo content, an agent with too many permissions, cost that runs away, and tools that nobody approved. The pack names a control for each one.

**The house rules.** ① No personal information or secrets in a prompt. ② A person reviews and owns every change, and an agent never approves its own work. ③ Agent code goes through the same pipeline gates as everybody else's. ④ Only approved tools, models and plugins. ⑤ Say so when an agent wrote it.

**Making them stick.** From strongest to weakest: 🔒 managed settings → 🪝 hooks that block → 🚧 build gates → 👀 human review. When a rule is missing, we add the rule and a check for it.

**Rollout and stepping out.** The pilot group learns by doing its own tickets with the plugin. Each new team then gets a champion trained by the pilot group. I can step out when: ✅ D&E people run the plugin releases ✅ every team has a champion ✅ somebody else publishes the scorecard.

**Bring the people along.** 📰 *Every week* a short bulletin goes out. AI drafts it from the merge history, a person sends it, and it sounds like us. It may tease the work but never the person. 🛠️ *Every month* a team shows one thing that worked and one that did not, in a workshop. 🌡️ *Every quarter* we run a survey on confidence, friction and trust.

**Copilot.** It stays until Claude Code is approved. The pilot group then uses both for one cycle and reports the difference. Teams move over at licence renewal, so nobody pays double for long.

## 1c. Measuring adoption, value, quality and cost

| Axis | What we track |
|---|---|
| 👥 Adoption | Active users each week; teams on the plugin |
| ⚙️ Usage | Lane runs; the mix of small, middle and large models |
| 🚀 Value | Lead time for a change; review turnaround |
| 🛡️ Quality | Change failure rate; escaped defects; gate pass rate; share of agent pull requests accepted |
| 💰 Cost | Spend per developer; cost per merged change |
| 🌡️ Feeling | The quarterly survey |

We borrowed from DORA and SPACE and added what only AI work needs. Each team sees its own card, and there is no league table. This is not the Currie Cup.

**Baseline and return.** Four to six weeks of history give us the baseline, and we compare the pilot group with teams that have not started. Then `return = hours saved × loaded cost − licences − platform team`.

**Helping a team that struggles.** 🚦 A red robot on a team's card means the team needs help. It is not there to blame anybody. With low use and low trust, we pair with them. With high use and more failures, we tighten the gates. With a high cost per change, we look at the model mix.

**Tiered models are the cost lever.** On short prompts Sonnet 5.5 costs about 20× what Haiku 5.5 costs, and Opus 5.5 about 40× ([Anthropic's price list](https://platform.claude.com/docs/en/about-claude/pricing), 11 November 2026). That is why the plugin pins a size to each job: small for checks, middle for daily work, and large to plan and judge. My project runs 7 small, 9 middle and 4 large agents.

**What I am assuming.** Security approval comes by about week 3. If it is later, we make a plan: the dates move and the pilot group applies the same standards with Copilot. Tests and pipelines are in a fair state. If not, tests come first. The company holds customer location data, so POPIA applies, and compliance must confirm any other regulator.
