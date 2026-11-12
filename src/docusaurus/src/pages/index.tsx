import type { ReactNode } from "react";
import Layout from "@theme/Layout";
import Hero from "@site/src/components/home/Hero";
import HowItFits from "@site/src/components/home/HowItFits";
import StartingPoint from "@site/src/components/home/StartingPoint";
import Features from "@site/src/components/home/Features";
import Pillars from "@site/src/components/home/Pillars";
import SharingLevels from "@site/src/components/home/SharingLevels";
import BaseFramework from "@site/src/components/home/BaseFramework";

export default function Home(): ReactNode {
  return (
    <Layout
      title="Adaptive Context Engineering for AI agents"
      description="Open Forge gives your coding agent the rules, docs and decisions each task needs, and a place to keep what it learns for the next session."
    >
      <Hero />
      <main>
        <Pillars />
        <StartingPoint />
        <SharingLevels />
        <BaseFramework />
        <HowItFits />
        <Features />
      </main>
    </Layout>
  );
}
