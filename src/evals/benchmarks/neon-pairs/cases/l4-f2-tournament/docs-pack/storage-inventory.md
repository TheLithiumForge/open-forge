# Storage inventory

This page lists everything the game writes to browser storage today, so a person clearing data or reviewing a change can see it at a glance.

The game writes one key to browser storage: `neon-pairs.preferences.v1`.

| Key | Holds | Written by | Dropped when |
| --- | --- | --- | --- |
| `neon-pairs.preferences.v1` | The saved board size and best moves by size | `src/game/session/` | Never, apart from the person clearing site data |

[Persistence overview](persistence-overview.md) explains what is saved and what stays in memory. [Best score records](best-score-records.md) covers the preference's shape.
