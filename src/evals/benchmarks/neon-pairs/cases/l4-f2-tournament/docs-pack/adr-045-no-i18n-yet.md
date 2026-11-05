# ADR 045: Use English for the current interface

This record explains the English-only scope and the existing separation between labels, formats and game data.

## Decision and context

The current interface is English. The entry document declares `lang="en"`, control and status labels are English, and the specified turn and load messages are English. There is no locale picker, translation catalogue or localization framework in the application.

This was a scope choice. The small product could give its current labels, accessible names and acceptance journey one coherent review instead of shipping partially translated controls. It was not a claim that memory games are culturally neutral or that translating visible strings alone would make the whole experience localized.

## Where text lives now

Deal and reproduction labels are named constants in `controls/control-labels.ts`. Status labels live in `status/status-labels.ts`. Session notices and turn-result text live with session behavior. Face names come from ordered design data, and card position/state labels are constructed in `card-state.ts`.

These locations make the current vocabulary findable without pretending that it is already a translation system. A string constructed from pair count still needs correct grammar; the first matched pair and later pairs use different forms. A face label has a specified order of palette, fill, shape and marker language. [Voice guide](voice-guide.md) and [Glossary](glossary.md) explain the current terms.

## Formats are a different boundary

Board codes and JSON field names are versioned data syntax. Their meaning does not change because interface text is English. The parser expects the stated code form and exact JSON keys. Localizing a button label would be a presentation change; translating serialized field names would change a format contract.

Likewise, the deck stores numeric face IDs rather than English names. `faceDesign` supplies the name for presentation. That distinction keeps the pure pair comparison independent of the wording a player hears or reads. [Pair identity](pair-identity.md) and [Format versioning guide](format-versioning-guide.md) cover these boundaries.

## Alternatives considered

A translation framework from the start would have provided extraction and lookup machinery but no reviewed translations by itself. Embedding placeholder catalogues could imply broader support than the product had evidence for. Relying on automatic browser translation would leave accessible names and exact instructional language outside the team's reviewed behavior.

The accepted choice was explicit English scope with ordinary source organization. This avoids committing to an unimplemented locale-detection algorithm, fallback language sequence or translation file layout. Those would be new product and architecture decisions, not consequences that can be inferred from the existing label constants.

## Costs and evidence

Players who need another language do not receive a supported localized interface in this version. That is a real limitation. The current automated accessibility journey checks English names and messages, so it cannot be cited as evidence for translated speech or layout.

Text expansion, directionality, plural rules and number conventions would all deserve attention in a localization effort. A translation is broader than replacing a handful of labels.

The title, help text, controls and completion summary remain in one page, and current wrapping is tested at constrained viewports. [ADR 049](adr-049-zoom-floor.md), [Typography](typography.md) and [Screen reader notes](screen-reader-notes.md) describe that English presentation evidence. The present decision is to document what is supported accurately rather than infer a multilingual feature from an otherwise language-independent matching rule.

## The declared page language

The document language is part of the current accessibility context. The entry has no locale-loading script or alternate language route.

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="theme-color" content="#090B14" />
    <title>Neon Pairs</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.tsx"></script>
  </body>
</html>
```
