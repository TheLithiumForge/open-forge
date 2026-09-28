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
          The base Framework has two parts. <strong>Core</strong> defines how work is done. <strong>Memory</strong> keeps what's worth remembering, sorted by how far it can be
          trusted. At startup the rules have an agent read the loader and the entrypoints of most categories, plus any entries those entrypoints mark to load. Everything else opens
          only when the task needs it. Optional Extensions add categories and files inside both parts.
        </p>
        <FrameworkMap />
        <Link to="/docs/concepts">Read the concepts →</Link>
      </div>
    </section>
  );
}
