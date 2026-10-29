import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import ThemedImage from "@theme/ThemedImage";
import useBaseUrl from "@docusaurus/useBaseUrl";
import styles from "./home.module.css";

interface Level {
  readonly name: string;
  readonly reach: string;
  readonly body: string;
}

const levels: readonly Level[] = [
  { name: "Workspace", reach: "One repo", body: "The main root for agent context, and outside routes for your team's own docs." },
  { name: "Library", reach: "Several repos", body: "One shared folder, such as a Git submodule, linked into place instead of copied." },
  { name: "Extension", reach: "People, teams, projects, orgs", body: "Packages of files that any project installs and then adapts." },
];

const alt =
  "Three levels of sharing, drawn as rings. The inner ring is one repo, your workspace: the .agents context folder for agent context and outside routes such as docs for your own documents. The middle ring covers several repos through Libraries: a shared folder in the workspace, such as a Git submodule, linked into place. The outer ring covers people, teams, projects and organizations through Extensions: packages you install in any project.";

export default function SharingLevels(): ReactNode {
  return (
    <section className={`${styles.section} ${styles.sectionAlt}`}>
      <div className={`container ${styles.split}`}>
        <ThemedImage
          alt={alt}
          className={styles.levelsImage}
          sources={{ light: useBaseUrl("/img/integration-levels-light.svg"), dark: useBaseUrl("/img/integration-levels-dark.svg") }}
        />
        <div>
          <Heading as="h2" className={styles.sectionTitle}>
            Share at your scale
          </Heading>
          <p className={styles.sectionLead}>Open Forge shares context at three levels. Each one reaches further than the one inside it, and you can stop at any of them.</p>
          <dl className={styles.levels}>
            {levels.map((level) => (
              <div key={level.name} className={styles.level}>
                <dt>
                  {level.name} <span>{level.reach}</span>
                </dt>
                <dd>{level.body}</dd>
              </div>
            ))}
          </dl>
          <Link to="/docs/getting-started/share-with-your-team">Share with your team →</Link>
        </div>
      </div>
    </section>
  );
}
