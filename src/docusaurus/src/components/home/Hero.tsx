import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import styles from "./home.module.css";

const installCommand = "npm install -g @thelithiumforge/open-forge@beta";

export default function Hero(): ReactNode {
  return (
    <header className={styles.hero}>
      <div className={styles.heroGrid} aria-hidden="true" />
      <div className={styles.heroGlow} aria-hidden="true" />
      <div className="container">
        <p className={styles.eyebrow}>Adaptive Context Engineering</p>
        <Heading as="h1" className={styles.title}>
          Open <span className={styles.titleAccent}>Forge</span>
        </Heading>
        <p className={styles.lead}>An agent doesn't need to know everything. It needs to know where everything is.</p>
        <p className={styles.sublead}>A small Markdown framework for working with AI agents, built around your projects, your tools, and the way you like to work.</p>
        <div className={styles.actions}>
          <Link className="button button--primary button--lg" to="/docs/getting-started/installation">
            Get started
          </Link>
          <Link className="button button--secondary button--outline button--lg" to="/docs/extensions">
            Explore extensions
          </Link>
        </div>
        <pre className={styles.install} aria-label="Install command">
          <span className={styles.prompt}>$</span> {installCommand}
        </pre>
      </div>
    </header>
  );
}
