---
open-forge:
  description: Analysis of how Open Forge can turn incomplete natural conversation into decision-ready context without questionnaires, mandatory phases, or avoidable ceremony
  tags: [Memory, Archived, Analysis, Reasoning, Contextual, Historical, Product, Experience, Decision, Elicitation, Workflow, SelfGrowth]
---

# Adaptive Decision Elicitation

## Outcome

The analysis produced the accepted [Adaptive Decision Elicitation](../../crystallized/decisions/adaptive-decision-elicitation.md) decision and the corresponding loader, Workflow, current-document, and public-entry changes. A short [novice and expert interaction probe](../sessions/2026-07-29_adaptive-decision-elicitation-probe.md) supported the model and identified one wording refinement. Later [brownfield and greenfield expertise probes](../sessions/2026-07-29_adaptive-expertise-probes.md) confirmed adaptive questioning and added proportional delivery depth to the contract.

## Question

How should Open Forge help technical and nontechnical users surface consequential decisions, reach a strong starting direction, and preserve the resulting knowledge without requiring them to understand software design, framework roles, document schemas, or a predefined development process?

## Current Conclusion

The existing architecture already provides most of the required storage, authority, lifecycle, and specialization mechanisms. Vision, Architecture, Planning, Experience Design, Templates, Memory transitions, and #Core promotion can represent the desired result.

The main gap is the default behavioral bridge between an incomplete natural request and those mechanisms. The Framework says to clarify material ambiguity, but it does not yet express a sufficiently strong and natural contract for proactive decision elicitation.

The desired experience can be summarized as:

> Conversation is the primary input. Durable context is the structured output and future input.

Users should provide goals, reactions, preferences, and consequential judgment in ordinary language. Agents should infer a working model from available context, expose assumptions, recommend coherent defaults, ask only questions whose answers may materially change the result, and translate accepted direction into the appropriate authoritative sources.

Users should not need to know whether they are "using Open Forge correctly." Selecting `routes`, Workflows, Skills, Templates, Memory states, and final destinations is agent work. The user-facing evidence of correct use is a conversation in which:

- The agent explains its current understanding and important assumptions
- Consequential choices become visible before dependent work
- Recommendations make tradeoffs understandable
- Questions ask for user judgment rather than outsourced technical specification
- Settled direction is not repeatedly reopened
- Accepted results and remaining uncertainty are summarized clearly
- Durable material appears in the workspace without requiring the user to operate the taxonomy

## Desired Experience

### Technical User

A technical user may already specify structure, boundaries, or implementation preferences. The agent should reuse that information, challenge only material risks or contradictions, and avoid asking questions the user has already answered.

### Nontechnical User

A nontechnical user should not need to invent architecture or know which Open Forge document applies. The agent should:

- Explain choices through outcomes, risks, cost, control, and tradeoffs rather than implementation jargon
- Recommend a direction instead of presenting an unbounded menu
- Ask for judgment only where the user's goals or preferences are genuinely authoritative
- Translate accepted outcomes into technical direction and durable workspace context

### Small And Established Work

When accepted context already determines the relevant behavior, ordinary work should proceed without a discovery ritual. Readiness is contextual, not a fixed checklist or document count.

### Large Or Ambiguous Work

When missing information could materially change product behavior, experience, structure, risk, cost, or verification, the agent should surface those choices before implementation depends on silent assumptions.

## Current Strengths

- The loader gives clear user direction authority and requires clarification before material ambiguity affects dependent work.
- The Vision Workflow asks only questions whose answers could materially change direction.
- Vision separates stated needs, observations, inference, assumptions, and candidates.
- Architecture requires responsibilities, boundaries, dependency direction, failure containment, tradeoffs, and adoption slices.
- Planning requires accepted outcomes, non-goals, dependencies, decision points, and observable verification.
- Experience Design covers journeys, alternate states, accessibility, recovery, trust, and implementation handoff.
- Memory distinguishes active, candidate, accepted, and historical material.
- Accepted current sources, Decisions, and #Core roles already separate present meaning, rationale, and reusable behavior.
- Templates can make warranted artifacts cheap without requiring the user to fill forms.
- Observations can preserve grounded preferences and recurring corrections before evidence justifies a Pattern, Guidance entry, Directive, or another current source.

## Current Gaps

### Proactive Elicitation Is Too Implicit

The loader says to clarify ambiguity, but an agent may still begin implementation before noticing an important unasked product, experience, architectural, or verification decision.

The current rule reacts to recognized ambiguity. It does not clearly require the agent to identify consequential missing decisions from sparse intent.

### Workflow Selection Depends On Agent Initiative

The installed Workflow descriptions are visible, but a user who simply asks to build something depends on the agent recognizing that Vision, Architecture, Experience Design, or Planning would materially help.

The Framework does not yet state the transition rule strongly enough:

> If the request is not decision-ready, shape it before developing it. If it is decision-ready, do not manufacture another phase.

### Confirmation Can Become Interrogation

The Workflows identify what must become clear, but they do not consistently describe the interaction style used to reach clarity.

Without a shared interaction contract, one agent may recommend and ask one high-value question while another emits a large questionnaire.

### Preservation Is Correct But Distributed

Vision and Architecture route accepted results to current sources and useful supporting material to #Core or #Memory. The Memory contracts explain acceptance and transition.

This is structurally correct, but the complete conversational outcome depends on the agent synthesizing several contracts:

- Accepted present meaning into a current authoritative source
- Useful rationale into a Decision
- Unsettled alternatives into Emerging
- Standing behavior into Directives or Guidance
- Reusable inspectable form into Patterns
- Grounded user tendencies into Observations until their scope and recurrence justify promotion

The concepts should remain separate, but one compact behavioral instruction should connect them.

### Public Product Language Understates The Experience

The README explains routes, authority, Memory, and growth well. It does not yet make the simplest user promise explicit:

> Start by talking normally. Open Forge helps the agent surface important decisions and preserve what the workspace should remember.

## Candidate Improvements

### Add A Compact Baseline Decision Contract

Add one small loader-level contract because this behavior should apply before optional Workflows are selected:

- Build the best current model from the request and available accepted context.
- Identify only missing choices that could materially change the outcome, scope, experience, structure, risk, cost, or verification.
- Recommend a coherent default and explain its important tradeoffs.
- Ask for user judgment where goals or contextual preferences are authoritative.
- Phrase questions through understandable outcomes rather than requiring implementation expertise.
- Do not repeat settled questions, demand exhaustive specifications, or block ordinary work on low-impact preferences.
- Preserve assumptions visibly when work can safely proceed without another answer.

This should be one compact `axiom` or a very small group, not a new methodology.

### Strengthen Workflow Transitions

Vision should explicitly use adaptive conversation:

1. Infer a candidate model from current information.
2. Surface assumptions and a recommendation.
3. Ask the smallest useful question or question group.
4. Incorporate the answer and repeat only while material uncertainty remains.
5. Stop when the direction is decision-ready at the fidelity the work needs.

Development should explicitly return to the relevant shaping capability when a missing choice would materially alter the change. It should not require a complete Vision or Architecture document when accepted context already makes the slice ready.

Architecture and Experience Design should be selected for their distinct unresolved questions, not as automatic stages after Vision.

### Make Documents An Outcome, Not A Form

The user should not normally choose a Template or populate its headings.

After conversation establishes warranted accepted material, the agent can instantiate or update the appropriate document, remove irrelevant sections, and show the result for review. Templates remain internal accelerators for coherent capture.

### Make Capability Selection Visible Without Ceremony

When selecting a Workflow or Skill materially changes the interaction, the agent should state what it is doing and why in one natural sentence.

The explanation should describe the useful action, such as clarifying product direction or mapping a destructive update journey. It should not require the user to understand route paths or internal framework vocabulary.

Users may select a capability explicitly, but ordinary natural intent should remain sufficient.

### Keep Core And Toolkit Responsibilities Distinct

The universal decision-elicitation contract belongs in the base Framework because users should benefit before any optional Workflow is installed.

Detailed Vision, Architecture, Planning, Development, Review, and Experience Design methods can remain in the optional development toolkit. They deepen a selected kind of work without making development methodology part of Core.

Onboarding may recommend the toolkit for development work, but the base Framework should still prevent silent consequential assumptions and unnecessary questioning.

### Make Self-Growth Deliberate

At meaningful decision boundaries, the agent should consider whether the interaction produced:

- Accepted current meaning
- Useful rationale
- A reusable behavior or choice heuristic
- A reusable inspectable shape
- A grounded preference or correction worth observing
- An unsettled idea or alternative worth preserving

"No durable material warranted" remains valid. Extraction should maximize future value, not file count.

### Explain The Natural Conversation Path

Public documentation should include one short example in which a sparse request becomes:

- A small number of consequential questions
- A recommended direction with explicit assumptions
- Accepted Vision or Architecture
- A supporting Decision
- A scoped Pattern, Directive, Guidance entry, or Observation only when warranted
- An executable plan

The example should demonstrate that users do not need to know Open Forge vocabulary before benefiting from it.

### Add Deterministic Assistance Later

The CLI can eventually make role selection and artifact creation cheaper through `help roles`, syntax help, placement suggestions, and Template-backed scaffolding.

It should not privately infer which product decision is correct. It may expose missing deterministic structure and show transparent candidate destinations.

## What Not To Add

- No mandatory requirements phase
- No universal questionnaire
- No fixed document set or completeness score
- No rule requiring Vision, Architecture, Experience Design, and Planning for every change
- No automatic promotion of inferred preferences
- No new primitive for requirements, discovery, or conversation
- No large baseline prompt that restates ordinary reasoning

## Evaluation

Test the behavior with at least four matched cases:

1. A technical user who already supplies most consequential direction
2. A nontechnical user with a broad product goal
3. A small change whose accepted context is already sufficient
4. A large or risky change with several consequential unknowns

Compare Framework-only and toolkit-assisted runs where useful.

Measure:

- Material decisions surfaced before implementation
- Consequential omissions
- Unnecessary or repeated questions
- Use of recommendations rather than unbounded option lists
- Clarity for a nontechnical user
- Whether the user can understand which decision is being requested and why without knowing Open Forge vocabulary
- Separation of accepted, inferred, and candidate material
- Quality and sufficiency of resulting current documents
- Correct extraction into Decisions, #Core, Emerging, and Observations
- Number of turns and amount of ceremony required to become decision-ready
- Whether the final implementation can proceed without guessing material intent

## Related Current Sources

- [Open Forge Vision](../../crystallized/documents/vision.md)
- [Open Forge Principles](../../crystallized/documents/principles.md)
- [Accepted State and Synchronization](../../crystallized/documents/framework/truth.md)
- [Memory Transitions](../../crystallized/documents/framework/memory/transitions.md)
- [Vision Workflow](../../../../workflows/vision.md)
- [Architecture Workflow](../../../../workflows/architecture.md)
- [Planning Workflow](../../../../workflows/planning.md)
- [Experience Design Skill](../../../../skills/experience-design/SKILL.md)
- [Documentation comprehension probes](../ideas/documentation-comprehension-probes.md)
