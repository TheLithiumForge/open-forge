import type { Artifact, Batch } from "./records/records.types.ts";
import { evidencePath } from "./data/sources.ts";
import { useSource } from "./source-context.ts";

const Image = /\.(png|jpe?g|webp)$/i;
const CodeFolder = "files/";
// Named run files and what each holds.
const Named: readonly [string, string][] = [["game.html", "Built game, as HTML source"], ["conversation.json", "Conversation with the simulated user"], ["events.jsonl", "Tool event log"]];

/** A run's or a grade's saved files. Links open them from the source the records came from. */
export function Evidence({ batch, artifacts }: { readonly batch: Batch; readonly artifacts: readonly Artifact[] }) {
  const source = useSource();
  const link = (artifact: Artifact) => source.file(evidencePath(batch, artifact));
  if (artifacts.length === 0) return <p>No saved files.</p>;
  if (artifacts.every((artifact) => link(artifact) === null)) return <p>{artifacts.length} saved files. Fetch the latest results, run the site locally or open a results folder to see them.</p>;
  const images = artifacts.filter((artifact) => Image.test(artifact.path) && !artifact.path.startsWith(CodeFolder));
  const code = artifacts.filter((artifact) => artifact.path.startsWith(CodeFolder));
  return <div className="eval-evidence-files">
    {images.length === 0 ? null : <div className="eval-shots">{images.map((artifact) => {
      const href = link(artifact);
      return href === null ? null : <a key={artifact.path} href={href} target="_blank" rel="noreferrer"><img src={href} alt={artifact.path} loading="lazy" /><span>{artifact.path}</span></a>;
    })}</div>}
    <ul>{Named.map(([name, label]) => {
      const artifact = artifacts.find((item) => item.path === name), href = artifact === undefined ? null : link(artifact);
      return href === null ? null : <li key={name}><a href={href} target="_blank" rel="noreferrer">{label}</a></li>;
    })}</ul>
    {code.length === 0 ? null : <details><summary>Code, {code.length} files</summary><ul className="eval-files">{code.map((artifact) => {
      const href = link(artifact);
      return <li key={artifact.path}>{href === null ? artifact.path.slice(CodeFolder.length) : <a href={href} target="_blank" rel="noreferrer">{artifact.path.slice(CodeFolder.length)}</a>} <span className="eval-muted">{artifact.bytes} bytes</span></li>;
    })}</ul></details>}
  </div>;
}
