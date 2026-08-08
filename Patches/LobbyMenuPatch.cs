using System.Collections.Generic;
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

    [HarmonyPatch(nameof(LobbyMenu.OnSyncLobbyLoadout))]
    private static bool OnSyncLobbyLoadout_Prefix(LobbyMenu __instance, int lobbyID, int loadoutLevel,
        InventoryItem.ID[] items, List<PerkID> perks)
    {
        LobbyLayout.EnsureExpanded(__instance);

        var lobbyController = LobbyController.instance;
        if (lobbyController == null)
            return false;

        var playerIndex = lobbyController.GetPlayerIndex(lobbyID);
        if (playerIndex < 0 || playerIndex >= lobbyController.players.Count)
            return false;

        var player = lobbyController.players[playerIndex];
        player.loadoutLevel = loadoutLevel;
        player.perks = perks;
        player.updatedLobbyLoadout = true;

        if (__instance.slots != null && playerIndex < __instance.slots.Length && __instance.slots[playerIndex] != null)
        {
            __instance.slots[playerIndex].UpdateWithLoadout(player, loadoutLevel, items);
        }

        return false;
    }
}
