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
    lead: "Give your agent a first build request. Keep planning light while the requirements take shape.",
    points: [
      { key: "request", content: "Start from a request, not a full specification" },
      { key: "questions", content: "Answer the choices that matter as the agent asks" },
      { key: "review", content: "Review what it builds, then keep useful outcomes for the next task" },
    ],
    demo: "/docs/demos/greenfield",
  },
  {
    title: "Joining an existing codebase",
    lead: "Ask how the tests run, then try a small change. Point the agent to the reasons behind the code.",
    points: [
      { key: "sources", content: "Link to the docs you already have instead of moving them" },
      { key: "reasons", content: "Keep the reasons behind important choices from now on" },
      { key: "rules", content: "Add a rule when a mistake repeats, not in advance" },
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
      </div>
    </section>
  );
}
