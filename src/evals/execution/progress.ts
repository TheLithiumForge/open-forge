import { EventEmitter } from "node:events";

export interface ProgressMessage {
  readonly at: string;
  readonly batchId: string;
  readonly text: string;
}

const KeptMessages = 300;
const ProgressEvent = "progress";

/** Progress for the UI. Messages never name a run's setup, so blind judging stays blind. */
class Progress extends EventEmitter {
  private readonly recent: ProgressMessage[] = [];

  report(batchId: string, text: string): void {
    const message = { at: new Date().toISOString(), batchId, text };
    this.recent.push(message);
    if (this.recent.length > KeptMessages) this.recent.shift();
    this.emit(ProgressEvent, message);
  }

  history(batchId: string): ProgressMessage[] {
    return this.recent.filter((message) => message.batchId === batchId);
  }

  subscribe(listener: (message: ProgressMessage) => void): () => void {
    this.on(ProgressEvent, listener);
    return () => this.off(ProgressEvent, listener);
  }
}

export const progress = new Progress();
