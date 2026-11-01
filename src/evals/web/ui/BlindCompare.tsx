import { Eye, Monitor, Smartphone } from "lucide-react";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { cn } from "@/lib/utils";
import { PreferenceWinner } from "../../history/statuses.ts";
import type { BatchResponse, ComparePair } from "../server/api.types.ts";
import { api } from "./api.ts";

interface BlindCompareProps {
  readonly view: BatchResponse;
  readonly onChange: (view: BatchResponse) => void;
}

const Width = { desktop: "desktop", phone: "phone" } as const;
type Width = (typeof Width)[keyof typeof Width];

function GameFrame({ label, url, width }: { label: string; url: string; width: Width }) {
  return (
    <Card className="gap-3 py-4">
      <CardHeader className="px-4">
        <CardTitle>Game {label}</CardTitle>
      </CardHeader>
      <CardContent className="flex justify-center px-4">
        <iframe title={`Game ${label}`} src={url} className={cn("h-[640px] rounded-md border bg-black", width === Width.phone ? "w-[390px] max-w-full" : "w-full")} />
      </CardContent>
    </Card>
  );
}

/**
 * Two finished games side by side, labeled only A and B. Setups stay hidden until the reveal, so
 * the pick is about the games, not the setups.
 */
export function BlindCompare({ view, onChange }: BlindCompareProps) {
  const [pair, setPair] = useState<ComparePair | null | undefined>(undefined);
  const [width, setWidth] = useState<Width>(Width.desktop);
  const batchId = view.batch.id;

  useEffect(() => {
    api.pair(batchId).then(setPair, () => setPair(null));
  }, [batchId, view.compared]);

  const choose = async (winner: PreferenceWinner) => {
    if (pair === null || pair === undefined) return;
    setPair(undefined);
    onChange(await api.prefer(batchId, pair.left, pair.right, winner));
  };

  return (
    <div className="space-y-4 pt-2">
      <div className="flex flex-wrap items-center gap-3">
        <p className="text-sm text-muted-foreground">
          {view.compared} of {view.pairs} pairs compared. {view.batch.revealed ? "Setups are revealed." : "Setups stay hidden until you reveal them."}
        </p>
        <ToggleGroup type="single" variant="outline" size="sm" className="ml-auto" value={width} onValueChange={(value) => setWidth(value === Width.phone ? Width.phone : Width.desktop)}>
          <ToggleGroupItem value={Width.desktop} aria-label="Desktop width">
            <Monitor />
          </ToggleGroupItem>
          <ToggleGroupItem value={Width.phone} aria-label="Phone width">
            <Smartphone />
          </ToggleGroupItem>
        </ToggleGroup>
        {view.batch.revealed ? null : (
          <Button variant="outline" size="sm" onClick={() => api.reveal(batchId).then(onChange)}>
            <Eye /> Reveal setups
          </Button>
        )}
      </div>
      {pair === undefined ? <p className="text-sm text-muted-foreground">Loading the next pair…</p> : null}
      {pair === null ? <p className="text-sm text-muted-foreground">No pairs left to compare. Finished games appear here as runs complete.</p> : null}
      {pair === null || pair === undefined ? null : (
        <>
          <p className="text-sm">Play both games. Which one would you rather keep playing?</p>
          <div className="grid gap-4 xl:grid-cols-2">
            <GameFrame label="A" url={pair.leftGame} width={width} />
            <GameFrame label="B" url={pair.rightGame} width={width} />
          </div>
          <div className="flex flex-wrap justify-center gap-2">
            <Button onClick={() => choose(PreferenceWinner.left)}>A is better</Button>
            <Button variant="secondary" onClick={() => choose(PreferenceWinner.tie)}>
              About the same
            </Button>
            <Button onClick={() => choose(PreferenceWinner.right)}>B is better</Button>
          </div>
        </>
      )}
    </div>
  );
}
