using BepInEx.Logging;
using System.Collections.Generic;

public class WobbloaderEntry
{
    public static ManualLogSource Logger = null;
    public static void Initialize(Dictionary<string, object> args)
    {
        Logger = (ManualLogSource)(args["Logger"]);
        Logger.LogInfo("TestMod is executing Initialize!");
    }

    public static void Start(Dictionary<string, object> args)
    {
        Logger.LogInfo("TestMod is executing Start!");
    }
}