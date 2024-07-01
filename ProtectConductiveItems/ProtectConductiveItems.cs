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

        filterConfigEntry = Instance.Config.Bind("General", "Name Filter", "",
                                                 "A comma-separated list of items that should no longer be targeted by lightning");

        protectToolsEntry = Instance.Config.Bind("General", "Protect Tools", true,
                                                 "If true, will generally prevent tools from being targeted by lightning");

        filterConfigEntry.SettingChanged += (_, _) => ReadAndApplyFilter();

        protectToolsEntry.SettingChanged += (_, _) => ResetMetalObjects();

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    private static void ReadAndApplyFilter() {
        FilterList.Clear();

        var filterString = filterConfigEntry.Value;
        filterString = filterString.ToLower().Replace(", ", ",");

        FilterList.AddRange(filterString.Split(","));

        FilterList.RemoveAll(string.IsNullOrWhiteSpace);

        ResetMetalObjects();
    }

    private static void ResetMetalObjects() {
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

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Unpatch() {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}