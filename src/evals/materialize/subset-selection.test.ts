import assert from "node:assert/strict";
import { test } from "node:test";
import { subsetOptions } from "./cli.ts";
import { sha256 } from "./files.ts";
import { selectionChanges, selectionStamp, SubsetFlag, type SubsetSelection } from "./subset-selection.ts";

const PreviousClauses = [{ id: "R05.1", text: "Count moves." }, { id: "R07.1", text: "Count time." }] as const;
const Reason = "Accept the spec version 2 counter caps.";
const Previous: SubsetSelection = { sourceHash: sha256("previous source"), clauses: PreviousClauses, changes: [] };

test("selection changes need the acceptance flag and record additions, removals and text changes", () => {
  const next = [{ id: PreviousClauses[0].id, text: "Count moves up to the cap." }, { id: "R05.5", text: "Cap moves." }] as const;
  const sourceHash = sha256("updated source");
  assert.throws(() => selectionChanges(Previous, sourceHash, next));
  assert.throws(() => selectionChanges(Previous, sourceHash, next, " "));
  const changes = selectionChanges(Previous, sourceHash, next, Reason);
  assert.deepEqual(changes, [{ from: selectionStamp(Previous.sourceHash, PreviousClauses), to: selectionStamp(sourceHash, next), reason: Reason,
    added: [next[1].id], removed: [PreviousClauses[1].id], updated: [PreviousClauses[0].id] }]);
  assert.deepEqual(selectionChanges({ sourceHash, clauses: next, changes }, sourceHash, next), changes);
  assert.deepEqual(selectionChanges(Previous, sourceHash, PreviousClauses), []);
});

test("subset CLI requires one flag and a nonempty reason", () => {
  assert.deepEqual(subsetOptions([]), {});
  assert.deepEqual(subsetOptions([SubsetFlag.acceptSelectionChange, Reason]), { acceptSelectionChange: Reason });
  for (const args of [[SubsetFlag.acceptSelectionChange], [SubsetFlag.acceptSelectionChange, " "], ["--force", Reason],
    [SubsetFlag.acceptSelectionChange, SubsetFlag.acceptSelectionChange], [SubsetFlag.acceptSelectionChange, Reason, "extra"]]) assert.throws(() => subsetOptions(args));
});
