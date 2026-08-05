using HarmonyLib;

namespace IncreasedLobbyLimit.Patches;

[HarmonyPatch(typeof(ZombieDistanceMatrix))]
internal static class ZombieDistanceMatrixPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZombieDistanceMatrix.AllocateMatrix))]
    private static bool AllocateMatrix_Prefix(ZombieDistanceMatrix __instance, int zombieCap)
    {
        __instance.curZombieCap = zombieCap + ZombieDistanceMatrix.defaultZombiePadding;
        __instance.curPlayerCap = Plugin.MaxPlayers;
        __instance.distanceSqr = new float[__instance.curPlayerCap, __instance.curZombieCap];

        return false;
    }
}
