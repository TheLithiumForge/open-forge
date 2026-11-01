import { Badge } from "@/components/ui/badge";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import type { StateResponse } from "../server/api.types.ts";

interface Status {
  readonly ok: boolean;
  readonly label: string;
  readonly fix: string;
}

/** Sign-in and Docker at a glance, with the fix in a tooltip. */
export function StatusBadges({ state }: { state: StateResponse }) {
  const statuses: readonly Status[] = [
    { ok: state.signIn.claude, label: "Claude Code", fix: "Run npx claude setup-token and put the token in evals/.env.local." },
    { ok: state.signIn.codex, label: "Codex", fix: "Run npm run login:codex." },
    { ok: state.docker, label: "Docker", fix: "Start Docker, or set isolation to local." },
  ];
  return (
    <div className="flex flex-wrap gap-2">
      {statuses.map((status) => (
        <Tooltip key={status.label}>
          <TooltipTrigger asChild>
            <Badge variant="outline" className="gap-1.5">
              <span className={cn("size-2 rounded-full", status.ok ? "bg-success" : "bg-warning")} />
              {status.label}
            </Badge>
          </TooltipTrigger>
          <TooltipContent>{status.ok ? "Ready" : status.fix}</TooltipContent>
        </Tooltip>
      ))}
    </div>
  );
}
