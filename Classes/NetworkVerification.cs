using System;
using System.Collections.Generic;
using Steamworks;

namespace IncreasedLobbyLimit.Classes;

internal static class NetworkVerification
{
    // * This can be any values ranging from 6-254 (0-5 and 255 are used)
    public const byte PacketCategory = 210;
    public const string HandshakeMessage = "IAmUsingILL";

    private static readonly HashSet<int> VerifiedConnections = [];

    public static void SendModHandshake()
    {
        var connectionsController = SteamConnectionsController.instance;
        if (connectionsController == null)
            return;

        var buffer = new Buffer();
        buffer.StartWrite();
        buffer.PushByte(PacketCategory);
        SerializationUtils.PushString128(buffer, HandshakeMessage);

        var payload = new byte[buffer.writeCount];
        Array.Copy(buffer.content, payload, buffer.writeCount);

        connectionsController.SendToServer(payload, P2PSend.Reliable);
        Plugin.Logger.LogInfo("Sent custom handshake packet to server.");
    }

    public static void RegisterVerified(int connectionID)
    {
        VerifiedConnections.Add(connectionID);
        Plugin.Logger.LogInfo($"Registered connection {connectionID} as verified modded client.");
    }

    public static bool IsVerified(int connectionID)
    {
        return VerifiedConnections.Contains(connectionID);
    }

    public static void Unregister(int connectionID)
    {
        VerifiedConnections.Remove(connectionID);
    }

    public static void Clear()
    {
        VerifiedConnections.Clear();
    }
}
