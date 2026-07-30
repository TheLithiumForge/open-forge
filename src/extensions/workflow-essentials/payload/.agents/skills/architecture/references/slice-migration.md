# Slice Migration

## Capability

- Split the change into small slices with clear behavior, ownership, and verification.
- Preserve compatibility boundaries when the system cannot change atomically.
- Identify prerequisite decisions, temporary bridges, cleanup steps, and rollback points.
- Route implementation-ready slices to the implementation workflow when useful.

## Expected Result

The architecture can be implemented without relying on one large risky change.
