# Private browsing notes

These notes explain how private browser sessions change the lifetime of saved preferences without changing the game's in-memory rules.

Private browsing and blocked storage are different conditions. A private context may allow the preference write and a same-session reload to work, then remove that data when the private session ends. A blocked context may reject the storage getter or a method immediately. Neon Pairs does not detect private browsing or branch on a browser mode name; it responds to the storage operations available to the mounted page.

## Browser behavior, with appropriate limits

Chrome Help's “Browse in Incognito mode” describes site data as temporary during an Incognito session and removed when that session ends. Firefox Help's “How Private Windows protect you” similarly describes deleting session site data after all Private Windows close. These are lifetime descriptions, not guarantees that every site can always write under every privacy setting.

WebKit's “Tracking Prevention in WebKit” describes Safari private browsing as an ephemeral session and documents isolation between private tabs. Consequently, a test that assumes every browser shares one private storage area among all its private tabs can misread a browser's intended behavior as lost application data. Browser version, settings and the exact closing action belong in any report of private-session behavior.

Those browser descriptions explain why a preference can survive reload but fail to survive a later private visit. They do not imply that game progress was automatically saved. Even with fully working storage, Neon Pairs starts a fresh deal after every reload. Only selected size and eligible bests are carried by the preference aggregate.

## A visit with usable temporary storage

Suppose a fresh private context opens the game at a stable HTTP origin. With no accepted preference value, the game starts at 4x4. The player chooses 6x6. `replaceGame` changes the in-memory selected size, and the React save effect attempts to write the aggregate. If the browser accepts it, reloading within the same storage context can start a fresh 6x6 deal.

If the player finishes a game, the current best is updated in memory and may also be written. It can appear again after a same-context reload. When the browser later removes that private context's site data, a new visit has no such value to read. A fresh 4x4 start with no bests is consistent with private-session cleanup.

There is no server copy, browser-account synchronization or preference export path in the game that restores the earlier aggregate. A board code reproduces a deal, not the private context's remembered settings. A snapshot restores one game position, not the browser's collection of local bests. [Sharing FAQ](sharing-faq.md) and [Persistence overview](persistence-overview.md) explain these boundaries.

## A visit with denied operations

When `window.localStorage` throws during acquisition, `browserStore` returns null. `useGame` captures that value for its mounted lifetime. Reading returns default preferences and later writes through the null store do nothing. The selected size and completed bests can still change inside the session, so New Game and Restart continue to work with the current in-memory preferences.

A store can also be obtainable while `getItem` or `setItem` throws. The reader and writer catch those failures independently. There is no need for the game to identify whether the cause is private browsing, a site setting or a browser policy. [Storage failure policy](storage-failure-policy.md) covers the feature presentation and the current adapter's silent behavior.

A successful write in one moment cannot promise a value will remain after the session ends. Conversely, a default board after reopening does not establish that the original write was denied. Looking at both same-context reload and a genuinely new private session separates those scenarios.

## What automated isolation proves

The browser harness creates a fresh context for each test and closes it afterward. This prevents one journey's preferences from leaking into another. It does not record a complete manual lifecycle for every branded browser's private-window UI. In particular, passing the Chromium storage-denial injection is evidence for the app's catch boundary, not evidence of Firefox or Safari behavior.

For ordinary persistence journeys, tests receive an HTTP `gameURL` from the artifact server. Other browser journeys can open the built file directly. The technology specification deliberately uses an HTTP origin for saved-preference checks because direct-disk storage varies by browser. A private test that changes delivery method at the same time as mode introduces two independent variables.

The README names a tested Chromium combination and records other evidence limits. The test results do not include a private-browser matrix. Reports from additional environments are most useful when they distinguish the browser's storage lifecycle from the app's own save and validation operations.

## A compact manual comparison

A practical comparison uses the same built game at the same HTTP address in an ordinary profile and in a new private context. In each, choose a nondefault size, finish a known board, then reload. The accepted size and best should be interpreted independently from the fresh board state. Inspecting the raw preference key helps distinguish an accepted write from merely visible in-memory data.

Then end the private session using that browser's documented action and start a new one. Returning to a clean preference state is expected when the earlier context's storage has been removed. Merely closing one tab may not end the same session in every browser, and opening another private tab may not share the same store in every browser. Record what was actually closed rather than writing only “reopened browser.”

If play works but reload loses a new best, compare the stored value before and after completion. A readable old value with a new visible best suggests a failed update; an absent value after session closure suggests a lifetime boundary. An invalid value points to validation instead. [Corrupt data policy](corrupt-data-policy.md) explains why malformed stored text can lead to the same default display.

Private mode is therefore a useful environment for testing limited lifetime, but an explicit throwing store remains the clearer test of operation failure. Separating those experiments keeps a browser's privacy behavior from being mistaken for a game-rule regression.
