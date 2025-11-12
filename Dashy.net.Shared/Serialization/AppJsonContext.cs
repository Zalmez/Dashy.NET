using System.Text.Json.Serialization;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Shared.Serialization;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false
)]
[JsonSerializable(typeof(DashboardConfigVm))]
[JsonSerializable(typeof(List<DashboardListItemVm>))]
[JsonSerializable(typeof(SectionVm))]
[JsonSerializable(typeof(HeaderButtonVm))]
[JsonSerializable(typeof(ItemVm))]
[JsonSerializable(typeof(CreateItemDto))]
[JsonSerializable(typeof(UpdateItemDto))]
[JsonSerializable(typeof(CreateSectionDto))]
[JsonSerializable(typeof(UpdateSectionDto))]
[JsonSerializable(typeof(CreateHeaderButtonDto))]
[JsonSerializable(typeof(UpdateHeaderButtonDto))]
[JsonSerializable(typeof(CreateDashboardDto))]
[JsonSerializable(typeof(UpdateDashboardDto))]
[JsonSerializable(typeof(AuthenticationProviderVm))]
[JsonSerializable(typeof(AuthenticationProviderSettingVm))]
[JsonSerializable(typeof(CreateAuthenticationProviderDto))]
[JsonSerializable(typeof(UpdateAuthenticationProviderDto))]
[JsonSerializable(typeof(AuthenticationProviderSettingDto))]
[JsonSerializable(typeof(AuthenticationProviderTemplate))]
[JsonSerializable(typeof(AuthenticationProviderSettingTemplate))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class AppJsonContext : JsonSerializerContext
{
}
