-- Reproducible row model for the MCP technical report, revised 2026-07-13.
-- Values are verified extracts and assessment outputs documented in
-- 2026-07-12_open-forge-framework-assessment.md. This SQL does not replace the
-- Markdown reports, source code, or tests as factual provenance.

-- query: headline_metrics
SELECT
  19 AS startup_files,
  20 AS payload_files,
  3397 AS startup_words,
  4329 AS dogfood_startup_words,
  20 AS structured_reports,
  0 AS v11_clean_causal_reports,
  34 AS passing_tests,
  248 AS assertions;
-- end query

-- query: benchmark_core_scores
WITH benchmark_core_scores(
  dimension,
  average_score,
  perfect_count,
  report_count,
  maximum_score,
  scope
) AS (
  VALUES
    ('Product fidelity', 1.8125, 13, 16, 2, 'v7-v10'),
    ('Directive compliance', 1.75, 12, 16, 2, 'v7-v10'),
    ('Routing behavior', 1.625, 10, 16, 2, 'v7-v10'),
    ('Communication', 1.5625, 9, 16, 2, 'v7-v10'),
    ('Memory growth', 1.5625, 9, 16, 2, 'v7-v10')
)
SELECT *
FROM benchmark_core_scores
ORDER BY average_score DESC, dimension ASC;
-- end query

-- query: risk_register
WITH risk_register(rank, priority, severity, risk, status, required_fix) AS (
  VALUES
    (1, 'P0', 'Critical', 'Route containment', 'Fresh exploit proof plus source verification', 'Reject absolute, parent, drive, and realpath/symlink escapes everywhere routes resolve.'),
    (2, 'P0', 'High', 'Effective-context mismatch', 'Source verified; observed overwrite miss', 'Resolve active loader chains, ancestors, overwrites, and Required Routes with explanation and digest.'),
    (3, 'P0', 'High', 'Partial mutation', 'Source verified; no whole-operation rollback tests', 'Preflight, stage, validate, atomically apply, and roll back install/extend/create/index/update/remove.'),
    (4, 'P0', 'High', 'Benchmark isolation and provenance', 'Raw-report contradictions and v11 collisions', 'Use no-context workers, run UUIDs, clean tree hashes, traces, validated JSON reports, controls, and replication.'),
    (5, 'P1', 'High', 'Extension trust and ownership', 'Documented MVP gap plus broken textual dependency', 'Add manifest, dependencies, conflicts, owned files, hashes, privilege classes, verify/update/remove.'),
    (6, 'P1', 'Medium', 'Bootstrap/context tax', 'Fresh measurement', 'Budget startup words/tokens and A/B demote noncritical empty or historical roots.'),
    (7, 'P1', 'Medium', 'Promotion and memory decay', 'Contract and route-state review', 'Add owner acceptance, read-only proposals, warranted writes, staleness, and supersession hygiene.')
)
SELECT *
FROM risk_register
ORDER BY rank ASC;
-- end query

-- query: perspectives
WITH perspectives("order", perspective, excellent, main_cost, blocker) AS (
  VALUES
    (1, 'Solo developer', 'Portable files, no service, Doctor, reduced rediscovery', 'Taxonomy and memory ceremony before local value', 'No five-minute safe profile or preview/rollback'),
    (2, 'Team lead / reviewer', 'Candidate vs current truth, rationale, bounded diffs', 'No approval ownership; noisy memory/index changes', 'No effective-context receipt or authority policy'),
    (3, 'New adopter', 'Clear voice and coherent concepts after learning', 'High jargon and twenty files before payoff', 'Installs a substrate, not a completed golden path'),
    (4, 'Runtime integrator', 'Relative paths, JSON output, native skill deference', 'Self-enforcement and no conformance spec', 'No safe deterministic effective-context API'),
    (5, 'Security skeptic', 'Plain text, local bundles, release checksums', 'Trust is inspected after mutation', 'No containment, privilege plan, provenance, or rollback'),
    (6, 'Maintainer', 'Dogfood, candid reports, descriptors, tests', 'Mirror/prose maintenance and subjective evidence', 'No automated alignment and benchmark release gate')
)
SELECT *
FROM perspectives
ORDER BY "order" ASC;
-- end query

-- query: roadmap
WITH roadmap("order", priority, action, acceptance) AS (
  VALUES
    (1, 'P0', 'Contain every route resolution', 'Windows/POSIX adversarial fixtures cannot read or write outside the selected root.'),
    (2, 'P0', 'Ship active-context resolve with overwrite order, ancestry, recursive requirements, reasons, budgets, and digest', 'Output exactly matches versioned conformance fixtures and reconstructs expected startup/closeout context.'),
    (3, 'P0', 'Make lifecycle operations planned and atomic', 'Injected last-file failures leave targets byte-identical; collisions require explicit policy.'),
    (4, 'P0', 'Add extension manifest, ownership, dependencies, privilege, verify/update/remove', 'Install-update-remove round trip preserves local content and detects every collision before write.'),
    (5, 'P1', 'Budget bootstrap and A/B a lean load tier', 'At least 35% fewer mandatory default words with no material routing-fidelity loss.'),
    (6, 'P1', 'Add promotion authority, read-only proposals, warranted writes, and decay hygiene', 'No silent user-intent promotion; stale/applied candidates are detected and correctly routed.'),
    (7, 'P2', 'Build isolated, collision-proof, machine-readable benchmark runner', 'Unique UUID, clean hashes, exact versions, no inherited context, traces, validated JSON, and zero canary leaks.'),
    (8, 'P2', 'Add equal-content controls, replication, multiple model/runtime families, and blinded ratings', 'Release claims have confidence intervals, objective validators, and independent reproduction.'),
    (9, 'P3', 'Prove minimalism and long-horizon memory value', 'At least 20% context-efficiency gain and 30% fewer rediscoveries without fidelity or defect regression.'),
    (10, 'P3', 'Put conformance and benchmark smoke in CI', 'Every payload change receives route, safety, alignment, and regression evidence before merge.')
)
SELECT *
FROM roadmap
ORDER BY "order" ASC;
-- end query

-- query: name_candidates
WITH name_candidates(
  rank,
  candidate,
  accuracy,
  organic_fit,
  distinctiveness,
  category_clarity,
  memorability,
  collision_safety,
  total_score,
  recommended_role
) AS (
  VALUES
    (1, 'Context-Routed Development', 5, 3, 5, 5, 4, 3, 25, 'Recommended formal methodology category'),
    (2, 'Routed Context Cultivation', 5, 5, 5, 3, 3, 5, 26, 'Recommended broader organic practice'),
    (3, 'Context-Grown Development', 4, 5, 5, 4, 4, 4, 26, 'Best warm formal alternative; routing becomes implicit'),
    (4, 'Relevance-Routed Development', 4, 2, 5, 5, 4, 5, 25, 'Best mechanism-plus-minimalism challenger'),
    (5, 'Human-Governed Context Development', 4, 4, 5, 4, 3, 5, 25, 'Best authority-first formal alternative'),
    (6, 'Just-Enough Context Development', 3, 2, 5, 5, 5, 5, 25, 'Best minimalism-first phrase; enough remains unproved'),
    (7, 'Human-Stewarded Context Development', 4, 5, 5, 4, 3, 5, 26, 'Best warm human-governance name'),
    (8, 'Flow-Routed Development', 3, 4, 5, 5, 4, 5, 26, 'Best natural-flow construction; context graph is implicit'),
    (9, 'Context-Cultivated Development', 4, 5, 4, 4, 3, 4, 24, 'Deliberate growth without autonomy; awkward modifier direction'),
    (10, 'Context Wayfinding Development', 4, 3, 5, 4, 5, 4, 25, 'Natural navigation metaphor; CWD acronym collision'),
    (11, 'Current-Guided Development', 3, 4, 5, 5, 4, 5, 26, 'Current-truth and natural-current wordplay; routing implicit'),
    (12, 'Warranted Context Development', 3, 4, 5, 5, 4, 5, 26, 'Excellent evidence language; selection mechanics disappear'),
    (13, 'Intent-Routed Context Development', 5, 2, 5, 4, 3, 5, 24, 'Most literal causal name; cumbersome in speech'),
    (14, 'Work-Grown Context Development', 4, 5, 5, 4, 3, 5, 26, 'Context grows from real work; unfamiliar word order'),
    (15, 'Context-on-Demand Development', 4, 2, 4, 5, 5, 4, 24, 'Clear pull model; can imply automatic retrieval'),
    (16, 'Repo-Native Context Development', 4, 3, 4, 5, 3, 5, 24, 'Honest substrate description; weak as methodology'),
    (17, 'Cognition-Scaffolded Development', 3, 3, 5, 5, 4, 5, 25, 'Best human-like-inspired alternative; routing absent'),
    (18, 'Endogenous Context Development', 3, 5, 5, 4, 3, 4, 24, 'Internally grown learning; academic and acronym-crowded'),
    (19, 'Epistemic Wayfinding', 4, 4, 5, 3, 4, 4, 24, 'Strong research umbrella; not an obvious SDD successor'),
    (20, 'Routecology', 3, 5, 5, 2, 4, 5, 24, 'Leading coined frontier field; requires comprehension testing')
)
SELECT *
FROM name_candidates
ORDER BY rank ASC;
-- end query

-- query: frontier_name_atlas
WITH frontier_name_atlas(sort_order, family, candidate, meaning, best_role, verdict) AS (
  VALUES
    (1, 'Coined field', 'Routecology', 'Routing plus ecology of selecting, growing, pruning, and governing routes', 'Public movement above CRD', 'Leading frontier wildcard; test comprehension and pronunciation'),
    (2, 'Coined field', 'Contexticulture', 'Context plus cultivation and culture', 'Organic research umbrella', 'Evocative but hard to say; obscure older academic uses'),
    (3, 'Coined field', 'Routogenesis', 'Formation and evolution of routes', 'Research program', 'Distinctive; genesis can imply autonomous generation'),
    (4, 'Coined field', 'Contextogenesis', 'Context formed through work', 'Lifecycle model', 'Clear after explanation; deliberate routing is absent'),
    (5, 'Coined brand', 'Wayroot', 'Wayfinding anchored in repository truth', 'Product or community brand', 'Short and warm; mechanism is opaque'),
    (6, 'Coined component', 'Routeweave', 'Route strands composed into working context', 'Resolver or composition engine', 'Excellent component name; tool-like as a methodology'),
    (7, 'Coined practice', 'Routecraft', 'Skilled deliberate construction of routes', 'Practitioner discipline', 'Memorable; underplays evidence and lifecycle'),
    (8, 'Coined component', 'Pathloom', 'A loom that weaves paths into context', 'Resolver or product', 'Imaginative; high education cost'),
    (9, 'Wordplay', 'Root-and-Route Development', 'Repository roots plus navigable routes', 'Campaign phrase', 'Faithful and memorable; may sound playful'),
    (10, 'Organic brand', 'Routegarden', 'Routes are planted, pruned, and cultivated', 'Ecosystem or visual model', 'Warm; too metaphorical for formal category'),
    (11, 'Coined pattern', 'Contextbraid', 'Truth, rules, memory, and workflow are braided', 'Composition pattern', 'Strong metaphor; coined compound needs teaching'),
    (12, 'Research', 'Epistemic Wayfinding', 'Navigation by authority, relevance, and epistemic state', 'Research umbrella', 'Precise but academic'),
    (13, 'Organic model', 'Context Metabolism', 'Context is absorbed, transformed, reused, and retired', 'Lifecycle model', 'Powerful; can imply autonomous homeostasis'),
    (14, 'Behavioral metaphor', 'Desire-Path Development', 'Useful routes emerge from repeated real behavior', 'Essay or metaphor', 'Suggests bypassing safeguards; phrase used elsewhere'),
    (15, 'Flow', 'Current-Guided Development', 'Current truth plus a natural current', 'Accessible organic alternate', 'Clever; mechanism under-specified'),
    (16, 'Human-inspired', 'Cognition-Scaffolded Development', 'Repository context scaffolds bounded reasoning', 'Formal alternate', 'Defensible; routing is not named'),
    (17, 'Human-inspired', 'Enactive Context Development', 'Context and knowledge arise through doing', 'Cognitive-science framing', 'Deep fit; too obscure for primary category'),
    (18, 'Coordination', 'Stigmergic Context Development', 'Durable work traces shape later work', 'Research descriptor', 'Accurate but crowded and unfamiliar'),
    (19, 'Organic metaphor', 'Mycelial Context Development', 'Distributed local connections support growth', 'Visual philosophy', 'Can falsely imply decentralization or autonomy'),
    (20, 'Protocol', 'Living Context Protocol', 'Portable context protocol that evolves over time', 'Future specification layer', 'Strong future name; living imports maintenance expectations'),
    (21, 'Occupied', 'Contexture Development', 'Context as interwoven arrangement', 'None', 'Reject: adjacent DDD and AI products use Contexture'),
    (22, 'Occupied', 'Contextweave', 'Context strands woven together', 'None', 'Reject: multiple AI/context tools and a package use it'),
    (23, 'Occupied', 'Contextome', 'The complete environment of context', 'None', 'Reject: biological/product use and conflicts with minimalism'),
    (24, 'Occupied', 'Repoesis', 'Repository plus poiesis', 'Essay title only', 'Reject as category: art use and unclear pronunciation')
)
SELECT *
FROM frontier_name_atlas
ORDER BY sort_order ASC;
-- end query

-- query: solution_options
WITH solution_options(
  decision_order,
  decision,
  option_id,
  option_name,
  relative_cost,
  recommendation,
  main_tradeoff
) AS (
  VALUES
    (1, 'Route containment', 'A', 'Lexical containment', 'Low', 'Emergency interim', 'Does not stop symlink or junction escape'),
    (1, 'Route containment', 'B', 'Canonical containment', 'Medium', 'Recommended default', 'Cross-platform and race handling require careful tests'),
    (1, 'Route containment', 'C', 'Capability-scoped external roots', 'Medium', 'Add after B', 'More policy and receipt surface'),
    (2, 'Context resolution', 'A', 'Extend find with active phase', 'Medium', 'Interim only', 'Overloads exploration and activation'),
    (2, 'Context resolution', 'B', 'Dedicated resolve and receipt', 'High', 'Recommended default', 'Adds command and schema'),
    (2, 'Context resolution', 'C', 'Persisted context bundle', 'Medium', 'Reject as truth', 'Creates stale parallel context'),
    (3, 'Mutation', 'A', 'Complete preflight plus direct writes', 'Medium', 'S-level milestone', 'Crash or I/O failure can leave partial state'),
    (3, 'Mutation', 'B', 'Virtual tree plus journaled apply', 'High', 'Recommended default', 'Recovery subsystem and fault matrix are substantial'),
    (3, 'Mutation', 'C', 'Git patch or worktree', 'Medium', 'Optional assurance mode', 'Excludes non-Git workspaces'),
    (4, 'Extensions', 'A', 'Manifest-lite', 'Medium', 'Milestone only', 'No installed ownership for update or remove'),
    (4, 'Extensions', 'B', 'Manifest, capabilities, and visible lock', 'High', 'Recommended default', 'Requires dependency and three-way merge logic'),
    (4, 'Extensions', 'C', 'Route-only hard sandbox', 'Medium', 'Default privilege tier', 'Too restrictive as the complete model'),
    (5, 'Startup', 'A', 'Keep strict baseline', 'None', 'Retain as opt-in', 'Weak minimalist claim'),
    (5, 'Startup', 'B', 'One lean baseline', 'Medium', 'Good default candidate', 'No visible opt-up without profiles'),
    (5, 'Startup', 'C', 'Visible startup profiles', 'Medium', 'Recommended after A/B test', 'Expands conformance matrix'),
    (6, 'Promotion', 'A', 'Human approval for all CurrentTruth', 'Low', 'Safe but rigid', 'Mechanical facts become ceremony'),
    (6, 'Promotion', 'B', 'Claim-class authority', 'Medium', 'Recommended default', 'Needs clear classification fixtures'),
    (6, 'Promotion', 'C', 'Git owner enforcement', 'Medium', 'Optional team layer', 'Provider-dependent'),
    (7, 'Memory decay', 'A', 'Age warnings', 'Low', 'Insufficient', 'Age is not staleness'),
    (7, 'Memory decay', 'B', 'Lifecycle fields and advisory audit', 'Medium', 'Recommended default', 'Requires explicit review practice'),
    (7, 'Memory decay', 'C', 'Automatic expiry', 'Medium', 'Reject', 'Can destroy rare valid knowledge'),
    (8, 'Evidence', 'A', 'Lean kernel proof', 'Low', 'Build first', 'Cannot support broad causal claims'),
    (8, 'Evidence', 'B', 'Balanced product proof', 'High', 'Recommended destination', 'Material run and instrumentation cost'),
    (8, 'Evidence', 'C', 'Research-grade external proof', 'Very high', 'Only for broad public claims', 'Large matrix and external coordination')
)
SELECT *
FROM solution_options
ORDER BY decision_order ASC, option_id ASC;
-- end query

-- query: startup_profiles
WITH startup_profiles(profile, mandatory_files, mandatory_words, reduction_vs_strict, intended_use, status) AS (
  VALUES
    ('lean', 9, 2153, 0.3662, 'Fresh focused work', 'Candidate; must preserve fidelity in A/B test'),
    ('continuity', 12, 2538, 0.2529, 'Resumed and handoff-heavy work', 'Candidate'),
    ('strict', 19, 3397, 0.0, 'Audits and maximum early exposure', 'Current baseline')
)
SELECT *
FROM startup_profiles
ORDER BY mandatory_words ASC;
-- end query

-- query: implementation_sequence
WITH implementation_sequence(phase_order, phase, deliverables, exit_gate) AS (
  VALUES
    (0, 'Stop the line', 'Canonical containment in every current route read/write path', 'Zero accepted escape fixtures on Windows and POSIX'),
    (1, 'Read kernel', 'PathPolicy, RouteGraph, resolve, receipts, budgets, conformance v1', 'Every fixture deterministic and manual/CLI mismatches resolved'),
    (2, 'Write kernel', 'VirtualTree, plan schema, journaled apply, rollback, crash recovery', 'All injected ordinary failures roll back and crash states recover deterministically'),
    (3, 'Local lifecycle', 'Manifest, capabilities, dependencies, lock, verify/update/remove', 'Install-update-remove round trip without silent local-content loss'),
    (4, 'Minimal human-led product', 'Lean profiles, Solo Starter, claim authority, proposal mode, memory audit', 'At least 35% startup reduction without fidelity loss and unaided first-task success'),
    (5, 'Evidence V2', 'Schemas, isolated runner, traces, controls, generated reports, CI tiers', 'No ownership/isolation/canary defect and confirmation is powered/preregistered'),
    (6, 'Independent S++ proof', 'Second resolver, external extension, runtime/model evidence, long-horizon memory', 'Every published claim passes its scoped gate')
)
SELECT *
FROM implementation_sequence
ORDER BY phase_order ASC;
-- end query
