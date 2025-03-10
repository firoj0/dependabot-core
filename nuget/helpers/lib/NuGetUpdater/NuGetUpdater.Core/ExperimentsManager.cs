using System.Text.Json;

using NuGetUpdater.Core.Run;

namespace NuGetUpdater.Core;

public record ExperimentsManager
{
    public bool InstallDotnetSdks { get; init; } = false;
    public bool UseLegacyDependencySolver { get; init; } = false;
    public bool UseDirectDiscovery { get; init; } = false;

    public Dictionary<string, object> ToDictionary()
    {
        return new()
        {
            ["nuget_install_dotnet_sdks"] = InstallDotnetSdks,
            ["nuget_legacy_dependency_solver"] = UseLegacyDependencySolver,
            ["nuget_use_direct_discovery"] = UseDirectDiscovery,
        };
    }

    public static ExperimentsManager GetExperimentsManager(Dictionary<string, object>? experiments)
    {
        return new ExperimentsManager()
        {
            InstallDotnetSdks = IsEnabled(experiments, "nuget_install_dotnet_sdks"),
            UseLegacyDependencySolver = IsEnabled(experiments, "nuget_legacy_dependency_solver"),
            UseDirectDiscovery = IsEnabled(experiments, "nuget_use_direct_discovery"),
        };
    }

    public static async Task<ExperimentsManager> FromJobFileAsync(string jobFilePath, ILogger logger)
    {
        var jobFileContent = await File.ReadAllTextAsync(jobFilePath);
        try
        {
            var jobWrapper = RunWorker.Deserialize(jobFileContent);
            return GetExperimentsManager(jobWrapper.Job.Experiments);
        }
        catch (JsonException ex)
        {
            logger.Info($"Error deserializing job file: {ex.ToString()}: {jobFileContent}");
            return new ExperimentsManager();
        }
    }

    private static bool IsEnabled(Dictionary<string, object>? experiments, string experimentName)
    {
        if (experiments is null)
        {
            return false;
        }

        if (experiments.TryGetValue(experimentName, out var value))
        {
            if ((value?.ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
