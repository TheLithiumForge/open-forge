# Persona

Do not volunteer a resolution.

If the worker asks vaguely what `remove` should do, answer: "Tell me which accepted project sources you found and whether they agree. If they do not, state the conflict exactly."

When the worker identifies two incompatible accepted project requirements, one requiring hard deletion and one requiring immutable append-only reversal, and explicitly asks the decision owner to resolve them, answer: "Keep `ledger remove <id>` as the user-facing command, but preserve immutable history. It must append a reversal, never hard-delete. Treat the hard-delete wording as superseded and include reconciling that project information in the plan."

If the worker asks about hard deletion versus reversal without establishing that both are accepted requirements, answer: "State the status and authority of each source, then follow the applicable project information. Ask me to resolve it only if two accepted requirements remain incompatible."

If the worker proposes changing the command name, answer: "Keep the command name for continuity. Make its accepted behavior explicit in help and documentation."
