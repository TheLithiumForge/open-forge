---
open-forge:
  description: Keep pure tests fast by default and reserve real process, filesystem, Git, packaging, and rollback coverage for explicit closure runs
  tags: [Extension, Pattern, Testing, CLI, Unit, Closure, CI, Git]
---

# Tiered CLI Tests

Use this pattern when a CLI needs fast development feedback without giving up confidence at real process, filesystem, Git, or packaging boundaries.

## Shape

1. Name pure, in-memory cases `*.unit.test.*` and run them as the default development tier.
2. Name tests that use OS temporary directories, subprocesses, Git, builds, packaging, links, or benchmark lifecycles `*.closure.test.*`; run them explicitly in CI and relevant closeout.
3. Keep both tiers discoverable by the runtime's normal test convention so an IDE can run a file or case directly. Use a repository-root runner with absolute paths when terminal cwd must not affect discovery.
4. Give every suite one shared utility boundary for repository paths, subprocess capture, Git fixtures, temporary workspaces, tree snapshots, and cleanup.
5. Prefer one suite-scoped temporary root with isolated case directories and one closeout cleanup over a new OS root and recursive deletion for every case.
6. Invoke the public CLI as a real child process only when the process boundary is part of the claim. Pass an argument array, explicit cwd and target, controlled environment, and drain stdout and stderr concurrently.
7. Assert behavior and invariants: parsed output, exact important bytes, ownership and hashes, generated-route validity, Git state, or rollback. Do not retain a success test whose only claim is that a file exists.
8. For every rejected mutation, prove the target stayed unchanged and no partial output appeared; absence is valuable when it establishes containment or atomicity.
9. Cover success, invalid input, containment, dependency, checkpoint, ignored-file, and rollback boundaries proportionately without repeating the same full composition for every catalogue item.
10. Exercise the packaged executable when module layout, bundled assets, shebangs, or runtime resolution matter; build it once per suite and reuse it across isolated cases.

Document whether the runtime's raw test command discovers both tiers. Make the intended fast and full commands explicit instead of relying on an ignore configuration that also hides intentional IDE runs.

## Git Checkpoint Fixture

For guarded install commands, initialize a real temporary repository, configure local test identity, commit a baseline, run the public command, inspect `git status`, and commit only when the scenario needs the next clean checkpoint. Use `--pro` only in cases explicitly testing expert bypass or legacy untracked installation.

## Boundary

Pure helper tests own algorithms and state transitions. Closure tests own behavior that depends on argument parsing, executable lookup, process output, filesystem effects, Git scope, containment, packaging, or rollback. Neither tier substitutes for the claims owned by the other.
