My notes about modding.

# Code
The managed library is stored in the Wobbledogs steam folder, with the path `Wobbledogs_Windows_64/Wobbledogs_Data/Managed/Assembly-CSharp.dll`
(presumably `Windows_64` depends on the platform).
Most Wobbledogs-related code is stored in the empty namespace, but there is other code in namespaces, probably some libraries Wobbledogs was compiled with. 
This code can be decompiled with tools such as ILSpy or dnSpy.

`Assembly-CSharp.dll` can probably be loaded dynamically by another Mono assembly, then patched with Harmony to add hooks.

# Assets
Existing game assets are stored in the Wobbledogs steam folder, with the path `Wobbledogs_Windows_64/Wobbledogs_Data` (presumably `Windows_64` depends on the platform).
Mods will probably load asset bundles from separate files to not mess with the existing game files too much, but looking at the asset bundles will help determine
how the game loads some useful things.

I used ILSpy and Asset Bundle Extractor for this research.

- Assets are stored in a few different files: `globalgamemanagers.assets`, `resources.assets`, `sharedassets0.assets`, `sharedassets1.assets`, and `sharedassets2.assets`.
  - `globalgamemanagers.assets` stores mostly references to C# classes
  - `resources.assets` stores most textures, meshes, and other assets, but also `MonoBehaviour`s, which contain instances of game elements with common functionality, such as gut flora,
    inventory items,  wallpapers, etc.
  - The `sharedassetsX.assets` store pretty much the same things as `resources.assets`. I'm not sure why they're separated, but it means you have to potentially look through
    four files to find the correct asset.
- It's important to load `globalgamemanagers` (without an extension) into Asset Bundle Extractor. This has all of the containers that give some extra context to what the assets are.
  - In the InventoryManager class, for example, the LoadInventoryObjects() method appears to import all of the items into the game. Indirectly, it loads assets from "paths" based
    on `Items/Food/` or `Items/Eggs/`. These actually find containers with a name such as `items/food/hamslider` which contains an asset called `hamslider` which is the
    actual instance corresponding to a Ham Slider

Sometimes when looking at assets in Asset Bundle Extractor, it will ask for class information, which if you give it to it properly will allow it to retrieve much more data
about the assets. Let me be clear, **you want this to happen**. You can spur it by opening up `globalgamemanagers.assets`, `resources.assets`, and `sharedassets0.assets`,
then selecting certain assets (Flora MonoBehaviour assets work) and clicking View Data.
I don't exactly know what it wants, but here's what's worked for me:

- The first file selector window will come up. Enter the Wobbledogs_Data/Managed folder, and type in `Assembly-CSharp.dll`.
- The second one will request a file you don't have. Close that window.
- Close all further windows that it gives you
- Let it load for a few second

# Content
## Gut Flora
The very first research into this game's internals (after finding the code, of course) was gut flora (I don't know exactly why I did that first, but all the assets stuff
was figured out by exploring how gut flora were loaded).

Basic data about a flora type is stored as a `GutFloraResource`, while more complex behavior and data is stored in a `GutFloraBase`.

`GutFloraResource`s are normally loaded from assets named `flora_[flora name]` or `Flora_[Capitalized Flora Name]` (they show up differently depending on whether Asset Bundle Extractor
has loaded `globalgamemanagers` or not). The `GameObject`s attached to them are stored in `GutFlora_[Capitalized Flora Name]` (always capital). Both of these are in `resources.assets`.

`GutFloraBase`s are also stored in assets, but they are unnamed. The only link I can find between a `GutFloraResource` and its `GutFloraBase` stored in the assets is
that the `GutFloraResource`'s `gutFloraPrefab` contains the `GutFloraBase` as a component (the 2nd one).

`GutFloraResource` is an asset type that loads information about a gut flora. If adding a new gut flora, start with a `GutFloraResource` and create its corresponding
`GameObject` and `GutFloraBase` instances.

`GutFloraBase` is a component of a gut flora `GameObject` that determines its behavior. If adding custom behavior to a gut flora, you should patch `GutFloraBase`.

### Sprites
Sprites are stored in a complicated way.

The `GutFloraBase` contains a reference to a `SpriteRenderer` called `floraGraphicRef`. `floraGraphicRef` contains a reference to a `Sprite` called `sprite`,
which contains the `Texture2D` reference in a field called `texture`.

However, the `GutFloraResource` contains a sprite called `gutFloraPreviewSprite`. From the name, this would seem to be the image in the menu, but this is untested.

The `GutFloraBase` also contains a reference to a `RigidBody2D` called `rigidBodyRef`, which presumably handles collisions and movement.

### GutFloraResource

- `gutFloraName` : a string representing the English name, but English might use `floraNameLocalized`, I'm not sure. Edit both when I figure out how to add localized texts.
- `floraNameLocalized`, `floraDescriptionLocalized` : `LocalizedString`s linking to localized text? Not sure where to edit those.
- `gutFloraPreviewSprite` : a `Sprite`. Maybe this is the sprite that gets shown in the menu?
- `gutFloraPrefab` : a `GameObject`. This probably stores the actual sprite inside it.
- `associatedItemSet` : an `ItemSet`. I'm not sure what this does.

## Items
Data about items appears to be stored as an `InventoryItem` per item. Actual instances of items are just `GameObject`s. Some of these `GameObject`s have
a component containing inventory items titled `spawnOnDestroy` and `saveAsAlternativeItem`, but otherwise the `GameObject`s have no link to their `InventoryItem`.

Edibility is stored as a component of a `GameObject`, as an `Eatable`. This is a prime target for new gut flora, as they would have to be obtained from the
`List<GutFloraResource>`s named `gutFloraTypes` and `boostedGutFloraTypes`. These lists are public and could possibly be modified at runtime to inject more flora
into the game without adding any new foods.
