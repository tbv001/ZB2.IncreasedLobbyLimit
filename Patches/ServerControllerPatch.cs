using HarmonyLib;
using UnityEngine;

namespace IncreasedLobbyLimit.Patches;

[HarmonyPatch(typeof(ServerController))]
internal static class ServerControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ServerController.TryStartServer))]
    private static bool TryStartServer_Prefix(ServerController __instance, int maxPlayers, bool friendsOnly)
    {
        if (__instance.state != ServerController.State.Off)
            return true;


        __instance.mode = ServerController.Mode.Multiplayer;
        __instance.state = ServerController.State.Starting;

        var instanceTraverse = Traverse.Create(__instance);
        instanceTraverse.Property("FriendsOnly").SetValue(friendsOnly);
        instanceTraverse.Property("ConfiguredMaxPlayers").SetValue(Mathf.Clamp(maxPlayers, 2, Plugin.MaxPlayers));

        __instance.StartCoroutine(__instance.StartLobbyCoroutine());

        return false;
    }
}
