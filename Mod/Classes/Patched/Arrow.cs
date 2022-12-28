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

    public static Color GetColor(int index) {
      switch (index) {
        case (int)(MyGlobals.ArrowTypes.Ghost):
          return Calc.HexToColor("FFFFFF");
        default:
          return Arrow.Colors[index];
      }
    }

    public static Color GetColorB(int index) {
      switch (index) {
        case (int)(MyGlobals.ArrowTypes.Ghost):
          return Calc.HexToColor("3CBCFC");
        default:
          return Arrow.ColorsB[index];
      }
    }

    public static string GetArrowName(int index) {
      switch (index) {
        case (int)(MyGlobals.ArrowTypes.Ghost):
          return "GHOST";
        default:
          return Arrow.Names[index];
      }
    }

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

    private void my_ArrowUpdate()
    {
      if (!patch_Level.IsAntiGrav()) {
        orig_ArrowUpdate();
        return;
      }
      switch (this.State) {
        case ArrowStates.Gravity:
          if (this.State < ArrowStates.Stuck) {
            this.TravelFrames += Engine.TimeMult;
          }
          this.Speed.X = Calc.Approach (this.Speed.X, 0f, 0.03f * Engine.TimeMult);
          this.Speed.Y = Math.Max(this.Speed.Y + GetGravity() * Engine.TimeMult, GetMaxFall());
          if (this.Speed != Vector2.Zero) {
            this.Direction = Calc.Angle (this.Speed);
          }
          base.MoveH (this.Speed.X * Engine.TimeMult, this.onCollideH);
          base.MoveV (this.Speed.Y * Engine.TimeMult, this.onCollideV);
          break;
        default:
          orig_ArrowUpdate();
          break;
      }
    }

    public void patch_ArrowUpdate()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullySlowArrows) {
        // Engine.TimeMult *= AwfullySlowArrowMult;
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullySlowArrowMult, null);
        my_ArrowUpdate();
        // Engine.TimeMult /= AwfullySlowArrowMult;
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullySlowArrowMult, null);
      } else if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullyFastArrows) {
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullyFastArrowMult, null);
        my_ArrowUpdate();
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullyFastArrowMult, null);
      } else
        my_ArrowUpdate();
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

    private float GetGravity()
    {
      return patch_Level.IsAntiGrav() ? -0.2f : 0.2f;
    }

    private float GetMaxFall()
    {
      return patch_Level.IsAntiGrav() ? -5.5f : 5.5f;
    }
  }
}