using HarmonyLib;
using IncreasedLobbyLimit.Classes;

namespace IncreasedLobbyLimit.Patches;

[HarmonyPatch(typeof(LobbyMenu))]
internal static class LobbyMenuPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyMenu.Initialize))]
    private static void Initialize_Postfix(LobbyMenu __instance)
    {
        LobbyLayout.EnsureExpanded(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyMenu.OnEnable))]
    private static void OnEnable_Postfix(LobbyMenu __instance)
    {
        if (LobbyLayout.EnsureExpanded(__instance) && __instance.lobby != null)
        {
            __instance.UpdateAllSlots();
        }

        LobbyLayout.ApplyLayout(__instance);
        LobbyLayout.RepositionSlots(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("RepositionAllUIElements")]
    private static void RepositionAllUIElements_Prefix(LobbyMenu __instance)
    {
        LobbyLayout.ApplyLayout(__instance);
    }
}
