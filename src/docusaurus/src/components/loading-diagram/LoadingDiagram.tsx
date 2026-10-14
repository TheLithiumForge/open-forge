import type { ReactNode } from "react";
import Link from "@docusaurus/Link";
import { diagramCaption, diagramLabel, diagramTitle, loadingSteps } from "./loading-diagram-data";
import styles from "./loading-diagram.module.css";

export default function LoadingDiagram(): ReactNode {
  return (
    <figure className={styles.diagram} aria-label={diagramLabel}>
      <p className={styles.title}>{diagramTitle}</p>
      <ol className={styles.steps} role="list">
        {loadingSteps.map((step, index) => (
          <li key={step.title} className={`${styles.step} ${step.gate ? styles.gate : ""}`}>
            <div className={styles.heading}>
              <span className={styles.number} aria-hidden="true">
                {index + 1}
              </span>
              <strong>{step.title}</strong>
            </div>
            <div className={styles.body}>
              <p>{step.text}</p>
              {step.note ? <p className={styles.note}>{step.note}</p> : null}
              {step.links?.map((link) => (
                <p key={link.href} className={styles.note}>
                  <Link to={link.href}>{link.label}</Link>
                </p>
              ))}
            </div>
          </li>
        ))}
      </ol>
      <figcaption className={styles.caption}>{diagramCaption}</figcaption>
    </figure>
  );
}
