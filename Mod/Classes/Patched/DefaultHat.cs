using Microsoft.Xna.Framework;
using Monocle;
using System;
using MonoMod;

namespace TowerFall
{
  public class patch_DefaultHat : DefaultHat
  {
    public patch_DefaultHat (ArcherData archerData, Allegiance allegiance, Vector2 position, bool flipped, Arrow arrow, int ownerIndex)
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
        this.image.Rotation += MathHelper.Clamp(Calc.AngleDiff(this.image.Rotation, 0f), -0.08726647f, 0.08726647f) * Engine.TimeMult;
        base.Speed.X = Calc.Approach (base.Speed.X, 0f, 0.1f * Engine.TimeMult);
      } else {
        base.Speed.X = Calc.Approach (base.Speed.X, this.sine.Value * 1f, 1f * Engine.TimeMult);
        this.image.Rotation = base.Speed.X * -40f * 0.0174532924f;
        if (patch_Level.IsAntiGrav()) {
          base.Speed.Y = Math.Max(base.Speed.Y + GetGravity() * Engine.TimeMult, GetMaxFall());
        } else {
          base.Speed.Y = Math.Min(base.Speed.Y + GetGravity() * Engine.TimeMult, GetMaxFall());
        }
      }
      base.MoveH(base.Speed.X * Engine.TimeMult, this.onCollideH);
      base.MoveV(base.Speed.Y * Engine.TimeMult, this.onCollideV);
      base_Update();
    }

    private float GetGravity()
    {
      return patch_Level.IsAntiGrav() ? -0.08f : 0.08f;
    }

    private float GetMaxFall()
    {
      return patch_Level.IsAntiGrav() ? -1.2f : 1.2f;
    }
  }
}