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

# Run code every frame
There doesn't appear to be a global method to patch for that, though you can come really close. `DogHome.Update` seems to run every frame as long as you're in game and not in a menu, and you can patch in logic here (I was worried for a minute that
this method would be inlined, making it impossible to patch, since it's really short, but it works fine). `TitleScreen.Update` and `TitleScreen.HandleInput` don't work with patching for some reason, so custom title screen update logic would have to use a
different method. Some possibilities include creating a new GameObject with a separate update method or finding some random method in Unity that gets called every frame.
There are other similar methods for the title screen and other menu screens. Also, the upside of this is that there Update methods for individual things, like `DoggyBrain.Update` or `GutFloraBase.Update`.

You might be also able to create an invisible `GameObject` with a `Component` attached to it to create your own global update method.

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

## Tails, Wings, Ears, Horns, and Noses
These are all stored in the class `ModelLoader` with a master list and Dictionary for each part. The values are GameObjects, while the keys are a different enum type for each part. We can't add any more *named* values to enums,
but enums are really just named integers and we can use integers that aren't specified in the enum. The part values specifically are 32-bit integers, which means we can have about 4 billion appearances **for each part**, so we'll probably never run out of those.
In theory you should be able to add more appearances by adding the `GameObject`s to the list and Dictionary and giving them each an integer key; in practice this has not been tried yet.

It might be a good idea to set up a string-to-integer mapping ID system in this modloader (or perhaps in a framework mod), so that IDs like these won't conflict so easily.
