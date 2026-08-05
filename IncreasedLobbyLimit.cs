using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace IncreasedLobbyLimit;

[BepInPlugin(PluginGuid, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    private const string PluginGuid = "com.theblackvoid.increasedlobbylimit";
    private readonly Harmony _harmony = new(PluginGuid);
    public const int MaxPlayers = 24;

    private void Awake()
    {
        Logger = base.Logger;
        try
        {
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }
}
