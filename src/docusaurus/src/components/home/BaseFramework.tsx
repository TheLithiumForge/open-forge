import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import CodeBlock from "@theme/CodeBlock";
import Tabs from "@theme/Tabs";
import TabItem from "@theme/TabItem";
import styles from "./home.module.css";

interface Setup {
  readonly value: string;
  readonly label: string;
  readonly summary: ReactNode;
  readonly command: string;
  readonly tree: string;
}

const hosts = `AGENTS.md                     <- read the loader first
CLAUDE.md                     <- bridge for CLAUDE.md tools`;

const minimalFiles = `  loader.md                   <- loading and routing rules
  README.md                   <- a guide and the file formats
  directives/_directives.md   <- required behavior`;

const skillsFiles = `  skills/_skills.md           <- native SKILL.md capabilities
  skills/open-forge-cli/
    SKILL.md                  <- how to use the CLI
    references/               <- opened when needed
      discovery.md
      routes.md
      packages.md
      common.md`;

const setups: readonly Setup[] = [
  {
    value: "minimal",
    label: "Minimal",
    summary: "7 Markdown files. Rules and accepted knowledge, plus the documents you already have. The recommended start.",
    command: "open-forge install --preset minimal",
    tree: `${hosts}
.agents/
${minimalFiles}
  memory/
    _memory.md                <- what is worth remembering
    crystallized/_crystallized.md
                              <- accepted knowledge`,
  },
  {
    value: "essentials",
    label: "Essentials",
    summary: "16 Markdown files. Minimal plus Patterns, Skills with the CLI guide, and Working and Emerging Memory. New Working records stay out of Git.",
    command: "open-forge install --preset essentials",
    tree: `${hosts}
.agents/
${minimalFiles}
  patterns/_patterns.md       <- reusable shapes
${skillsFiles}
  memory/
    _memory.md                <- what is worth remembering
    working/_working.md       <- records kept out of Git
    emerging/_emerging.md     <- useful, not settled yet
    crystallized/_crystallized.md
                              <- accepted knowledge`,
  },
  {
    value: "full-core",
    label: "Full Core",
    summary: "20 Markdown files. Every built-in category and Memory state, with no Install-owned Git-ignore entries.",
    command: "open-forge install --preset full-core",
    tree: `${hosts}
.agents/
${minimalFiles}
  guidance/_guidance.md       <- advice for recurring choices
  patterns/_patterns.md       <- reusable shapes
${skillsFiles}
  templates/_templates.md     <- copy-ready starting files
  maps/_maps.md               <- pointers to important sources
  memory/
    _memory.md                <- what is worth remembering
    working/_working.md       <- temporary state
    emerging/_emerging.md     <- useful, not settled yet
    crystallized/_crystallized.md
                              <- accepted knowledge
    archived/_archived.md     <- history`,
  },
  {
    value: "custom",
    label: "Custom",
    summary: <>Pick each row: add it, add it and keep its records out of Git, or leave it out. This example starts from Minimal and adds Guidance and private Working records.</>,
    command: "open-forge install --preset minimal \\\n  --route guidance=add --route memory/working=git-ignore",
    tree: `${hosts}
.agents/
${minimalFiles}
  guidance/_guidance.md       <- added with guidance=add
  memory/
    _memory.md                <- what is worth remembering
    working/_working.md       <- records kept out of Git
    crystallized/_crystallized.md
                              <- accepted knowledge`,
  },
  {
    value: "your-docs",
    label: "With your docs",
    summary: <>Any preset, plus a folder your team writes in. Folder Include registers it as an outside route, and the CLI keeps its catalogue current.</>,
    command: "open-forge route include docs/team",
    tree: `${hosts}
.agents/                      <- main root, any preset
  loader.md                   <- lists docs/team/_team.md
  ...
docs/
  team/                       <- outside route: your own docs
    _team.md                  <- its catalogue
    testing.md                <- written by your team`,
  },
];

export default function BaseFramework(): ReactNode {
  return (
    <section className={`${styles.section} ${styles.sectionAlt}`}>
      <div className={`container ${styles.split} ${styles.splitTop}`}>
        <div>
          <Heading as="h2" className={styles.sectionTitle}>
            What you get
          </Heading>
          <p>
            Each preset includes the one before it, and each is a useful place to stop. Pick one to see the files it installs. Apart from Memory's state entrypoints and the{" "}
            <code>open-forge-cli</code> Skill and guides, every category ships empty. The content comes from your work, or from Extensions you choose: a correction you keep
            repeating, a decision you don't want to explain again, a workflow worth reusing.
          </p>
          <dl className={styles.stats}>
            <div className={styles.stat}>
              <dt>20</dt>
              <dd>Markdown files in Full Core</dd>
            </div>
            <div className={styles.stat}>
              <dt>0</dt>
              <dd>runtimes or databases to run</dd>
            </div>
          </dl>
          <p>
            <Link to="/guides/cli#status">
              Estimate your setup with <code>open-forge status</code>
            </Link>
            . The estimate covers tag-selected context. Required reads can add to it.
          </p>
          <p className={styles.pathLinks}>
            <Link to="/docs/getting-started/installation#choose-the-installed-routes">Compare the setups →</Link>
            <Link to="/docs/getting-started/grow-your-framework">Grow your own framework →</Link>
          </p>
        </div>
        <div className={styles.setupTabs}>
          <Tabs defaultValue="minimal">
            {setups.map((setup) => (
              <TabItem key={setup.value} value={setup.value} label={setup.label}>
                <p className={styles.setupSummary}>{setup.summary}</p>
                <CodeBlock language="sh">{setup.command}</CodeBlock>
                <CodeBlock language="text" title={`${setup.label} layout`}>
                  {setup.tree}
                </CodeBlock>
              </TabItem>
            ))}
          </Tabs>
        </div>
      </div>
    </section>
  );
}
