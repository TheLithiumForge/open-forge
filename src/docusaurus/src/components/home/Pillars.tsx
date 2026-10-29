import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import styles from "./home.module.css";

interface Pillar {
  readonly title: string;
  readonly body: ReactNode;
  readonly link: string;
  readonly linkLabel: string;
}

const pillars: readonly Pillar[] = [
  {
    title: "Spec-driven context",
    body: (
      <>
        Rules, decisions and accepted facts live in plain Markdown the agent follows. Each task reads only the routes it needs: <code>LoadNow</code> marks what loads, and{" "}
        <code>applyTo</code> limits it to matching files.
      </>
    ),
    link: "/docs/concepts/routing",
    linkLabel: "How routing works",
  },
  {
    title: "Safe by default",
    body: (
      <>
        Every CLI command that changes files offers <code>--dry-run</code> to preview its plan. Linking or tagging a document never makes it accepted, and Memory keeps accepted
        knowledge apart from unconfirmed findings.
      </>
    ),
    link: "/docs/concepts/memory",
    linkLabel: "The Memory model",
  },
  {
    title: "Your docs stay yours",
    body: (
      <>
        Keep your team's documents in <code>docs/</code>, written by hand or with AI help. The CLI keeps their catalogue current, and you decide which ones are accepted sources.
      </>
    ),
    link: "/docs/getting-started/your-own-docs",
    linkLabel: "Your own docs",
  },
];

export default function Pillars(): ReactNode {
  return (
    <section className={styles.section}>
      <div className="container">
        <Heading as="h2" className={styles.sectionTitle}>
          What it gives you
        </Heading>
        <div className={styles.featureGrid}>
          {pillars.map((pillar) => (
            <article key={pillar.title} className={styles.card}>
              <Heading as="h3" className={styles.cardTitle}>
                {pillar.title}
              </Heading>
              <p>{pillar.body}</p>
              <Link to={pillar.link} className={styles.cardLink}>
                {pillar.linkLabel} →
              </Link>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
