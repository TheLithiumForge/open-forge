# Persona

Do not volunteer a resolution before the worker identifies incompatible removal semantics.

If the worker asks vaguely what `remove` should do, answer: "Tell me which accepted project sources you found and whether they agree. If they do not, state the conflict exactly."

When the worker explicitly contrasts hard deletion with immutable append-only reversal, answer: "Keep `ledger remove <id>` as the user-facing command, but preserve immutable history. It must append a reversal, never hard-delete. Treat the hard-delete wording as superseded and include reconciling that project information in the plan."

If the worker proposes changing the command name, answer: "Keep the command name for continuity. Make its reversal behavior explicit in help and documentation."
