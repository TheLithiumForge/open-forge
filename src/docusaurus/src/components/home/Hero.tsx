import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import styles from "./home.module.css";

const installCommands = `npm install -g @thelithiumforge/open-forge@beta
cd /path/to/your-project
open-forge install`;

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
          <Link className="button button--secondary button--outline button--lg" to="/docs/demos">
            Try a demo
          </Link>
        </div>
        <div className={styles.tryIt}>
          <Heading as="h2" className={styles.cardTitle}>
            Try it
          </Heading>
          <p>With Node.js 22.18 or later</p>
          <pre className={styles.install} aria-label="Install the CLI and set up your project">
            <code>{installCommands}</code>
          </pre>
          <p>Then ask your agent for a normal task.</p>
          <blockquote className={styles.examplePrompt}>
            Explain how this project runs its tests. Follow <code>AGENTS.md</code>, and name the Open Forge files you read.
          </blockquote>
        </div>
      </div>
    </header>
  );
}
