import { Tool } from "../../config/choices.ts";
import { PinnedTool, imageToolCommand } from "../providers/pinned-tools.ts";
import { evalImageTag } from "./docker-image.ts";
import { collectProject, copyProjectIn } from "./files.ts";
import { registerContainer, type RunSpace } from "./executor.ts";
import { ContainerPaths, StageKind } from "./settings.ts";
import { ContainerStage, type StageOptions } from "./stage.ts";
import { ToolHome, toolEnvironment, type ToolHomeOptions } from "./tool-home.ts";

export async function withAgentStage<T>(space: RunSpace, provider: Tool, options: StageOptions & ToolHomeOptions, action: () => Promise<T>): Promise<T> {
  const stage = new ContainerStage(options);
  const home = new ToolHome(provider, space, options);
  let ready = false;
  let unregister = (): void => {};
  try {
    await stage.open(evalImageTag(StageKind.agent));
    await copyProjectIn(stage, space);
    await home.copyIn(stage);
    await stage.prepare(ContainerPaths.work, ContainerPaths.home);
    unregister = registerContainer(space, {
      spawn(command, args) {
        const allowed = provider === Tool.codex ? PinnedTool.codex : PinnedTool.claude;
        if (command !== allowed) throw new Error("The agent container only runs its chosen provider.");
        const tool = imageToolCommand(allowed);
        return stage.spawn([tool.command, ...tool.args, ...args], toolEnvironment(provider), home.auth);
      },
      async syncHome() { await home.sync(stage); },
    });
    ready = true;
    const result = await action();
    stage.checkActive();
    return result;
  } finally {
    unregister();
    try {
      if (ready) {
        await stage.stop();
        try { await collectProject(stage, space); }
        finally { await home.sync(stage); }
      }
    } finally {
      home.dispose();
      await stage.remove();
    }
  }
}
