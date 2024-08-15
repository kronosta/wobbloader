using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Wobbloader
{
    public class ModLoader
    {
        public string ModDirectory { get; set; }
        public List<Type> Mods { get; set; }

        public void Initialize()
        {
            this.ModDirectory = Path.Combine(
                Directory.GetParent(
                    Directory.GetParent(
                        Plugin.Instance.ThisDirectory
                    ).ToString()
                ).ToString(),
                Path.Combine("Wobbloader", "Mods")
            );
            this.Mods = new List<Type>();
        }
        public void LoadMods()
        {
            foreach (string modPath in Directory.EnumerateDirectories(this.ModDirectory))
            {
                LoadMod(modPath);
            }
            foreach (Type mod in Mods)
            {
                StartMod(mod);
            }
        }

        public void LoadMod(string modPath)
        {
            foreach (string dllPath in Directory.EnumerateFiles(modPath, "*.dll"))
            {
                Assembly entryAssembly = null;
                Type entryType = null;
                MethodInfo initMethod = null;
                MethodInfo startMethod = null;
                try
                {
                    entryAssembly = Assembly.LoadFile(dllPath);
                    entryType = entryAssembly?.GetType("WobbloaderEntry");
                    initMethod = entryType?.GetMethod("Initialize", new Type[] { typeof(Dictionary<string, object>) });
                    startMethod = entryType?.GetMethod("Start", new Type[] { typeof(Dictionary<string, object>) });
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, new object[] { CreateInitDictionary(modPath) });
                    }
                    if (startMethod != null)
                    {
                        Mods.Add(entryType);
                    }
                    Plugin.Logger.LogMessage($"Wobbloader initialized mod at '{modPath}'");
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogMessage($"Wobbloader couldn't load the dll at '{dllPath}'. Reason:\n{ex}");
                }
            }
        }

        public void StartMod(Type mod)
        {
            if (mod == null)
            {
                Plugin.Logger.LogError("A mod type was null in the mod list.\n");
                return;
            }
            MethodInfo entryMethod = mod.GetMethod("Start", new Type[] { typeof(Dictionary<string, object>) });
            if (entryMethod == null)
            {
                Plugin.Logger.LogError($"The mod type in assembly '{mod.Assembly}' in the mod list was invalid.\n");
                return;
            }
            entryMethod.Invoke(null, new object[] { CreateStartDictionary(mod) });
            Plugin.Logger.LogMessage($"Wobbloader mod started: {mod.Assembly.FullName}");
        }

        public Dictionary<string, object> CreateInitDictionary(string modPath)
        {
            return new Dictionary<string, object>
            {
                ["Logger"] = Plugin.Logger,
                ["Path"] = modPath
            };
        }

        public Dictionary<string, object> CreateStartDictionary(Type mod)
        {
            return new Dictionary<string, object>();
        }
    }
}
