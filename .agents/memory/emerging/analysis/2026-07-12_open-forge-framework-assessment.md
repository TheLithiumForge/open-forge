---
open-forge:
  description: Evidence-backed multi-perspective assessment of Open Forge, its evolution beyond SDD, expanded naming atlas, current risks, dogfood performance, and path to S++
  tags: [Memory, Analysis, Reasoning, Contextual, Candidate, Framework, Review, Dogfood, Benchmark, Security, Roadmap]
---

# Open Forge Assessment: From SDD To Context-Routed Development

Date: 2026-07-12. Revised: 2026-07-13 with an expanded naming atlas and implementable S++ decision options; implementation addendum 2026-07-17. Status: candidate analysis, not accepted current truth.

## Technical Summary

> **Snapshot notice:** The verdict and priority list in this opening section describe the 2026-07-12 implementation. Containment, in-process transaction rollback, dependency closure, collision handling, and Git review checkpoints were materially improved on 2026-07-17; read the [2026-07-17 Implementation Addendum](#2026-07-17-implementation-addendum) for current shipped state before using this snapshot to prioritize work.

Open Forge has evolved beyond spec-driven development. Its defining unit is no longer the specification; it is the smallest useful set of routed context for a goal. The best precise category name remains **Context-Routed Development**. The strongest organic companion is **Routed Context Cultivation**; the leading experimental frontier coinage is **Routecology**; and the authority language should be **human-governed** or **human-stewarded**, not human-like. Specifications are one possible #CurrentTruth source among decisions, directives, patterns, workflows, skills, evidence, and lifecycle memory.

The 2026-07-12 verdict was deliberately split:

- **Design kernel: A-.** The routing model, primitive boundaries, epistemic memory states, and plain-file reviewability are unusually coherent.
- **Shipped alpha: B.** The core works and the CLI is useful, but context resolution, mutation safety, update ownership, onboarding, and extension lifecycle are not release-grade.
- **Evidence base: C+.** The harness is candid and useful for engineering regression, but causal claims are weakened by one-model coverage, dirty provenance, unrecorded context isolation, single runs per cell, subjective scoring, and generation-11 collisions.
- **S++ readiness: B-.** Open Forge is an S-tier design candidate. It is not S-tier by its own rating philosophy because several important claims are not yet measured automatically or reproduced independently.

The best parts should remain: plain Markdown, bounded generated regions, recursive entrypoints, decision-grade descriptions, typed agent primitives, the four memory states, optional extensions, and the human-led philosophy.

The highest-priority problems in that 2026-07-12 snapshot were not missing features. They were trust boundaries:

1. `find --route` and Required Routes can escape the selected workspace through unchecked `..` paths.
2. `find --tag KeepInMind --bodies` is a global tag search, not the active loaded-parent closeout chain, and it omits overwrite companions.
3. install, extend, create, and index are not transactionally preflighted; a later failure can leave earlier mutations behind.
4. extension dependencies, collisions, provenance, update, remove, and rollback are unresolved.
5. the benchmark corpus supports strong product-fidelity signals, but several accepted synthesis claims do not survive raw-report reconciliation.

The shortest path to S++ is therefore a coherent deterministic stack rather than a feature pile: shared path and route-graph primitives; an explainable context resolver and receipt; a virtual-tree planner with rollback-capable apply; capability-scoped extension manifests and a visible lock; a lean base with additive profiles; claim-class promotion and advisory memory decay; and a tiered evidence program. Do not add more root routes, reserved tags, memory states, or hidden runtime truth.

## Naming The Paradigm: A Precise Core And An Organic Frontier

SDD makes a specification the primary contract that drives implementation. Open Forge now does something broader:

```text
intent
  -> select the smallest useful route set
  -> compose current truth, constraints, form, judgment, capability, and workflow
  -> execute and verify
  -> route candidate learning by epistemic state
  -> promote, supersede, or archive through human governance
  -> improve the next context selection
```

This loop is not spec-first. A specification may be a crystallized document, but directives, narrower patterns, decisions, source-of-truth maps, evidence, and workflow obligations may constrain or outweigh it. The architecture is closer to a **repository-native context protocol plus a governed learning lifecycle** than to a conventional workflow engine.

### One name cannot honestly carry six jobs

The framework has a mechanism, philosophy, product, loop, possible public movement, and possible non-software future. Compressing all six into one chain of adjectives produces names such as `Human-Led Organic Intent-Routed Context Development`: accurate, unusable, and impossible to remember.

The strongest naming architecture is layered:

| Layer | Recommended wording | Why it belongs here |
|---|---|---|
| Product | **Open Forge**, provisionally | The current CLI, payload, and extension ecosystem. Its public brand collision remains a separate decision. |
| Formal methodology | **Context-Routed Development** | Names the distinctive mechanism: intent selects relevant context routes instead of making one specification universally primary. |
| Broader organic practice | **Routed Context Cultivation** | Connects routing with deliberate growth and can later extend beyond software without claiming autonomous evolution. |
| Philosophy | **Organic growth, deliberate promotion** | Useful knowledge emerges from real work, while people govern what becomes current truth. |
| Operating loop | **Route → Work → Verify → Cultivate** | Says what practitioners actually do. `The Forge Loop` remains a usable product-specific nickname. |
| Experimental frontier field | **Routecology** | A coined umbrella for the ecology of selecting, growing, verifying, pruning, and governing context routes. It should be user-tested before public adoption. |

Recommended formal definition:

> **Context-Routed Development is a human-governed, repo-native practice in which intent selects the smallest useful set of plain-file context routes, work is verified, and useful learning is deliberately cultivated into—or kept out of—current truth.**

`Routed` must remain an honest structural term: people or agents select declared routes, and deterministic tooling can resolve those selections. The current runtime does **not** autonomously discover semantic relevance.

The strongest short positioning remains:

> **Open Forge practices Context-Routed Development: organic growth, deliberate promotion, and only the context the work needs.**

### The “human-like” vocabulary needs one correction—and reveals a useful new direction

`Androgynous` means combining conventionally masculine and feminine characteristics; it does not mean human-like. `Anthropomorphic` means assigning human qualities to something nonhuman, while `anthropic` means human-related but is inseparable in this market from [Anthropic](https://www.anthropic.com/company). Neither accurately describes Open Forge. `Humanoid` primarily describes bodily form. Those words would create a category with the wrong promise.

There is, however, a nearby word that may be what was intended: **endogenous** means arising from within a system. That is genuinely relevant. Open Forge's reusable context is meant to arise from work inside the repository rather than arrive as a frozen external specification. **Endogenous Context Development** is therefore an intellectually strong, if academic, candidate.

| Vocabulary | What it actually signals | Fit here |
|---|---|---|
| **Human-governed** | People retain authority over normative truth and promotion | Most accurate governance term |
| **Human-stewarded** | People cultivate, prune, and care for a living context system | Best warmer alternative |
| **Human-led** | People set intent and remain accountable | Accessible, but can overstate intervention in every step |
| **Human-centered** | Established human-factors/design orientation | Useful principle; too broad to name this mechanism |
| **Humanistic** | Human agency, values, dignity, and interpretation | Philosophically attractive; technically under-specific |
| **Humane** | Designed to reduce harm or burden | Good value claim only if the product proves it |
| **Cognition-inspired** | Borrows ideas from memory, attention, and situated reasoning | Good inspiration language; risks implying a cognitive architecture |
| **Cognition-scaffolded** | External files and routes scaffold reasoning without imitating a person | Best human-like-inspired formal alternate |
| **Enactive** | Knowledge develops through action and feedback | Conceptually close, but obscure outside cognitive science |
| **Endogenous** | Knowledge arises within the working system | Strong organic concept; not a synonym for human-like |
| **Symbiotic** | Humans and agents mutually adapt as peers | Aspirational; current tooling does not yet demonstrate mutual adaptation |
| **Stigmergic** | Work leaves durable traces that shape later work | Technically insightful research descriptor; too obscure and increasingly crowded for the main name |

The practical conclusion is: use **human-governed** for authority, **human-stewarded** for warmth, **cognition-scaffolded** for the human-like inspiration, and **endogenous** only when emphasizing that learning grows from work inside the system.

### Decision scorecard: twenty serious candidates

Scores are judgment aids, not measured preference or legal clearance. Each dimension is 1-5: `accuracy` names the implemented mechanism; `organic fit` expresses learning and lifecycle; `distinctiveness` creates a recognizable category; `category clarity` sounds like a development method; `memorability` measures conversational usability; `collision safety` reflects only lightweight current screening. The totals deliberately do not decide the recommendation: a methodology name must first name the methodology's defining job.

| Candidate | Accuracy | Organic fit | Distinctive | Category clarity | Memorable | Collision safety | Total / 30 | Best role or hidden cost |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| **Context-Routed Development** | 5 | 3 | 5 | 5 | 4 | 3 | **25** | Recommended formal category; spell it out because `CRD` is Kubernetes vocabulary. |
| **Routed Context Cultivation** | 5 | 5 | 5 | 3 | 3 | 5 | **26** | Best broader organic practice; less obviously a software-development method. |
| **Context-Grown Development** | 4 | 5 | 5 | 4 | 4 | 4 | **26** | Best warm formal alternate; hides routing and may sound automatically self-growing. |
| **Relevance-Routed Development** | 4 | 2 | 5 | 5 | 4 | 5 | **25** | Strong minimalist challenger; leaves the thing being routed implicit. |
| **Human-Governed Context Development** | 4 | 4 | 5 | 4 | 3 | 5 | **25** | Best authority-first alternate; long and omits routing. |
| **Just-Enough Context Development** | 3 | 2 | 5 | 5 | 5 | 5 | **25** | Best minimalism-first phrase; “enough” is an unproved outcome claim. |
| **Human-Stewarded Context Development** | 4 | 5 | 5 | 4 | 3 | 5 | **26** | Best warm human-governance name; mechanism is implicit. |
| **Flow-Routed Development** | 3 | 4 | 5 | 5 | 4 | 5 | **26** | Best natural-flow construction; “flow” obscures the typed context graph. |
| **Context-Cultivated Development** | 4 | 5 | 4 | 4 | 3 | 4 | **24** | Deliberate growth without autonomy; slightly awkward modifier direction. |
| **Context Wayfinding Development** | 4 | 3 | 5 | 4 | 5 | 4 | **25** | Natural, human navigation metaphor; `CWD` also means current working directory. |
| **Current-Guided Development** | 3 | 4 | 5 | 5 | 4 | 5 | **26** | Clever double meaning: current truth plus natural current; does not name routing. |
| **Warranted Context Development** | 3 | 4 | 5 | 5 | 4 | 5 | **26** | Excellent evidence/promotion language; selection mechanics disappear. |
| **Intent-Routed Context Development** | 5 | 2 | 5 | 4 | 3 | 5 | **24** | Most literal `intent → route → context` name; cumbersome in speech. |
| **Work-Grown Context Development** | 4 | 5 | 5 | 4 | 3 | 5 | **26** | Precisely says context grows from real work; word order is unfamiliar. |
| **Context-on-Demand Development** | 4 | 2 | 4 | 5 | 5 | 4 | **24** | Clear pull model; sounds like an automatic retrieval service. |
| **Repo-Native Context Development** | 4 | 3 | 4 | 5 | 3 | 5 | **24** | Honest current architecture; describes substrate more than method. |
| **Cognition-Scaffolded Development** | 3 | 3 | 5 | 5 | 4 | 5 | **25** | Best human-like-inspired candidate; routing and lifecycle are absent. |
| **Endogenous Context Development** | 3 | 5 | 5 | 4 | 3 | 4 | **24** | Deeply aligned with internally grown learning; academic and acronym-crowded. |
| **Epistemic Wayfinding** | 4 | 4 | 5 | 3 | 4 | 4 | **24** | Strong research umbrella; requires explanation and does not sound like SDD's successor. |
| **Routecology** | 3 | 5 | 5 | 2 | 4 | 5 | **24** | Best coined frontier field; ownable-feeling, but unfamiliar and pronunciation varies. |

### The expanded naming atlas

The scorecard contains decision candidates. The atlas below is intentionally much wider: more than one hundred constructions spanning conservative, organic, human-inspired, and deliberately strange directions. Many are better taglines, loop names, essays, packages, or research vocabulary than formal methodology names.

| Family | Candidate territory | What the family foregrounds |
|---|---|---|
| **Route and mechanism** | Context-Routed Development; Routed-Context Development; Relevance-Routed Development; Intent-Routed Development; Intent-Routed Context Development; Goal-Routed Development; Task-Routed Development; Demand-Routed Development; Need-Routed Context Development; Repository-Routed Development; Route-Guided Development; Context-Composed Development; Context Wayfinding Development; Path-of-Relevance Development; Development through Routed Context | The real differentiator: intent selects a route set and context is composed from declared sources. |
| **Minimal and on demand** | Context-on-Demand Development; Just-Enough Context Development; Minimum-Relevant-Context Development; Relevance-Bounded Development; Purpose-Bounded Development; Intent-Scoped Development; Selective-Context Development; Scoped-Relevance Development; Context-Lean Development; Least-Context Development; Pull-Context Development; Just-in-Context Development; Precision-Context Development; Context-Sparse Development | The minimalist promise. Most require evidence before “enough,” “minimum,” or “efficient” becomes a public claim. |
| **Natural flow** | Natural-Flow Development; Flow-Routed Development; Context in Flow; Context-Flow Development; Routed Context Flow; Current-Guided Development; Current-Fed Development; Streamfed Context Development; Tributary Development; Headwater Development; Riverbed Method; Flow Before Form; Work Follows Context; Path of Least Context; Contextstream Development; Gradient-Guided Development | Movement, pull, and graceful selection. These can falsely suggest linearity or effortless self-organization. |
| **Organic growth** | Context-Grown Development; Context-Cultivated Development; Work-Grown Context Development; Evidence-Grown Development; Warranted Context Development; Routed Context Cultivation; Deliberate Context Growth; Context Ecology Method; Living Context Development; Context Lifecycle Development; Context Ontogeny; Repo Ontogeny; Endogenous Context Development; Truth-Cultivated Development; Root-and-Route Development; Mycelial Context Development; Rhizomatic Context Development; Dendritic Development; Organismic Context Development; Regenerative Context Development | Emergence, cultivation, branching, decay, and reuse. Governance language is needed to prevent autonomy or uncontrolled-growth implications. |
| **Human governance and cognition** | Human-Governed Context Development; Human-Stewarded Context Development; Human-Led Context-Routed Development; Human-Steered Living Development; Context Stewardship Practice; Governed Coagency Development; Coagentic Context Development; Deliberative Context Development; Cognition-Scaffolded Development; Cognition-Inspired Development; Extended-Mind Development; Enactive Context Development; Situated-Cognition Development; Exocortical Development; Human-in-the-Truth-Loop Development; Human-Sovereign Context Development; Humane Agentic Development; Humanistic Context Development; Stewarded Agency Method; Grounded Coagency | Authority, interpretation, external memory, and collaboration. “Human-like” is weaker than saying precisely what people govern. |
| **Evidence and promotion** | Verified Context-Routed Development; Evidence-Grounded Context-Routed Development; Grounded Context Development; Evidence-Informed Context Development; Verified Context Cultivation; Evidence-Promoted Development; Promotion-Governed Development; Truth-Routed Development; Warranted Growth Development; Cultivate-to-Truth Development; Context Before Action; Context Is the Workflow; Route–Work–Verify–Cultivate; Truth Follows Evidence | The epistemic lifecycle and proof burden. “Verified” can falsely imply a universal mechanical gate today. |
| **Repo and plain-file substrate** | Repo-Native Context Development; Plain-File Context Development; File-Native Context Development; Diff-Native Development; Reviewable Context Development; Transparent Context Development; Repository-Grown Development; File-Grown Context Development; Local-First Context Development; Repository Memory Development | Portability, reviewability, and inspectability. These name implementation architecture more than the evolved method. |
| **Coined frontier words** | Routecology; Contexticulture; Routogenesis; Contextogenesis; Routeweave; Routecraft; Contextcraft; Pathloom; Wayroot; Routegarden; Routebloom; Context Grove; Arboroute; Contextbraid; Workloom; Flowroot; Truthgarden; Context Habitat; Living Route Protocol; Routed Context Praxis; Repoesis; Contextome; Contexture; Contextweave | Potential category ownership. Every gain in distinctiveness creates an education, pronunciation, and clearance cost. |
| **Organic mnemonics** | ROUTE Method; FORGE Method; ORGANIC Doctrine; CRAFT Practice; CURATE Loop; CROP Loop; RITE Practice; BRAID Method; ROOTED Development; GROW Loop; ALIVE Practice; FLORA Method; GROVE Method; HABITAT Method | Strong campaign or subsystem labels. Backronyms become brittle when the expansion is forced or current implementation cannot support every word. |

### Frontier lab: the strange names worth preserving

| Candidate | Intended meaning | Best possible role | Verdict now |
|---|---|---|---|
| **Routecology** | Routing + ecology: routes grow, compete for attention, decay, and are governed | Public movement or field above CRD | **Best frontier wildcard.** Test comprehension and pronunciation. |
| **Contexticulture** | Context + cultivation/culture | Organic research umbrella | Evocative but difficult to say; obscure older academic uses exist. |
| **Routogenesis** | The formation and evolution of routes | Research program or architecture essay | Highly distinctive; “genesis” can imply autonomous generation. |
| **Contextogenesis** | Context formed through work | Lifecycle paper or subsystem | Organic and clear after explanation; misses deliberate routing. |
| **Wayroot** | Wayfinding + rooted repository truth | Product or community brand | Short and warm; mechanism is opaque. |
| **Routeweave** | Multiple route strands composed into working context | Resolver or composition engine | Excellent component name; sounds tool-like as a methodology. |
| **Routecraft** | Skilled, deliberate construction of routes | Practitioner discipline | Memorable; underplays evidence and memory lifecycle. |
| **Pathloom** | A loom that weaves paths into context | Resolver/product name | Imaginative and visual; high education cost. |
| **Root-and-Route Development** | Repository roots plus navigable routes | Campaign phrase | Charming and faithful; may sound playful rather than frontier-serious. |
| **Routegarden** | Routes are planted, pruned, and cultivated | Ecosystem or visual model | Warm; too metaphorical for the formal method. |
| **Contextbraid** | Truth, rules, memory, and workflow are braided | Composition pattern | Strong architecture metaphor; coined compound needs teaching. |
| **Epistemic Wayfinding** | Navigating claims by authority, relevance, and state | Research umbrella | Intellectually precise; academic tone. |
| **Context Metabolism** | Context is absorbed, transformed, reused, and retired | Lifecycle model | Powerful, but can imply autonomous homeostasis. |
| **Desire-Path Development** | Useful routes emerge from repeated real behavior | Essay/metaphor | User-centered, but suggests bypassing formal safeguards and is already used elsewhere. |
| **Current-Guided Development** | Guided by both current truth and a flowing current | Accessible organic alternate | Clever and memorable; mechanism remains under-specified. |
| **Cognition-Scaffolded Development** | Repository context scaffolds bounded reasoning | Human-inspired formal alternate | Defensible; does not identify routing as the novelty. |
| **Enactive Context Development** | Context and knowledge arise through doing | Cognitive-science framing | Deep fit; too obscure for a primary category. |
| **Stigmergic Context Development** | Durable traces from one worker guide later workers | Technical research descriptor | Surprisingly exact, but crowded in agent research and unfamiliar publicly. |
| **Mycelial Context Development** | Distributed local connections support resilient growth | Visual philosophy | Organic and frontier-feeling; inaccurate if it implies decentralization or autonomy. |
| **Living Context Protocol** | A portable protocol whose context evolves over time | Standard/specification layer | Strong future standard name; “living” imports maintenance expectations. |
| **Contexture Development** | Context as an interwoven arrangement | Coined-feeling category | **Reject:** directly occupied by adjacent DDD and AI products. |
| **Contextweave** | Context strands woven together | Component or product | **Reject:** multiple current AI/context tools and a package already use it. |
| **Contextome** | The total environment of context | Research vocabulary | **Reject:** biological meaning and current product usage; also suggests total, not minimal, context. |
| **Repoesis** | Repository + poiesis, bringing knowledge into being | Artful essay title | **Reject as category:** an existing art use surfaced and pronunciation is unclear. |

### Mnemonic systems: useful only if they do not become acronym theater

| Mnemonic | Possible expansion | Good use | Main problem |
|---|---|---|---|
| **ROUTE** | Repository-native, On-demand, User-governed, Traceable Engineering | Operating principles | Best technically faithful mnemonic, but generic and not independently ownable. |
| **FORGE** | File-native, On-demand, Routed, Governed Engineering | Product doctrine | Excellent continuity; “governed” remains partly aspirational and the product-name collision remains. |
| **ORGANIC** | On-demand, Route-selected, Governed, Auditable, Native-to-repository, Intent-led Context | Manifesto or internal doctrine | Delightful but visibly reverse-engineered; do not make the expansion the formal name. |
| **CRAFT** | Context-Routed, Auditable, File-native Truth | Philosophy | Human and deliberate; describes truth, not the whole development loop. |
| **CURATE** | Context Under Review: Authority, Traceability, Evidence | Promotion subloop | Strong lifecycle mnemonic; expansion is strained. |
| **CROP** | Context Routing and Organic Promotion | Promotion loop | Memorable and organic; too cute for the main category. |
| **RITE** | Routed Intent, Traceable Evidence | Verification practice | Clean and ceremonial; omits context cultivation. |
| **BRAID** | Bounded, Routed, Auditable, Intent-Driven | Composition model | Strong visual; “bounded” is not yet a fully defensible runtime claim. |
| **ROOTED** | Routed, On-demand, Observable, Traceable, Evidence-led Development | Values statement | Faithful and memorable; expansion is long. |
| **GROW** | Governed, Routed, On-demand Work | Loop nickname | Excellent compact philosophy; too generic for category ownership. |
| **ALIVE** | Auditable, Local, Intent-routed, Verified, Evolving | Aspirational quality model | “Verified” and “evolving” overstate current enforcement. |
| **FLORA** | File-native, Local, On-demand, Routed, Auditable | Architecture principles | Organic and coherent; does not say development or governance. |
| **GROVE** | Governed, Routed, On-demand, Verifiable Engineering | Product/method candidate | **Do not lead with it:** `Grove Method` and GROVE AI uses already exist. |
| **HABITAT** | Human-governed, Auditable, Bounded, Intent-routed, Traceable, Agent-facing Truth | Context-system metaphor | Rich but overlong and generic; some expansion terms remain unproved. |

### Five viable naming strategies

| Strategy | Stack | When to choose it | Cost and risk |
|---|---|---|---|
| **A. Descriptive category** | Context-Routed Development + organic growth, deliberate promotion | Technical credibility and immediate comprehension matter most | Lowest education cost; acronym collision and less emotional warmth |
| **B. Layered category and practice** | Context-Routed Development + Routed Context Cultivation + Forge Loop | **Recommended.** The framework needs precision and an organic identity | More names to govern, but each has one clear job |
| **C. Organic formal rename** | Context-Grown Development or Context-Cultivated Development | Community warmth matters more than mechanism-first precision | Routing novelty disappears; autonomous-growth expectations rise |
| **D. Human-authority formal rename** | Human-Governed or Human-Stewarded Context Development | Trust, accountability, and team governance are the primary market wedge | Long names; route selection and minimalism become secondary |
| **E. Frontier field creation** | Routecology, formally described as Context-Routed Development | The goal is to create and lead a new discourse, not merely describe a tool | Highest distinctiveness and highest explanation, pronunciation, and clearance risk |

Strategy B is the best current decision. Strategy E is worth a naming experiment, not an immediate irreversible rename. A frontier word becomes valuable only if people can repeat its meaning without the founder in the room.

### Suggested public language by desired personality

**Precise:**

> Open Forge practices Context-Routed Development: intent selects the smallest useful context, work is verified, and people govern what becomes truth.

**Organic:**

> Open Forge is Routed Context Cultivation: context grows from real work, earns reuse through evidence, and stays under human stewardship.

**Frontier:**

> Open Forge pioneers Routecology—the discipline of growing, selecting, verifying, pruning, and governing the routes through which humans and agents work.

**Human-inspired:**

> Open Forge is cognition-scaffolded, not human-mimicking: it externalizes memory and attention while people retain authority over truth.

**Minimalist:**

> Route only what matters. Cultivate only what earns reuse.

Additional tagline territory: **Context follows intent. Truth follows evidence.** / **Grow context, not prompts.** / **Only the context the work needs.** / **From real work to warranted truth.** / **Context grows; truth is governed.** / **Route less. Learn better.**

### Names and claims to reject or qualify

Reject as materially false today: **Autonomous Context Development**, **Agent-Driven Development**, **Self-Routing Development**, **Automatically Routed Development**, **Self-Growing Context Development**, **Self-Evolving Development**, **Deterministic Context Development**, and **Context-Orchestrated Development**. They misstate human authority, semantic selection, tag self-enforcement, explicit memory writes, or the fact that workflows are Markdown recipes rather than a runtime orchestrator.

Use only with qualification:

- **Natural, organic, living, adaptive, evolutionary, regenerative, endogenous:** can imply automatic, beneficial, or unmanaged change.
- **Symbiotic, co-adaptive, coagentic:** can imply peer agency and demonstrated mutual adaptation.
- **Verified, evidence-gated, safe, bounded:** the framework has method obligations, but current enforcement gaps prevent universal guarantees.
- **Minimal, just enough, on demand, context-efficient:** accurate design intent, not yet a demonstrated comparative outcome.
- **Agent-agnostic:** plain-file portability is defensible; cross-runtime behavioral equivalence remains unproved.
- **Routed:** define it as declared structural routing plus human/agent selection, not autonomous semantic retrieval.

Avoid **Organic Context Development** because `OCD` is an unsuitable acronym, **Living-System Development** because `LSD` is noisy, **Context-Flow Development** because `CFD` strongly means computational fluid dynamics, **Context-Routing Method** because `CRM` is dominated by customer relationship management, and **Relevance-Bounded Development** if DDD confusion matters because [Bounded Context](https://martinfowler.com/bliki/BoundedContext.html) is established domain-driven design vocabulary.

### Current collision and ownability screen

This was a lightweight public-web screen, not trademark, corporate-name, package-registry, domain, linguistic, or legal clearance.

- The exact `OpenForge` name is already used in the same broad market by [GitGuardian's AI agent plugin and skill catalog](https://github.com/orgs/GitGuardian/repositories). This is a material public searchability and confusion risk.
- `CRD` is the standard Kubernetes abbreviation for [CustomResourceDefinition](https://kubernetes.io/docs/concepts/extend-kubernetes/api-extension/custom-resources/). Lead with the full phrase.
- [Context-driven development](https://cdd.dev/) is active agentic-development language. `Context-Driven Development` is therefore less distinct than the routed form.
- [Contextive](https://contextive.tech/) and [Contexture](https://contexture.domains/) occupy adjacent DDD/context-tool territory. Contexture also appears in other current AI/software products.
- `Contextweave` is already used by current AI/context systems and a Python package; the name is too directly adjacent to adopt.
- [Seedwork](https://www.seedwork.ai/) already presents an LLM-native cultivation methodology. Avoid making seed/growth language the sole differentiator.
- `Grove Method` has an existing [ITSM use](https://bobroark.com/grove-for-itsm), and GROVE has an [AI story-generation framework](https://arxiv.org/abs/2310.05388).
- Stigmergy is already active in agent infrastructure and appears as an AI-assisted software-engineering coordination model in [Nidus](https://arxiv.org/abs/2604.05080). Keep it as explanatory research vocabulary.
- `Routecology` and `Routogenesis` produced no obvious exact software-method collision in this screen. `Contexticulture` produced obscure older academic appearances rather than a current AI methodology. That is encouraging, not clearance.

Brand options therefore remain separate:

1. **Keep Open Forge internally and qualify it publicly** as `Open Forge — Context-Routed Development`; lowest migration cost, but the direct-market collision remains.
2. **Keep the methodology and rename the product before public launch**; highest near-term cost, lowest long-term confusion.
3. **Use Open Forge as a codename and make the method or frontier field the public identity**; plausible for a community specification, weaker for a CLI and extension marketplace.

### The recommendation and a low-cost decision experiment

The report's recommendation is now:

1. Adopt **Context-Routed Development** as the candidate formal methodology.
2. Adopt **Routed Context Cultivation** as the broader organic practice if a second layer is useful.
3. Keep **organic growth, deliberate promotion** as the philosophy.
4. Keep **Route → Work → Verify → Cultivate** as the operating loop.
5. Preserve **Routecology** as the leading frontier-field experiment; keep **Contexticulture** and **Routogenesis** as secondary wildcards.
6. Use **human-governed** or **human-stewarded**, not human-like, when describing authority.

Before a public naming commitment, run a small blinded naming test on five finalist stacks rather than asking whether people “like” a word. Show each to 10-20 relevant developers for five seconds, then measure: mechanism inferred; false autonomy inferred; unaided recall after ten minutes; pronunciation agreement; ability to explain it to another person; and confusion with an existing product or method. Follow that with real trademark, package, domain, and multilingual screening. Choose the candidate that best preserves truthful comprehension—not merely the highest subjective appeal.

## Current Scorecard

| Area | Grade | Confidence | Assessment |
|---|---:|---:|---|
| Architectural coherence | A | High | Selection metadata, routed truth, primitive types, scope, and state transitions form a coherent system. |
| Human reviewability | A- | High | Plain files, bounded generated regions, and Git diffs are real advantages; effective context is still distributed. |
| Routing kernel | A- | High | Recursive entrypoints and direct-only indexes work well; path moves and overwrite invisibility remain fragile. |
| Memory lifecycle | B+ | Medium | Four states and behavior/state separation are strong; promotion authority, decay, and ritual write pressure need work. |
| Workflow model | B+ | Medium | Goal/Required Routes/Steps/Loop/Outputs/Completion is expressive; orchestration and dependency lifecycle are lightly proven. |
| Minimalism and context efficiency | B- | High | Opinionated content is minimal, but the default mandatory chain is 19 files and 3,397 words before task bodies. |
| CLI integrity and context safety | C+ | High | Doctor, index, and find are useful and tested; workspace escape, active-chain mismatch, and non-atomic writes are material. |
| Extension/update lifecycle | C | High | Install mechanics work, but dependencies, preview, collision handling, ownership, update, remove, provenance, and rollback are absent. |
| Demonstrated agent agnosticism | C / unproven | High | Files are portable in principle; reproducible evidence covers one Codex/GPT-5 runtime family. |
| Evaluation rigor | C+ | High | Fixed seeds and independent verification are good; causal validity, isolation, replication, and provenance are incomplete. |
| New-adopter experience | B- | Medium | README voice is strong, but twenty files and a large ontology precede the first concrete local payoff. |

The scorecard intentionally does not average these grades into false precision. The routing and epistemic kernel is much better than the operational lifecycle around it.

## Scope, Evidence, And Method

The review treated prior syntheses as claims to test, not truth to repeat. Evidence was triangulated across:

- current installed `.agents/` instructions and memory;
- the installable payload under `src/open-forge/`;
- maintainer descriptors under `docs/framework/`;
- `README.md`, CLI and extension documentation;
- `src/cli/cli.ts` and `src/cli/cli.test.ts`;
- 20 structured v7-v11 benchmark reports;
- 11 pre-harness reports used only as contextual history;
- five accepted evaluation syntheses;
- retained benchmark workspaces and reported baselines inspected by the evidence reviewer;
- fresh `find`, `doctor`, test, build, size, and path-boundary checks;
- three independent review lenses: architecture, evidence quality, and adversarial practitioner experience.

The grain of the benchmark data is one run per seed, generation, model label, and overlay combination. Core scores use a subjective 0-2 rubric. Different seeds are different tasks and cannot be treated as independent replicates of one treatment. Generation averages are descriptive only because the harness and framework changed between generations.

Fresh verification as of 2026-07-12:

- `open-forge doctor`: 0 errors, 0 warnings.
- `bun test --coverage`: 34 tests passed, 0 failed, 248 assertions.
- the coverage display reported 100%, but it instrumented only `cli.test.ts`; production CLI code runs in spawned subprocesses, so production line coverage remains unknown.
- two clean temporary builds produced the same source-archive SHA-256, confirming same-environment deterministic packaging for the inspected worktree.
- installable payload: 20 Markdown files, 24,591 bytes, about 3,432 words total.
- mandatory default route chain: 19 files, 529 lines, 3,397 words before selected destination bodies.
- mandatory dogfood route chain: 19 files, 558 lines, 4,329 words before selected destination bodies.
- entire current `.agents/`: 79 Markdown files and 48,886 words; most bodies remain on demand, but their loaded category indexes grow with the corpus.
- current CLI source: 1,759 lines; test source: 851 lines.

## Evidence Quality: Useful, But Not Yet Causal

### Strong evidence

- All 20 v7-v11 reports contain the required sections and five core rubric rows.
- The evidence reviewer matched all 20 reported baseline commits to retained disposable workspaces.
- Generation 8 and generation 9 baseline trees are byte-identical across all four seeds.
- Generation 9 to generation 10 build baselines differ only by the closeout loader overwrite, making generation 10 the cleanest treatment comparison.
- All 12 v7-v10 build runs scored Product Fidelity 2/2. This is strong evidence that precise routed content, prompts, and a capable model can carry non-obvious product semantics into implementation.
- Scope writes fell from three of three v7 build runs to zero of nine v8-v10 build runs after the combined absolute-root and working-directory intervention.
- All three generation-10 treated build runs executed the exact closeout command. The effect is salient, although 3/3 remains a small sample.
- Independent orchestrator verification repeatedly found real defects and overstatements that worker finals missed.

### Claims that require downgrade

- The accepted `roughly 50% full-closeout baseline` is not supported by observable artifacts. A session file was a necessary condition in that definition, yet generation 8 plus 9 contain sessions in only 3/8 runs overall and 2/6 comparable build runs. The maximum observable full-loop rate is therefore 37.5% overall or 33.3% for build seeds.
- `Recall-window discovery is conclusively model territory` is contradicted by the corpus. V7 captured the correct window at least partially, and the same model label directly asked it in v11. V11 is contaminated as an experiment but still falsifies the absolute claim.
- The v10 report attributes vision reference loading to `worker-vision` Required Routes, but that workflow declares `Required Routes: none`; the references were relevance-selected extras.
- V11 does not isolate one variable. It combines the sessions tag, an explicit write axiom, a generated parent-tag change, and already-promoted loader wording.
- V11 does not prove the sessions treatment. Every v11 report states that concurrent orchestrators wrote the same report path and explicitly says not to count the run as a clean causal comparison. Eight v11 workspaces exist for four reports.
- The v11 Codex seed-0 run is out of plan. The preregistered plan said to skip it unless running a second-model comparison.
- Filesystem separation does not prove worker-context isolation. V7-v11 never record a no-inherited-context spawn setting, even though the collaboration runtime can inherit surrounding orchestrator context by default.
- `Product fidelity is settled` is too strong. The coarse core score is ceiling-saturated and can hide seed-level partials or verified deviations.
- `Agent agnostic` is an architecture hypothesis, not a measured result.
- `Minimal/on-demand` has not been compared against a flat-file or memoryless control on tokens, latency, tool calls, or review cost.

Evidence-quality grade:

- internal engineering regression: B;
- causal attribution: C-;
- cross-model/runtime external validity: D+;
- overall evidence base: C+.

## The Good

### Epistemic status is first-class

Working, emerging, crystallized, and archived material do not share authority. `#Contextual` and `#CurrentTruth` make candidate reasoning visibly different from accepted state. That is more valuable than a generic docs tree and is one of Open Forge's clearest improvements over ordinary SDD.

### State does not own behavior

The rule that memory records state while directives, patterns, guidance, skills, workflows, and workspace routes own operational meaning is excellent. It limits the chance that an old session or analysis silently becomes a permanent instruction.

### Selection and execution surfaces are separated

Descriptions help an agent select or skip a route; routed bodies contain the contract. Direct-child indexes prevent the loader from becoming one enormous flattened registry. The distinction held up well during this review.

### The primitive ontology is coherent

Mandatory behavior, inspectable form, adaptable judgment, bounded capability, goal-oriented recipes, and location maps are distinct. The distinctions are useful in practice and make local extension possible without inventing parallel systems.

### The workflow shape is flexible without being mystical

Goal, Required Routes, Steps, Loop, Outputs, and Completion cover deterministic, iterative, and goal-seeking work. Required Routes correctly separate containment from cross-tree dependency.

### Plain-file inspectability is real

This entire investigation was possible because instructions, decisions, reports, generated navigation, and history were inspectable and diffable. Bounded generated regions are especially good: machine output is visible and does not own the authored contract.

### Minimalism is protected at the opinion layer

Core does not force a language, architecture, tracker, or TDD ritual. Optional packs are the right architectural home for strong opinions.

### Dogfood already changes the design

Scope-control and closeout interventions came from observed failures rather than taste. The observation-to-intervention-to-validation loop is real, even though the evidence needs stronger controls.

## The Bad

### Minimal in opinions does not mean small at startup

A compliant agent reads 19 framework files before selected task bodies. The default chain contains 3,397 words; this dogfood workspace carries 4,329. Every subagent repeats the contract unless its runtime supplies inherited context. The system is structurally on demand, but its bootstrap is a substantial taxonomy and instruction budget.

The loaded indexes also grow with direct decisions, documents, analyses, ideas, sessions, handoffs, and observations. Bodies stay lazy, but the selection surface is not automatically flat as the workspace grows.

### The product is a substrate before it is a workflow

The README promises idea-to-docs-to-tasks-to-implementation-to-review, yet Core ships no opinionated workflow and the current dogfood installation has no matching assessment/review workflow. The source includes useful optional workflows, but a new user first receives taxonomy and must design or install the first practical path.

### Human-led promotion is not a Core invariant

The vision workflow requires explicit confirmation before promotion, but the crystallized-memory contract itself does not establish a general approval rule. A user-intent conclusion can become #CurrentTruth without a universal owner/acceptance policy.

### Memory can become ceremony

Session, handoff, observation, analysis, decision, and document have distinct purposes, but benchmark scoring can reward producing all of them. That encourages diary churn and Goodharting. An observation should be warranted by recurrence or material risk; a handoff should exist when transfer/resume value exists; a session should preserve useful chronological context that has no clearer owner.

### Organic growth lacks an equally strong decay loop

Applied or stale candidate files remain in emerging memory. `workflow-redesign.md` says it was accepted and applied but still loads as an emerging idea. `rating-ladder.md` contains stale ratings. `cli-design.md` mixes shipped behavior with deferred ideas. The framework grows more reliably than it prunes.

### Agent agnosticism is conditional

Markdown and relative paths are portable, but activation still depends on a runtime honoring `AGENTS.md`, native skill conventions, and self-enforced load policy. Compatibility aliases and `rune:` metadata prove input flexibility, not behavioral equivalence.

### Doctor proves graph health, not semantic health

Zero errors and warnings do not detect conflicting #CurrentTruth, unauthorized promotion, stale working memory, contradictory overwrite content, incomplete workflow sections, selected-pattern deviations, or description quality.

## The Ugly

### Critical: deterministic context lookup can leave the workspace

`find --route` joins a user-supplied route to the target without a containment check. Required Routes are handled the same way. A safe proof against public repository content showed that selecting `src/open-forge` as the target and asking for `../../README.md` successfully read the repository README outside that target.

This is not merely a path-purity issue. A hostile or injected workflow can put `..` in Required Routes and cause `find --follow-required --bodies` to ingest a file outside the workspace into agent context or logs. `doctor` uses the same unchecked resolution and would treat an existing outside file as valid.

Required fix: resolve and realpath every route; reject absolute paths, parent traversal, drive changes, and symlink escapes; use the same containment primitive in `find`, Required Routes, doctor, install, extend, and generated-entry expansion; add Windows and POSIX adversarial tests.

### High: the closeout command does not implement the loader contract

The loader says KeepInMind activation exists through loaded parent entrypoints. `find --tag KeepInMind` scans the entire routed tree and can surface follow-ups under inactive parents. At the same time, overwrite companions are excluded from generated routing and are not emitted after selected base files. Gen11 seed-1 missed `loader.overwrite.md`, demonstrating the invisible-overlay failure.

Required fix: build an active-context resolver that starts at the loader, follows only the load-policy chain, emits base then overwrite, recursively follows Required Routes when asked, explains why every file is present, and supports separate startup and closeout phases with a budget and stable digest.

### High: lifecycle mutation is non-transactional

Install and extend copy files before index validation. Indexes are rewritten sequentially. A malformed later route can fail after earlier files have changed. Extensions can overwrite Markdown and binary files anywhere inside the target, not only `.agents/`, without a preflight collision report.

Required fix: plan the complete mutation, validate every source and destination, show collisions and privilege class, stage the result, validate the staged tree, then commit atomically or roll back.

### High: extension trust and ownership are unresolved

`dev-workflow` depends textually on skills from `workflow-essentials`, but manifests do not encode dependencies. There is no file ownership inventory, version, compatibility range, checksum lock, update, remove, verify, or rollback. Multiple extensions can silently win by install order.

Required fix: a visible manifest/lock with id, version, compatibility, dependencies, conflicts, owned files, hashes, source, and risk class. Route-only extensions, scripts, AGENTS changes, workspace-wide directives, load-policy additions, and non-`.agents` overlays should have different consent levels.

### High: benchmark confidence exceeds benchmark validity

The benchmark design contains excellent ideas—fixed seeds, planted traps, worker/orchestrator separation, independent verification—but later syntheses sometimes convert small, confounded samples into settled causal conclusions. The corpus lacks exact model revisions, worker traces, transcripts, no-context-fork evidence, blinded scoring, a no-framework control, true replication, randomized order, and machine-readable run ownership.

Required fix: treat reports as interpreted summaries, preserve trace and provenance, and generate reports from validated per-run JSON. Generation 11 should remain product evidence only, not a treatment verdict.

### Fresh dogfood recurrence: path intent still loses to process cwd

During this assessment, a diagnostic created and validated a safe temporary directory but failed to switch the shell process into it before running the build. The build therefore regenerated the repository's ignored `dist/` twice. No tracked file changed, and the corrected temp build passed, but the event reproduces the earlier tool-boundary class outside the benchmark patch tool.

The lesson is broader than `apply_patch`: safe target computation does not constrain process execution. Every mutating command must receive or assert an explicit working directory at the execution boundary.

## Perspective Review

| Perspective | What is excellent | Main cost | Adoption blocker |
|---|---|---|---|
| Solo developer | No service, portable files, decisions and handoffs reduce rediscovery, Doctor is useful | Taxonomy and repeated memory writes before local value | No safe five-minute profile and no preview/rollback for reinstall or extension work |
| Team lead/reviewer | Candidate versus current truth, rationale, evidence, and bounded diffs support review | No ownership/approval model; memory/index diffs can swamp product changes | No effective-context receipt and no policy for approving directives or #CurrentTruth |
| New adopter | Clear voice and coherent concepts after learning | High jargon and twenty installed files before a worked local path | Installs a methodology substrate, not an immediate completed workflow |
| Runtime integrator | Relative paths, JSON output, deterministic indexes, native skill deference | Self-enforcement, narrow frontmatter parser, no conformance spec | No safe effective-context API and no multi-runtime matrix |
| Security skeptic | No hidden database, no registry fetch, release checksums, inspectable text | Review happens after mutation; instruction files are executable policy | No plan/apply/rollback, provenance lock, containment policy, or extension privilege model |
| Maintainer | Dogfood, candid reports, descriptors, tests, and route integrity are strong | Payload/descriptors/self-install/tests/overlays create mirror and prose-maintenance tax | No automated semantic alignment or benchmark release gate |

## How Well Open Forge Worked On This Review

### What worked

- The loader immediately exposed the correct source-of-truth map.
- Accepted decisions, candidate analysis, historical context, and raw reports had clear epistemic status.
- Generated entries made targeted discovery faster than blind search.
- The handoff route provided a shared worker contract for three independent lenses.
- The no-edit worker contract prevented subagent write collisions; workers returned candidate observations to the root for routing.
- `doctor`, `index`, and `find` were useful and easy to invoke from the current source tree.
- The framework made its own missing workflow visible: there was no matching installed assessment route, so an external technical-report skill and the routed handoff filled the gap explicitly.

### What did not work cleanly

- Nineteen mandatory files and 4,329 words preceded substantive evidence work in every fresh review lens.
- The handoff plus report plus observation closeout creates nontrivial artifact overhead for one assessment.
- The no-edit worker contract conflicted with the observations write axiom; higher-priority task scope won, but the framework lacks an explicit read-only proposal mode.
- The global KeepInMind command was not a faithful representation of loaded active context.
- The build diagnostic reproduced a process-cwd scope failure despite safe temp-path checks.
- External runtime skills worked well but were not selected or orchestrated by Open Forge's empty local skills/workflows routes.

Overall, Open Forge helped navigation and epistemic discipline more than it hindered. It did not make the review minimal, safe by construction, or self-validating. That is the precise frontier between the current B-grade alpha and an S-tier product.

## The Smallest Credible Path To S++

The existing internal ladder has the right philosophy: S means measured rather than asserted; S++ means the measurement is automatic, independent, and survives without the maintainer. The roadmap below applies that rule.

### P0: Make context and mutation trustworthy

1. Fix route containment everywhere.
   - Reject absolute, parent-traversing, cross-drive, and realpath/symlink escapes.
   - Cover `find --route`, Required Routes, doctor, generated entry expansion, install, extend, and scoped updates.
   - Acceptance: adversarial Windows and POSIX fixtures cannot read or write outside the selected root.

2. Ship an active-context resolver.
   - Suggested surface: `open-forge resolve --route <route> --phase start|closeout --follow-required=recursive --explain --budget`.
   - Traverse only loader-reachable load-policy chains.
   - Emit ancestor axioms, base then overwrite, Required Routes, selection reasons, provenance, per-file/total cost, and a stable digest.
   - Acceptance: the resolver's output exactly matches conformance fixtures and a worker receipt can reconstruct what was supposed to be loaded.

3. Preflight and atomically apply mutation.
   - Add plan/dry-run for install, extend, update, remove, create, and index.
   - Detect all collisions and malformed later routes before the first write.
   - Acceptance: injected last-file failures leave the target byte-identical to its starting state.

4. Define extension manifest and privilege model v1.
   - id, version, Open Forge compatibility, dependencies, conflicts, owned files, hashes, source, and risk class.
   - Explicit consent for directives, load-policy tags, AGENTS changes, scripts, and non-`.agents` files.
   - Acceptance: plan, install, verify, update, and remove round-trip without losing local content.

### P1: Restore the minimalist and human-led claims

1. Budget startup context.
   - Add word/token estimates and route-depth warnings to Doctor/resolve.
   - A/B test demoting empty guidance, pattern, skill, workflow, archived, and historical roots from #LoadNow.
   - Initial experimental gate: reduce mandatory default words by at least 35% from 3,397 without a material routing-fidelity loss.

2. Add a general promotion authority rule.
   - User intent, product direction, and contested conclusions stay emerging until an owner accepts them.
   - Mechanically verified facts may be recorded with source and validation date under a declared policy.

3. Add read-only mutation mode.
   - Diagnosis/review workers return memory proposals; a coordinator or user decides whether to persist them.

4. Rework memory writes around downstream value.
   - Observation: recurrence, material risk, or promotion candidate.
   - Handoff: actual transfer, pause, or cold-resume value.
   - Session: useful chronological reconstruction with no clearer owner.
   - Benchmark correct non-writing as success when no artifact is warranted.

5. Close the decay loop.
   - Archive applied candidate designs, split shipped facts from deferred ideas, flag stale working memory, and keep source/replacement links.

6. Ship two optional reference profiles.
   - safe-solo bootstrap;
   - team review/verification with ownership and evidence.

### P2: Make evidence trustworthy

1. Give every run a UUID and machine-readable manifest.
   - clean commit or patch/tree hashes;
   - exact model revision, runtime, OS, tool versions, and reasoning settings;
   - prompt, rubric, seed, overlay, baseline, and artifact hashes;
   - worker context-isolation mode and visible-file manifest.

2. Enforce hard worker isolation.
   - no inherited orchestrator turns;
   - separate process or explicit no-context fork;
   - rubric-only canary leaks in zero runs.

3. Make report ownership collision-proof.
   - atomic ownership; UUID and replicate in filenames; reports generated from validated JSON.

4. Add causal controls and real replication.
   - no framework;
   - one flat equal-content truth file;
   - routing without helpers;
   - routing with active-context helpers.
   - Use 5-10 replicates per cell only as a variance-estimation pilot. Power and preregister the confirmatory matrix; ten flawless trials cannot support a 90% lower-confidence-bound gate. Broad release claims need randomized order, multiple orchestrators, at least three model families, and at least two runtimes.

5. Use objective primary outcomes.
   - Seed-owned executable validators for product contracts.
   - Trace route reads and ordering instead of relying on debrief.
   - Blind two independent raters for subjective items and require high agreement.

6. Factor closeout variables independently.
   - command wording, sessions #KeepInMind, and explicit session-write axiom require separate factors, not one bundled treatment.

### P3: Prove S++ value

Suggested release gates:

- zero critical out-of-scope reads or writes;
- zero partial mutations under adversarial failure;
- at least 95% required-route and closeout compliance with a lower confidence bound of 90% across model/runtime strata;
- at least 95% executable contract fidelity with a lower bound of 90%;
- at least 20% fewer input tokens or context-loading calls than an equal-content flat control with no fidelity loss;
- at least 30% fewer re-asks or rediscoveries in a long-horizon memory comparison with no defect increase;
- conformance reproduced by an independent runtime or implementation;
- one third-party extension authored successfully without maintainer help;
- benchmark smoke in CI for every payload change and the full matrix as a release gate.

Adversarial scenarios should include concurrent workers, stale or poisoned candidates, real CurrentTruth contradictions, missing CLI, malformed indexes, deep/large route trees, interruption recovery, cross-repo work, secret-bearing workspaces, extension collisions, and symlink boundaries.

## S++ Solution Architectures And Decision Options

### S++ should be claim-scoped, not one vague badge

Open Forge cannot responsibly become “S++” in one jump or by accumulating features. S++ should be earned and versioned separately for:

- the deterministic routing and mutation kernel;
- behavioral and product-contract fidelity;
- context efficiency;
- long-horizon memory value;
- model and runtime portability;
- extension lifecycle and third-party usability.

The implementation should have four planes:

~~~mermaid
flowchart LR
  A["Markdown workspace"] --> B["PathPolicy + RouteGraph"]
  B --> C["ContextResolver"]
  C --> D["Ordered context receipt"]
  E["Install / extension sources"] --> F["VirtualTree planner"]
  B --> F
  F --> G["Transaction engine"]
  G --> H["Installed Markdown + visible lock"]
  I["Authority + lifecycle policy"] --> C
  I --> F
  J["Conformance fixtures + benchmark V2"] --> B
  J --> C
  J --> F
  J --> G
~~~

Plans, receipts, journals, and locks are tooling evidence. They never become agent authority; installed Markdown remains runtime truth.

### Four overall build strategies

| Strategy | What it does first | Advantage | Main failure mode | Recommendation |
|---|---|---|---|---|
| **Kernel-first** | Containment, RouteGraph, resolver, planner, transaction engine | Removes critical risk and creates reusable foundations | Delays polished onboarding | **Recommended primary strategy** |
| **Evidence-first** | Freeze baselines and build Benchmark V2 before product work | Gives cleaner attribution | Leaves known path escape and partial mutation live | Capture a small baseline after the containment hotfix, not before it |
| **Adoption-first** | Golden workflow, wizard, profiles, better docs | Fast visible payoff | Builds trust claims on unsafe lifecycle primitives | Use only for documentation experiments until P0 is complete |
| **Ecosystem-first** | Remote extensions, registry, signatures, marketplace | Could attract contributors | Multiplies an unresolved ownership and privilege problem | Explicitly defer |

The best sequence is a hybrid: stop the path escape; freeze a machine-readable legacy baseline; then follow kernel-first development. Adoption and evidence work can proceed once they consume the same resolver and transaction contracts.

### Implementation approach: extract, do not rewrite

The current CLI is a 1,759-line monolith. There are three architectural options:

| Option | Shape | Benefit | Risk | Judgment |
|---|---|---|---|---|
| **Incremental extraction** | Characterize current behavior, then move pure path, graph, planning, and lifecycle logic behind the existing commands | Lowest compatibility risk; every extraction can be tested | Temporary adapters and duplication | **Recommended now** |
| **Clean CLI rewrite** | Replace the monolith with a new command architecture in one release | Cleaner end state quickly | Relearns subtle install, local-block, entrypoint, and Windows behavior; large regression surface | Reject for the next phase |
| **Spec-first second implementation** | Publish the resolver contract and build another implementation before refactoring the first | Strong independence signal | Freezes current defects or an immature contract | Use after the TypeScript contract and fixtures stabilize |

Suggested boundaries:

~~~text
src/cli/path-policy.ts
src/cli/route-graph.ts
src/cli/context-resolver.ts
src/cli/virtual-tree.ts
src/cli/transaction.ts
src/cli/extensions.ts
src/cli/memory.ts
src/cli/commands/
spec/context-resolution-v1.md
conformance/v1/
~~~

The existing CLI remains the compatibility shell while these modules become importable, property-testable units.

### Decision 1: route containment and external roots

| Option | Mechanism | Strength | Weakness | Cost | Verdict |
|---|---|---|---|---:|---|
| **A. Lexical containment** | Reject absolute paths; use path.resolve and path.relative; reject a relative result beginning with parent traversal or becoming absolute | Small, fast security patch | A symlink or Windows junction can still escape | Low | Ship only as an emergency patch if canonical containment cannot land immediately |
| **B. Canonical containment** | Lexical check plus realpath of the trusted root and every existing target or nearest existing ancestor | Covers parent traversal, drives, UNC paths, symlinks, and junctions | More cross-platform edge cases; concurrent link replacement remains a threat-model question | Medium | **Recommended default** |
| **C. Capability-scoped external roots** | B by default; a human may add an external root for one invocation or signed plan | Supports monorepos and linked vaults without weakening authored routes | More policy and receipt surface | Medium | Add after B; Required Routes can never grant this capability |
| **D. Trust authored Required Routes** | Treat paths in workflow Markdown as implicitly allowed | No friction | An instruction file becomes a filesystem capability | None | Reject |

The shared primitive should behave as follows:

~~~text
resolveInside(root, authoredPath, operation):
  reject NUL, absolute, drive-qualified, UNC/device, and explicit parent segments
  canonicalRoot = realpath(root)
  lexicalCandidate = resolve(root, authoredPath)
  assert lexicalCandidate is inside root
  canonicalAnchor = realpath(nearest existing ancestor of lexicalCandidate)
  candidate = canonicalAnchor plus the not-yet-existing suffix
  assert candidate is inside canonicalRoot
  immediately before access, recheck the relevant existing components
~~~

For reads, realpath the full target. For writes, realpath the nearest existing ancestor and reject a symlinked final component. Use no-follow file flags where the platform exposes them, but state honestly that protection against a hostile process racing filesystem links may require stricter platform-specific handling. A symlinked workspace or top-level .agents folder can remain supported by defining its canonical realpath as the trusted mount.

Apply this one primitive to find, Required Routes, Doctor, generated entries, install, extend, create, update, remove, profile changes, and memory moves. No caller gets a private path join.

Acceptance:

- Windows and POSIX cases cover absolute paths, parent traversal, mixed separators, cross-drive paths, drive-relative paths, UNC/device paths, case changes, symlinks, and junctions.
- Property tests prove every accepted result is under an allowed canonical root.
- A route may never expand its own allowed roots.
- Reads and writes outside the root remain zero under concurrent and fault-injected fixtures.

### Decision 2: effective-context resolution

| Option | CLI shape | Strength | Weakness | Cost | Verdict |
|---|---|---|---|---:|---|
| **A. Extend find** | open-forge find --active --phase closeout | Smallest command surface | Mixes global discovery with normative context activation; difficult to evolve cleanly | Medium | Acceptable interim |
| **B. Dedicated resolver** | open-forge resolve --route PATH --phase start or closeout --follow-required recursive | Clear contract, stable automation API, clean receipts | One new command and schema | High | **Recommended** |
| **C. Persisted context bundle** | Compile selected bodies into .agents/context files | Easy ingestion and caching | Stale parallel truth, hidden selection, giant-file pressure | Medium | Reject as persistent truth; allow ephemeral stdout or temp output |
| **D. Semantic/ranked resolution** | Model or embeddings choose relevance | Better recall on large corpora | Nondeterministic and vendor-dependent | High | Keep in Rune or another optional companion, never deterministic Core |

Recommended command:

~~~text
open-forge resolve \
  --route .agents/workflows/dev/_dev.md \
  --phase start \
  --follow-required recursive \
  --format json \
  --max-tokens 8000 \
  --on-budget error \
  --explain
~~~

Recommended deterministic order:

1. loader base, then loader overwrite;
2. loader-reachable LoadNow and KeepInMind chains in listed order;
3. ancestors of the selected route before its destination;
4. every base file immediately followed by its overwrite;
5. recursively followed Required Routes in declared order, with cycle detection;
6. deduplication by canonical file identity while retaining every selection reason;
7. start or closeout phase filtering against the active chain, never a global tag scan.

Receipt shape:

~~~json
{
  "schemaVersion": 1,
  "phase": "start",
  "selectedRoute": ".agents/workflows/dev/_dev.md",
  "files": [
    {
      "path": ".agents/loader.md",
      "sha256": "...",
      "reason": "loader",
      "parent": null,
      "bytes": 7231,
      "words": 1042,
      "estimatedTokens": 1560
    }
  ],
  "excluded": [],
  "totals": {"files": 12, "words": 2538, "estimatedTokens": 3900},
  "digest": "sha256-of-schema-options-ordered-paths-and-content-hashes"
}
~~~

The text and body formats must be projections of the same receipt, not separate traversal code. Budgets may warn or fail before bodies are emitted; they must never silently truncate. The receipt should distinguish “delivered context” from later file reads because neither proves comprehension.

Acceptance:

- language-neutral fixtures define ancestry, tags, overrides, recursion, cycles, missing blockers, scoped routes, and start/closeout behavior;
- two calls on the same tree yield byte-identical canonical JSON and digest;
- the resolver matches a manual contract interpretation on every fixture;
- every included file has a reason and parent edge;
- every excluded active candidate can be explained when requested;
- global find remains available but is no longer documented as effective context.

### Decision 3: planned, failure-atomic mutation

Portable filesystems cannot promise an instantaneous atomic switch for arbitrary files across a whole repository. The honest S++ target is: all predictable failures happen before writing; ordinary apply failures roll back; process crashes are deterministically recoverable; and success or completed recovery leaves one coherent state.

| Option | Mechanism | Strength | Weakness | Cost | Verdict |
|---|---|---|---|---:|---|
| **A. Complete preflight, direct writes** | Compute and validate the final tree before sequential writes | Fixes the current malformed-late-route partial mutation | I/O failure or crash can still leave partial state | Medium | Good S-level milestone |
| **B. Virtual tree plus journaled apply** | Hash preconditions, stage sibling temp files, use an exclusive lock and operation journal, reverse on failure, recover after crash | Works in Git and non-Git workspaces; reusable by every mutating command | Brief partial visibility during a multi-file commit; substantial recovery testing | High | **Recommended default** |
| **C. Managed-subtree shadow swap** | Build a full sibling .agents tree and rename old/new trees | Stronger activation boundary for the small managed subtree | AGENTS and approved outside overlays still need a journal; Windows open handles can block rename | High | Use inside B as an optimization |
| **D. Git patch/worktree** | Produce a patch or isolated worktree for human apply | Excellent review and rollback | Git-dependent; not a complete product lifecycle | Medium | Offer as a high-assurance output mode |
| **E. Immutable store plus pointer** | Activate a version by switching a pointer | Closest to atomic activation | Hidden indirection becomes runtime truth | High | Reject |

Recommended command model:

~~~text
open-forge plan install TARGET --out open-forge.plan.json
open-forge plan extend EXTENSION TARGET --out open-forge.plan.json
open-forge apply open-forge.plan.json
open-forge recover --status
open-forge recover --resume TRANSACTION
open-forge recover --rollback TRANSACTION
~~~

Existing install, extend, create, and index commands can remain convenience wrappers that display a plan and require the normal approval behavior.

A plan records:

- schema, operation, selected canonical root, and source identities;
- before hashes and file modes for every affected path;
- creates, updates, removals, generated-region changes, preserved local blocks, and collisions;
- extension ownership and capability changes;
- the expected final RouteGraph digest and Doctor result;
- approval requirements and an expiration or stale-plan condition.

Apply flow:

1. construct the prospective tree in a VirtualTree;
2. compute all generated indexes without writing;
3. run containment, collision, manifest, and Doctor-equivalent validation against the prospective tree;
4. acquire an exclusive transaction lock;
5. recheck all precondition hashes;
6. stage temp siblings on the same volume;
7. journal each rename/backup and apply;
8. validate the installed result and lock;
9. delete the journal only after success; otherwise reverse or require deterministic recovery.

The journal is temporary operational recovery data, documented and inspectable, not routed policy. A committed lock is lifecycle provenance, not runtime truth.

Acceptance:

- inject a failure after every operation and verify byte-identical rollback, including file modes;
- kill the process at every journal state and verify the next command can resume or roll back;
- locked-file, disk-full, concurrent-apply, stale-plan, and interrupted-recovery cases are covered on Windows and POSIX;
- a malformed last index changes zero target bytes;
- no plan can be applied after a relevant source or target hash changes;
- repeated installation is idempotent.

### Decision 4: extension trust, dependencies, and ownership

| Option | Mechanism | Strength | Weakness | Cost | Verdict |
|---|---|---|---|---:|---|
| **A. Manifest-lite** | Add id, version, compatibility, dependencies, and conflicts to extension.json | Quickly fixes the textual dev-workflow dependency | Cannot safely update or remove without installed ownership | Medium | Useful milestone only |
| **B. Manifest plus visible lock and capability plan** | Package manifest, computed file inventory, installed ownership/hashes, plan/install/verify/update/remove | Complete local lifecycle while preserving copied Markdown | Requires lock schema, dependency graph, and three-way conflict logic | High | **Recommended** |
| **C. Route-only sandbox** | Extensions may add ordinary routed leaves only | Very small trust surface | Blocks legitimate directives, skill scripts, AGENTS patches, and templates | Medium | Use as the default privilege tier, not the entire model |
| **D. Mounted package roots** | Keep extensions under a separate extensions tree | Easy provenance/removal | Breaks native skill paths and the accepted existing-tree model | High | Reject |
| **E. Signed remote registry** | Registry, signatures, transparency log, remote resolution | Supply-chain assurance at scale | Premature network, identity, revocation, and governance surface | Very high | Post-S++, only after local lifecycle is proven |

Suggested author manifest:

~~~json
{
  "schemaVersion": 1,
  "id": "dev-workflow",
  "name": "Dev Workflow",
  "version": "0.1.0",
  "description": "Task-driven development workflow",
  "requires": {
    "openForge": ">=0.1.0 <1.0.0",
    "extensions": {"workflow-essentials": "^0.1.0"}
  },
  "conflicts": [],
  "permissions": ["routes:workflows"],
  "payload": "payload"
}
~~~

The packager, not the author, computes paths, hashes, and actual privileges. The planner classifies:

- ordinary on-demand route leaves;
- entrypoint and overwrite modifications;
- LoadNow or KeepInMind additions;
- workspace-wide directives;
- CurrentTruth additions;
- native skills containing scripts or executables;
- AGENTS changes;
- non-.agents workspace overlays.

Ordinary route additions may use normal approval. Every higher class requires explicit, separately visible consent. Required Routes never grant install capability.

Installed ownership has three options:

| Ledger | Benefit | Cost | Judgment |
|---|---|---|---|
| **open-forge.lock.json** | Canonical parsing, deterministic diff, easy hashing | Less friendly to hand-edit; should not be hand-edited | **Recommended machine ledger; CLI renders a human explanation** |
| **open-forge.lock.md with generated region** | Markdown-first and inspectable | Mixed prose/structured parsing, easier accidental corruption | Valid alternative if Markdown purity is a product requirement |
| **One receipt per extension** | Natural ownership boundary | Shared files and dependency state become harder to reconcile | Avoid as sole authority |

The chosen lock must be visible, committed when the user wants reproducible installs, and explicitly excluded from agent routing. This narrowly revises the rejected hidden-version-metadata idea: hidden runtime metadata remains rejected; visible installer provenance is allowed.

Update/remove rules:

- if current content equals the old installed hash, update or remove safely;
- if current content already equals the new hash, no-op;
- if locally modified, compute a three-way merge from old installed content, current content, and new content during planning;
- unresolved merges block before apply;
- removal never deletes a locally modified file without explicit force;
- identical files may be shared by multiple owners; differing content at one path is a collision;
- removing one owner retains a file owned by another;
- dependency resolution performs no unapproved network fetch.

Acceptance:

- install, verify, update, remove, and rollback round-trip with unchanged, locally edited, shared, and conflicting files;
- the dev-workflow dependency is installed or fails before any copy;
- a third-party package can be authored from the public schema without maintainer help;
- every installed byte has source, owner, version, and hash provenance;
- capability escalation appears in the plan and cannot be approved implicitly.

### Decision 5: a genuinely lean base and optional profiles

Profiles should be install-time composition or visible tag changes, never hidden runtime personas. After installation, normal Markdown remains the complete behavioral truth.

| Option | Mechanism | Strength | Weakness | Cost | Verdict |
|---|---|---|---|---:|---|
| **A. Keep the strict baseline** | Current 19 files and 3,397 installable-payload words | Strong early exposure | Repeated bootstrap cost and weak minimalist claim | None | Keep as a strict profile, not the best default |
| **B. One lean baseline** | Demote historical, working, idea/analysis, and empty primitive catalogs to relevance loading | Small and simple | No opt-up for continuity-heavy work without manual edits | Medium | Better default than current |
| **C. Visible startup profiles** | One payload; plan/apply deterministic tag changes for lean, continuity, or strict | Choice is diffable and agent-visible | More conformance cells and migration logic | Medium | **Recommended if profiles control only loading** |
| **D. Monolithic profile extensions** | solo-starter and team-review copy their full contents | Works with the current extension shape | Duplication and current lifecycle risk | Low | Pilot only |
| **E. Runtime/model-adaptive modes** | A resolver silently chooses a profile | Potential average savings | Hidden divergent behavior and vendor dependence | High | Reject |

Fresh counts from the installable payload show a concrete starting experiment:

| Profile | Mandatory files | Mandatory words | Reduction from strict | Intended use |
|---|---:|---:|---:|---|
| **lean** | 9 | 2,153 | 36.6% | Default candidate for fresh focused work |
| **continuity** | 12 | 2,538 | 25.3% | Resumed work and handoff-heavy teams |
| **strict** | 19 | 3,397 | baseline | Audits, framework work, maximum early exposure |

The lean candidate keeps loader, directives, memory root, crystallized root, decisions index, documents index, emerging root, observations index, and workspace root. It demotes archived; working/handoffs/sessions; emerging analysis and ideas; and empty guidance, pattern, skill, and workflow catalogs. Their selection descriptions remain visible in ancestor entries and are loaded when relevant. This is an experiment, not a foregone conclusion: workflow and pattern discovery fidelity must be measured.

There are two different profile jobs:

1. **Startup profile:** changes only load-policy tags or generated installation choices. Suggested surface: open-forge profile plan/apply/show.
2. **Use-case bundle:** additive extensions such as Solo Starter or Team Review. Suggested surface: open-forge install --profile solo-starter --plan, implemented as dependency-resolved extension composition.

Suggested Solo Starter:

- a small workspace map for source roots, external authorities, verification commands, exclusions, and protected paths;
- one scoped approval/safety directive;
- one on-demand change workflow: orient, work, verify, cultivate only if warranted;
- no mandatory session, handoff, observation, or analysis artifact;
- at most two new baseline files and roughly 600 added startup words after the lean base exists.

Suggested Team Review:

- ownership and external-source map;
- promotion/approval policy for directives and CurrentTruth;
- proposal-only path for unauthorized agents;
- on-demand review workflow;
- effective-context receipt and CI conformance command;
- collision-proof parallel working-memory identifiers.

Suggested onboarding journey, which is not another runtime profile:

- one first verified change in a tiny fixture;
- a read-only route trace showing why each file was selected and what was not loaded;
- only three immediate actions, not an ontology tour;
- memory states and primitives introduced progressively when first needed;
- plan and rollback demonstrated before extension trust is requested.

Acceptance:

- lean saves at least 35% of startup words/tokens with no material fidelity loss;
- profiles add no root route, reserved tag, memory state, or hidden runtime rule;
- profile plan shows every changed tag, dependency, privilege, and context-budget delta;
- profile switching is transactional and CLI-less agents still see the full installed truth;
- at least 80% of external newcomers complete the fixture unaided, with a median install-to-verified-result under ten minutes;
- zero out-of-scope writes and zero unwarranted memory artifacts.

Do not call the profile “safe-solo” until containment, planning, rollback, and protected paths are real. Solo Starter is the honest pre-S++ name.

### Decision 6: authority and promotion

| Option | Rule | Strength | Weakness | Cost | Verdict |
|---|---|---|---|---:|---|
| **A. Human approval for every CurrentTruth item** | Everything begins emerging; a human promotes | Simple, strongest human control | Mechanical facts and solo use become ceremonial | Low | Safe but too rigid |
| **B. Claim-class authority** | Normative truth requires owner acceptance; verified facts require reproducible evidence; external state defers; inference remains emerging | Matches real epistemic differences | Misclassification must be guarded by clear examples | Medium | **Recommended core policy** |
| **C. Git/CODEOWNERS enforcement** | Route owners approve promotion in review | Strong team audit | Git/provider-dependent | Medium | Optional Team Review enforcement |
| **D. Agent confidence threshold** | High-confidence conclusions promote automatically | Low friction | Nondeterministic and makes the model the authority | Medium | Reject |

Recommended claim classes:

- **Normative:** intent, priority, policy, architecture choice, product direction. Requires explicit human or declared owner acceptance.
- **Verified fact:** reproducible repository/system fact. May enter crystallized memory with source, method, date, and scope under a declared policy.
- **External pointer:** another system owns truth. The local record is a pointer or dated snapshot and says it defers.
- **Inference:** interpretation, recommendation, or contested conclusion. Remains emerging until accepted.
- **Operational state:** working memory may be maintained by the active agent but never outranks current truth.

Suggested visible record:

~~~text
## Authority

- class: normative
- authority: owner:product
- accepted-by: user-in-task-2026-07-13
- accepted-at: 2026-07-13
- evidence: none
- supersedes: none
~~~

Suggested commands:

~~~text
open-forge memory propose PATH --to crystallized
open-forge memory promote PATH --class normative --accepted-by OWNER
open-forge memory promote PATH --class verified --evidence ROUTE-OR-COMMAND
~~~

These commands produce a transactional plan. They never infer that silence, a prior chat, or high confidence means acceptance. Existing crystallized records can be grandfathered; require the block on new or materially changed records first. Doctor validates fields and links, not whether evidence semantically proves a claim.

Acceptance:

- normative promotion without a recorded accepter fails strict validation;
- verified facts include a rerunnable source or command;
- external snapshots cannot override their declared source;
- model reasoning alone cannot be labeled verified evidence;
- read-only work produces a proposal and writes nothing;
- supersession links and preserves the old record.

### Decision 7: memory decay without destructive automation

| Option | Mechanism | Strength | Weakness | Cost | Verdict |
|---|---|---|---|---:|---|
| **A. Prose and age warnings** | Guidance plus mtime/filename warnings | Cheap | Age is not staleness; mtime is unreliable | Low | Insufficient |
| **B. Explicit lifecycle fields plus advisory audit** | Review dates/events and links; deterministic memory audit; human-approved moves | Reviewable, portable, no hidden telemetry | Requires metadata adoption and review habit | Medium | **Recommended** |
| **C. Usage telemetry** | Rank rarely read material | Could reveal selection cost | Privacy, runtime dependence, and unused does not mean invalid | High | Optional external analysis only |
| **D. Semantic duplicate/conflict suggestions** | Rune or another companion proposes related/stale items | Useful at scale | Nondeterministic | Medium | Optional; never Core authority |
| **E. TTL auto-archive/delete** | Move or delete after age | Low maintenance | Destroys rare but valid knowledge | Medium | Reject |

Suggested lifecycle block for working or emerging records that need review:

~~~text
## Lifecycle

- created: 2026-07-13
- review-after: 2026-08-13
- resolution: open
- promoted-to: none
- superseded-by: none
~~~

Time-sensitive crystallized facts may instead declare source, last-validated, and review-after under a Validity section. Timeless decisions need no artificial expiry.

Suggested commands:

~~~text
open-forge memory audit --state emerging --as-of 2026-08-13 --json
open-forge memory plan archive PATH --reason TEXT --replaced-by ROUTE
open-forge memory move PATH --to archived --apply PLAN
~~~

Audit reports only deterministic conditions:

- an explicit review date has passed;
- a record declares applied, rejected, completed, promoted, or superseded but remains in the old route;
- a promoted-to, superseded-by, source, or replacement link is broken;
- an archived record lacks required origin/reason;
- a time-sensitive fact is past its declared validation date;
- direct index entries create measurable startup cost.

Audit never writes, infers semantic staleness from age, auto-deletes, or resolves contradictions. Observation writes become recurrence-, material-risk-, or promotion-driven; correct non-writing is a successful outcome when nothing durable was learned.

Acceptance:

- the already-applied workflow-redesign case is flagged after lifecycle data is added;
- an old but explicitly active decision is not flagged;
- audit with a fixed as-of date is byte-for-byte stable;
- archive/promote/move operations reuse the transaction engine;
- failure leaves sources, destinations, and indexes unchanged;
- a curated memory corpus reduces cost without losing current truth in a cold-resume test.

### Decision 8: Benchmark V2 and evidence strength

The current corpus remains legacy engineering evidence. Do not relabel it as causal proof. Three evidence architectures are viable:

| Route | Instrument | Scale | What it can honestly prove | Relative cost |
|---|---|---:|---|---:|
| **Lean/kernel proof** | Schemas, UUID ownership, hashes, objective validators, deterministic conformance, minimal traces, one model/runtime | Roughly 18-24 paired agent runs per release plus exhaustive deterministic tests | Internal regression; S++ only for a deterministic kernel after independent implementation | Low |
| **Balanced/product proof** | Full manifests/traces, flat/routed/resolved arms, six or more cases, two model/runtime pairings, randomized pilot, closeout factorial, short memory trajectories | Roughly 180 main runs, 72 closeout runs, and 12-24 trajectories before powered confirmation | Scoped fidelity, efficiency, and tested-pair portability | Medium-high |
| **Research-grade/public proof** | Preregistered protocol, 12+ cases, four arms, three model families by two runtimes, blinded rating, signed artifacts, external reproduction | An initial 864-run matrix at only three repeats per block; confirmatory size follows power | Suite-scoped public S++ claims, never universal proof | Very high |

Recommended destination: balanced, reached through lean. Use research-grade only for claims such as agent-agnostic, 20% more context-efficient, or long-horizon memory improvement.

Suggested evidence tree:

~~~text
benchmarks/
  protocol/
    schemas/
    conformance/
    claims.json
  cases-v2/
    CASE/
      case.json
      truth/
      validators/
  experiments/
    EXPERIMENT.json
  runner/
    adapters/
  evidence/
    EXPERIMENT/
      RUN-UUID/
        manifest.json
        worker-visible.json
        receipt-start.json
        receipt-closeout.json
        events.jsonl
        verification.json
        result.json
        report.md
        checksums.sha256
~~~

Reports are generated from validated JSON. Large transcripts may live in durable release storage, but their URI and hash remain in the evidence bundle. Opaque vendor logs cannot be the sole source.

Every pre-run manifest records:

- UUIDv7 run and experiment identity, arm, replicate, block, and randomization position;
- clean commit/tree and optional patch hash;
- framework, harness, case, overlay, prompt, rubric, validator, and claim-registry hashes;
- exact model ID/revision or system fingerprint when exposed, runtime/build, OS, architecture, tool versions, reasoning settings, temperature, context limits, locale, and timezone;
- fresh-session/no-context mode, filesystem root, network policy, environment allowlist hash, canary hash, and worker-visible manifest digest;
- time, token, tool-call, and cost ceilings.

Worker-visible composition fails before spawn if rubric, persona, report template, orchestrator memory, or undeclared files are visible. Worker and orchestrator run in separate processes and fresh sessions; temp directories stay inside the sandbox; result directories are atomically claimed; before/after tree hashes and treatment deltas are verified.

Trace levels remain distinct:

1. context delivered to the model, strong and observable;
2. file/tool access, observable but not proof of comprehension;
3. worker debrief, diagnostic only.

The primary process claim should be “delivered before Step 1,” not “understood.”

Controls match the claim:

- no framework for combined product value;
- one flat equal-content CONTEXT file;
- the same fragments in routed-self selection;
- the same route tree with deterministic routed-resolved receipts;
- a separate cold-continuation comparison of code-only, flat MEMORY, and routed lifecycle memory;
- a real 2 × 2 × 2 closeout factorial for command wording, session KeepInMind, and explicit session-write axiom.

Objective primary outcomes:

- executable product-contract validators;
- out-of-scope read/write and canary events;
- route/overwrite/Required Route delivery from receipts and traces;
- input tokens, context-loading calls, latency, and cost;
- re-asks, rediscovery calls, decision-citation accuracy, and defects in memory trajectories.

Communication ratings remain secondary and use two blind raters with preregistered agreement.

Statistical correction:

- assertions within one run are correlated and do not increase sample size;
- different tasks provide generalization; repeated runs estimate stochasticity;
- 5-10 runs per cell are pilots, not proof;
- with a two-sided 95% interval, 10/10 successes have only about a 69-72% lower bound depending on exact versus Wilson method;
- 36/36 flawless independent trials are the conservative exact-binomial minimum to put the lower bound at or above 90%; any failure requires more;
- samples must be powered from pilot rates, clustering, the non-inferiority margin, and multiplicity, with stopping rules fixed before confirmation.

Recommended gates:

- zero path escapes, out-of-scope writes, canary leaks, and unrecovered partial mutations;
- contract fidelity and context compliance lower 95% bound at least 90% for the declared population;
- paired context-token/loading-call ratio upper 95% bound no more than 0.80;
- fidelity non-inferiority to flat equal-content control, initially no worse than -5 percentage points and -2 for research-grade claims;
- memory re-ask/rediscovery rate ratio upper bound no more than 0.70 with no defect increase;
- two-rater agreement at least 0.80 by a preregistered statistic.

CI tiers:

| Tier | Cadence | Contents | Gate |
|---|---|---|---|
| **0 deterministic** | Every PR on Windows, Linux, and macOS | schemas, containment, resolver conformance, transaction fault injection, corpus validation | Hard block |
| **1 behavioral smoke** | Payload/harness PRs | one runtime, four cases, one run each | Block critical objective failures; no statistical claim |
| **2 nightly** | Nightly | current vs last release, four cases × two arms × three repeats | Alert and triage |
| **3 release evidence** | Release candidate | balanced randomized and powered matrix | Hard release gate |
| **4 external proof** | Major release or scheduled renewal | independent implementation, replay, memory, and research matrix | Required to renew public S++ claims |

Paid secrets and model credentials never run on untrusted forks.

### Decision 9: independent conformance and agnosticism

| Option | Evidence | What it proves | Limitation | Verdict |
|---|---|---|---|---|
| **A. Multiple runtime adapters over one TypeScript core** | Codex/Claude/etc. consume identical receipts | Integration portability | Not an independent implementation | Necessary, insufficient |
| **B. Language-neutral spec and golden fixtures** | Any implementation can reproduce canonical JSON | Reimplementability and contract clarity | Fixtures may share the original authors' blind spots | **Required foundation** |
| **C. Independently authored read-only resolver in Python or Rust** | No imports from TypeScript; byte-identical receipts | Real independent routing-kernel reproduction | Does not prove model behavior | **Required for routing S++** |
| **D. Three models on one runtime** | Model variation | Some model portability | Runtime and harness remain confounded | Do not call agent-agnostic |
| **E. Crossed or clearly reported model/runtime pairs plus external replay** | Multiple implementations, runtimes, models, and an external evidence bundle | Strongest suite-scoped portability | Expensive and never universal | Required for public agnosticism claims |

The normative specification covers entrypoint discovery, frontmatter/tag parsing, direct-child Entries, Required Routes recursion, scope precedence, base/overwrite order, phases, containment, ordering, digests, and errors. A deterministic fake agent verifies adapter delivery. A pinned open-weight model should appear in public evidence so at least one stratum is temporally reproducible.

Acceptance:

- an independent implementation produces byte-identical receipts for every fixture;
- adapters pass with the fake agent before behavioral runs;
- at least three model families and two runtimes are tested, and un-crossable pairs are reported as combined strata;
- an external party replays a hashed experiment bundle and returns a signed or otherwise attributable result bundle;
- one external author ships an extension without maintainer support.

### Recommended integrated stack

If only one architecture package is approved now, approve:

1. canonical PathPolicy and RouteGraph;
2. the dedicated resolver and receipt;
3. VirtualTree planning and journaled apply;
4. manifest v1, capability computation, and a visible lock.

Profiles, promotion, memory audit, and Benchmark V2 then reuse those foundations. Building them first would multiply lifecycle surfaces on top of unsafe primitives.

### Concrete implementation sequence and exit gates

| Phase | Deliverables | Exit gate |
|---|---|---|
| **0. Stop the line** | central canonical containment; patch find/doctor/Required Routes; adversarial Windows/POSIX tests | zero accepted escape fixtures; release blocker closed |
| **1. Read kernel** | extracted PathPolicy and RouteGraph; resolver; ordered receipts; budgets; context conformance v1 | every fixture deterministic; current manual/CLI mismatches resolved |
| **2. Write kernel** | VirtualTree; all-index precompute; plan schema; journaled apply; rollback/recovery; fault injection | every injected normal failure rolls back; crash states recover deterministically |
| **3. Local lifecycle** | extension manifest, capabilities, dependencies, lock, verify/update/remove; migrate current bundled extensions | install-update-remove round trip; no silent local-content loss |
| **4. Minimal human-led product** | lean/continuity/strict experiment; Solo Starter; claim-class authority; read-only proposals; memory audit | at least 35% startup reduction without fidelity loss; newcomers complete first verified task |
| **5. Evidence V2** | schemas, isolated runner, traces, controls, CI tiers, generated reports; lean then balanced pilots | no collision/canary/isolation defects; confirmatory design powered and preregistered |
| **6. Independent S++ proof** | public conformance spec, second resolver, external extension, crossed model/runtime evidence, long-horizon trajectories | each published S++ claim passes its own gate and names its tested population |

### Decisions the maintainer needs to make

| Decision | Options | Recommended default |
|---|---|---|
| External local files | never; explicit per-invocation roots; workspace-persisted allowlist | explicit per-invocation roots first; a persisted allowlist only through reviewed workspace policy |
| Resolver surface | extend find; new resolve; persisted bundle | new resolve; ephemeral body/bundle projection |
| Transaction guarantee | preflight only; journaled recovery; Git-only | journaled recovery, plus optional Git patch/worktree mode |
| Lock format | JSON; Markdown generated ledger; per-extension receipts | JSON for machine authority with CLI human rendering; choose Markdown only if format consistency outweighs parser simplicity |
| Extension default privilege | unrestricted; route-only; capability tiers | route-only normal tier plus explicit elevated capabilities |
| Base loading | strict; universally lean; visible startup profiles | test lean as default, preserve strict as opt-in |
| Promotion | all-human; claim classes; model confidence | claim classes, with humans owning normative truth |
| Decay | warnings; advisory audit; automatic expiry | advisory deterministic audit; never automatic deletion |
| Evidence destination | lean; balanced; research | balanced destination through lean; research only for broad public claims |
| Public naming stack | descriptive only; layered; organic rename; human-authority rename; coined frontier field | Context-Routed Development + Routed Context Cultivation + organic-growth philosophy; test Routecology before using it publicly |
| Product brand | keep; qualify; rename | do a deliberate clearance/rebrand decision before public launch because direct-market OpenForge collisions now exist |

## Keep, Add, Remove, Avoid

### Keep

- plain Markdown and Git-first review;
- one loader entrypoint;
- recursive route entrypoints and bounded generated regions;
- selection descriptions separate from execution bodies;
- directives, patterns, guidance, skills, workflows, and workspace as distinct primitives;
- working, emerging, crystallized, and archived memory states;
- memory does not own behavior;
- optional opinionated packs;
- human-led direction and inspect-before-trust posture.

### Add

- root containment and realpath policy;
- effective-context resolution with overwrite merging and receipts;
- mutation plan/apply/rollback;
- visible extension provenance, ownership, dependencies, and privileges;
- context budgets and CI gates;
- general promotion authority and read-only proposal mode;
- memory hygiene and conflict/supersession links;
- safe-solo and team-review profiles;
- machine-readable, isolated, replicated benchmark runner;
- multi-runtime conformance fixtures and compatibility matrix.

### Remove Or Replace

- replace the global KeepInMind command as the normative closeout contract; retain it as global exploration;
- replace unconditional observation-write pressure with warranted recurrence/risk criteria;
- archive applied emerging designs and refresh or retire stale rating snapshots;
- replace artifact-count memory scoring with downstream-value and correctness scoring;
- separate route-only extensions from arbitrary workspace overlays instead of granting them the same default trust;
- qualify `tiny`, `on demand`, and `agent agnostic` until budgets and multi-runtime evidence exist.

### Avoid

- more reserved tags, root categories, or memory states;
- semantic search or relevance ranking in the deterministic core;
- a hidden database or runtime scheduler;
- opinionated language/framework defaults in Core;
- semantic conflict detection pretending to be deterministic Doctor logic;
- benchmark conclusions that outrun their sample, provenance, or controls.

## Limitations And Further Questions

- The review inspected the current dirty worktree as local active truth. Extensive staged and untracked user changes predated the assessment.
- Exact historical model revisions are unavailable; the corpus uses family labels.
- Historical worker transcripts and route-read traces were not preserved, so timing claims depend partly on debrief summaries.
- The safe path-escape proof read public repository content only; no secret-bearing file was tested.
- Build reproducibility was checked twice in one environment, not across OS/runtime versions.
- Naming collision checks were lightweight public-web searches, not trademark, corporate-name, package-registry, domain, or legal clearance.
- Candidate-name scores and solution-option judgments are structured expert comparisons, not measured user-preference data.
- The scorecard is an expert assessment, not a statistical model.

Questions for the maintainer after this report:

1. Should the public stack stay purely descriptive, use the recommended **Context-Routed Development + Routed Context Cultivation** layering, or attempt the frontier-field move with **Routecology** after comprehension testing?
2. Does the direct-market `OpenForge` collision justify a product rename before public launch, or should the current name remain a qualified project codename?
3. Should the lifecycle ledger be `open-forge.lock.json` for strict machine determinism or a generated `open-forge.lock.md` for Markdown consistency?
4. Should external local roots be allowed only per invocation, or can a reviewed workspace policy persist an allowlist?
5. Is route-only the default extension privilege, with explicit approval for directives, load tags, CurrentTruth, scripts, AGENTS, and non-`.agents` overlays?
6. Should the measured 9-file lean load profile become the default if its A/B run preserves fidelity, with continuity and strict profiles available visibly?
7. Who owns normative promotion in solo and team use, and which mechanically verified facts—if any—may a declared policy crystallize without a separate human action?
8. Is the first adoption target Solo Starter, Team Review, or a bare integrator kernel? Building all three simultaneously would dilute the next evidence round.
9. Which public S++ claims are actually worth funding: kernel conformance, context efficiency, memory value, runtime portability, or the complete research matrix?
10. Is Open Forge ultimately limited to software repositories, or should non-software `Context-Routed Work` remain a later evidence-backed expansion?

## 2026-07-17 Implementation Addendum

This addendum records what changed after the assessment. It does not retroactively inflate the historical ratings or turn candidate judgment into accepted truth.

Implemented since the original review:

- Route lookup, generated expansion, Required Routes, `doctor`, `chain`, and index planning now reject absolute, parent-traversing, cross-drive, and physical link escapes. `chain` exposes loader, ancestor entrypoints, native skill boundary, target, and overwrite companions for any Markdown heading.
- Normal installation is Core-first and target-scoped Git-checkpointed. Noninteractive work outside Git fails without mutation; interactive use requires explicit approval. Core managed, scoped, and generated writes share link/hardlink preflight and rollback. Extensions cannot install Git control paths that would hide their own diff. `--pro` bypasses lifecycle gates only.
- The first-party catalogue now has 18 independently selectable or composable units across skill-only, workflow-only, directive-only, pattern-only, mixed, and dependency-only shapes. Transitive dependencies auto-select as required. Native/APM skills work additively at distinct paths; substituting one for a first-party capability is deliberately not claimed.
- Workflows now start with Mode (`linear` or `iterative`), keep Constraints always present, and use one common goal-oriented contract. The loader defaults non-trivial work to a matching workflow, makes multiple matches ordered handoffs, recommends workflow/direct choice once when no exact match exists, and honors explicit no-workflow requests.
- Directive applicability is explicit through `Applies To`; root placement no longer silently means workspace-wide. Missing, empty, inherited, and none local category Axioms add nothing while ancestor axioms remain active.
- Architecture and vision now classify starting state, elucidate provenance, derive warranted directives/patterns/guidance/workspace/memory support, require user accord before normative promotion, and keep useful unaccepted deductions emerging.
- CLI command contracts run through real subprocesses in fresh OS temporary workspaces with real Git, filesystem and no-partial-write assertions, packaged-layout smoke tests, real catalogue integration, and additive external-skill byte-preservation coverage.

What still prevents an honest S++ claim:

- `find --tag KeepInMind` is global discovery, not an active loaded-context receipt. The system still lacks a stable context digest, budget, and recursively composed Required Routes/overwrite receipt.
- Extension ownership, provenance receipts, compatibility solving, update/remove/migration, and substitution of externally managed capabilities remain unimplemented.
- Workflow-first wording and static contracts are not behavioral proof. Exact-match, no-match, opt-out, ordered-handoff, and greenfield support derivation still need repeated multi-model seeds without harness prompting.
- Independent conformance, third-party author success, cross-OS release evidence, longitudinal memory value, and causal/public benchmark controls remain absent.

The revised option matrix and shortest credible sequence live in `.agents/memory/emerging/ideas/rating-ladder.md`. The framework is materially safer and more coherent than the 2026-07-12 snapshot, but the evidence still supports “S-tier design candidate,” not “S++ demonstrated system.”

## Artifact Source Notes

- Delivery mode: MCP app technical report; the routed Markdown analysis is the supporting source, not a second report surface. The full report was revised on 2026-07-13 rather than replacing the prior evidence with a naming-only addendum. The expanded revision evaluates more than one hundred conventional, organic, human-inspired, mnemonic, and coined constructions while keeping the serious decision set intentionally smaller.
- Reproducible artifact-row SQL: `.agents/memory/emerging/analysis/2026-07-12_open-forge-framework-assessment.sql`; it materializes only verified constants and assessment outputs, while the Markdown reports, source, and tests remain factual provenance.
- Required-structure mapping: Technical Summary; Key Findings (`The Good`, `The Bad`, `The Ugly`); Naming and Positioning; Scope/Data/Definitions; Method and Evidence Quality; Limitations and Robustness; S++ Requirements; S++ Solution Options; Concrete Sequence; Further Questions.
- Chart map: `Mean core rubric scores, v7-v10`; question - which tested dimensions were strongest; family - horizontal comparison bar; rows - five dimensions across 16 structured reports; fields - dimension, average score, perfect-report count, report count, maximum score; supported claim - product fidelity and directive compliance scored above routing, memory, and communication; palette - single blue root; scale - zero to two; caveat - subjective descriptive scores, changing harness, no causal interpretation, v11 excluded.
- No additional quantitative chart was used because startup cost has only two comparable observations (default and current dogfood), while generation-by-generation lines would be underpowered and misleading as a trend because the harness changed.
- Name scores and the frontier atlas remain tables because they are explicit judgment dimensions and qualitative screening, not measured preference data; charting totals would imply false precision.
- The 9-file/2,153-word lean and 12-file/2,538-word continuity candidates were recomputed directly from the current installable payload. They are design inputs, not performance outcomes.
- Current naming context used public pages for Kubernetes CRDs, Context-Driven Development, Adaptive Software Development, evolutionary development, living documentation, direct OpenForge market collisions, Contextive, Contexture, Contextweave, Seedwork, Grove Method/GROVE, and stigmergic agent coordination. Routecology, Routogenesis, and Contexticulture received exact-phrase screens. This was collision screening only.
- Source calculations and report data were independently recomputed from current files before artifact validation.
