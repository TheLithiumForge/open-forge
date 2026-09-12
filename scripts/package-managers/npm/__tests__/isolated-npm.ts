import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { mkdirSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { basename, join } from "node:path";

const npmFlags = ["--offline", "--ignore-scripts", "--no-save", "--no-package-lock", "--no-audit", "--no-fund", "--global=false"];

export class IsolatedNpm {
  readonly environment: NodeJS.ProcessEnv;
  readonly commands: string[][] = [];
  readonly cli: string;
  readonly scratchRoot: string;

  constructor(scratchRoot: string) {
    this.scratchRoot = scratchRoot;
    const cli = process.env["npm_execpath"];
    assert.ok(cli, "Run the package evidence through its root npm script.");
    this.cli = cli;
    const home = join(scratchRoot, "home");
    const cache = join(scratchRoot, "cache");
    const prefix = join(scratchRoot, "prefix");
    const temporary = join(scratchRoot, "tmp");
    const userConfig = join(scratchRoot, "user.npmrc");
    const globalConfig = join(scratchRoot, "global.npmrc");
    for (const directory of [home, cache, prefix, temporary]) {
      mkdirSync(directory, { recursive: true });
    }
    writeFileSync(userConfig, "", "utf8");
    writeFileSync(globalConfig, "", "utf8");
    const inherited = Object.fromEntries(Object.entries(process.env).filter(([name]) => !name.toLowerCase().startsWith("npm_config_")));
    this.environment = {
      ...inherited,
      HOME: home,
      USERPROFILE: home,
      APPDATA: home,
      LOCALAPPDATA: home,
      TMP: temporary,
      TEMP: temporary,
      TMPDIR: temporary,
      npm_config_cache: cache,
      npm_config_prefix: prefix,
      npm_config_userconfig: userConfig,
      npm_config_globalconfig: globalConfig,
      npm_config_global: "false",
      npm_config_update_notifier: "false",
      NO_UPDATE_NOTIFIER: "1",
    };
  }

  run(argumentsToPass: readonly string[]): string {
    const argumentsForNpm = [this.cli, ...argumentsToPass, ...npmFlags];
    this.commands.push([process.execPath, ...argumentsForNpm]);
    return execFileSync(process.execPath, argumentsForNpm, { cwd: this.scratchRoot, env: this.environment, encoding: "utf8", stdio: "pipe" });
  }

  pack(packageDirectory: string, outputDirectory: string): { path: string; name: string; version: string; files: string[] } {
    mkdirSync(outputDirectory, { recursive: true });
    assert.deepEqual(readdirSync(outputDirectory), [], "Each package has a fresh pack destination.");
    const output: unknown = JSON.parse(this.run(["pack", packageDirectory, "--pack-destination", outputDirectory, "--json"]));
    assert.ok(Array.isArray(output));
    assert.equal(output.length, 1);
    const packed: unknown = output[0];
    assert.ok(typeof packed === "object" && packed !== null);
    assert.ok("filename" in packed && typeof packed.filename === "string");
    assert.ok("name" in packed && typeof packed.name === "string");
    assert.ok("version" in packed && typeof packed.version === "string");
    assert.ok("files" in packed && Array.isArray(packed.files));
    assert.equal(basename(packed.filename), packed.filename);
    assert.deepEqual(readdirSync(outputDirectory), [packed.filename]);
    const path = join(outputDirectory, packed.filename);
    assert.equal(statSync(path).isFile(), true);
    const files = packed.files.map((entry: unknown) => {
      assert.ok(typeof entry === "object" && entry !== null && "path" in entry && typeof entry.path === "string");
      return entry.path;
    });
    return { path, name: packed.name, version: packed.version, files: files.sort() };
  }
}
