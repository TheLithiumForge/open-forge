using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Reading;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Presentation.Context.Models;
using OpenForge.Cli.Core.Presentation.Find.Models;
using OpenForge.Cli.Core.Presentation.References.Models;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.List.Models;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.Core.UnitTests.Contracts;

public sealed class PublishedTokensTests
{
    [Fact(DisplayName = "Published-process tokens match their production owners"), Trait("Feature", "public-contract"), Trait("Evidence", "Unit")]
    public void EveryTokenMatchesItsOwner()
    {
#pragma warning disable xUnit2000 // The projected constants are under test, and the production owners supply the expected values.
        var install = InstallSymbols.Create();
        Assert.Equal(install.Configure.Name, PublishedTokens.Install.Configure);
        Assert.Equal(install.Preset.Name, PublishedTokens.Install.Preset);
        Assert.Equal(InstallConfigurationChoices.Name(InstallPreset.Custom), PublishedTokens.Install.CustomPreset);
        Assert.Equal(UpdateDefinitions.UpdateCommand.Name, PublishedTokens.Update.Command);
        Assert.Equal(UpdateDefinitions.Automatic.Name, PublishedTokens.Update.Automatic);
        Assert.Equal(RouteIncludeDefinitions.DryRun.Name, PublishedTokens.RouteInclude.DryRun);
        Assert.Equal(RouteIncludeDefinitions.Tag.Name, PublishedTokens.RouteInclude.Tag);
        Assert.Equal(RouteIncludeDefinitions.ApplyTo.Name, PublishedTokens.RouteInclude.ApplyTo);
        Assert.Equal(RouteExcludeDefinitions.DryRun.Name, PublishedTokens.RouteExclude.DryRun);
        Assert.Equal(CliSyntaxDefinitions.Format.FiniteSpellings.Single(pair => pair.Value == CliFormat.Text).Key, PublishedTokens.Syntax.TextFormat);
        Assert.Equal(WorkspaceSettingsDefinitions.RootsProperty, PublishedTokens.Settings.RootsProperty);
        Assert.Equal(CliJsonFields.Effects, PublishedTokens.Json.Effects);
        Assert.Equal(CliJsonFields.After, PublishedTokens.Json.After);
        Assert.Equal(InstallDefinitions.InstallCommand.Name, PublishedTokens.Install.Command);
        Assert.Equal(InstallDefinitions.Automatic.Name, PublishedTokens.Install.Automatic);
        Assert.Equal(InstallDefinitions.DryRun.Name, PublishedTokens.Install.DryRun);
        Assert.Equal(InstallDefinitions.Frontmatter.Name, PublishedTokens.Install.Frontmatter);
        Assert.Equal(InstallDefinitions.ReadMachineName(InstallFindingCode.WorkspaceUnsafe), PublishedTokens.Install.WorkspaceUnsafeCode);

        Assert.Equal(WorkspaceLayoutNames.LoaderFileName, PublishedTokens.Layout.LoaderFileName);
        Assert.Equal(WorkspaceLayout.NativeSkillsPath, PublishedTokens.Layout.NativeSkillsPath);
        Assert.Equal(SourceFormClassifier.NativeSkillFileName, PublishedTokens.Layout.NativeSkillFileName);
        Assert.Equal(SourceFormClassifier.UnderscoreReferencesFileName, PublishedTokens.Layout.UnderscoreReferencesFileName);
        Assert.Equal(WorkspaceSettingsDefinitions.RelativePath, PublishedTokens.Settings.RelativePath);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, PublishedTokens.Settings.SchemaVersion);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersionProperty, PublishedTokens.Settings.SchemaVersionProperty);
        Assert.Equal(WorkspaceSettingsDefinitions.MainRootProperty, PublishedTokens.Settings.MainRootProperty);
        Assert.Equal(WorkspaceSettingsDefinitions.FrontmatterProperty, PublishedTokens.Settings.FrontmatterProperty);
        Assert.Equal(WorkspaceSettingsDefinitions.FrontmatterRootName, PublishedTokens.Settings.RootFrontmatter);

        Assert.Equal(IndexDefinitions.IndexCommand.Name, PublishedTokens.Index.Command);
        Assert.Equal(IndexDefinitions.DryRun.Name, PublishedTokens.Index.DryRun);

        Assert.Equal(StatusDefinitions.StatusCommand.Name, PublishedTokens.Status.Command);

        Assert.Equal(DoctorDefinitions.DoctorCommand.Name, PublishedTokens.Doctor.Command);

        Assert.Equal(CleanupDefinitions.CleanupCommand.Name, PublishedTokens.Cleanup.Command);
        Assert.Equal(CleanupDefinitions.DryRun.Name, PublishedTokens.Cleanup.DryRun);

        Assert.Equal(RemoveDefinitions.CommandIdentity, PublishedTokens.Remove.Command);
        Assert.Equal(RemoveDefinitions.AutomaticOption, PublishedTokens.Remove.Automatic);

        Assert.Equal(RouteDefinitions.RouteGroup.Name, PublishedTokens.Route.Command);
        Assert.Equal(RouteInspectDefinitions.InspectCommand.Name, PublishedTokens.RouteInspect.Command);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteInspectData.Read)), PublishedTokens.RouteInspectData.Read);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteInspectDataRead.AtStart)), PublishedTokens.RouteInspectData.AtStart);

        Assert.Equal(RouteListDefinitions.ListCommand.Name, PublishedTokens.RouteList.Command);
        Assert.Equal(RouteListDepth.Default.FiniteValue, PublishedTokens.RouteList.DefaultDepth);
        Assert.Equal(RouteListDefinitions.CommandIdentity, PublishedTokens.RouteList.CommandIdentity);
        Assert.Equal(RouteListDefinitions.Depth.Name, PublishedTokens.RouteList.Depth);
        Assert.Equal(RouteListDefinitions.AllDepth, PublishedTokens.RouteList.AllDepth);
        Assert.Equal(RouteListDefinitions.ReadFindingCode(RouteListFindingCode.InvalidDepth), PublishedTokens.RouteList.InvalidDepthCode);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListData.Rows)), PublishedTokens.RouteListData.Rows);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListData.Depth)), PublishedTokens.RouteListData.Depth);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListPresentationRow.HasOverwrite)), PublishedTokens.RouteListData.HasOverwrite);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteListPresentationRow.AbsoluteDepth)), PublishedTokens.RouteListData.AbsoluteDepth);

        Assert.Equal(RouteIncludeDefinitions.Command.Name, PublishedTokens.RouteInclude.Command);
        Assert.Equal(RouteIncludeDefinitions.Description.Name, PublishedTokens.RouteInclude.Description);
        Assert.Equal(RouteIncludeDefinitions.RecoveryHistoryCode, PublishedTokens.RouteInclude.RecoveryHistoryCode);

        Assert.Equal(RouteExcludeDefinitions.Command.Name, PublishedTokens.RouteExclude.Command);

        Assert.Equal(RouteCreateDefinitions.DryRun.Name, PublishedTokens.RouteCreate.DryRun);
        Assert.Equal(RouteCreateDefinitions.CreateCommand.Name, PublishedTokens.RouteCreate.Command);
        Assert.Equal(RouteCreateDefinitions.Description.Name, PublishedTokens.RouteCreate.Description);
        Assert.Equal(RouteCreateDefinitions.Tag.Name, PublishedTokens.RouteCreate.Tag);

        Assert.Equal(RouteUpdateDefinitions.UpdateCommand.Name, PublishedTokens.RouteUpdate.Command);
        Assert.Equal(RouteUpdateDefinitions.Description.Name, PublishedTokens.RouteUpdate.Description);

        Assert.Equal(RouteMoveDefinitions.MoveCommand.Name, PublishedTokens.RouteMove.Command);

        Assert.Equal(RouteRemoveDefinitions.RemoveCommand.Name, PublishedTokens.RouteRemove.Command);
        Assert.Equal(RouteRemoveDefinitions.Automatic.Name, PublishedTokens.RouteRemove.Automatic);

        Assert.Equal(RouteInitDefinitions.InitCommand.Name, PublishedTokens.RouteInit.Command);
        Assert.Equal(RouteInitDefinitions.Description.Name, PublishedTokens.RouteInit.Description);
        Assert.Equal(RouteInitDefinitions.Tag.Name, PublishedTokens.RouteInit.Tag);

        Assert.Equal(ExtensionDefinitions.ExtensionGroup.Name, PublishedTokens.Extension.Command);

        Assert.Equal(ExtensionInstallDefinitions.InstallCommand.Name, PublishedTokens.ExtensionInstall.Command);
        Assert.Equal(ExtensionInstallDefinitions.Automatic.Name, PublishedTokens.ExtensionInstall.Automatic);
        Assert.Equal(ExtensionInstallDefinitions.Source.Name, PublishedTokens.ExtensionInstall.Source);

        Assert.Equal(ExtensionUpdateDefinitions.UpdateCommand.Name, PublishedTokens.ExtensionUpdate.Command);
        Assert.Equal(ExtensionUpdateDefinitions.Automatic.Name, PublishedTokens.ExtensionUpdate.Automatic);
        Assert.Equal(ExtensionUpdateDefinitions.Source.Name, PublishedTokens.ExtensionUpdate.Source);

        Assert.Equal(ExtensionRemoveDefinitions.RemoveCommand.Name, PublishedTokens.ExtensionRemove.Command);
        Assert.Equal(ExtensionRemoveDefinitions.Automatic.Name, PublishedTokens.ExtensionRemove.Automatic);

        Assert.Equal(CliSyntaxDefinitions.Format.Name, PublishedTokens.Syntax.FormatOption);
        Assert.Equal(CliSyntaxDefinitions.Detail.Name, PublishedTokens.Syntax.DetailOption);
        Assert.Equal(CliSyntaxDefinitions.Format.FiniteSpellings.Single(pair => pair.Value == CliFormat.Json).Key, PublishedTokens.Syntax.JsonFormat);

        Assert.Equal(CliPresentationDefinitions.Full, PublishedTokens.Presentation.FullDetail);
        Assert.Equal(CliPresentationDefinitions.Minimal, PublishedTokens.Presentation.MinimalDetail);

        Assert.Equal(CurrentUserDataHome.OverrideVariable, PublishedTokens.DataHome.Variable);

        Assert.Equal(WorkspaceRuntimeBootstrap.IgnoreContent, PublishedTokens.Runtime.IgnoreContent);

        Assert.Equal(SourceLogicalPath.AgentsRoot, PublishedTokens.Sources.AgentsRoot);
        Assert.Equal(SourceLogicalPath.LoaderPath, PublishedTokens.Sources.LoaderPath);

        Assert.Equal(WorkspaceOwnershipDefinitions.RelativePath, PublishedTokens.Ownership.RelativePath);

        Assert.Equal(WorkspaceSettingsDefinitions.FrontmatterScopedName, PublishedTokens.Settings.ScopedFrontmatter);

        Assert.Equal(ExtensionPackageLayout.ContentDirectoryName, PublishedTokens.ExtensionPackage.ContentDirectory);

        Assert.Equal(RecoveryBundleFormatV1.FinalFilePrefix, PublishedTokens.RecoveryFormat.FinalFilePrefix);
        Assert.Equal(RecoveryBundleFormatV1.FinalFileExtension, PublishedTokens.RecoveryFormat.FinalFileExtension);
        Assert.Equal(RecoveryBundleFormatV1.DraftFileExtension, PublishedTokens.RecoveryFormat.DraftFileExtension);
        Assert.Equal(RecoveryBundleFormatV1.OperationIdFormat, PublishedTokens.RecoveryFormat.OperationIdFormat);

        Assert.Equal(RecoveryWireVocabulary.StoreKind(RecoveryStoreKind.Workspace), PublishedTokens.Recovery.WorkspaceStore);
        Assert.Equal(RecoveryWireVocabulary.StoreKind(RecoveryStoreKind.Legacy), PublishedTokens.Recovery.LegacyStore);
        Assert.Equal(RecoveryWireVocabulary.Integrity(RecoveryBundleIntegrity.Verified), PublishedTokens.Recovery.VerifiedIntegrity);

        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Complete).MachineName, PublishedTokens.Statuses.Complete);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Complete).Disposition.ExitCode, PublishedTokens.Statuses.CompleteExitCode);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Invalid).Disposition.ExitCode, PublishedTokens.Statuses.InvalidExitCode);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Invalid).MachineName, PublishedTokens.Statuses.Invalid);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Attention).MachineName, PublishedTokens.Statuses.Attention);
        Assert.Equal(CliStatusDefinitions.Read(CliSemanticStatus.Incomplete).MachineName, PublishedTokens.Statuses.Incomplete);

        Assert.Equal(CliReportVocabulary.Name(CliSeverity.Warning), PublishedTokens.Report.WarningSeverity);

        Assert.Equal(CliJsonFields.Data, PublishedTokens.Json.Data);
        Assert.Equal(CliJsonFields.Command, PublishedTokens.Json.Command);
        Assert.Equal(CliJsonFields.Status, PublishedTokens.Json.Status);
        Assert.Equal(CliJsonFields.Findings, PublishedTokens.Json.Findings);
        Assert.Equal(CliJsonFields.Code, PublishedTokens.Json.Code);
        Assert.Equal(CliJsonFields.Severity, PublishedTokens.Json.Severity);
        Assert.Equal(CliJsonFields.Subject, PublishedTokens.Json.Subject);
        Assert.Equal(CliJsonFields.Path, PublishedTokens.Json.Path);

        Assert.Equal(StatusRecoveryJsonFields.Recovery, PublishedTokens.StatusRecovery.Recovery);
        Assert.Equal(StatusRecoveryJsonFields.Stores, PublishedTokens.StatusRecovery.Stores);
        Assert.Equal(StatusRecoveryJsonFields.Candidates, PublishedTokens.StatusRecovery.Candidates);
        Assert.Equal(StatusRecoveryJsonFields.Kind, PublishedTokens.StatusRecovery.Kind);
        Assert.Equal(StatusRecoveryJsonFields.Path, PublishedTokens.StatusRecovery.Path);
        Assert.Equal(StatusRecoveryJsonFields.Integrity, PublishedTokens.StatusRecovery.Integrity);

        Assert.Equal(DoctorDataJsonFields.RecoveryStores, PublishedTokens.DoctorData.RecoveryStores);
        Assert.Equal(DoctorDataJsonFields.Kind, PublishedTokens.DoctorData.Kind);
        Assert.Equal(DoctorDataJsonFields.Path, PublishedTokens.DoctorData.Path);
        Assert.Equal(DoctorDataJsonFields.Candidates, PublishedTokens.DoctorData.Candidates);

        Assert.Equal(CleanupDataJsonFields.Stores, PublishedTokens.CleanupData.Stores);
        Assert.Equal(CleanupDataJsonFields.Kind, PublishedTokens.CleanupData.Kind);
        Assert.Equal(CleanupDataJsonFields.Path, PublishedTokens.CleanupData.Path);
        Assert.Equal(CleanupDataJsonFields.Candidates, PublishedTokens.CleanupData.Candidates);
        Assert.Equal(CleanupDataJsonFields.Cause, PublishedTokens.CleanupData.Cause);
        Assert.Equal(ContextDefinitions.CommandIdentity, PublishedTokens.Context.Command);
        Assert.Equal(ContextDefinitions.Content.Name, PublishedTokens.Context.Content);
        Assert.Equal(ContextDefinitions.FollowLinks.Name, PublishedTokens.Context.FollowLinks);
        Assert.Equal(ContextDefinitions.All, PublishedTokens.Context.All);
        Assert.Equal(ContextDefinitions.Frontmatter, PublishedTokens.Context.Frontmatter);
        Assert.Equal(FindDefinitions.Metadata, PublishedTokens.Find.Metadata);
        Assert.Equal(FindDefinitions.CommandIdentity, PublishedTokens.Find.Command);
        Assert.Equal(FindDefinitions.Heading.Name, PublishedTokens.Find.Heading);
        Assert.Equal(FindDefinitions.Content.Name, PublishedTokens.Find.Content);
        Assert.Equal(FindDefinitions.Exclude.Name, PublishedTokens.Find.Exclude);
        Assert.Equal(FindDefinitions.Body, PublishedTokens.Find.Body);
        Assert.Equal(ReferencesDefinitions.CommandIdentity, PublishedTokens.References.Command);
        Assert.Equal(SourceFormClassifier.IndexFileName, PublishedTokens.Layout.IndexFile);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix, PublishedTokens.Layout.LiteralReferencePrefix);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(ReferencesData.Scanned)), PublishedTokens.ReferencesData.Scanned);
        Assert.Equal(WorkspaceLayoutNames.HumanGuideFileName, PublishedTokens.Layout.GuideFile);
        Assert.Equal(WorkspaceLayoutNames.AgentHostFileName, PublishedTokens.Layout.AgentHost);
        Assert.Equal(WorkspaceLayoutNames.SkillsDirectoryName, PublishedTokens.Layout.SkillsDirectory);
        Assert.Equal(SourceFormClassifier.MarkdownExtension, PublishedTokens.Layout.MarkdownExtension);
        Assert.Equal(SourceOverwritePath.ReadAdjacentPath(SourceFormClassifier.MarkdownExtension), PublishedTokens.Layout.OverwriteSuffix);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(FrameworkSkillMetadataYamlDocument.Name)), PublishedTokens.Metadata.SkillName);
        Assert.Equal(FrameworkMetadataFieldReader.Description, PublishedTokens.Metadata.Description);
        Assert.Equal(FrameworkMetadataFieldReader.Tags, PublishedTokens.Metadata.Tags);
        Assert.Equal(MarkdownEntriesSectionReader.EntriesHeadingText, PublishedTokens.Metadata.Entries);
        Assert.Equal(MarkdownEntriesSectionReader.EmptyEntry, PublishedTokens.Metadata.EmptyEntry);
        Assert.Equal(SourceLoadingClosureResolver.LoadNowTag, PublishedTokens.Metadata.LoadNow);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(ContextData.Sources)), PublishedTokens.ContextData.Sources);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(FindData.Matches)), PublishedTokens.FindData.Matches);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(FindDataMatch.Parts)), PublishedTokens.FindData.Parts);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(ReferencesData.Source)), PublishedTokens.ReferencesData.Source);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(ReferencesData.Incoming)), PublishedTokens.ReferencesData.Incoming);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(ReferencesData.Outgoing)), PublishedTokens.ReferencesData.Outgoing);
        Assert.Equal(JsonNamingPolicy.CamelCase.ConvertName(nameof(ReferencesDataOutgoing.ResolvedPath)), PublishedTokens.ReferencesData.ResolvedPath);
        Assert.Equal(CliJsonFields.Id, PublishedTokens.Json.Id);
#pragma warning restore xUnit2000
    }
}
