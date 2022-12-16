using Microsoft.Xna.Framework;
using Mod;
using Monocle;
using MonoMod;
using System;

namespace TowerFall
{
  public class patch_Lantern : Lantern
  {
    public patch_Lantern (Vector2 position)
      : base (position)
    {
      // No-op. MonoMod ignores this
    }

    public bool IsAntiGrav()
    {
      return patch_Level.IsAntiGrav();
    }

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    public void patch_Update ()
    {
      if (this.falling) {
        if (!base.CheckBelow ()) {
          this.vSpeed = Math.Min (this.vSpeed + GetGravity() * Engine.TimeMult, GetMaxFall());
          this.sprite.Rotation += MathHelper.Clamp (Calc.AngleDiff (this.sprite.Rotation, -1.57079637f), -0.08726647f, 0.08726647f) * Engine.TimeMult;
        }
        base.MoveV (this.vSpeed * Engine.TimeMult, this.onFallCollide);
      } else if (base.CollideCheck (GameTags.Player)) {
        this.Fall ();
      }
      if (!this.dead && base.Level.OnInterval (5)) {
        base.Level.ParticlesBG.Emit (Particles.BGTorch, 1, base.Position + new Vector2 (0f, 8f), new Vector2 (1f, 0f));
      }
      base_Update ();
    }

    public float GetGravity()
    {
      return IsAntiGrav() ? -0.2f : 0.2f;
    }

    public float GetMaxFall()
    {
      return IsAntiGrav() ? -4f : 4f;
    }
 }
}