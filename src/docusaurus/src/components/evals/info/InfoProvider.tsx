import { useState, type ReactNode } from "react";
import Link from "@docusaurus/Link";
import MDXContent from "@theme/MDXContent";
import type { Batch } from "../records/records.types.ts";
import { Dialog } from "../Dialog.tsx";
import { Definitions, DefinitionKind, readMore, type DefinitionId } from "./definitions.ts";
import { InfoContext, type InfoRequest } from "./info-context.ts";
import { DefinitionNotes } from "./notes.ts";

const Cli = "open-forge";
const quoted = (arg: string) => /\s/.test(arg) ? `"${arg}"` : arg;

/** What a setup meant in one batch, from the batch's setup records. */
function BatchFacts({ id, batch }: { readonly id: DefinitionId; readonly batch: Batch }) {
  const setup = Definitions[id].kind === DefinitionKind.setup ? batch.setups.find((item) => item.id === id) : undefined;
  return <section className="eval-facts" aria-label="In this batch">
    <h3>In this batch</h3>
    <dl>
      <dt>Batch</dt><dd><code>{batch.experiment.id}</code></dd>
      <dt>Open Forge CLI</dt><dd>{batch.experiment.cliVersion ?? "Not reported"}</dd>
      {setup === undefined ? null : <>
        <dt>Project files after setup</dt><dd>{setup.files.length}</dd>
        <dt>Install commands</dt>
        <dd>{setup.commands.length === 0 ? "None" : <ol className="eval-commands">
          {setup.commands.map((command, index) => <li key={index}><code>{[Cli, ...command].map(quoted).join(" ")}</code></li>)}
        </ol>}</dd>
      </>}
    </dl>
  </section>;
}

/** Holds the one explanation dialog that every label on the page opens. */
export function InfoProvider({ children }: { readonly children: ReactNode }) {
  const [request, setRequest] = useState<InfoRequest | null>(null);
  const definition = request === null ? null : Definitions[request.id];
  return <InfoContext.Provider value={setRequest}>
    {children}
    <Dialog open={request !== null} onClose={() => setRequest(null)} title={definition?.title ?? ""}>
      {request === null || definition === null ? null : <>
        <div className="eval-definition"><MDXContent><definition.Content /></MDXContent></div>
        {DefinitionNotes[request.id] === undefined ? null : <p>{DefinitionNotes[request.id]}</p>}
        {request.batch === undefined ? null : <BatchFacts id={request.id} batch={request.batch} />}
        <p><Link to={readMore(request.id)}>Read more about {definition.title}</Link></p>
      </>}
    </Dialog>
  </InfoContext.Provider>;
}
