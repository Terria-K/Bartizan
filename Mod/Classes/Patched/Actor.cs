using Microsoft.Xna.Framework;
using Monocle;
using Mod;
using System;

namespace TowerFall
{
  public class patch_Actor : Actor
  {
    public patch_Actor (Vector2 position)
      : base (position)
    {
      // No-op. MonoMod ignores this
    }

    public bool IsReverseGrav()
    {
      return patch_Level.IsReverseGrav();
    }

    public override bool CheckBelow ()
    {
      return IsReverseGrav()
        ? base.CollideCheck (GameTags.Solid, base.Position - Vector2.UnitY) || (!this.IgnoreJumpThrus && base.CollideCheckOutside (GameTags.JumpThru, base.Position - Vector2.UnitY))
        : base.CollideCheck (GameTags.Solid, base.Position + Vector2.UnitY) || (!this.IgnoreJumpThrus && base.CollideCheckOutside (GameTags.JumpThru, base.Position + Vector2.UnitY));
    }

    public override bool CheckBelow (int addX)
    {
      return IsReverseGrav()
        ? base.CollideCheck (GameTags.Solid, base.Position - new Vector2 ((float)addX, 1f)) || (!this.IgnoreJumpThrus && base.CollideCheckOutside (GameTags.JumpThru, base.Position - new Vector2 ((float)addX, 1f)))
        : base.CollideCheck (GameTags.Solid, base.Position + new Vector2 ((float)addX, 1f)) || (!this.IgnoreJumpThrus && base.CollideCheckOutside (GameTags.JumpThru, base.Position + new Vector2 ((float)addX, 1f)));
    }

    public override Platform GetBelow ()
    {
      if (IsReverseGrav()) {
        Entity entity;
        if ((entity = base.CollideFirst (GameTags.Solid, base.Position - Vector2.UnitY)) != null) {
          return entity as Platform;
        }
        if (!this.IgnoreJumpThrus && (entity = base.CollideFirstOutside (GameTags.JumpThru, base.Position - Vector2.UnitY)) != null) {
          return entity as Platform;
        }
        return null;
      } else {
        Entity entity;
        if ((entity = base.CollideFirst (GameTags.Solid, base.Position + Vector2.UnitY)) != null) {
          return entity as Platform;
        }
        if (!this.IgnoreJumpThrus && (entity = base.CollideFirstOutside (GameTags.JumpThru, base.Position + Vector2.UnitY)) != null) {
          return entity as Platform;
        }
        return null;
      }
    }

    public bool patch_IsRiding(Solid solid)
    {
      if (patch_Level.IsReverseGrav()) {
        return base.CollideCheck(solid, base.X, base.Y - (base.Height / 2));
      } else {
        return base.CollideCheck(solid, base.X, base.Y + 1f);
      }
    }
  }
}