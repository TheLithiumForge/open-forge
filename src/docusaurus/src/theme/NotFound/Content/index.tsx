import type { ReactNode } from "react";
import clsx from "clsx";
import Link from "@docusaurus/Link";
import Heading from "@theme/Heading";
import type { Props } from "@theme/NotFound/Content";

export default function NotFoundContent({ className }: Props): ReactNode {
  return (
    <main className={clsx("container margin-vert--xl", className)}>
      <div className="row">
        <div className="col col--6 col--offset-3">
          <Heading as="h1" className="hero__title">
            Page not found
          </Heading>
          <p>We could not find this page. Continue with one of these guides:</p>
          <ul>
            <li>
              <Link to="/docs">Introduction</Link>
            </li>
            <li>
              <Link to="/docs/getting-started/installation">Installation</Link>
            </li>
            <li>
              <Link to="/guides/cli">CLI reference</Link>
            </li>
          </ul>
        </div>
      </div>
    </main>
  );
}
