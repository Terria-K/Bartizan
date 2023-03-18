using Microsoft.Xna.Framework;
using Monocle;
using System;
using MonoMod;

namespace TowerFall
{
  public class patch_WoodenHat : WoodenHat
  {
    public patch_WoodenHat (ArcherData archerData, Allegiance allegiance, Vector2 position, bool flipped, Arrow arrow, int ownerIndex)
      : base (archerData, allegiance, position, flipped, arrow, ownerIndex)
    {
      // no-op. MonoMod ignores this
    }

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    public void patch_Update ()
    {
      if (base.CheckBelow ()) {
        base.Speed.X = Calc.Approach(base.Speed.X, 0f, 0.2f * Engine.TimeMult);
        float radiansB = Calc.ShorterAngleDifference(this.image.Rotation, 0f, 3.14159274f);
        this.image.Rotation += MathHelper.Clamp(Calc.AngleDiff (this.image.Rotation, radiansB), -0.104719758f, 0.104719758f) * Engine.TimeMult;
      } else {
        if (patch_Level.IsAntiGrav()) {
          base.Speed.Y = Math.Max(base.Speed.Y + GetGravity() * ((Math.Abs (base.Speed.Y) <= 0.5f) ? 0.5f : 1f) * Engine.TimeMult, GetMaxFall());
        } else {
          base.Speed.Y = Math.Min(base.Speed.Y + GetGravity() * ((Math.Abs (base.Speed.Y) <= 0.5f) ? 0.5f : 1f) * Engine.TimeMult, GetMaxFall());
        }
        this.image.Rotation += this.spin * Engine.TimeMult;
      }
      base.MoveH(base.Speed.X * Engine.TimeMult, this.onCollideH);
      base.MoveV(base.Speed.Y * Engine.TimeMult, this.onCollideV);
      base_Update ();
    }

    private float GetGravity()
    {
      return patch_Level.IsAntiGrav() ? -0.3f : 0.3f;
    }

    private float GetMaxFall()
    {
      return patch_Level.IsAntiGrav() ? -3f : 3f;
    }
  }
}