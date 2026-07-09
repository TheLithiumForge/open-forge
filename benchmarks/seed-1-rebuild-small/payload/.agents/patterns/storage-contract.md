---
open-forge:
  description: Shape for local JSON storage owned by the bookmarks CLI
  tags: [Extension, Pattern, Storage, Data]
---

# Storage Contract

## Shape

Persist one JSON document with a stable top-level shape, for example:

```json
{
  "bookmarks": [
    {
      "url": "https://example.com/",
      "tags": ["docs"],
      "createdAt": "2026-01-01T00:00:00.000Z",
      "updatedAt": "2026-01-01T00:00:00.000Z"
    }
  ]
}
```

## Rules

- Read missing files as an empty store.
- Treat malformed JSON as an error.
- Treat wrong-shaped JSON as an error.
- Save pretty-printed JSON with a trailing newline.
- Prefer write-temp-then-rename for safer local writes.
- Do not add cross-process locking unless the task explicitly grows to need it.
