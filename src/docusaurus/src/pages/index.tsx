import type { ReactNode } from "react";
import Layout from "@theme/Layout";
import Hero from "@site/src/components/home/Hero";
import HowItFits from "@site/src/components/home/HowItFits";
import StartingPoint from "@site/src/components/home/StartingPoint";
import Features from "@site/src/components/home/Features";
import BaseFramework from "@site/src/components/home/BaseFramework";

export default function Home(): ReactNode {
  return (
    <Layout
      title="Adaptive Context Engineering for AI agents"
      description="Open Forge is a small Markdown framework that gives AI agents a map of your workspace instead of all of it at once."
    >
      <Hero />
      <main>
        <HowItFits />
        <StartingPoint />
        <Features />
        <BaseFramework />
      </main>
    </Layout>
  );
}
