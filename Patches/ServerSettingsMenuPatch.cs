using HarmonyLib;
using UnityEngine;

namespace IncreasedLobbyLimit.Patches;

[HarmonyPatch(typeof(ServerSettingsMenu))]
internal static class ServerSettingsMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ServerSettingsMenu.OnEnterMenu))]
    private static void OnEnterMenu_Postfix(ServerSettingsMenu __instance)
    {
        var sliderTraverse = Traverse.Create(__instance).Field("playersSlider");
        var playersSlider = sliderTraverse.GetValue<ZBSlider>();
        playersSlider.targetSlider.maxValue = Plugin.MaxPlayers;
        playersSlider.targetSlider.value = Mathf.Clamp(playersSlider.targetSlider.value,
            playersSlider.targetSlider.minValue, playersSlider.targetSlider.maxValue);
        playersSlider.OnValueChanged();
    }
}
