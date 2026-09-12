import { join } from "node:path";
import { MainPackageName } from "../package-model.ts";
import { PlatformPackages } from "../package-model.ts";
import { npm } from "../process.ts";
import { stagePackages, type StageRequest } from "./stage.ts";

export function packNpmPackages(request: StageRequest & { outputDirectory: string }) {
  const staged = stagePackages(request);
  const { repositoryRoot: root, outputDirectory: output, runtime } = request;
  for (const directory of [staged.mainPackageDirectory, staged.platformPackageDirectory]) {
    npm(["pack", directory, "--pack-destination", output, "--offline", "--ignore-scripts", "--no-audit", "--no-fund"], root);
  }
  const tarball = (name: string) => join(output, `${name.replace(/^@/u, "").replaceAll("/", "-")}-${staged.version}.tgz`);
  return { mainTarball: tarball(MainPackageName), platformTarball: tarball(PlatformPackages[runtime].packageName) };
}
