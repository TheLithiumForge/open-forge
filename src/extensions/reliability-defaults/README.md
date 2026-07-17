# Reliability Defaults

Optional cross-language process directives for workspaces that want stronger baseline safeguards.

It installs three workspace-wide directives:

- workspace containment and scope
- mutation safety
- evidence and closeout

Because directive files beside `.agents/directives/_directives.md` are mandatory across the workspace, install this pack only when those defaults should apply to all agent work.

Install it with:

```sh
open-forge extend reliability-defaults
```

The directives are language- and tool-agnostic. They do not impose benchmark-specific temporary-file rules.
