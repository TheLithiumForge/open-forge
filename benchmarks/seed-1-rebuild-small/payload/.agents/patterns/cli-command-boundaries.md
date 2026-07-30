---
open-forge:
  description: Keep CLI commands small, explicit, and easy to inspect
  tags: [Extension, Pattern, CLI, Commands]
---

# CLI Command Boundaries

## Shape

Each command should have a clear boundary:

- parse only the arguments it owns,
- call core functions for behavior,
- call storage functions for persistence,
- return or print one clear result,
- convert expected user mistakes into friendly messages.

## Command Rules

- Unknown commands show usage and fail.
- Missing required values show usage and fail.
- Empty result commands can succeed when the user asked a valid question.
- Command names and options should stay boring and predictable.

## Avoid

- Hidden global state.
- Command handlers that contain storage parsing details.
- Storage modules that know about CLI syntax.
