# Bartizan (Plus) TowerFall Modding Framework

This is a fork of Bartizan, the mod framework for TowerFall. The additions
I've made are listed below. The original README stated that the patcher
didn't work on mac, but I've exclusively been using this on the mac version
with no problem.

## General changes
- Created an easy-to-use GUI patcher
- Support for 8-player and 4-player versions
- Support for Steam and Itch versions
- Fixed glitched replay gifs by removing screen shake from replays
- Fixed asymmetrical treasure spawning when symmetrical treasure is enabled
- Fixed bug where the game starts with rumble on even when the setting is set to off
- Integrated with my stat tracker API which is currently a private repo so don't worry about it

## Variants

### Crown Summons Chalice Ghost

In this mode, putting on someone's fallen crown summons the chalice ghost.
If the crown is knocked off or the player dies, the chalice ghost vanishes.

### Chalice Ghosts Hunt Ghosts

Chalice ghosts will go after player ghosts in addition to players.

### Gotta Bust Ghosts

The round won't end until all ghosts have been busted.

### Variety Pack

Start with one of every arrow type

### Fast Ghosts

Ghost are faster

### CalvinFall

New random variants every round!

### Ghost Revives

Ghosts can revive, but you can't revive yourself.

### Ghost Items

Ghosts can get powerups (everything except arrows).

### Ghost Joust

Ghosts can kill other ghosts by dashing into them.

### Meaner Monsters

Enemies other than bats and green slimes will come out of dark portals.

## How to Patch your game

Download the latest version of the patcher from the [releases page](https://github.com/zpchavez/Bartizan/releases)

## How to work on your own mods

This is a little different than the original Bartizan, and it differs between Windows and Mac.

Note: you may need to use Visual Studio 2019, since later versions can't target .NET 4.0

In `bin`, create a directory called `originals`. Inside that directory create directories for
the versions of TowerFall you'll be working with. For the Steam version name it `4-player`. For
the 4-player Itch version name it `4-player-itch`. For 8-player name it `8-player`.  Finally,
copy the corresponding TowerFall.exe files into those directories.

On Mac, copy `FNA.dll` into `bin`.

There are multiple build configs for the Mod project that you can use depending on which version
you are patching and whether you are including certain features in the build.

After building, a releasable build will be output at `/bin/builds` whose directory will match
the name of the one in `/bin/originals`.
Also, a copy of the patched `TowerFall.exe` will be copied to `TowerFall-4-player.exe` or `TowerFall-8-player.exe`, etc.

In order to work without patching the game after every build, create a copy of TF that includes the following
symlinks (I'll use 8-player as an example. Replace 8 with 4 as needed):

- `TowerFall.exe -> <BartizanPath>/bin/TowerFall-8-player.exe` (this should replace the existing TowerFall.exe)
- `Content/Atlas/modAtlas.png -> <BartizanPath>/bin/builds/8-player/modAtlas.png`
- `Content/Atlas/modAtlas.xml -> <BartizanPath>/bin/builds/8-player/modAtlas.xml`

Then any time you build the Mod project, just run that copy of TowerFall and it will have the latest changes.

Note that when you change build configurations, you may need to manually switch which
BaseTowerFall.exe you include in your references for the Mod project. If you see a compile error
about the number of arguments not matching, this is likely the issue.

### Windows Specific Things

Check out the `windows-build` branch of the repo.

You'll need to copy the following dll files to `bin`.

- Microsoft.Xna.Framework.dll
- Microsoft.Xna.Framework.Game.dll
- Microsoft.Xna.Framework.Graphics.dll
- Microsoft.Xna.Framework.Net.dll
- Microsoft.Xna.Framework.Xact.dll

These files can be found in `c:\Windows\Microsoft.NET\assembly\GAC_32` and `c:\Windows\Microsoft.NET\assembly\GAC_MSIL`.

# Original Readme

A mod framework for [TowerFall Ascension](http://www.towerfall-game.com/) (copyright 2013 Matt Thorson, obviously).

* [Included Mods](#included-mods)
  * [Game Modes](#game-modes)
  * [Variants](#variants)
  * [UI Enhancements](#ui-enhancements)
  * [Dev Mods](#dev-mods)
* [Installation](#installation)
* [Hacking](#hacking)

# Included Mods

## Game Modes

### Respawn
<p align="center">
  <img src="img/respawn.png?raw=true"/>
</p>


![](img/respawn.gif?raw=true)
![](img/respawn2.gif?raw=true)

Best played with Gunn Style activated, obviously.
Not shown on the replay gifs: our awesome in-game kill count HUDs!

### Crawl

<p align="center">
 <img src="img/crawl.png?raw=true"/>
</p>

![](img/crawl.gif?raw=true)
![](img/crawl2.gif?raw=true)

*Variants: No Balancing, No Treasure, Start with Toy Arrows*

Inspired by a certain other indie game - kill living players to regain your humanity!
Unlike in other game modes, you score points for killing enemy ghosts.
This may be our most ambitious mod yet, and therefore not quite yet balanced. Toy arrows are a good way to nerf living players if you feel the ghosts are too weak.

## Variants

### No Head Bounce ![](Mod/Content/Atlas/modAtlas/variants/noHeadBounce.png?raw=true)

![](img/noHeadBounce.gif?raw=true)

### No Ledge Grab ![](Mod/Content/Atlas/modAtlas/variants/noLedgeGrab.png?raw=true)

Koala hunters no more.

### Awfully Slow Arrows ![](Mod/Content/Atlas/modAtlas/variants/awfullySlowArrows.png?raw=true)

![](img/awfullySlowArrows2.gif?raw=true)
![](img/awfullySlowArrows.gif?raw=true)

### Awfully Fast Arrows ![](Mod/Content/Atlas/modAtlas/variants/awfullyFastArrows.png?raw=true)

![](img/awfullyFastArrows.gif?raw=true)

### Infinite Arrows ![](Mod/Content/Atlas/modAtlas/variants/infiniteArrows.png?raw=true)

![](img/infiniteArrows.gif?raw=true)
![](img/infiniteArrows2.gif?raw=true)

### No Dodge Cooldowns ![](Mod/Content/Atlas/modAtlas/variants/noDodgeCooldowns.png?raw=true)

![](img/noDodgeCooldown.gif?raw=true)

## UI Enhancements

### Win Counter
![](img/winCounter.png?raw=true)

Resets when you return to the Player Select screen.

## Dev Mods

These mods are intended for development or are simply unfinished and only available if you compile Bartizan from source. If enough people want to see one of these included in the official releases, we may flesh them out and include them.

### Keyboard Config for Second Player

Walk/aim with WASD, jump J, shoot K, dash Right Shift

### Slow Time Orb on Back Button

For those perfect-looking quest runs. Only available for Xbox game pads.

### End Round on Center (Steam) Button

Useful for immediately saving a scene to a gif. Only available for Xbox game pads.


Installation
============

* Extract the zip from our [releases](https://github.com/Kha/Bartizan/releases/) according to you platform, then start `Wizard.exe` (`mono Wizard.exe <path to TowerFall installation>` from a Linux shell). This will patch TowerFall.exe and the graphics resources, and save the original files in a new folder `Original`.
* On new TowerFall releases, you'll need to delete the `Original` folder and re-run the Wizard (and possibly need a new release of Bartizan if the update has broken any mods).
* To uninstall, simply reset your TowerFall installation by selecting `Properties -> Local Files -> Verify Integrity of Game Cache` in the Steam context menu for TowerFall.

Hacking
=======

While most of Bartizan was developed using MonoDevelop on Linux, using Visual Studio or any other IDE on Windows should work just as well. We haven't put any time into getting it to work on OS X, and at a cursory glance it looks like while the Patcher works, the generated TowerFall.exe crashes the runtime so bad not even the stacktrace can be displayed, so... OS X support isn't likely to happen.
If you're an OS X / mono wizard and want to take a look at how to fix this, we'd love the help.

* Copy Steam/SteamApps/common/TowerFall to bin/Original (or at least FNA.dll, TowerFall.exe and Content/Atlas, to save some copying time)
* Build Bartizan.sln. The AfterBuild targets should do all the dirty work:
  * Using [Mono.Cecil](https://github.com/jbevain/cecil), the base image `BaseTowerFall.exe` is derived from `Original/TowerFall.exe` by marking members of TowerFall classes as public and virtual (where applicable) so that we can use/override them in `Mod.dll` (and `DevMod.dll`, which is ignored by the Wizard and contains the development-only mods).
  * Any members of classes marked as `[Patch]` in `*Mod.dll` will be merged back into their respective base class to form the resulting `TowerFall.exe`.
* Copy (or just symlink) the new `TowerFall.exe`, `*Mod.dll` and `Content/Atlas/*` back to the TowerFall Steam directory.

As an aside, due to the rather unusual way we're patching the game, you won't be able to use all of the fancy C# language features in your mods. If you're planning on using obscure features, expect obscure error messages.
