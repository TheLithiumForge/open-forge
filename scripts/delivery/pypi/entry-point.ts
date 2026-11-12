import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";

export function pythonEntryPoint(runtime: SupportedRuntime): string {
  return `import os
import subprocess
import sys
from pathlib import Path


def main():
    executable = str(Path(__file__).parent / "bin" / "${PlatformPackages[runtime].nativeFileName}")
    arguments = [executable, *sys.argv[1:]]
    if os.name != "nt":
        os.execv(executable, arguments)
    try:
        return subprocess.call(arguments)
    except KeyboardInterrupt:
        return 130
`;
}
