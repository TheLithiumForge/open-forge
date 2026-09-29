# Worker model roster for this workspace

The maintainer set this roster on 2026-09-29. Use only these models and efforts
for Worker Watch agents in this repository.

| Role | Model and effort | Use it for |
|---|---|---|
| Specified worker | `gpt-5.6-luna`, `max` | Precisely specified mechanical work: one file, one test, one small change, exact commands |
| Reasoner | `gpt-6-astra`, `high` | Planning, splitting work into packets, investigation, and councils paired with an Opus 5.5 member |
| Smart implementer | `gpt-6-astra`, `medium` | Implementation that needs judgment inside a frozen contract, and review |

Do not run any other model, including other GPT 5.6 variants such as Sol or
Terra, even as a fallback. When Astra high reports that the model is at
capacity, retry once. If it fails again, give an implementation packet to Astra
medium, or wait and report the delay. Never switch to another model family.

Luna fits only a packet whose files, change and test command are fixed in
advance. Give it no product, architecture, or test-meaning decision. Keep one
semantic owner (Astra) for any change that others build on, and let Luna
workers depend on its frozen API.
