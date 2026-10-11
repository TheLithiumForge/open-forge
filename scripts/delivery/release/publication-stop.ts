export const PublicationSignal = { interrupt: "SIGINT", terminate: "SIGTERM" } as const;

export function publicationStop(): { requested: () => boolean; dispose: () => void } {
  let stopped = false;
  const stop = () => {
    stopped = true;
    process.stderr.write("Cancellation requested. No further upload will start. Waiting for the current upload to finish or fail.\n");
  };
  process.on(PublicationSignal.interrupt, stop);
  process.on(PublicationSignal.terminate, stop);
  return {
    requested: () => stopped,
    dispose: () => {
      process.off(PublicationSignal.interrupt, stop);
      process.off(PublicationSignal.terminate, stop);
    },
  };
}
