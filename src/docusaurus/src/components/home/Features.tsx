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
    title: "Routes, not dumps",
    body: "A loader and one short entrypoint per folder. The agent starts from the task, follows the routes that matter, and skips the rest.",
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
    title: "Grows without bloating",
    body: "Frontend rules live in a frontend scope, database rules in a database scope. Adding knowledge doesn't mean every task reads more of it.",
    link: "/docs/concepts/scopes",
    linkLabel: "Scopes",
  },
  {
    title: "Memory that stays honest",
    body: "Working, Emerging, Crystallized, Archived. Temporary state, unsettled findings, and accepted knowledge never blur together.",
    link: "/docs/concepts/memory",
    linkLabel: "The Memory model",
  },
  {
    title: "Extensions when you want them",
    body: "Planning, project documents, development workflows, task coordination. Install the packages that fit and skip the rest.",
    link: "/docs/extensions",
    linkLabel: "Browse packages",
  },
  {
    title: "A CLI that's optional",
    body: "It finds context, keeps navigation correct, and previews every change with --dry-run. Everything it does, you can do by editing files.",
    link: "/guides/cli",
    linkLabel: "CLI guide",
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
