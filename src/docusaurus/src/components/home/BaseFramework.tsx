import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import CodeBlock from "@theme/CodeBlock";
import styles from "./home.module.css";

const installedTree = `AGENTS.md                     <- read the loader first
CLAUDE.md                     <- bridge for CLAUDE.md tools
.agents/
  loader.md                   <- loading and routing rules
  README.md                   <- a guide and the file formats
  directives/_directives.md   <- required behavior
  guidance/_guidance.md       <- advice for recurring choices
  patterns/_patterns.md       <- reusable shapes
  skills/_skills.md           <- native SKILL.md capabilities
  skills/open-forge-cli/
    SKILL.md                  <- how to use the CLI
    references/               <- opened when needed
      discovery.md
      routes.md
      packages.md
      common.md
  templates/_templates.md     <- copy-ready starting files
  maps/_maps.md               <- pointers to important sources
  memory/                     <- working, emerging,
                                 crystallized, archived`;

const stats = [
  { value: "20", label: "Markdown files in Full Core" },
  { value: "0", label: "runtimes or databases to run" },
] as const;

export default function BaseFramework(): ReactNode {
  return (
    <section className={`${styles.section} ${styles.sectionAlt}`}>
      <div className={`container ${styles.split}`}>
        <div>
          <Heading as="h2" className={styles.sectionTitle}>
            What you get
          </Heading>
          <p>
            Full Core supplies the files shown here, including four supporting CLI guides opened when needed. Essentials installs fewer categories, and Custom follows your
            selection. Apart from Memory's state entrypoints and the <code>open-forge-cli</code> Skill and guides, every category ships empty. The content comes from your work, or
            from Extensions you choose: a correction you keep repeating, a decision you don't want to explain again, a workflow worth reusing.
          </p>
          <dl className={styles.stats}>
            {stats.map((stat) => (
              <div key={stat.label} className={styles.stat}>
                <dt>{stat.value}</dt>
                <dd>{stat.label}</dd>
              </div>
            ))}
          </dl>
          <p>
            <Link to="/guides/cli#status">
              Estimate your setup with <code>open-forge status</code>
            </Link>
            . The estimate covers tag-selected context. Required reads can add to it.
          </p>
          <Link to="/docs/getting-started/grow-your-framework">Grow your own framework →</Link>
        </div>
        <CodeBlock language="text" title="What you get">
          {installedTree}
        </CodeBlock>
      </div>
    </section>
  );
}
