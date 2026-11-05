# Retired idea: timed mode

This is a design note about a countdown-based mode considered for Neon Pairs and retired before implementation.

The current game displays elapsed active time but has no countdown, time-based loss, timed-mode setting, or special timed score. A visible clock did not make the proposed mode a small extension of the existing rules.

## The sketch that started it

The idea was to offer a short, urgent variation: finish before a timer runs out. A clock already existed on the page, so the first estimate treated the work as changing a number from increasing to decreasing and adding a failure message.

That estimate missed the objective. Current play completes when all pairs are matched. Elapsed time describes the attempt and stops at completion; it is not another condition that can prevent completion. A countdown introduces a second terminal event and questions about how it interacts with every other state.

The team did not reject time-pressure games in general. It recognized that this would be a different mode with its own meaning, not a formatting option for the current Time display. The [elapsed-time rules](elapsed-time-rules.md) describe the current descriptive clock.

## What happens at zero was not obvious

One sketch froze all cards immediately at expiry. Another allowed the current pair attempt to finish. A third let the player continue but marked the final result late. These are three different experiences, especially when zero occurs between the first and second reveal.

The current selection union has closed, one-card, and mismatch cases. Completion is derived from matched positions. There is no separate lost state, post-expiry input policy, or late-result flag. Adding one would require deciding whether revealed information stays visible and whether the game can later become won after an expired condition.

The final-pair boundary was particularly revealing. If the last eligible second reveal and the deadline occur at the same logical time, their ordering determines the outcome. The existing replay rule already orders time advancement before actions, but it does not define a new deadline event. Reusing that mechanism would not by itself settle the proposed product behavior.

No deadline ordering was adopted.

## Active time was a design choice, not a technical default

Current elapsed time excludes hidden-page intervals and imported pauses. That serves the existing memory task: a player does not observe a miss while the page is hidden. A countdown could also use active time, but then switching away would suspend the pressure. Using wall time would keep pressure but could expire a game while its cards were unavailable to inspect.

Both interpretations can be coherent in another product. Neither follows automatically from the fact that the browser offers timestamps. The review found that people were arguing about the intended experience while describing it as a timer implementation detail.

Snapshot import made this more visible. An unfinished snapshot currently loads paused so the player can orient before Resume. A timed mode would need to say what a transported remaining budget means and whether the user can edit it. Local snapshot validation checks consistency, not trustworthy play history. A countdown would not become authoritative simply because it was stored as an integer.

The [snapshot pause decision](adr-037-snapshot-pause.md) and [import-safety page](import-safety-policy.md) explain the existing boundaries. The current format contains elapsed time, not an unused deadline field waiting to be activated.

## Board sizes could not share an arbitrary budget

The current board range spans eight to 128 pairs. A single countdown would place radically different demands on those sizes. Giving each size its own budget would require a difficulty model and evidence about what the budget was meant to encourage.

The discussion considered using pair count to scale the budget. That would be easy arithmetic, but memory difficulty does not increase only with the number of pairs. Scrolling, face similarity, inspection strategy, and the number of previously seen positions all affect the task. A linear formula would have looked precise without establishing the intended experience.

The study notes supplied no controlled timing data from which to select budgets. They were qualitative explorations of usability, not a distribution of completion performance. The [spring](research-playtest-spring.md) and [autumn](research-playtest-autumn.md) studies therefore could not justify a calibrated countdown table.

The review left those values unset. It did not introduce a hidden difficulty catalogue or imply that the largest board had an approved target completion time.

## The existing best had a different meaning

Current local bests compare completed move counts by size. A timed mode might compare remaining time, elapsed completion time, moves within a deadline, or some combination. Each would need a clear display and storage meaning so results from different objectives were not accidentally mixed.

An especially tempting shortcut was to retain the same best map and ignore the mode. That would make a careful untimed attempt and an urgent countdown attempt comparable only by moves, even though the new feature's purpose was speed pressure. Creating another record category would be a legitimate design task, but it would expand preference validation and the result interface.

The current `Preferences` object has no mode key. `rememberBest` receives size and moves. The [best-score records page](best-score-records.md) remains the current explanation.

## Urgency affected access to information

The accessibility discussion asked how a countdown would be conveyed without overwhelming card names and turn results. Announcing every decrement would compete with play. Announcing only selected thresholds would require deciding which information a nonvisual player could inspect and when.

Visual urgency could also invite flashing or repeated motion, while the current game uses restrained optional completion feedback and honors reduced motion. None of these questions makes a timed game impossible. They show that the feature includes an information and feedback design, not merely a second interval callback.

The current app stays silent, and visible elapsed time is available as a status value rather than an audible pressure signal. The retired proposal did not change that sound behavior or create a countdown announcement script. [Feedback without audio](feedback-without-audio.md) describes the present approach.

## Why it stayed retired

The team set the idea aside because it introduced a new objective, terminal-state ordering, budget calibration, result categories, and progress-transport semantics at once. Those choices deserved a coherent mode design if the project ever pursued them.

For the current game, a player can still observe elapsed time and informally try to improve. The app does not enforce that personal goal or reinterpret the local move record around it. [Roadmap notes](roadmap-notes.md) leave future exploration open without reserving a control, deadline field, or delivery date. The implemented objective remains finding every pair under [Rules](rules.md).
