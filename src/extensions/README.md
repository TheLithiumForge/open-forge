# Open Forge Extensions

First-party Open Forge extensions will live here.

Each bundled extension uses this shape:

```text
{extension-id}/
  payload/
    .agents/
      ...
```

The MVP CLI installs bundled extensions with:

```sh
open-forge extend {extension-id}
```

The installed payload files remain runtime truth. Extension metadata, manifests, previews, and update/remove behavior are future work.
