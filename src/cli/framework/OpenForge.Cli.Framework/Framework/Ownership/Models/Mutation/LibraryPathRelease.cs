namespace OpenForge.Cli.Core.Framework.Ownership.Models.Mutation;

// The caller establishes the mapped destination and guarded link effect. The
// ownership store consumes the resulting identity without reinterpreting it.
internal sealed record LibraryPathRelease(string LibraryId, string SourceRelativePath);
