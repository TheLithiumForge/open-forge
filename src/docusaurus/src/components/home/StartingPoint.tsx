import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import styles from "./home.module.css";

interface Path {
  readonly title: string;
  readonly lead: string;
  readonly points: readonly { readonly key: string; readonly content: ReactNode }[];
  readonly demo: string;
}

const paths: readonly Path[] = [
  {
    title: "Starting something new",
    lead: "The requirements are still taking shape, so keep planning light.",
    points: [
      { key: "request", content: "Start from a request, not a full specification" },
      {
        key: "decisions",
        content: (
          <>
            Record <Link to="/docs/extensions/planning">Decisions</Link> as you make them
          </>
        ),
      },
      {
        key: "rules",
        content: (
          <>
            Turn the conversation into rules and specs with <Link to="/docs/extensions/core-templates">Core Templates</Link> and{" "}
            <Link to="/docs/extensions/project-documents">Project Documents</Link>
          </>
        ),
      },
    ],
    demo: "/docs/demos/greenfield",
  },
  {
    title: "Joining an existing codebase",
    lead: "The code shows what the system does. The reasons usually live somewhere else.",
    points: [
      { key: "map", content: "Point a Map at your existing docs instead of moving them" },
      {
        key: "decisions",
        content: (
          <>
            Record <Link to="/docs/highlights#decisions-keep-the-why">Decisions</Link> for important choices from now on
          </>
        ),
      },
      { key: "rules", content: "Add a Directive when a mistake repeats, not in advance" },
    ],
    demo: "/docs/demos/brownfield",
  },
];

export default function StartingPoint(): ReactNode {
  return (
    <section className={`${styles.section} ${styles.sectionAlt}`}>
      <div className="container">
        <Heading as="h2" className={styles.sectionTitle}>
          New project or existing codebase?
        </Heading>
        <div className={styles.pathGrid}>
          {paths.map((path) => (
            <article key={path.title} className={styles.card}>
              <Heading as="h3" className={styles.cardTitle}>
                {path.title}
              </Heading>
              <p>{path.lead}</p>
              <ul className={styles.pathPoints}>
                {path.points.map((point) => (
                  <li key={point.key}>{point.content}</li>
                ))}
              </ul>
              <div className={styles.pathLinks}>
                <Link to="/docs/getting-started/greenfield-and-brownfield">Read the guide →</Link>
                <Link to={path.demo}>Try the demo →</Link>
              </div>
            </article>
          ))}
        </div>
        <aside className={styles.highlight}>
          <strong>Keep the reasons.</strong> A Decision, from the Planning Extension, records what was chosen and why. The reason stays on record after the conversation ends and
          the people involved move on, so a later agent tempted to "clean it up" can find out why first.{" "}
          <Link to="/docs/highlights#decisions-keep-the-why">See the highlights →</Link>
        </aside>
      </div>
    </section>
  );
}
