using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Presentation.Extension.Inspect;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using TheLithium.Imprint;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Inspect.Shared.Rendering;

[Trait("Feature", "state-retirement-output"), Trait("Evidence", "Unit")]
public sealed class ExtensionInspectOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect minimal text comparison matches its reviewed snapshot")]
    public void MinimalText() => Render(CliFormat.Text, CliDetail.Minimal).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect standard text comparison matches its reviewed snapshot")]
    public void StandardText() => Render(CliFormat.Text, CliDetail.Standard).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect minimal JSON comparison matches its reviewed snapshot")]
    public void MinimalJson() => Render(CliFormat.Json, CliDetail.Minimal).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect standard JSON comparison matches its reviewed snapshot")]
    public void StandardJson() => Render(CliFormat.Json, CliDetail.Standard).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect generated boundaries minimal text match the reviewed snapshot")]
    public void GeneratedMinimalText() => Render(CliFormat.Text, CliDetail.Minimal, true).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect generated boundaries standard text match the reviewed snapshot")]
    public void GeneratedStandardText() => Render(CliFormat.Text, CliDetail.Standard, true).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect generated boundaries minimal JSON match the reviewed snapshot")]
    public void GeneratedMinimalJson() => Render(CliFormat.Json, CliDetail.Minimal, true).AssertSnapshot();

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Inspect generated boundaries standard JSON match the reviewed snapshot")]
    public void GeneratedStandardJson() => Render(CliFormat.Json, CliDetail.Standard, true).AssertSnapshot();

    private static string Render(CliFormat format, CliDetail view, bool includeGenerated = false)
    {
        var generated = new ExtensionInspectGenerated { State = ExtensionInspectGeneratedState.Complete, Ownership = "derived-navigation-only", Regions = [] };
        if (includeGenerated)
        {
            var reader = new MarkdownFingerprintReader();
            var facts = new Dictionary<string, MarkdownFingerprintFacts>
            {
                [".agents/changed.md"] = reader.Read(Encoding.UTF8.GetBytes("# Changes\n\n## Entries\n\n- none - No entries - #Empty\n")),
                [".agents/current.md"] = reader.Read(Encoding.UTF8.GetBytes("# Current\n")),
            };
            generated = ExtensionInspectGeneratedBuilder.Build(facts, facts, ExtensionInspectPathState.Complete, new List<ExtensionInspectFinding>());
        }
        var comparisons = new[]
        {
            Comparison(".agents/changed.md", 'a', 'b', ExtensionInspectPathRelation.Changed),
            Comparison(".agents/current.md", 'b', 'b', ExtensionInspectPathRelation.Unchanged),
            Comparison(".agents/retired.md", 'a', null, ExtensionInspectPathRelation.Retired),
            Comparison(".agents/unchanged.md", 'a', 'a', ExtensionInspectPathRelation.Unchanged),
        };
        var findings = new[]
        {
            Finding(ExtensionInspectFindingCode.PathChanged, ".agents/changed.md", "The current workspace differs from the selected package."),
            Finding(ExtensionInspectFindingCode.PathRetired, ".agents/retired.md", "The intended source no longer contains a previously managed path."),
        };
        var seed = ExtensionInspectResultBuilder.InvalidStableId(null, "toolkit", "Unused seed finding.");
        var result = seed with
        {
            Status = CliSemanticStatus.Attention,
            Findings = findings,
            Next = null,
            Subject = seed.Subject with { State = ExtensionInspectSubjectState.Resolved, Id = "toolkit", Form = ExtensionInspectSubjectForm.StableId },
            Source = seed.Source with { State = ExtensionInspectSourceState.Available, Identity = "embedded catalogue", Kind = ExtensionInspectSourceKind.EmbeddedCatalogue },
            Lifecycle = new()
            {
                DocumentPath = ".agents/open-forge.lock.json",
                ReadState = ExtensionInspectLifecycleReadState.Complete,
                Trust = ExtensionInspectLifecycleTrust.Trusted,
                Coverage = ExtensionInspectCoverageState.Complete,
            },
            Installed = new()
            {
                State = ExtensionInspectInstalledState.Present,
                Package = new() { Id = "toolkit", Version = "1.0.0", Source = "embedded catalogue", Dependencies = [], Paths = comparisons.Select(value => value.Path).ToArray() },
            },
            Available = new()
            {
                State = ExtensionInspectAvailableState.Present,
                Package = new()
                {
                    Id = "toolkit",
                    Name = "Toolkit",
                    Description = "Review helpers.",
                    Version = "2.0.0",
                    ManifestPath = "toolkit/extension.json",
                    Dependencies = [],
                    Payload = comparisons.Where(value => value.Intended is not null).Select(value => new ExtensionInspectPackageFile
                    {
                        Path = value.Path,
                        TargetPath = value.Path,
                        State = ExtensionInspectPackageFileState.Available,
                        ByteLength = 20,
                        Sha256 = value.Intended!.Sha256,
                    }).ToArray(),
                },
            },
            Dependencies = new()
            {
                State = ExtensionInspectDependencyState.Complete,
                Declared = [],
                Resolved = [new() { Id = "toolkit", Version = "2.0.0", Source = "embedded catalogue", State = ExtensionInspectDependencyPackageState.Available }],
                Order = ["toolkit"],
            },
            PathFacts = new()
            {
                State = ExtensionInspectPathState.Complete,
                Declared = comparisons.Where(value => value.Intended is not null).Select(value => new ExtensionInspectDeclaredPath
                {
                    Path = value.Path,
                    SourcePath = value.Path,
                    State = ExtensionInspectDeclaredPathState.Available,
                }).ToArray(),
                Current = comparisons.Select(value => new ExtensionInspectCurrentPath
                {
                    Path = value.Path,
                    State = ExtensionInspectCurrentPathState.Present,
                    PhysicalIdentity = null,
                    ByteLength = 20,
                    ExactSha256 = value.Current!.Sha256,
                }).ToArray(),
            },
            Comparison = new()
            {
                State = ExtensionInspectComparisonState.Complete,
                Mode = ExtensionInspectComparisonMode.InstalledAndAvailable,
                Current = Side(comparisons.Select(value => new ExtensionInspectFingerprintFact { Path = value.Path, Fingerprint = value.Current }).ToArray()),
                Intended = Side(comparisons.Where(value => value.Intended is not null).Select(value => new ExtensionInspectFingerprintFact { Path = value.Path, Fingerprint = value.Intended }).ToArray()),
                Paths = comparisons,
                Dependencies = new() { State = ExtensionInspectDependencyComparisonState.Available, Current = [], Intended = [], Relation = ExtensionInspectDependencyRelation.Equal },
            },
            Generated = generated,
            Counts = seed.Counts with
            {
                InstalledPackages = 1,
                AvailablePackages = 1,
                DeclaredPaths = 3,
                CurrentPaths = 4,
                IntendedPaths = 3,
                Dependencies = 1,
                UnchangedPaths = 2,
                ChangedPaths = 1,
                MissingPaths = 0,
                NewPaths = 0,
                RetiredPaths = 1,
                SharedPaths = 0,
                GeneratedRegions = generated.Regions.Count,
                ExcludedGeneratedBytes = generated.Regions.Sum(region => region.ExcludedInteriorByteLength ?? 0),
                Findings = 3,
            },
        };
        var rendering = ExtensionInspectPresentation.Rendering;
        var selected = CliReportSelection.Select(result, new CliSelection(view), rendering);
        return format == CliFormat.Json
            ? CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo)
            : CliTextRenderer.Render(selected, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
    }

    private static ExtensionInspectComparisonSide Side(ExtensionInspectFingerprintFact[] values)
        => new() { State = ExtensionInspectComparisonSideState.Available, Fingerprints = values };

    private static ExtensionInspectPathComparison Comparison(string path, char current, char? intended, ExtensionInspectPathRelation relation)
        => new()
        {
            Path = path,
            Current = Fingerprint(current, ExtensionInspectFingerprintOrigin.OperationTimeCurrent),
            Intended = intended is { } value ? Fingerprint(value, ExtensionInspectFingerprintOrigin.OperationTimeIntended) : null,
            Relation = relation,
            CurrentOwners = ["toolkit"],
            IntendedOwners = intended is null ? [] : ["toolkit"],
        };

    private static ExtensionInspectFingerprint Fingerprint(char value, ExtensionInspectFingerprintOrigin origin)
        => new() { Kind = ExtensionInspectFingerprintKind.Semantic, Policy = "open-forge-markdown-v1", Sha256 = new string(value, 64), Origin = origin };

    private static ExtensionInspectFinding Finding(ExtensionInspectFindingCode code, string path, string cause)
        => new() { Code = code, Status = CliSemanticStatus.Attention, Subject = null, PackageId = null, Dependency = null, Path = path, Cause = cause, Location = null, Candidates = [] };
}
