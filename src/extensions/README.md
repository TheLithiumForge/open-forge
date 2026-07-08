# Open Forge Extensions

First-party Open Forge extensions live here.

Each bundled extension uses this shape:

```text
{extension-id}/
  extension.json
  README.md
  payload/
    .agents/
      ...
```

The MVP CLI installs bundled extensions with:

```sh
open-forge extend
open-forge extend --list
open-forge extend --ids {extension-id},{extension-id}
open-forge extend {extension-id}
```

`extension.json` is for CLI display. The installed payload files remain runtime truth.

Payload files should use #Extension plus their route type and useful scope tags when the file format is Open Forge-authored. Runtime-native files such as `SKILL.md` should keep native metadata. Do not use #OpenForge in extension payloads; it is reserved for core framework routes.

Shared behavior should live once inside a shared route or shared extension pack. The current CLI does not resolve dependencies, so first-party packs should either be standalone or clearly grouped with their shared skills and workflows.
