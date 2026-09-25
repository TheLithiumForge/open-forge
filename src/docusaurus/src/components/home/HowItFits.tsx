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
          Everything in the workspace answers one of two questions. <strong>Core</strong> says how work should be done. <strong>Memory</strong> keeps what's worth remembering, and
          how far to trust it. The loader connects them, so an agent reads a map instead of everything.
        </p>
        <FrameworkMap />
        <Link to="/docs/concepts">Read the concepts →</Link>
      </div>
    </section>
  );
}
