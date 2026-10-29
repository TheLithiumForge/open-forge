---
open-forge:
  description: How Open Forge READMEs, the documentation site and public introductions are structured and worded, from the maintainer's voice guide
  responsibility: Define the structure, flow and wording of public Open Forge documentation, with accuracy and terminology supplied by the Writing Standard
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Writing, Documentation, Voice, Internal]
---

# Open Forge Project Voice

Project Voice is the maintainer's voice guide for public Open Forge documentation. It says how to order a doc, how to disclose each concept and which words to use.

Use it for READMEs, the documentation site's pages, package READMEs and other public introductions. Read it before you write or rewrite one, and check your output against it. Each rule is pass or fail. The [Writing Standard](writing.md) supplies the shared accuracy, terminology and lossless-rewrite requirements, and the voice of Framework rules, contracts and technical reference text. The [Open Forge Dictionary](helpers/dictionary.md) supplies the terms.

## Core rules

1. Every doc opens with an overview. The first sentence after the title says what the thing is and what it does.
2. Write in logical progression. Give the main concepts first, then the supporting concepts in the order the main ones need them.
3. Disclose each concept when the reader needs it. A supporting concept can be named with a link before it is explained, as long as the sentence still reads.
4. Show how you get from A to B. Each step says what it gives the next one.
5. Cover each topic fully in one place. Everywhere else, give a one-line summary and a link to it.
6. Put a requirement or condition before the step it affects.
7. Give an example where the reader needs one to act or to tell two things apart, and show its result.
8. Default to short notation for code. Label code that doesn't exist as Proposed or Illustrative.
9. Introduce every list with a heading or with a line that ends in a colon.
10. Use plain words and replace everything from the left column of the [Words](#words) table. Call each thing by its code name, and keep that name to the end.
11. Write one idea per sentence. Keep the punctuation plain, as [Punctuation](#punctuation) describes.
12. Say what the thing does. Write "not" only for something the reader would expect to happen.
13. Talk to the reader as "you". Use "we" for team decisions in internal docs.
14. State each status once, in the words the source uses. Put undecided items in one Not decided yet list at the end.
15. In a rewrite, keep every fact with its scope, actor, timing, condition and number. Add none.
16. Keep headings to 1-4 words that name the thing or the task.

## Structure

### Overview

Every doc opens in this order:

1. Line 1 is `# Name`.
2. Line 2 is one sentence: `<Name> is a <kind of thing> that <does what>.`
3. Then a short paragraph on the problem it solves, the goal behind it or the job it does, with one concrete case.
4. Then badges in a README, or the status line in a design doc.
5. Then the first main concept.

No front matter, Owner line, Purpose line or bold label line goes before the first sentence. If a tool needs front matter, such as the documentation site, add only the fields the tool reads.

### Logical progression

Before you write, sort the concepts into two groups:

- Main concepts - the highlights the reader came for.
- Supporting concepts - the parts that make the main ones work.

Then write in this order:

1. The overview.
2. The main concepts, each with its example.
3. The supporting concepts, in the order the main ones need them.
4. Details, options, limits and Not decided yet.

Decide case by case how to disclose each concept:

- Explain now - the next sentence depends on it, or the reader can't guess what it is.
- Name and link - the reader can follow it by its name for now. Link to where it's explained.
- Leave out - this doc doesn't need it. It lives in its own doc.

A good doc passes this test:

- A reader who stops after the overview knows what the thing is and what it does.
- A reader who stops after the main concepts can use it or explain it.
- A reader who goes on never meets a term they can't place.

A glossary, a list of links or a list of parts doesn't replace the main concepts. Open with the thing itself.

### Order by doc type

The progression stays the same in every doc. What counts as a main concept depends on the doc type:

| Doc type               | Main concepts after the overview                                | Then                                                                |
| ---------------------- | --------------------------------------------------------------- | ------------------------------------------------------------------- |
| README                 | Requirements, Install, the smallest working use                 | How to use, options, Limits, developer sections, Roadmap, Changelog |
| Design or internal doc | The main idea or conclusion, then the key flow with one example | Parts in dependency order, options with their cost, Not decided yet |
| Detail doc             | The one topic it covers, and how it fits its parent             | The full rules of that topic, then a link back to the parent        |

In every doc type, these rules also hold:

- Types, enums and terms go before the functions that use them.
- Parts go before the things built from them.
- The normal path goes before alternatives, failures and exceptions.
- If an overview lists N parts, N sections follow, in the same order and with the same names.

### Headings

- Name the thing (`readline()`, `Parameters`) or the task (`Install`, `How to use`, `Send message`).
- Reuse the common set when it fits: Install, Intro, Requirements, Setup, How to use, Examples, Parameters, Limits, Build, Contribution, Roadmap, Changelog.
- No numbers, no colon at the end and no heading written as a statement.

### Splitting and linking

- Move a topic to its own doc when it has its own sub-sections or tables. Keep a small topic in place.
- In the parent doc, write 1-3 sentences on the topic, then the link. The sentence says what the reader finds there.
- Say why the topic matters here before you link it. The text must make sense without opening the link.
- Put each link once, in the sentence where its topic comes up. A closing list is only for outside resources the text doesn't cite.
- A link is never the subject of a sentence. Write "See `[Delivery](delivery.md)`." or "`[Delivery](delivery.md)` covers returns and deadlines."
- Each detail doc links back to its parent once, at the top or at the bottom.
- Before you write a fact, search the docs. If another section or doc has it, link to it.
- Add a Contents list only to a doc with more than 8 sections. First check whether the doc should be split.

### Lists and tables

- Introduce every list with a heading or a line that ends in a colon.
- Use a numbered list only when order matters: steps that depend on each other, priorities, precedence.
- Give each step its own item, starting with a verb or the command.
- Write named items as `name` - meaning.
- Keep feature bullets short.
- Use a table only when every row has the same 2 or more fields. Cells hold values or short phrases.
- A limit that changes one step goes beside that step. Other limits go once, under Limits.

### Code examples

Pick the example that fits the situation:

- Short notation - one call that stands for the whole step, such as `connect()`. This is the default.
- Real code - code that exists in the codebase. Prefer it when it exists, and copy it as it is.
- Full example - a complete snippet the reader follows line by line. Use it only when the order of the calls or the shape of the data matters.

Label every example that isn't real code, on the line above the block:

- "Proposed:" - an API we plan to build.
- "Illustrative:" - code that shows the idea and isn't meant to be built as written.

A label covers made-up names, not made-up behavior. The example must still do exactly what the text says. Comment what each line produces.

## Flow

- Default path first. Take the basic case all the way to a result the reader can see, then cover options and other cases.
- Show the chain. Name what each step produces and which step uses it.
- Name the link between two steps with "then", "based on", "because", "since" or "so".
- Put the condition or the purpose first.
- List Prerequisites before the work that needs them.
- Give the reason before the conclusion.
- Give the problem in one sentence, then the fix.
- Give one sentence of reason for any step that looks odd.
- Keep similar states apart. Say which one the next step needs.
- For a decision, give the chosen option and its reason, then each other option and its cost, one line each.
- Say what isn't known and how to find out. No "maybe", "probably" or "possibly".
- A step that depends on an undecided item says so where it happens. Then add the item to Not decided yet.
- Before a results table, say in 1-2 lines what was measured and on what.

### Status and facts

- Keep three states apart: not specified, not decided, not built. Write only the one the source states.
- If the source leaves an order, a limit or a scope open, keep it open.
- "At most one" stays "at most one". "The same owner" stays "the same owner". Exact words for scope and count are facts.
- Before a rewrite, map each source fact to the one place it will live. After the rewrite, check that each fact is there.

## Language

### Person and tone

- "You" is the reader. "The user" is only the app's end user.
- "We" is the team, in internal docs.
- Use the present tense for what a thing does and "will" for what a step produces.
- Use "can" for options, "need to" for requirements and "might" when you're unsure.
- Use contractions: don't, doesn't, can't.
- Describe a feature by what it does. Every quality word needs the fact behind it.
- To explain an unfamiliar tool, name a known one once.
- Give a warning as one short sentence where it applies.
- Write for a capable reader. Explain what is new to them, and skip what any developer already knows.

### Sentence shape

- One idea per sentence, and one point per paragraph.
- Use a verb instead of a -tion or -ment noun.
- Introduce an example with "E.g." or "For example", then the literal value.
- Say what the reader needs instead of listing negatives to sound firm.
- Write the content instead of announcing it.
- Use a heading or a list instead of a bold lead-in sentence or a bold label line.

### Punctuation

Keep the punctuation plain:

- Periods end sentences. If a sentence needs a semicolon, make it two sentences.
- No em dashes or en dashes. Use a period or a comma instead.
- " - " with spaces joins a name and its meaning, once per line.
- A colon introduces a list, an example or a definition. It never joins two sentences.
- Commas separate items in a list, with no comma before the final "and".
- A slash joins only parallel names, such as "TS/JS". No slash chains.
- `->` is only for click paths and for input -> output.
- Parentheses hold a short aside. Keep the sentence correct without them.
- Straight double quotes mark quoted text. Backticks mark names, commands, paths and values.
- No exclamation marks, no ellipsis to trail off and no "&" in prose.

### Naming

- A brand name gets a plain descriptor on the same line.
- Call the thing what it is.
- A term from the code gets a one-line definition at first use, then keeps the same word.
- A term that isn't in the code gets replaced with a plain word.

### Words

Replace the left column with the right one:

| Instead of                                                    | Write                                  |
| ------------------------------------------------------------- | -------------------------------------- |
| "The user should run `x`."                                    | "Run `x` to `<result>`."               |
| "In case of `<case>`, `<action>` is required."                | "If you `<case>`, `<action>`."         |
| "It is possible to `<action>`."                               | "You can `<action>`."                  |
| "The default value is `x`."                                   | "Defaults to `x`."                     |
| "It remains to be tested whether..."                          | "We need to test `<what>`." (internal) |
| admission, admit                                              | accept                                 |
| aims to provide                                               | provides                               |
| authority, authoritative                                      | source of truth, allowed               |
| basically, kind of, technically (as filler)                   | drop it                                |
| boundary                                                      | limit, or "between X and Y"            |
| canonical, normalized                                         | name the format, or drop it            |
| cardinality                                                   | how many                               |
| certify                                                       | confirm, show                          |
| conflate                                                      | mix up                                 |
| consumer, the developer, application authors (for the reader) | you, your app                          |
| deliberate, deliberately                                      | on purpose, or drop it                 |
| disposition                                                   | what happens to it                     |
| durable                                                       | what survives: "kept after restart"    |
| establish                                                     | set up, create, start                  |
| explicit, explicitly (as filler)                              | drop it, or write the call or value    |
| handoff, crossing                                             | pass to, call from X to Y              |
| inspect, inspectable                                          | check, read                            |
| intuitive, clean, seamless, user-friendly                     | what it does                           |
| may                                                           | can, or might when unsure              |
| meaningful, intended, visible                                 | drop it, or say the effect             |
| merely, likewise, thereby                                     | drop it                                |
| observe, observation, observable                              | wait for, see, result                  |
| ordinary                                                      | normal, default or drop it             |
| own, owns, owner, ownership (docs or modules)                 | covers, handles, or name what does it  |
| projection                                                    | view                                   |
| promptly                                                      | right away                             |
| prove, proof, claim, guarantee (about a doc or feature)       | tested, shows, or cut the sentence     |
| provenance                                                    | source                                 |
| qualify, qualification                                        | test on, tested on                     |
| rather than                                                   | instead of                             |
| realization, realize                                          | implementation, build                  |
| remain, remains (meaning "is still")                          | is, stays, still                       |
| remains unselected, deferred, not selected                    | not decided yet                        |
| retain, preserve                                              | keep                                   |
| selected, accepted (meaning decided)                          | chosen, decided                        |
| semantics                                                     | behavior, meaning                      |
| settle                                                        | finish, complete                       |
| supply, supplies                                              | give, provide                          |
| surface (about an API)                                        | API, functions                         |
| truthful, dependable, coherent                                | correct, reliable, consistent          |

A code name stays as it is, even when it matches a word in the left column. E.g. Open Forge's defined terms, such as Crystallized, Directive or ownership record, keep their names.

## Doc types

The doc types differ in reader, person, tone, examples, status and ending:

| Doc type               | Reader                                          | Person                              | Tone                                              | Examples                                  | Status                                                  | End                                   |
| ---------------------- | ----------------------------------------------- | ----------------------------------- | ------------------------------------------------- | ----------------------------------------- | ------------------------------------------------------- | ------------------------------------- |
| README (public)        | Someone new who wants to use it                 | "you"                               | Full sentences, written for a capable reader      | Real calls and commands the reader copies | None. Plans go in Roadmap                               | Roadmap, Changelog                    |
| Design or internal doc | The team, and agents working on the code        | "we" for decisions, "you" for steps | Compact: one fact per line, lists over paragraphs | Flows, payloads, input -> output          | One line in the overview, and one per part that differs | Not decided yet                       |
| Detail doc             | Someone who followed a link and needs one topic | Same as its parent                  | Compact                                           | One per distinction that matters          | Only if it differs from the parent                      | Not decided yet, then the parent link |

Mix the tones when a doc needs both. E.g. a README's Limits section can be compact, and a design doc's overview can use full sentences. The documentation site's landing page can lead with the value of each feature, as long as every claim names the fact behind it.

## Final check

Run this on your output before you hand it over, and fix every item that fails:

- [ ] The overview says what the thing is and what it does, and why it matters. Nothing sits before it.
- [ ] Main concepts come before supporting concepts. A glossary or link list doesn't open the doc.
- [ ] Every concept is explained before it's needed, or named with a link where the sentence still reads.
- [ ] The default path reaches a result the reader can see before any option or branch.
- [ ] Each step says what it gives the next one. Every branch starts with its condition.
- [ ] Each topic has one full description. Other mentions are one line plus a link that says what it covers.
- [ ] Examples appear where the reader needs them, and each one shows its result.
- [ ] Code that doesn't exist is labelled Proposed or Illustrative, and it does only what the text says.
- [ ] Every list follows a heading or a line that ends in a colon.
- [ ] No word from the left column of the Words table appears in prose. Each thing keeps one name.
- [ ] No semicolons, em dashes, en dashes, exclamation marks or slash chains in prose.
- [ ] No negative lists, and no sentence announces a section or what comes next.
- [ ] READMEs use "you". Internal docs use "we" for team decisions.
- [ ] Each status appears once, in the source's words. Undecided items are in one Not decided yet list at the end.
- [ ] In a rewrite, every fact is still there with its scope, actor, timing, condition and number, and nothing new is added.
- [ ] Headings have 1-4 words, no numbers and no trailing colon.

The [User-Facing Writing Decision](../../decisions/framework/user-facing-writing.md) records why public documentation uses this voice. The [README](../../../../../README.md) applies it to a complete introduction.
