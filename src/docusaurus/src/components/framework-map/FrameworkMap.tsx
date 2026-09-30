import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import { codeSpans } from "./code-spans";
import {
  captions,
  coreRoles,
  entryNodes,
  entrypointNote,
  linkedFiles,
  loadingGuide,
  loadingLabels,
  memoryLoading,
  memoryNote,
  memoryStates,
  pitch,
  startupFootprint,
  taskSteps,
} from "./framework-map-data";
import type { EntryNode, FileRow, Loading } from "./framework-map-data";
import styles from "./framework-map.module.css";

// The two loading states: tinted with a solid outline, or unfilled with a dashed one.
const loadingClass: Record<Loading, string> = {
  startup: styles.startup,
  demand: styles.demand,
};

function Badge({ loading }: { readonly loading: Loading }): ReactNode {
  return <span className={`${styles.badge} ${loadingClass[loading]}`}>{loadingLabels[loading]}</span>;
}

function Prose({ text }: { readonly text: string }): ReactNode {
  return codeSpans(text).map((span, index) => (span.code ? <code key={index}>{span.text}</code> : span.text));
}

const present = (values: readonly (string | undefined)[]): readonly string[] => values.filter((value) => value !== undefined);

// A card's lower half: the rows for what its entrypoint links to, then examples.
function Files({ rows, lines }: { readonly rows: readonly FileRow[]; readonly lines: readonly string[] }): ReactNode {
  return (
    <div className={styles.files}>
      {rows.map((row) => (
        <span key={row.text} className={`${styles.fileRow} ${loadingClass[row.loading]}`}>
          {row.text}
        </span>
      ))}
      {lines.map((line) => (
        <span key={line} className={styles.muted}>
          {line}
        </span>
      ))}
    </div>
  );
}

function Node({ node, className }: { readonly node: EntryNode; readonly className?: string }): ReactNode {
  return (
    <div className={`${styles.node} ${styles.startup} ${className ?? ""}`}>
      <code>{node.file}</code>
      <span>{node.note}</span>
    </div>
  );
}

function Entry(): ReactNode {
  const [agents, loader] = entryNodes;
  return (
    <div className={styles.entry}>
      <Node node={agents} />
      <span className={styles.arrow} aria-hidden="true" />
      <Node node={loader} className={styles.loader} />
    </div>
  );
}

function Core(): ReactNode {
  return (
    <section className={`${styles.panel} ${styles.core}`} aria-labelledby="map-core">
      <h3 id="map-core" className={styles.panelTitle}>
        Core <small>How to work</small>
      </h3>
      <ul className={styles.roles}>
        {coreRoles.map((role) => (
          <li key={role.name} className={styles.card}>
            <div className={`${styles.index} ${loadingClass[role.loading]}`}>
              <strong>{role.name}</strong>
              <span>{role.role}</span>
              <Badge loading={role.loading} />
            </div>
            <Files rows={role.files ?? [linkedFiles]} lines={present([role.example, role.contents])} />
          </li>
        ))}
      </ul>
    </section>
  );
}

function Memory(): ReactNode {
  return (
    <section className={`${styles.panel} ${styles.memory}`} aria-labelledby="map-memory">
      <div className={styles.panelHeading}>
        <h3 id="map-memory" className={styles.panelTitle}>
          Memory <small>What to remember</small>
        </h3>
        <Badge loading={memoryLoading} />
      </div>
      <ol className={styles.states}>
        {memoryStates.map((state) => (
          <li key={state.name} className={styles.stateItem}>
            <div className={styles.card}>
              <div className={`${styles.index} ${loadingClass[state.loading]}`}>
                <strong>
                  {state.name} <span className={styles.tense}>{state.tense}</span>
                </strong>
                <span>{state.holds}</span>
                <Badge loading={state.loading} />
                {state.loadingNote ? <span className={styles.muted}>{state.loadingNote}</span> : null}
              </div>
              <Files rows={[linkedFiles]} lines={present([state.contents])} />
            </div>
            {state.next ? <span className={styles.transition}>{state.next}</span> : null}
          </li>
        ))}
      </ol>
      <p className={styles.note}>{memoryNote}</p>
    </section>
  );
}

export default function FrameworkMap(): ReactNode {
  return (
    <figure className={styles.map}>
      <div className={styles.pitch}>
        <p>{pitch}</p>
        <p className={styles.footprint}>{startupFootprint}</p>
      </div>
      <Entry />
      <p className={styles.fork}>{entrypointNote}</p>
      <div className={styles.panels}>
        <Core />
        <Memory />
      </div>
      <ol className={styles.steps} aria-label="Every task">
        {taskSteps.map((step) => (
          <li key={step.text} className={step.loading ? loadingClass[step.loading] : undefined}>
            {step.text}
          </li>
        ))}
      </ol>
      <figcaption className={styles.caption}>
        <p>
          <Link to={loadingGuide.href}>{loadingGuide.label}</Link>
        </p>
        {captions.map((caption) => (
          <p key={caption}>
            <Prose text={caption} />
          </p>
        ))}
      </figcaption>
    </figure>
  );
}
