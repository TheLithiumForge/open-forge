// A child process for process.test.ts: "flood" writes one endless line to stdout, "ready" writes one short word.
const mode = process.argv[2];
if (mode === "ready") process.stdout.write("ready");
if (mode === "flood") {
  const chunk = "x".repeat(65_536);
  const write = (): void => {
    while (process.stdout.write(chunk));
  };
  process.stdout.on("drain", write);
  write();
}
