using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.References.Models.Inspection;

internal sealed record ReferencesLayerInspectionInput(
    SourceReadSession Session,
    SourceLogicalSource Source,
    SourceLayer Layer,
    ReferencesDirection Direction,
    ReferencesProvenance Provenance);

internal sealed record ReferencesLayerInspectionResult(
    ReferencesInspectionFacts? Inspection,
    bool Established,
    bool Blocked,
    bool Interrupted,
    bool Failed);

internal sealed record ReferencesLayerScanInput(
    SourceReadSession Session,
    SourceLogicalSource Source,
    ReferencesDirection Direction,
    ReferencesProvenance Provenance,
    SourceCatalogue Catalogue);
