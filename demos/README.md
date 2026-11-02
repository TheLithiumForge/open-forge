# Demos

Small, realistic projects for trying Open Forge with your own agent. Each one comes with requests at several levels of detail and a list of checks, so you can see what changes when the workspace carries its own rules and reasons.

These expense splitter demos came before Open Forge Evals and were never evaluated, so they have no eval results. The main demos now use Neon Pairs, the memory game the evals build and grade. The [Demos pages](https://thelithiumforge.github.io/open-forge/docs/demos) describe them, and their inputs live in [`src/evals/benchmarks/neon-pairs/`](../src/evals/benchmarks/neon-pairs/).

| Demo                                                         | Start from                  | Try it to see                                                                                                |
| ------------------------------------------------------------ | --------------------------- | ------------------------------------------------------------------------------------------------------------ |
| [Expense splitter, greenfield](expense-splitter/greenfield/) | An empty folder and an idea | How much an agent gets right from a one-line idea versus a full set of records                               |
| [Expense splitter, brownfield](expense-splitter/brownfield/) | A half-built app with tests | Whether an agent keeps rules whose reasons live only in the author's notes, and records the choices it makes |

## Seed levels

Every demo's request comes at four levels. The levels change only how much you tell the agent up front.

| Level | What the agent gets                                                                    |
| ----- | -------------------------------------------------------------------------------------- |
| 1     | One sentence                                                                           |
| 2     | A short brief: who it's for, what it must do, constraints                              |
| 3     | The brief plus a table of exact expected results                                       |
| 4     | Open Forge records: Decisions, scenarios, and for greenfield a Vision and Architecture |

The level 4 records come from Extensions, not from the base install: Decisions from Planning, and Documents such as the Vision, Architecture, and scenarios from Project Documents. The Development Toolkit installs both.

## Before you start

- **Work outside this repository.** Run your agent in a separate folder, so it sees the demo and not Open Forge's own workspace.
- **Commit a starting point** before the agent changes anything, so `git diff` shows exactly what happened.
- **Run each demo twice** if you can: once with Open Forge installed and once without. Comparing the two runs shows what Open Forge changes.

## Demos as evaluations

The `checks.md` in each demo is a manual scoring sheet. The repeatable evaluation, Open Forge Evals, uses Neon Pairs instead.
