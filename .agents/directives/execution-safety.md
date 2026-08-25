---
open-forge:
  description: Keep workspace work local and reversible by default, and require exact authorization for external or destructive effects
  tags: [LoadNow, Core, Directive, Safety, Permission, ExternalEffect, Git, Destructive]
---

# Execution Safety

## Instructions

- Treat ordinary analysis, planning, implementation, review, testing, and documentation requests as local workspace authority only. They do not authorize contacting remotes, creating pull requests, publishing, deploying, changing external systems, sending data, or modifying credentials.
- Require explicit authorization for the exact external effect and target before a shell action contacts a remote system, sends workspace data, installs or downloads dependencies, mutates remote Git state, publishes, deploys, releases, changes an issue or pull request, uses credentials, or writes to any external system. Read-only public research through an authorized research capability does not imply shell or mutation authority.
- Do not infer external authorization from a branch name, release plan, workflow recipe, repository convention, prior unrelated authorization, or the fact that local work is complete.
- Treat environment files, private keys, credential stores, tokens, and secret-bearing configuration as a separate sensitive-read and sensitive-write boundary. Inspect or modify them only when the exact task requires it and authority is explicit; never copy their contents into prompts, logs, findings, or reusable memory.
- Treat destructive local recovery as a separate authority boundary. Before deleting broad paths, resetting state, cleaning untracked files, rewriting history, or overwriting unrelated work, identify the exact target, inspect current state, preserve unrelated changes, and require explicit authorization when the effect is not routine and safely reversible.
- Never route around a denied capability by using another command, tool, script, agent, or encoded payload. A denied operation is a stop condition until the required authority or capability is provided.
- Default shell permissions to `ask` or `deny` unless the user explicitly chooses a trusted, low-friction local-development role. A trusted primary or implementation role may default ordinary shell execution to `allow` while keeping recognizable deletion, content destruction, history erasure, remote mutation, publication, deployment, credential, and privileged-system commands at `ask` or `deny`.
- Treat an allow-by-default shell as a declared trust boundary, not containment. Shells, interpreters, scripts, aliases, Git plumbing, and platform-specific commands can route around command-pattern guards, so the approval rules reduce accidents but cannot make arbitrary command execution safe.
- When a target cannot represent these per-agent permission rules, treat the boundary as unenforced until the host sandbox and approval policy provide equivalent default-ask or default-deny behavior. Do not rely on generated prose alone or claim tool-layer enforcement on that target.
- Open a failure circuit after the same normalized tool, model, or infrastructure failure occurs twice without new evidence. Record the failure once, change the hypothesis or input, use one declared fallback, or return a blocker. Do not spawn replicas or repeat an unchanged call until it happens to work.
- Delegated work inherits these boundaries. The primary owner must not give a helper broader external or destructive authority than the user granted.
- Prefer offline or no-fetch verification when dependency state is already prepared. When a required command may contact a remote system implicitly, surface that effect instead of assuming the command name makes it local.
- Prefer read-only inspection, exact-path mutation, backups or patches, isolated worktrees, and reversible local commits when they reduce recovery risk.
