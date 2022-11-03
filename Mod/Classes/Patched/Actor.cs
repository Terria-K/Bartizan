using Microsoft.Xna.Framework;
using Monocle;
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

    public override bool CheckBelow ()
    {
      return base.CollideCheck (GameTags.Solid, base.Position - Vector2.UnitY) || (!this.IgnoreJumpThrus && base.CollideCheckOutside (GameTags.JumpThru, base.Position - Vector2.UnitY));
    }

    public override bool CheckBelow (int addX)
    {
      return base.CollideCheck (GameTags.Solid, base.Position - new Vector2 ((float)addX, 1f)) || (!this.IgnoreJumpThrus && base.CollideCheckOutside (GameTags.JumpThru, base.Position - new Vector2 ((float)addX, 1f)));
    }

    public override Platform GetBelow ()
    {
      Entity entity;
      if ((entity = base.CollideFirst (GameTags.Solid, base.Position - Vector2.UnitY)) != null) {
        return entity as Platform;
      }
      if (!this.IgnoreJumpThrus && (entity = base.CollideFirstOutside (GameTags.JumpThru, base.Position - Vector2.UnitY)) != null) {
        return entity as Platform;
      }
      return null;
    }
  }
}