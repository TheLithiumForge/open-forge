using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Shared;

internal static class RouteSharedText
{
    // @OpenForgeText route.shared.label.the-blocking-condition-is-not-defined
    internal static string LabelTheBlockingConditionIsNotDefined()
        => "the blocking condition is not defined";

    // @OpenForgeText route.shared.title.invalid-target
    internal static string TitleInvalidTarget()
        => "Invalid target";

    // @OpenForgeText route.shared.title.invalid-metadata
    internal static string TitleInvalidMetadata()
        => "Invalid metadata";

    // @OpenForgeText route.shared.title.invalid-template
    internal static string TitleInvalidTemplate()
        => "Invalid Template";

    // @OpenForgeText route.shared.title.identity-collision
    internal static string TitleIdentityCollision()
        => "Identity collision";

    // @OpenForgeText route.shared.title.metadata-is-unsafe
    internal static string TitleMetadataIsUnsafe()
        => "Metadata is unsafe";

    // @OpenForgeText route.shared.title.template-is-unsafe
    internal static string TitleTemplateIsUnsafe()
        => "Template is unsafe";

    // @OpenForgeText route.shared.title.inspection-is-incomplete
    internal static string TitleInspectionIsIncomplete()
        => "Inspection is incomplete";

    // @OpenForgeText route.shared.title.metadata-could-not-be-read
    internal static string TitleMetadataCouldNotBeRead()
        => "Metadata could not be read";

    // @OpenForgeText route.shared.title.template-could-not-be-read
    internal static string TitleTemplateCouldNotBeRead()
        => "Template could not be read";

    // @OpenForgeText route.shared.title.compatibility-entrypoint
    internal static string TitleCompatibilityEntrypoint()
        => "Compatibility entrypoint";

    // @OpenForgeText route.shared.title.invalid-source-reference
    internal static string TitleInvalidSourceReference()
        => "Invalid source reference";

    // @OpenForgeText route.shared.title.source-is-unsupported
    internal static string TitleSourceIsUnsupported()
        => "Source is unsupported";

    // @OpenForgeText route.shared.title.route-is-ambiguous
    internal static string TitleRouteIsAmbiguous()
        => "Route is ambiguous";

    // @OpenForgeText route.shared.label.matches-the-requested-source-reference
    internal static string LabelMatchesTheRequestedSourceReference()
        => "matches the requested source reference";

    // @OpenForgeText route.shared.title.parent
    internal static string TitleParent()
        => "Parent";

    // @OpenForgeText route.shared.label.files-scanned
    internal static string LabelFilesScanned()
        => "files scanned";

    // @OpenForgeText route.shared.label.the-framework
    internal static string LabelTheFramework()
        => "the Framework";

    // @OpenForgeText route.shared.title.category-is-unsafe
    internal static string TitleCategoryIsUnsafe()
        => "Category is unsafe";

    // @OpenForgeText route.shared.title.source-is-managed
    internal static string TitleSourceIsManaged()
        => "Source is managed";

    // @OpenForgeText route.shared.title.category-inventory-is-incomplete
    internal static string TitleCategoryInventoryIsIncomplete()
        => "Category inventory is incomplete";

    // @OpenForgeText route.shared.title.reference-scan-is-incomplete
    internal static string TitleReferenceScanIsIncomplete()
        => "Reference scan is incomplete";

    // @OpenForgeText route.shared.help.command-help
    internal static string HelpCommandHelp()
        => "Use open-forge route <command> --help for arguments, write policy, and examples.";

    // @OpenForgeText route.shared.help.heading.target
    internal static string HelpHeadingTarget()
        => "Target";

    // @OpenForgeText route.shared.help.heading.metadata
    internal static string HelpHeadingMetadata()
        => "Metadata";

    // @OpenForgeText route.shared.help.heading.template
    internal static string HelpHeadingTemplate()
        => "Template";

    // @OpenForgeText route.shared.help.heading.source
    internal static string HelpHeadingSource()
        => "Source";

    // @OpenForgeText route.shared.help.unmanaged-route-source
    internal static string HelpUnmanagedRouteSource()
        => "<source-reference> selects one ordinary unmanaged routed Markdown leaf by ID, base path, or overwrite path, or one complete unmanaged category by its recognized entrypoint path.";
}
