using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Doctor.Models;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DoctorData))]
internal sealed partial class DoctorDataJsonContext : JsonSerializerContext;
