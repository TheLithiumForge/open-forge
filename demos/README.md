# Demos

Small, realistic projects for trying Open Forge with your own agent. Each one comes with requests at several levels of detail and a list of checks, so you can see what changes when the workspace carries its own rules and reasons.

| Demo                                                         | Start from                  | Try it to see                                                                                       |
| ------------------------------------------------------------ | --------------------------- | --------------------------------------------------------------------------------------------------- |
| [Expense splitter, greenfield](expense-splitter/greenfield/) | An empty folder and an idea | How much an agent gets right from a one-line idea versus a full set of records                      |
| [Expense splitter, brownfield](expense-splitter/brownfield/) | A half-built app with tests | Whether an agent keeps rules that live only in the author's notes, and records the choices it makes |

## Seed levels

Every demo's request comes at four levels. The levels change only how much you tell the agent up front.

| Level | What the agent gets                                                                    |
| ----- | -------------------------------------------------------------------------------------- |
| 1     | One sentence                                                                           |
| 2     | A short brief: who it's for, what it must do, constraints                              |
| 3     | The brief plus a table of exact expected results                                       |
| 4     | Open Forge records: Decisions, scenarios, and for greenfield a Vision and Architecture |

## Before you start

- **Copy the demo out of this repository.** Run your agent in a separate folder, so it sees the demo and not Open Forge's own workspace.
- **Commit a starting point** before the agent changes anything, so `git diff` shows exactly what happened.
- **Run each demo twice** if you can: once with Open Forge installed and once without. The differences are the point.

## Demos as evals

The same inputs can compare agents, models, and tools. A planned task turns these checks into a repeatable evaluation. Until then, the `checks.md` in each demo is a manual scoring sheet.
