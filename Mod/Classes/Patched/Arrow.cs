#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0108 // 'member1' hides inherited member 'member2'. Use the new keyword if hiding was intended.

using Mod;
using Monocle;
using MonoMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TowerFall
{
  public abstract class patch_Arrow : Arrow
  {
    const float AwfullySlowArrowMult = 0.2f;
    const float AwfullyFastArrowMult = 3.0f;

    [MonoModPublic]
    public bool squished;

    [MonoModPublic]
    public bool didPopThroughJumpThru;

    [MonoModPublic]
    public Counter cannotPickupCounter;

    [MonoModPublic]
    public Counter cannotHitEnemiesCounter;

    [MonoModPublic]
    public Counter cannotCatchCounter;

    [MonoModPublic]
    public WrapHitbox otherArrowHitbox;

    public static readonly Color[] ModColors = new Color[12] {
      Calc.HexToColor ("F7EAC3"), // Regular
      Calc.HexToColor ("F8B800"), // Bomb
      Calc.HexToColor ("F8B800"), // Super Bomb
      Calc.HexToColor ("B8F818"), // Laser
      Calc.HexToColor ("F87858"), // Bramble
      Calc.HexToColor ("8EE8FF"), // Drill
      Calc.HexToColor ("00FF4C"), // Bolt
      Calc.HexToColor ("FF6DFA"), // Toy
      Calc.HexToColor ("BC70FF"), // Feather
      Calc.HexToColor ("1BB7EE"), // Trigger
      Calc.HexToColor ("DB4ADB"), // Prism
      Calc.HexToColor ("FFFFFF")  // Ghost
    };

    public static readonly Color[] ModColorsB = new Color[12] {
      Calc.HexToColor ("FFFFFF"),
      Calc.HexToColor ("F7D883"),
      Calc.HexToColor ("F7D883"),
      Calc.HexToColor ("D0F76C"),
      Calc.HexToColor ("F7B09E"),
      Calc.HexToColor ("D8F7FF"),
      Calc.HexToColor ("00D33B"),
      Calc.HexToColor ("FFB5FC"),
      Calc.HexToColor ("D5A5FF"),
      Calc.HexToColor ("56D4FF"),
      Calc.HexToColor ("FF52FF"),
      Calc.HexToColor ("3CBCFC"), // Ghost
    };

    public static readonly string[] ModNames = new string[12] {
      "+2",
      "BOMB",
      "SUPER BOMB",
      "LASER",
      "BRAMBLE",
      "DRILL",
      "BOLT",
      "TOY",
      "FEATHER",
      "TRIGGER",
      "PRISM",
      "GHOST",
    };

    public new static void Initialize ()
    {
      // Same as original, with Arrow.cached size extended for mod arrow types
      int arrowTypes = Calc.EnumLength(typeof(ArrowTypes)) + Calc.EnumLength(typeof(MyGlobals.ArrowTypes));
      Arrow.cached = new Stack<Arrow>[arrowTypes];
      for (int i = 0; i < Arrow.cached.Length; i++) {
        Arrow.cached[i] = new Stack<Arrow> ();
      }
    }

    public extern void orig_Added();
    public void patch_Added()
    {
      orig_Added();

      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullyFastArrows) {
        this.NormalHitbox = new WrapHitbox(6f, 3f, -1f, -1f);
        this.otherArrowHitbox = new WrapHitbox(12f, 4f, -2f, -2f);
      }
    }

    public extern void orig_ArrowUpdate();
    public void patch_ArrowUpdate()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullySlowArrows) {
        // Engine.TimeMult *= AwfullySlowArrowMult;
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullySlowArrowMult, null);
        orig_ArrowUpdate();
        // Engine.TimeMult /= AwfullySlowArrowMult;
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullySlowArrowMult, null);
      } else if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullyFastArrows) {
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullyFastArrowMult, null);
        orig_ArrowUpdate();
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullyFastArrowMult, null);
      } else
        orig_ArrowUpdate();
    }

    public new static Arrow Create (ArrowTypes type, LevelEntity owner, Vector2 position, float direction, int? overrideCharacterIndex = default(int?), int? overridePlayerIndex = default(int?))
    {
      // Same as original, with cases for mod arrow types added
      Arrow arrow;
      if (Arrow.cached [(int)type].Count > 0) {
        arrow = Arrow.cached [(int)type].Pop ();
      } else {
        switch (type) {
        default:
          throw new Exception ("Arrow Type not recognized");
        case ArrowTypes.Normal:
          arrow = new DefaultArrow();
          break;
        case ArrowTypes.Bomb:
          arrow = new BombArrow();
          break;
        case ArrowTypes.SuperBomb:
          arrow = new SuperBombArrow();
          break;
        case ArrowTypes.Laser:
          arrow = new LaserArrow();
          break;
        case ArrowTypes.Bramble:
          arrow = new BrambleArrow();
          break;
        case ArrowTypes.Drill:
          arrow = new DrillArrow();
          break;
        case ArrowTypes.Bolt:
          arrow = new BoltArrow();
          break;
        case ArrowTypes.Toy:
          arrow = new ToyArrow();
          break;
        case ArrowTypes.Feather:
          arrow = new FeatherArrow();
          break;
        case ArrowTypes.Trigger:
          arrow = new TriggerArrow();
          break;
        case ArrowTypes.Prism:
          arrow = new PrismArrow();
          break;
        case (ArrowTypes)MyGlobals.ArrowTypes.Ghost:
          arrow = new GhostArrow();
          break;
        }
      }
      arrow.OverrideCharacterIndex = overrideCharacterIndex;
      arrow.OverridePlayerIndex = overridePlayerIndex;
      arrow.Init (owner, position, direction);
      return arrow;
    }
  }
}