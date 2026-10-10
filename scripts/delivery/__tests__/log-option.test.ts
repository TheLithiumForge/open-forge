import assert from "node:assert/strict";
import { test } from "node:test";
import { extractLogOption, LogFlag } from "../log-option.ts";

const File = "artifacts/logs/command output.log";
const Args = ["--rid", "win-x64"];

test("shared log parsing removes only the log option and preserves command arguments", () => {
  assert.deepEqual(extractLogOption(Args), { args: Args });
  assert.deepEqual(extractLogOption([...Args, LogFlag, File]), { args: Args, log: File });
  assert.deepEqual(extractLogOption([`${LogFlag}=${File}`, ...Args]), { args: Args, log: File });
});

test("missing and repeated log files fail before dispatch", () => {
  for (const args of [[LogFlag], [`${LogFlag}=`], [LogFlag, "--plan"], [LogFlag, File, LogFlag, File]]) assert.throws(() => extractLogOption(args));
});
