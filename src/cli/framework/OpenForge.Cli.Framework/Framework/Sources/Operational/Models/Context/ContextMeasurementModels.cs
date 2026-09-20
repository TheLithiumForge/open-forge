using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;

internal sealed record OperationalIntegerObservation(
    OperationalValueState State,
    long? Value);

internal sealed record ContextMeasurementObservation(
    OperationalIntegerObservation Files,
    OperationalIntegerObservation Characters,
    OperationalIntegerObservation Utf8Bytes,
    OperationalIntegerObservation EstimatedTokens);

internal sealed record ContextLayerContributionObservation(
    string Path,
    long Utf8Bytes);
