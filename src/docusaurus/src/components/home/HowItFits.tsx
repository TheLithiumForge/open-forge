import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import FrameworkMap from "@site/src/components/framework-map/FrameworkMap";
import styles from "./home.module.css";

export default function HowItFits(): ReactNode {
  return (
    <section className={styles.section}>
      <div className="container">
        <Heading as="h2" className={styles.sectionTitle}>
          How it fits together
        </Heading>
        <p className={styles.sectionLead}>
          <strong>Core</strong> gives the agent rules and links to the context its task needs. <strong>Memory</strong> keeps useful outcomes for later, separating temporary notes,
          unconfirmed findings, accepted knowledge, and history. Startup reads the loader, the installed folder indexes tagged to load, and the files their tags or rules require.
        </p>
        <details className={styles.frameworkDetails}>
          <summary>Show how the files fit together</summary>
          <FrameworkMap />
        </details>
        <Link to="/docs/concepts">Read the concepts →</Link>
      </div>
    </section>
  );
}
