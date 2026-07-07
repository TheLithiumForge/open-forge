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

Shared behavior should live once inside a shared route or shared extension pack. The current CLI does not resolve dependencies, so first-party packs should either be standalone or clearly grouped with their shared skills and workflows.
