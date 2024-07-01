using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ProtectConductiveItems.Patches;

[HarmonyPatch(typeof(StormyWeather))]
public static class StormyWeatherPatch {
    [HarmonyPatch(nameof(StormyWeather.GetMetalObjectsAfterDelay), MethodType.Enumerator)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ReplaceFindObjectsOfTypeCall(IEnumerable<CodeInstruction> instructions) {
        ProtectConductiveItems.Logger.LogDebug("Searching for FindObjectsOfType...");

        var codes = new List<CodeInstruction>(instructions);
        foreach (var codeInstruction in codes) {
            if (codeInstruction.opcode != OpCodes.Call || codeInstruction.operand is not MethodInfo { Name: "FindObjectsOfType" }) {
                yield return codeInstruction;
                continue;
            }

            ProtectConductiveItems.Logger.LogDebug("Found!");

            var getFilteredObjectsMethod = AccessTools.Method(typeof(StormyWeatherPatch), nameof(GetFilteredObjects));

            ProtectConductiveItems.Logger.LogDebug("Replacing with method '" + getFilteredObjectsMethod + "'!");

            yield return new(OpCodes.Call, getFilteredObjectsMethod);
        }
    }

    public static GrabbableObject[] GetFilteredObjects() {
        ProtectConductiveItems.Logger.LogDebug("Filtering Objects!");

        var grabbableObjects = Object.FindObjectsOfType<GrabbableObject>();

        grabbableObjects ??= [
        ];

        var grabbableObjectsList = grabbableObjects.ToList();

        ProtectConductiveItems.Logger.LogDebug($"Unfiltered objects count: {grabbableObjectsList.Count}");

        grabbableObjectsList.RemoveAll(grabbableObject => {
            var itemProperties = grabbableObject?.itemProperties;

            if (itemProperties is null) return true;

            if (ProtectConductiveItems.protectToolsEntry.Value && !itemProperties.isScrap) return true;

            return ProtectConductiveItems.FilterList.Any(filter => itemProperties.itemName.ToLower().StartsWith(filter));
        });

        ProtectConductiveItems.Logger.LogDebug($"Filtered objects count: {grabbableObjectsList.Count}");

        return grabbableObjectsList.ToArray();
    }
}