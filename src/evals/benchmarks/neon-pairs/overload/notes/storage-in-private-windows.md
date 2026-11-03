# Storage in Private Windows

Analysis after a bug report from a tester.

Local storage behaves differently across browsers. Some private windows throw on every write, one browser gave a quota of zero, and a `file:` page shares or isolates storage depending on the browser. A thrown write used to stop the game.

We now treat every read as unknown data and every write as something that may fail. When storage fails, play continues for the current session and nothing is saved. Checking saved preferences needs a local HTTP origin, since `file:` results can't be trusted.

Open question: whether to tell the player that nothing is being saved. Nobody has asked for it yet.
