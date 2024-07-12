using HarmonyLib;

namespace ProtectConductiveItems.Patches;

[HarmonyPatch(typeof(GrabbableObject))]
public static class GrabbableObjectPatch {
    [HarmonyPatch(nameof(GrabbableObject.Start))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void RemoveFromMetalObjectsList(GrabbableObject __instance) {
        var stormyWeather = StormyWeatherPatch.stormyWeather;

        if (stormyWeather is null) return;

        if (!StormyWeatherPatch.MatchesFilter(__instance)) return;

        stormyWeather.metalObjects.Remove(__instance);
    }
}