---
open-forge:
  description: Open CLI tasks, the selected migration and on-demand follow-ups
  tags: [Memory, Working, CLI, Task, Contextual, Active]
---

# Open CLI Tasks

The current selection is 1.0 polish, listed below. Open and unstarted Tasks stay
here. Completed Tasks and their packets moved to Archived Memory on
2026-09-25, as listed under [Archived on 2026-09-25](#archived-on-2026-09-25).
Older completed and superseded records are preserved under
[Archived CLI Development](../../../archived/cli-development/_cli-development.md).

Read the parent Task before a phase subtask. A subtask carries actionable
evidence and may narrow, but never broaden, its parent Task.

The [Potential CLI Tasks](potential/_potential.md) route is a candidate queue,
not active execution authority.

## Beta ordering

Accepted on 2026-09-17. **The bar for beta is that everything a user touches is
polished and there are no stupid bugs.** Architecture work is explicitly second
priority, however much it wants doing.

Read this ordering before picking up unrelated beta work. The later Task 38
selection is retained as completed history; the current beta specification
selection above does not cancel these goals.

**Blocking beta — a user sees this**

1. [41 Beta journey scenarios](task41-beta-journey-scenarios.md) — nothing else
   is measurable until we know what a user actually does. Everything below is
   partly informed by what this finds.
2. [39 Output audit](task39-output-audit.md) — representative manifest errors corrected; broader command-family audit remains.
3. [42 Minimal core](task42-minimal-core.md) and
   [43 Workflows as a skill](task43-workflows-as-skill.md) — extraction is present in the shipped payload; documentation is aligned; historical upgrade-evidence reconciliation remains.
4. [46 Routed Skill resources](task46-routed-skill-resources.md) and
   [47 Entrypoint reachability](task47-entrypoint-reachability.md) — the current scratch recheck finds a healthy fresh install. Skill
   catalogue repair advice and missing-entry naming are corrected;
   the beta packet supersedes the older thirteen-warning report.
5. [44 Template and core file content](task44-template-content.md) — the prose a
   user reads most, and it depends on 42 deciding which files survive.
6. [37 Wording review](task37-wording-review-against-proposals.md) — held by the
   maintainer directly.
7. [32 Minimal output sweep](task32-minimal-output-sweep.md) — what shows on
   screen, and what it costs in tokens.
8. [34 Interpolated value markup](task34-interpolated-value-markup.md) — small,
   visible, cheap.

**Beta safety net — approved journeys implemented; coverage follow-ups remain**

9. [45 End-to-end observability](../../../archived/cli-development/tasks/task45-end-to-end-observability.md)
10. [40 Capture coverage](task40-capture-coverage.md)

Neither changes what a user sees. Both decide whether we would find out when it
breaks, which is why they sit above the architecture work rather than with it.

**After beta — architecture and behaviour**

11. [48 Scoping for Extension routes](task48-scoping-for-extension-routes.md) —
    the Core reduction left every nested managed route inside an Extension, and
    the scaffold cannot see them. Carries the route-init coverage that had to be
    dropped.
12. [38 Project and test split](../../../archived/cli-development/tasks/task38-project-and-test-split.md)
13. [31 Implementation duplication](task31-implementation-duplication.md)
14. [33 Managed content removal](task33-managed-content-removal.md)
15. [35 Removal and suppression model](task35-removal-and-suppression-model.md)
16. [36 Extension merge and guards](task36-extension-merge-and-guards.md)

[Task 30](task30-cli-experience-remediation.md) stays open underneath all of
this; its remaining phases feed 32, 37 and 39.

## 1.0 polish

Recorded on 2026-09-25, after the public beta. The maintainer selected the
documentation site for implementation and asked for the others as tasks.

1. [52 Documentation site](task52-documentation-site.md) — integrated into
   local `develop`. The maintainer merges to `main` and enables Pages.
2. [53 Loading and scoping audit](task53-loading-and-scoping-audit.md) —
   recorded, not started.
3. [54 Tag trimming](task54-tag-trimming.md) — recorded, not started. It runs
   after or alongside 53 because both edit the same frontmatter.
4. [55 Alternative root](task55-alternative-root.md) — recorded, not started.
   An investigation into roots such as `.apm`. Decide it before 1.0 fixes the
   layout, because a root change breaks installed workspaces.

## Archived on 2026-09-25

These completed records moved to [Archived CLI Tasks](../../../archived/cli-development/tasks/_tasks.md).
Their links from open Tasks now point there.

- [Task 38, Project and Test Split](../../../archived/cli-development/tasks/task38-project-and-test-split.md), with its [migration packet](../../../archived/cli-development/tasks/task38/_task38.md)
- [Task 45, End-to-end observability](../../../archived/cli-development/tasks/task45-end-to-end-observability.md), with its [G6 evidence](../../../archived/cli-development/tasks/task45/_task45.md)
- [Task 50, Unified Remove](../../../archived/cli-development/tasks/task50-unified-remove.md)
- The completed [Task 30 G1, B1, and G4 packets](../../../archived/cli-development/tasks/task30/_task30.md) and the [G4 execution packet](../../../archived/cli-development/tasks/task30-g4/_task30-g4.md)
- The completed [Task 31 M1, M3, and M4 subtasks](../../../archived/cli-development/tasks/task31/_task31.md)
- The completed [beta correction execution record](../../../archived/cli-development/tasks/beta-follow-ups/_beta-follow-ups.md)

## Entries

- [Remaining beta product work and delivery verification after the CLI migration](beta-follow-ups.md) - #Memory #Working #Backlog #CLI #Beta #Contextual
- [Specifications for actionable errors, stale Core documentation and Skill navigation follow-ups](beta-follow-ups/_beta-follow-ups.md) - #Memory #Working #Plan #CLI #Contextual
- [Candidate follow-up tasks distilled from the reviewed CLI analyses; not active execution authority](potential/_potential.md) - #Memory #Working #CLI #Task #Potential #Contextual
- [Open Task 30 for CLI experience remediation, with actionable phase state and evidence carried from Emerging Analysis](task30-cli-experience-remediation.md) - #Memory #Working #CLI #Task #Remediation #Contextual #Active
- [Task 30 phase subtasks carrying actionable evidence from the Emerging CLI analyses](task30/_task30.md) - #Memory #Working #CLI #Task #Subtask #Contextual #Active
- [Open Task 31 for removing duplicated CLI implementation while preserving command-local contracts](task31-implementation-duplication.md) - #Memory #Working #CLI #Task #Duplication #Refactoring #Contextual #Active
- [Task 31 phase subtasks carrying duplication measurements, refactoring boundaries, and acceptance evidence](task31/_task31.md) - #Memory #Working #CLI #Task #Subtask #Contextual #Active
- [Open Task 32 to review whether the Workspace echo and the Next action belong in minimal output across all 28 commands](task32-minimal-output-sweep.md) - #Memory #Working #CLI #Task #Presentation #Minimal #Contextual #Active
- [Open Task 33 to decide whether individually removing managed Extension and Framework content is supported, and to reconcile the removability promise with what the commands actually allow](task33-managed-content-removal.md) - #Memory #Working #CLI #Task #Removal #Ownership #Extension #Contextual #Active
- [Open Task 34 to visually distinguish command names, paths, identifiers and arguments interpolated into user-facing sentences, and to establish it as an authoring rule](task34-interpolated-value-markup.md) - #Memory #Working #CLI #Task #Presentation #Wording #Accessibility #Contextual #Active
- [Open Task 35 to explore a unified removal and suppression model across Routes, Extensions and Libraries, pinning current behaviour with characterization tests before any design is chosen](task35-removal-and-suppression-model.md) - #Memory #Working #CLI #Task #Exploration #Removal #Ownership #Lifecycle #Contextual #Active
- [Open Task 36 to design partial file merging by Extensions and to replace the comment guards in authored Markdown with a boundary an agent still reads as an instruction](task36-extension-merge-and-guards.md) - #Memory #Working #CLI #Task #Extensions #Markers #Authoring #Contextual #Active
- [Open Task 37 to compare every shipped CLI sentence against the unaccepted G4 output proposals and adopt, merge or reject each on its merits](task37-wording-review-against-proposals.md) - #Memory #Working #CLI #Task #Wording #Review #Contextual #Active
- [Open Task 39 to audit every CLI output for actionability, confirm the G4 conversion actually improved each command, and find remaining legacy and evidence gaps](task39-output-audit.md) - #Memory #Working #CLI #Task #Output #Audit #Regression #Contextual #Active
- [Open Task 40 to close the gap between the accepted finding vocabulary and the situations any capture actually exercises, so the output invariants guard more than a quarter of what the CLI can print](task40-capture-coverage.md) - #Memory #Working #CLI #Task #Capture #Coverage #Evidence #Contextual #Active
- [Open Task 41 to define the user journeys a beta must survive, run them by hand or by agent, and turn the findings into the polish list before release](task41-beta-journey-scenarios.md) - #Memory #Working #CLI #Task #Scenario #Beta #Release #Contextual #Active
- [Open Task 42 to shrink the installed core to what every workspace needs, moving deeper routes into Extensions a user opts into](task42-minimal-core.md) - #Memory #Working #CLI #Task #Core #Extensions #Routing #Beta #Contextual #Active
- [Open Task 43 to narrow Workflows and express them through the skill mechanism, so they stop overlapping with Skills and can be selected when they become relevant](task43-workflows-as-skill.md) - #Memory #Working #CLI #Task #Workflows #Skills #Framework #Beta #Contextual #Active
- [Open Task 44 to fix what the installed templates and core files say, since they are the first Open Forge prose a beta user reads](task44-template-content.md) - #Memory #Working #CLI #Task #Templates #Wording #Beta #Contextual #Active
- [Open Task 46 to let a native Skill's resources be used as routes and to read route metadata from the native SKILL.md frontmatter instead of demanding an Open Forge block](task46-routed-skill-resources.md) - #Memory #Working #CLI #Task #Skills #Routing #Metadata #Beta #Contextual #Active
- [Task 47 follow-up to make default Index reach routed resources through native Skills](task47-default-skill-indexing.md) - #Memory #Working #CLI #Task #Skill #Index #Contextual
- [Open Task 47 to make index and route navigation reach every recognized entrypoint form from the loader, so a catalogue does not need to be named by hand to stay current](task47-entrypoint-reachability.md) - #Memory #Working #CLI #Task #Routing #Navigation #Index #Beta #Contextual #Active
- [Open Task 48 to extend Framework route scaffolding and scope insertion to routes an Extension created, so scoping is a property of the routing model rather than of whatever Core happens to ship](task48-scoping-for-extension-routes.md) - #Memory #Working #CLI #Task #Routing #Scopes #Extensions #RouteInit #Contextual #Active
- [Task 52 Docusaurus documentation site with getting started, concepts, a file-by-file Extension reference, and GitHub Pages publication](task52-documentation-site.md) - #Memory #Working #Task #Documentation #Site #Docusaurus #Contextual #Active
- [Open Task 53 to audit every LoadNow and KeepInMind entry and the default scoping before 1.0, so startup context holds only what omission would cost more than reading](task53-loading-and-scoping-audit.md) - #Memory #Working #Task #Framework #Loading #Scope #Release #Contextual #Active
- [Open Task 54 to trim tags that add no selection, search, or loading value before 1.0, in shipped files first and then in this workspace](task54-tag-trimming.md) - #Memory #Working #Task #Framework #Tags #Release #Contextual #Active
- [Open Task 55 to investigate running Open Forge under another root such as .apm, so users can also use APM frontmatter, packaging, and compilation](task55-alternative-root.md) - #Memory #Working #Task #Framework #Root #APM #Investigation #Release #Contextual #Active
