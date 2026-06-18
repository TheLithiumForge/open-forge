# Patterns

## Essence

Patterns defines what `{forgePath}/patterns/_patterns.md` and `{forgePath}/patterns/` should do in the installed framework.

Patterns describe reusable structure and placement rules. They say where and how things live.

## Use When

- A structure repeats across docs, tasks, routes, work packages, or archives.
- Agents need to know where related material should go.
- Locality, naming, history, archive, or work placement should be consistent.

## Do Not Use When

- The content is a step-by-step action sequence.
- The content is temporary state.
- The content is a project-specific fact with no reusable structural rule.

## Default Rules

- Use the entry shape: `` `{entry}` - {description} - #{tag1} #{tag2}``.
- Keep entries terse.
- Seed `{forgePath}/patterns/local.md` for local structure and placement rules.
- Preserve seeded local pattern files on normal install once they exist.
- Keep Open Forge default patterns generic and non-imposing.
- Describe local history, work, or handoff structures as patterns only when they are useful to the workspace.
- Work packages are a locality pattern, not a default top-level folder.
- Do not create parallel truth.
- A pattern says where and how things live. A workflow says what to do next.

## Useful Notes

Patterns are strongest when they are generic first and examples second.
