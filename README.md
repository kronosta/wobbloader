This is the world's first Wobbledogs mod loader! If you like Wobbledogs and are familiar with C# and .NET, you can make your own mods too!

# How to install
Go into the releases section of this GitHub page, and follow the instructions there.

# How to mod
Use the `TemplateMod` (a Visual Studio project) in this GitHub repo to base your mod off of that one.

Wobbloader is currently extremely bare bones and only loads two specially named methods in your code, which can be used to call other code and patch existing game code using the Harmony library.

Mods go in `(Game executable folder)/Wobbloader/Mods/(Any subfolder name)`, and should be a .NET Framework 4.6 DLL. Wobbloader will look in each DLL for a class named `WobbloaderEntry` (must not be in a namespace). `WobbleloaderEntry` must contain a method
named `Start` that takes in one parameter, a `Dictionary<string, object>`. A method with the same parameter but named `Initialize` has different but extremely useful functionality.

`Initialize` is called right after the mod assembly is loaded, and contains entries in the Dictionary:

- `"Logger"` is a `ManualLogSource` from BepInEx, which can log things to the `BepInEx/LogOutput.log` file, which can be useful for debugging.
- `"Path"` is a `string`, which contains the path to the mod folder.

`Start` is called once all mods have initialized, in the order that they were. The Dictionary is currently empty and unused, and exists for compatibility and future expansion.

# How do I do anything??
Wobbledogs is easily decompilable with tools like ILSpy, and produces really readable code. The library to decompile is located at `(Game executable folder)/Wobbledogs_Data/Managed/Assembly-CSharp.dll`. Most game code is in the empty namespace, the other namespaces
namespaces are libraries used by the game. The main problem is that it's not immediately obvious where a specific mechanic is implemented, so searching and finding code is the main obstacle to get stuff done. Thankfully, ILSpy has tools such as searching and finding
all places where a specific method is called; these help, but sometimes code isn't called directly, and sometimes where it's called is really strange. I've made a few notes in `docs/notes` in this repo, please contribute if you find anything!

Right now, none of the work is done for you, and there aren't any hooks. You need to patch in custom logic with Harmony.