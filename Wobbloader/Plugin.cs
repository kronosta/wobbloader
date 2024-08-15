using BepInEx;
using BepInEx.Logging;

namespace Wobbloader;

[BepInPlugin("kronosta.wobbloader", "Wobbloader", "0.0.2.0")]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public static Plugin Instance;

    public string ThisDirectory;
    public ModLoader ModLoader;

        
    public void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! (Wobbloader)");
        this.ThisDirectory = BepInEx.Paths.PluginPath;
        Plugin.Instance = this;
        this.ModLoader = new ModLoader();
        this.ModLoader.Initialize();
        this.ModLoader.LoadMods();
    }
}
