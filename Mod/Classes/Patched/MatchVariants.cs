#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

using Mod;
using MonoMod;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TowerFall
{
  class patch_MatchVariants : MatchVariants
    {
    public static List<string> ModVariants = new List<string> {
      "NoHeadBounce",
      "NoLedgeGrab",
      "AwfullySlowArrows",
      "AwfullyFastArrows",
      "InfiniteArrows",
      "NoDodgeCooldowns",
      "VarietyPack",
      "GottaBustGhosts",
      "CrownSummonsChaliceGhost",
      "ChaliceGhostsHuntGhosts",
      "FastGhosts",
      "GhostRevives",
      "GhostItems",
      "GhostJoust",
      "CalvinFall",
      "MeanerMonsters",
      "StartWithGhostArrows",
    };

    [Header("MODS")]
    [PerPlayer, CanRandom]
    public Variant NoHeadBounce;
    [PerPlayer, CanRandom]
    public Variant NoLedgeGrab;
    [CanRandom]
    public Variant AwfullySlowArrows;
    [CanRandom]
    public Variant AwfullyFastArrows;
    [PerPlayer, CanRandom]
    public Variant InfiniteArrows;
    [PerPlayer, CanRandom]
    public Variant NoDodgeCooldowns;

    [PerPlayer, CanRandom, Description ("START WITH ONE OF EVERY ARROW TYPE")]
    public Variant VarietyPack;

    [CanRandom, Description ("THE ROUND WON'T END UNTIL GHOSTS ARE DEAD")]
    public Variant GottaBustGhosts;

    [CanRandom, Description ("PUTTING ON A CROWN SUMMONS THE CHALICE GHOST")]
    public Variant CrownSummonsChaliceGhost;

    [CanRandom, Description ("THE CHALICE GHOST WILL GO AFTER GHOSTS")]
    public Variant ChaliceGhostsHuntGhosts;

    [CanRandom, Description ("GHOSTS ARE REALLY FAST")]
    public Variant FastGhosts;

    [CanRandom, Description ("GHOSTS CAN REVIVE")]
    public Variant GhostRevives;

    [CanRandom, Description ("GHOSTS CAN OPEN NON-BOMB CHESTS AND COLLECT SOME ITEMS")]
    public Variant GhostItems;

    [CanRandom, Description ("GHOSTS CAN KILL OTHER GHOSTS BY DASHING INTO THEM")]
    public Variant GhostJoust;

    [Description ("NEW RANDOM VARIANTS EVERY ROUND")]
    public Variant CalvinFall;

    [CanRandom, Description ("MORE TYPES OF MONSTERS SPAWN FROM PORTAL")]
    public Variant MeanerMonsters;

    [PerPlayer, CanRandom]
    public Variant StartWithGhostArrows;

    public extern void orig_ctor(bool noPerPlayer);
    [MonoModConstructor]
    public void ctor(bool noPerPlayer = false)
    {
      orig_ctor(noPerPlayer);
      // mutually exclusive variants
      this.CreateLinks(NoHeadBounce, NoTimeLimit);
      this.CreateLinks(NoDodgeCooldowns, ShowDodgeCooldown);
      this.CreateLinks(AwfullyFastArrows, AwfullySlowArrows);
      this.StartWithGhostArrows.AddLinks(new Variant[] {
        this.StartWithBombArrows,
        this.StartWithLaserArrows,
        this.StartWithBrambleArrows,
        this.StartWithDrillArrows,
        this.StartWithBoltArrows,
        this.StartWithSuperBombArrows,
        this.StartWithFeatherArrows,
        this.StartWithRandomArrows,
        this.StartWithToyArrows,
        this.StartWithTriggerArrows,
        this.StartWithPrismArrows
      });
      this.StartWithBombArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithLaserArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithBrambleArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithDrillArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithBoltArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithSuperBombArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithFeatherArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithRandomArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithToyArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithTriggerArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
      this.StartWithPrismArrows.AddLinks(new Variant[] {this.StartWithGhostArrows});
    }

    public static Subtexture patch_GetVariantIconFromName (string variantName)
    {
      bool isModVariant = patch_MatchVariants.ModVariants.Contains(variantName);

      if (isModVariant) {
        return patch_TFGame.ModAtlas ["variants/" + variantName [0].ToString ().ToLower (CultureInfo.InvariantCulture) + variantName.Substring (1)];
      } else {
        return TFGame.MenuAtlas ["variants/" + variantName [0].ToString ().ToLower (CultureInfo.InvariantCulture) + variantName.Substring (1)];
      }
    }

    public extern ArrowTypes orig_GetStartArrowType(int playerIndex, ArrowTypes randomType);
    public ArrowTypes patch_GetStartArrowType(int playerIndex, ArrowTypes randomType)
    {
      if (this.StartWithGhostArrows[playerIndex]) {
        return (ArrowTypes)MyGlobals.ArrowTypes.Ghost;
      }
      return orig_GetStartArrowType(playerIndex, randomType);
    }
  }
}