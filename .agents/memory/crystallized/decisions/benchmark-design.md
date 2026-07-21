---
open-forge:
  description: Benchmarks compose agnostic scenarios, reusable primitive blocks, and reproducible meta-scenarios under trace-reviewing orchestrators
  tags: [Memory, Decision, CurrentTruth, Benchmark, Dogfood, Evaluation]
---

# Benchmark Design

Open Forge benchmarks are a composable engineering dogfood loop, not a self-authenticating causal-study platform.

## Stable Building Blocks

- A `scenario` is a small framework-agnostic task fixture: explicit string id, exact worker prompt, hidden task review, optional persona, and optional ordinary project files. It selects no Open Forge primitive or extension.
- A `primitive block` is one explicit reusable worker-visible input such as a directive, pattern, Memory record, guidance file, Workspace route, skill, workflow, or workspace-delivered local tool. A block has its own id, kind, non-empty payload, and hidden review focus. A local tool block may include its ordinary tool files and a `.agents/workspace/**` discovery route.
- Primitive blocks may be individually ordinary and become a probe or trap only in a particular combination. Logical conflict is intentional evidence; undeclared file collision remains an error.
- Folder hierarchy groups blocks for people. Manifests own identity and composition, so moving a folder does not change an id.
- Stable blocks should remain small enough to reuse across tasks and models without rewriting them for each run.

## Meta-Scenarios And Variants

- A `meta-scenario` is the normal runnable unit. Its plain manifest selects exactly one scenario, an ordered set of primitive blocks, and bundled extension ids. Its hidden review states why that combination is meaningful, including any intended trap.
- Every stable base or trap is its own exact meta-scenario. Base and trap meta-scenarios may share the same scenario and most inputs so their declared composition difference is inspectable without a second stable variant language.
- A run-local variant may add one or more external primitive packages or bundled extensions to an unchanged meta-scenario. Variants are fixed before workers launch, frozen outside the worker workspace, and recorded in resolved composition.
- On-demand variants do not mutate stable scenarios, blocks, meta-scenarios, or source framework files. Promote a useful repeated variant into a stable block or meta-scenario only after review.
- Plain files are the complete manual interface. CLI support may discover, compose, baseline, validate, and capture runs, but is not required to understand or reproduce the contract.

## Runs And Comparison

- A run composes current Core, declared extensions, the pure scenario payload, ordered primitive payloads, and run-local variants in a fresh Git workspace. Orchestrator-only prompts, reviews, personas, and traces remain outside it.
- The runner prepares a meta-scenario id. Pure scenarios and primitive blocks remain independently discoverable building blocks rather than separately executable runner modes.
- The resolved composition and every selected scenario, primitive, meta-scenario, and variant input are frozen with the run before worker work begins.
- Prepared runs live under an explicitly selected root that does not overlap the source repository. Equal worker cells compare both the frozen worker-prompt SHA-256 and baseline Git tree id rather than trusting ids alone.
- For model variance, use the same frozen instructions and composition across fresh workers and change only the declared model/runtime cell. For treatment variance, prepare control and treatment before either outcome is seen and keep the declared composition delta minimal.
- Provider-native tools are runtime inputs, not files the runner can honestly install. A run set freezes the requested tool surface and the orchestrator records the actual binding; a local tool shipped into the workspace remains an ordinary `tool` primitive block.
- The orchestrator owns scheduling, interaction, and trace review. It uses the complete observable subagent trace available from the runtime—messages, tool calls and results, and exposed reasoning summaries—without claiming access to invisible private chain-of-thought.
- After task work stops, the same worker gives a response-only self-review of what it believes it accomplished, how it worked, what it verified, its assumptions or deviations, and what may remain wrong.
- The orchestrator forms its Behavior, Outcome, and Limits findings from the trace, actual workspace, baseline delta, and proportionate checks before comparing them with the worker account.
- The worker account is evidence of awareness, not proof of outcome. When a runtime does not expose some behavior, that behavior remains unknown; a comparison must state the actual trace boundary.

## Validation And Scope

- `doctor` and `find --follow-required` remain the paired complete-workspace validators at preparation and finish. Preparation requires both to pass; finish records their failures as part of the observed outcome rather than preventing capture of a failed run.
- Default runs preserve frozen inputs, the live workspace and Git baseline, the final delta, validation output, observable trace, limitations, and both reviews. They do not require owner tokens, seals, eligibility classes, fixed rating ids, or causal claims.
- Coverage claims are semantic and bounded. A coverage map names what a meta-scenario is designed to observe; it does not infer adherence merely because a primitive was installed.
- Scenario reviews own task behavior and outcome independent of selected framework inputs. Primitive reviews own each block's observable semantic effect. Meta-scenario reviews own routing, relationships, traps, and interactions among the selected inputs; they do not duplicate the individual oracles.
- Historical reports remain raw engineering context. They do not define the current benchmark interface or prove framework causality.
