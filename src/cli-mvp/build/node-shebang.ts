import fs from "node:fs/promises";

const nodeShebang = "#!/usr/bin/env node";

export async function ensureNodeShebang(file: string): Promise<void> {
  const text = await fs.readFile(file, "utf8");
  if (text.startsWith("#!")) {
    await fs.writeFile(file, text.replace(/^#![^\r\n]*/, nodeShebang));
    return;
  }

  await fs.writeFile(file, `${nodeShebang}\n${text}`);
}
