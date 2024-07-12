using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ProtectConductiveItems.Dependencies;

namespace ProtectConductiveItems;

[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ProtectConductiveItems : BaseUnityPlugin {
    public static ProtectConductiveItems Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    internal static readonly List<string> FilterList = [
    ];

    internal static ConfigEntry<string> filterConfigEntry = null!;
    internal static ConfigEntry<bool> protectToolsEntry = null!;

    internal static void Patch() {
        Harmony ??= new(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    private static void ReadAndApplyFilter() {
        ReadFilter();

        ResetMetalObjects();
    }

    private static void ReadFilter() {
        FilterList.Clear();

        var filterString = filterConfigEntry.Value;
        filterString = filterString.ToLower().Replace(", ", ",");

        foreach (var filter in filterString.Split(",")) {
            if (filter is null) continue;

            var tempFilter = filter;

            while (tempFilter.StartsWith(" ")) tempFilter = tempFilter[1..];

            while (tempFilter.EndsWith(" ")) tempFilter = tempFilter[^1..];

            FilterList.Add(tempFilter);
        }

        FilterList.RemoveAll(string.IsNullOrWhiteSpace);
    }

    internal static void ResetMetalObjects() {
        var stormyWeather = FindObjectOfType<StormyWeather>();

        if (stormyWeather is null) return;

        if (!stormyWeather.isActiveAndEnabled) return;

        stormyWeather.metalObjects.Clear();
        stormyWeather.StartCoroutine(stormyWeather.GetMetalObjectsAfterDelay());
    }

    private void Awake() {
        Logger = base.Logger;
        Instance = this;

        if (DependencyChecker.IsLobbyCompatibilityInstalled()) {
            Logger.LogInfo("Found LobbyCompatibility Mod, initializing support :)");
            LobbyCompatibilitySupport.Initialize();
        }

        InitializeConfig();

        ReadFilter();

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void InitializeConfig() {
        filterConfigEntry = Instance.Config.Bind("General", "Name Filter", "",
                                                 "A comma-separated list of items that should no longer be targeted by lightning. "
                                               + "This will use 'StartsWith', so only the first characters are needed");

        protectToolsEntry = Instance.Config.Bind("General", "Protect Tools", true,
                                                 "If true, will generally prevent tools from being targeted by lightning");

        filterConfigEntry.SettingChanged += (_, _) => ReadAndApplyFilter();

        protectToolsEntry.SettingChanged += (_, _) => ResetMetalObjects();
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}