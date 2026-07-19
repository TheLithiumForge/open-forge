---
open-forge:
  description: Exercise public CLI commands in fresh OS temporary workspaces and assert process, filesystem, Git, and no-partial-write contracts
  tags: [Extension, Pattern, Testing, CLI, Integration, Git]
---

# OS-Temporary Black-Box CLI Command Tests

Use this pattern when correctness depends on argument parsing, process exit behavior, filesystem effects, Git state, environment isolation, packaging, or command composition.

## Shape

1. Create one fresh workspace per test with the runtime equivalent of `mkdtemp(join(tmpdir(), "tool-test-"))`.
2. Invoke the public CLI entrypoint as a real child process with an argument array, explicit target, controlled environment, and concurrently drained stdout and stderr.
3. Use real filesystem and Git operations for command-contract behavior. Do not mock the CLI dispatcher, filesystem, repository state, or executable lookup that the assertion depends on.
4. Assert exit code, stdout, stderr, installed or unchanged bytes, generated artifacts, and Git status as applicable.
5. For every rejected mutation, prove the target stayed unchanged and no partial output appeared.
6. Cover success, invalid input, containment, dependency, checkpoint, ignored-file, and rollback boundaries proportionately.
7. Exercise the packaged Node executable when module layout, bundled assets, shebangs, or runtime resolution matter; keep detailed behavior tests on the source CLI.
8. Remove every temporary root in `afterEach` or `finally`, including after failed assertions.

## Git Checkpoint Fixture

For guarded install commands, initialize a real temporary repository, configure local test identity, commit a baseline, run the public command, inspect `git status`, and commit only when the scenario needs the next clean checkpoint. Use `--pro` only in cases explicitly testing expert bypass or legacy untracked installation.

## Boundary

Direct helper tests may supplement this pattern for algorithms and state transitions, but they never replace command-level coverage. Use an OS temporary root unless an applicable directive explicitly requires a different isolated location.
