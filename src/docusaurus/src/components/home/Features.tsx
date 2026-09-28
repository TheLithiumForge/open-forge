import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import styles from "./home.module.css";

interface Feature {
  readonly title: string;
  readonly body: string;
  readonly link: string;
  readonly linkLabel: string;
}

const features: readonly Feature[] = [
  {
    title: "Routes, not bulk context",
    body: "A loader and one short entrypoint per folder. The entrypoints point the agent to the routes a task needs, and the rest stays closed.",
    link: "/docs/concepts/routing",
    linkLabel: "How routing works",
  },
  {
    title: "Plain Markdown you own",
    body: "No hidden database, no agent runtime, nothing tied to one vendor. Read it, diff it, and version it with the rest of your project.",
    link: "/docs/concepts/customizing",
    linkLabel: "Make it yours",
  },
  {
    title: "Grows without bloating context",
    body: "Frontend rules live in a frontend scope, database rules in a database scope. A scope loads only when a task selects it, so new knowledge doesn't add to every task.",
    link: "/docs/concepts/scopes",
    linkLabel: "Scopes",
  },
  {
    title: "Memory with clear trust levels",
    body: "Working, Emerging, Crystallized, and Archived keep temporary state, unconfirmed findings, accepted knowledge, and history apart.",
    link: "/docs/concepts/memory",
    linkLabel: "The Memory model",
  },
  {
    title: "Optional Extensions",
    body: "Packages for planning, project documents, development workflows, and task coordination. Install the ones that fit, and leave out the rest.",
    link: "/docs/extensions",
    linkLabel: "Browse packages",
  },
  {
    title: "An optional CLI",
    body: "It finds context, keeps navigation correct, and previews every change with --dry-run. Everything it does, you can do by editing files.",
    link: "/docs/cli",
    linkLabel: "Working with the CLI",
  },
];

export default function Features(): ReactNode {
  return (
    <section className={styles.section}>
      <div className="container">
        <div className={styles.featureGrid}>
          {features.map((feature) => (
            <article key={feature.title} className={styles.card}>
              <Heading as="h3" className={styles.cardTitle}>
                {feature.title}
              </Heading>
              <p>{feature.body}</p>
              <Link to={feature.link} className={styles.cardLink}>
                {feature.linkLabel} →
              </Link>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
