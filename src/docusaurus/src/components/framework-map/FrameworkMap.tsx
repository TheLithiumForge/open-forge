import type { ReactNode } from "react";
import { childRouteNote, coreRoles, legendOrder, loadingLabels, loadingNotes, memoryStates, taskSteps } from "./framework-map-data";
import type { Loading } from "./framework-map-data";
import styles from "./framework-map.module.css";

const loadingClass: Record<Loading, string> = {
  startup: styles.loadStartup,
  listed: styles.loadListed,
  checkpoints: styles.loadCheckpoints,
  demand: styles.loadDemand,
};

function Badge({ loading }: { readonly loading: Loading }): ReactNode {
  return <span className={`${styles.badge} ${loadingClass[loading]}`}>{loadingLabels[loading]}</span>;
}

function Entry(): ReactNode {
  return (
    <div className={styles.entry}>
      <div className={styles.node}>
        <code>AGENTS.md</code>
        <span>Tells the agent to read the loader</span>
      </div>
      <span className={styles.arrow} aria-hidden="true" />
      <div className={`${styles.node} ${styles.loader}`}>
        <code>.agents/loader.md</code>
        <span>The rules, and the routes to everything else</span>
      </div>
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
          <li key={role.name} className={styles.role}>
            <strong>{role.name}</strong>
            <span>{role.role}</span>
            <em>{role.example}</em>
            <Badge loading={role.loading} />
          </li>
        ))}
      </ul>
    </section>
  );
}

function Memory(): ReactNode {
  return (
    <section className={`${styles.panel} ${styles.memory}`} aria-labelledby="map-memory">
      <h3 id="map-memory" className={styles.panelTitle}>
        Memory <small>What to remember</small>
      </h3>
      <ol className={styles.states}>
        {memoryStates.map((state) => (
          <li key={state.name} className={styles.stateItem}>
            <div className={styles.state}>
              <strong>
                {state.name} <span className={styles.tense}>{state.tense}</span>
              </strong>
              <span>{state.holds}</span>
              <Badge loading={state.loading} />
            </div>
            {state.next ? <span className={styles.transition}>{state.next}</span> : null}
          </li>
        ))}
      </ol>
      <p className={styles.note}>A record can skip states. A decision made on the spot goes straight to Crystallized.</p>
    </section>
  );
}

export default function FrameworkMap(): ReactNode {
  return (
    <figure className={styles.map}>
      <Entry />
      <span className={styles.fork} aria-hidden="true" />
      <div className={styles.panels}>
        <Core />
        <Memory />
      </div>
      <ol className={styles.steps} aria-label="Every task">
        {taskSteps.map((step) => (
          <li key={step}>{step}</li>
        ))}
      </ol>
      <figcaption className={styles.caption}>
        <dl className={styles.legend}>
          {legendOrder.map((loading) => (
            <div key={loading} className={styles.legendItem}>
              <dt>
                <Badge loading={loading} />
              </dt>
              <dd>{loadingNotes[loading]}</dd>
            </div>
          ))}
        </dl>
        <p className={styles.childNote}>{childRouteNote}</p>
      </figcaption>
    </figure>
  );
}
