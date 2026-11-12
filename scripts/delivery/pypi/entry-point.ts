import { PlatformPackages } from "../package-model.ts";
import { pythonEnvironment } from "./environment-markers.ts";
import { pythonExecutable, pythonPlatformPackage } from "./package-model.ts";

export function pythonEntryPoint(): string {
  const targets = Object.values(PlatformPackages).map(({ runtime }) => {
    const environment = pythonEnvironment(runtime);
    return `    (${JSON.stringify(environment.platform)}, ${JSON.stringify(environment.machines)}, ${JSON.stringify(pythonPlatformPackage(runtime))}, ${JSON.stringify(pythonExecutable(runtime))}),`;
  });
  return `import os
import platform
import subprocess
import sys
from importlib.metadata import PackageNotFoundError, distribution


_TARGETS = [
${targets.join("\n")}
]


def _fail(message):
    print(message, file=sys.stderr)
    return 1


def main():
    machine = platform.machine()
    target = next((item for item in _TARGETS if item[0] == sys.platform and machine in item[1]), None)
    if target is None:
        return _fail(f"Open Forge does not provide a PyPI package for {sys.platform}/{machine}.")
    name, path = target[2:]
    try:
        executable = str(distribution(name).locate_file(path))
    except PackageNotFoundError:
        return _fail(f"The required platform package {name} is not installed.")
    arguments = [executable, *sys.argv[1:]]
    try:
        if os.name != "nt":
            os.execv(executable, arguments)
        return subprocess.call(arguments)
    except OSError as error:
        return _fail(f"Unable to start {name}: {error}")
    except KeyboardInterrupt:
        return 130
`;
}
