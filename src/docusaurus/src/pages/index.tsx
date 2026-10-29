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
      description="Open Forge is a Markdown framework that tells coding agents which of your project's rules, docs and decisions to read for each task, and keeps what they learn for the next one."
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
