import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { waitForProcess, type ProcessResult } from "../process.ts";

export const DockerCommand = "docker";
const clientVariables = new Set(["PATH", "SYSTEMROOT", "COMSPEC", "PATHEXT", "TEMP", "TMP", "WINDIR", "HOME", "USERPROFILE", "APPDATA", "LOCALAPPDATA", "DOCKER_HOST", "DOCKER_CONTEXT", "DOCKER_CONFIG", "DOCKER_TLS_VERIFY", "DOCKER_CERT_PATH"]);

export interface DockerOptions {
  readonly timeoutMs: number;
  readonly signal?: AbortSignal;
  readonly input?: string;
  readonly env?: Readonly<Record<string, string>>;
  readonly maxBytes?: number;
}

/** Keeps Docker behind one seam so tests never need the daemon. */
export interface Docker {
  spawn(args: readonly string[], options: DockerOptions): ChildProcessWithoutNullStreams;
  run(args: readonly string[], options: DockerOptions): Promise<ProcessResult>;
}

export function dockerEnvironment(extra: Readonly<Record<string, string>> = {}): NodeJS.ProcessEnv {
  return { ...Object.fromEntries(Object.entries(process.env).filter(([name]) => clientVariables.has(name.toUpperCase()))), ...extra };
}

export const docker: Docker = {
  spawn(args, options) {
    return spawn(DockerCommand, args, { windowsHide: true, env: dockerEnvironment(options.env) });
  },
  async run(args, options) {
    options.signal?.throwIfAborted();
    const child = this.spawn(args, options);
    const stop = (): void => { child.kill(); };
    options.signal?.addEventListener("abort", stop, { once: true });
    try {
      return await waitForProcess(child, { timeoutMs: options.timeoutMs, ...(options.input === undefined ? {} : { input: options.input }), ...(options.maxBytes === undefined ? {} : { maxBytes: options.maxBytes }) });
    } finally {
      options.signal?.removeEventListener("abort", stop);
    }
  },
};

export function requireDockerResult(result: ProcessResult): ProcessResult {
  if (result.code !== 0 || result.timedOut) throw new Error(`Docker command failed (${result.code ?? "no exit code"}): ${result.stderr.slice(-2000)}`);
  return result;
}
