using HarmonyLib;
using IncreasedLobbyLimit.Classes;

namespace IncreasedLobbyLimit.Patches;

[HarmonyPatch(typeof(ClientSpeaker))]
internal static class ClientSpeakerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ClientSpeaker.SendHandshake))]
    private static void SendHandshake_Postfix()
    {
        NetworkVerification.SendModHandshake();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ClientSpeaker.SendAuth))]
    private static void SendAuth_Prefix()
    {
        NetworkVerification.SendModHandshake();
    }
}
