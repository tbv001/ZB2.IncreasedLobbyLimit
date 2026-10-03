using HarmonyLib;
using IncreasedLobbyLimit.Classes;

namespace IncreasedLobbyLimit.Patches;

[HarmonyPatch(typeof(SteamConnectionsController))]
internal static class SteamConnectionsControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SteamConnectionsController.RemoveConnection), typeof(SteamP2PConnection))]
    private static void RemoveConnection_Prefix(SteamP2PConnection connection)
    {
        if (connection == null)
            return;

        NetworkVerification.Unregister(connection.ConnectionID);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SteamConnectionsController.ClearConnections))]
    private static void ClearConnections_Postfix()
    {
        NetworkVerification.Clear();
    }
}
