import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import CodeBlock from "@theme/CodeBlock";
import styles from "./home.module.css";

const installedTree = `AGENTS.md                     <- read the loader first
CLAUDE.md                     <- bridge for CLAUDE.md tools
.agents/
  loader.md                   <- loading and routing rules
  directives/_directives.md   <- required behavior
  guidance/_guidance.md       <- advice for recurring choices
  patterns/_patterns.md       <- reusable shapes
  skills/_skills.md           <- native SKILL.md capabilities
  skills/open-forge-cli/      <- how to use the CLI
  templates/_templates.md     <- copy-ready starting files
  maps/_maps.md               <- pointers to important sources
  memory/                     <- working, emerging,
                                 crystallized, archived`;

const stats = [
  { value: "15", label: "Markdown files in the base" },
  { value: "~5.7k", label: "tokens loaded at startup" },
  { value: "0", label: "runtimes or databases to run" },
] as const;

export default function BaseFramework(): ReactNode {
  return (
    <section className={`${styles.section} ${styles.sectionAlt}`}>
      <div className={`container ${styles.split}`}>
        <div>
          <Heading as="h2" className={styles.sectionTitle}>
            Small on purpose
          </Heading>
          <p>
            The base is a set of loading rules and a structure that scales. Apart from Memory's four states and the <code>open-forge-cli</code> Skill, every category ships empty.
            The content comes from your work, or from Extensions you choose: a correction you keep repeating, a decision you don't want to explain again, a workflow worth reusing.
          </p>
          <dl className={styles.stats}>
            {stats.map((stat) => (
              <div key={stat.label} className={styles.stat}>
                <dt>{stat.value}</dt>
                <dd>{stat.label}</dd>
              </div>
            ))}
          </dl>
          <Link to="/docs/getting-started/grow-your-framework">Grow your own framework →</Link>
        </div>
        <CodeBlock language="text" title="What you get">
          {installedTree}
        </CodeBlock>
      </div>
    </section>
  );
}
