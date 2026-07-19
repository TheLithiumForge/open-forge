# Reliability Defaults

Optional cross-language process directives for workspaces that want stronger baseline safeguards.

It installs three workspace-wide directives:

- workspace containment and scope
- mutation safety
- evidence and closeout

Each installed directive is a direct file under the baseline-loaded `.agents/directives/_directives.md` route, so loading that root makes every file binding workspace-wide. Install this pack only when those defaults should govern all agent work; use a narrower child directive route for narrower scope.

Install it with:

```sh
open-forge extend reliability-defaults
```

The directives are language- and tool-agnostic. They do not impose benchmark-specific temporary-file rules.
