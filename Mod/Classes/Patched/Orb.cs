#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;
using System.Xml;

namespace TowerFall
{
  public class patch_Orb : Orb
  {
    public patch_Orb(Vector2 position, bool explodes) : base(position, explodes)
    {
      // no-op. MonoMod ignores this
    }

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    public void patch_Update()
    {
      base_Update();
      if (this.falling) {
        if (patch_Level.IsAntiGrav()) {
          this.vSpeed = Math.Max(this.vSpeed + GetGravity() * Engine.TimeMult, GetMaxFall());
        } else {
          this.vSpeed = Math.Min(this.vSpeed + GetGravity() * Engine.TimeMult, GetMaxFall());
        }
        base.MoveV(this.vSpeed * Engine.TimeMult, this.shatter);
        Orb orb = base.CollideFirst (GameTags.Orb) as Orb;
        if (orb != null) {
          this.Shatter (null);
          orb.Fall (this.ownerIndex);
          return;
        }
        Enemy enemy = base.CollideFirst (GameTags.Enemy) as Enemy;
        if ((bool)enemy && enemy != this.CannotHit) {
          this.Shatter (null);
          enemy.Hurt (Vector2.UnitY * 3f, 1, this.ownerIndex, null, null, null);
          return;
        }
        TreasureChest treasureChest = base.CollideFirst (GameTags.TreasureChest) as TreasureChest;
        if (treasureChest != null) {
          this.Shatter (null);
          treasureChest.OpenChest (this.ownerIndex);
          return;
        }
      } else {
        base.Y = this.startY + this.sine.Value * 2f;
      }
      if (base.Level.OnInterval (6)) {
        base.Level.Particles.Emit (this.explodes ? Particles.ExplodingOrbGlow : Particles.OrbGlow, 1, base.Position, Vector2.One * 6f);
      }
      if ((bool)this.CannotHit && Vector2.DistanceSquared (base.Position, this.CannotHit.Position) > 400f) {
        this.CannotHit = null;
      }
      if (this.image != null) {
        this.image.BlurSpeed = new Vector2 (0f, 0f - this.vSpeed);
        this.image.SetBlurData (0, this.vSpeed / 4f * 0.3f, 2f);
        this.image.SetBlurData (1, this.vSpeed / 4f * 0.1f, 4f);
      }
      if (this.sprite != null) {
        this.sprite.BlurSpeed = new Vector2 (0f, 0f - this.vSpeed);
        this.sprite.SetBlurData (0, this.vSpeed / 4f * 0.3f, 2f);
        this.sprite.SetBlurData (1, this.vSpeed / 4f * 0.1f, 4f);
      }
    }

    private float GetGravity()
    {
      return patch_Level.IsAntiGrav() ? -0.2f : 0.2f;
    }

    private float GetMaxFall()
    {
      return patch_Level.IsAntiGrav() ? -4f : 4f;
    }
  }
}
