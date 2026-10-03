using HarmonyLib;
using UnityEngine;
using IncreasedLobbyLimit.Classes;

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

    [HarmonyPrefix]
    [HarmonyPatch("OnPacketReceived")]
    private static bool OnPacketReceived_Prefix(byte[] data, int connectionID)
    {
        if (data == null || data.Length < 2)
            return true;

        if (data[0] != NetworkVerification.PacketCategory)
            return true;

        var buffer = new Buffer(data.Length);
        buffer.SetContentReferenceForReading(data);
        buffer.StartRead(1);

        var message = SerializationUtils.PullString128(buffer);
        if (message == NetworkVerification.HandshakeMessage)
        {
            NetworkVerification.RegisterVerified(connectionID);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ServerController.OnClientAuthAttempt))]
    private static bool OnClientAuthAttempt_Prefix(ServerController __instance, string version, string clientName,
        int connectionID)
    {
        if (!NetworkVerification.IsVerified(connectionID) && __instance.lobby != null &&
            __instance.lobby.players.Count >= 6)
        {
            Plugin.Logger.LogWarning($"Unmodded client '{clientName}' rejected because player count is 6 or above.");
            __instance.KickPlayer(connectionID, DisconnectionReason.Capacity);
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ServerController.OnConnectionLost))]
    private static void OnConnectionLost_Postfix(SteamP2PConnection steamConnection)
    {
        if (steamConnection == null)
            return;

        NetworkVerification.Unregister(steamConnection.ConnectionID);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Shutdown")]
    private static void Shutdown_Postfix()
    {
        NetworkVerification.Clear();
    }
}
