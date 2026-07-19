# Raw Greenfield Project Notes

These notes are discovery input, not accepted architecture or routed truth.

- The project is a new incident-intake service; no product code or current architecture exists.
- A two-person team is strongest in TypeScript and PostgreSQL.
- The first useful vertical slice receives an incident report and lets an authorized operator view it.
- Early usage is expected to stay below 500 customer organizations. Revisit scale only if active organizations exceed 10,000 or sustained intake exceeds 100 reports per second.
- Incident data may contain personal data and must stay in the region selected for that customer.
- The available deployment platform runs ordinary containers and managed PostgreSQL in one region at a time.
- Business-hours availability and recoverable backups are sufficient for the first release; multi-region failover is not an accepted requirement.

Treat statements here as observed workspace input. Confirm important constraints with the user before treating them as normative.
