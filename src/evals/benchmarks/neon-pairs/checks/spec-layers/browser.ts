import { chromium, type Browser, type Page } from "playwright";
import { BehaviorFailure, Evidence } from "./outcomes.ts";
import type { Definition } from "./catalog.ts";
import { Method, LayerId as Layer } from "./requirements.ts";
import { session, type Session } from "./browser-session.ts";
import { clock, sizes, turns } from "./browser-play.ts";
import { best, invalidPreferences, preferences, stale, storageFailure } from "./browser-storage.ts";
import { activation, announcements, controlNames, navigation } from "./browser-input.ts";
import { appearance, faces, layout } from "./browser-view.ts";
import { accessibleNames, motion, scroll, zoom } from "./browser-access.ts";
import { boardControls, knownDeals, snapshotCases } from "./browser-formats.ts";
import { importedProgress, invalidLoads, loadedFocusAndSize } from "./browser-import.ts";
import { svgGeometry } from "./browser-geometry.ts";
import { sourceRules } from "./browser-source-rules.ts";
import { snapshotCaps, visibleTimeCap } from "./browser-caps.ts";
import { CapClause } from "./cap-policy.ts";
import { CheckArea, CheckId } from "../check-ids.ts";
import { CheckStatus } from "../../../../history/statuses.ts";

interface Journey { readonly groups: readonly string[]; readonly action: (page: Page, evidence: Evidence) => Promise<void>; readonly blockedStorage?: boolean; readonly file?: boolean }
const journeys: readonly Journey[] = [
  { groups: ["sizes"], action: sizes }, { groups: ["selection", "mismatch", "moves", "completion", "matched"], action: turns },
  { groups: ["clock"], action: clock }, { groups: ["preferences", "reset"], action: preferences }, { groups: ["best"], action: best },
  { groups: ["clock"], action: visibleTimeCap },
  { groups: ["stale"], action: stale }, { groups: ["storage"], action: invalidPreferences },
  { groups: ["storage", "offline"], action: storageFailure, blockedStorage: true },
  { groups: ["controls"], action: controlNames }, { groups: ["focus", "navigation"], action: navigation },
  { groups: ["activation"], action: activation }, { groups: ["announcements"], action: announcements },
  { groups: ["layout"], action: layout }, { groups: ["native", "colors", "typography", "outline", "motion", "offline"], action: appearance },
  { groups: ["faces"], action: faces }, { groups: ["names", "faces"], action: accessibleNames },
  { groups: ["scroll", "large"], action: scroll }, { groups: ["motion"], action: motion }, { groups: ["zoom", "offline"], action: zoom },
];
const specJourneys = [boardControls, snapshotCases, knownDeals, invalidLoads, importedProgress, loadedFocusAndSize, svgGeometry, sourceRules, snapshotCaps];

export async function browserChecks(html: string, definition: Definition, evidence: Evidence, launch: () => Promise<Browser> = () => chromium.launch()): Promise<void> {
  evidence.inherited.set(CheckId.facesDistinct, { id: CheckId.facesDistinct, area: CheckArea.faces, status: CheckStatus.error, evidence: "The later-layer checker did not finish its individual face captures." });
  const browser = await launch();
  const failures: string[] = [], audio: number[] = [];
  const stopped = new Map<string, { status: "error" | "blocked"; detail: string }>();
  try {
    for (const journey of journeys) {
      const clauses = definition.checks.filter((check) => journey.groups.some((group) => check.id === `l2-${group}`)).flatMap((check) => check.clauses);
      // Some journeys share a group. Complete missing assertions only after all journeys have run.
      let open: Session | undefined;
      const start = evidence.outcomes.length;
      try {
        open = await session(browser, html, journey);
        await journey.action(open.page, evidence);
        if (journey.action === faces && open.errors.length > 0) {
          const distinct = evidence.inherited.get(CheckId.facesDistinct);
          if (distinct) evidence.inherited.set(distinct.id, { ...distinct, status: CheckStatus.fail, evidence: `${distinct.evidence} Uncaught game error: ${open.errors.join(". ")}` });
        }
        audio.push(await open.page.evaluate(() => Number(Reflect.get(window, "__evalAudioCalls") ?? 0)));
        if (open.errors.length > 0) for (const outcome of evidence.outcomes.slice(start)) {
          if (outcome.status === "pass") {
            const index = evidence.outcomes.indexOf(outcome);
            evidence.outcomes[index] = { ...outcome, status: "fail", evidence: `Uncaught game error: ${open.errors.join(". ")}` };
          }
        }
      } catch (error) {
        const detail = `${journey.action.name}: ${error instanceof Error ? error.message : String(error)}`;
        failures.push(detail);
        const first = clauses.find((id) => !evidence.seen.has(id));
        if (error instanceof BehaviorFailure && first) evidence.test(first, false, detail);
        const earlier = evidence.outcomes.slice(start).find((item) => item.status !== "pass");
        for (const clause of clauses) stopped.set(clause, { status: earlier ? "blocked" : "error", detail: earlier ? `Stopped after ${earlier.clause}. ${detail}` : detail });
      } finally { await open?.close(); }
    }
    const file = await session(browser, html, { file: true });
    try {
      evidence.test("T08.2", file.requests.length === 0 && file.errors.length === 0, "Direct disk load requests only its main HTML file, with no uncaught errors.");
      evidence.test("T10.4", file.requests.length === 0, "Distribution makes no network requests when loaded from disk.");
    } finally { await file.close(); }
    if (definition.layer === Layer.three) for (const journey of specJourneys) {
      const open = await session(browser, html);
      const start = evidence.outcomes.length;
      try {
        await journey(open.page, evidence);
        if (open.errors.length > 0) for (const [index, outcome] of evidence.outcomes.entries()) if (index >= start && outcome.status === "pass") evidence.outcomes[index] = { ...outcome, status: "fail", evidence: `Uncaught game error: ${open.errors.join(". ")}` };
      }
      catch (error) { failures.push(`${journey.name}: ${error instanceof Error ? error.message : String(error)}`); }
      finally { await open.close(); }
    }
  } finally { await browser.close(); }
  evidence.test("S01.1", audio.length === journeys.length && audio.every((count) => count === 0), "Audio constructor and playback instrumentation stayed silent across reveal, match, miss, reset, restart and win journeys.");
  for (const clause of ["V18.1", "V18.2", "V18.3", "V18.4"]) {
    const outcome = evidence.outcomes.find((item) => item.clause === clause);
    if (outcome) evidence.merge("R06.6", outcome.status, `${clause}: ${outcome.evidence}`);
  }
  const browserClauses = definition.checks.filter((check) => check.method === Method.browser).flatMap((check) => check.clauses);
  for (const clause of browserClauses.filter((id) => !evidence.seen.has(id))) evidence.record(clause, stopped.get(clause)?.status ?? "error", stopped.get(clause)?.detail ?? (failures.join(". ") || "The browser checker did not collect this clause."));
  if (definition.layer === Layer.three) for (const outcome of evidence.outcomes.filter((item) => /^(?:D05|D06)\.|^D07\.[1-5]$/.test(item.clause) || item.clause === CapClause.snapshotElapsed || item.clause === CapClause.snapshotMoves)) evidence.merge("C09.2", outcome.status, `${outcome.clause}: ${outcome.evidence}`);
}
