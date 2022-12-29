#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Monocle;
using MonoMod;
using System;

namespace TowerFall
{
  public class patch_PlayerWings : PlayerWings
  {
    public patch_PlayerWings(LevelEntity owner) : base (owner)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_Render();
    public void patch_Render()
    {
      orig_Render();
      FlipSprite();
    }

    public void FlipSprite()
    {
      bool isRotated = this.sprite.FlipY;
      bool isAntiGrav = patch_Level.IsAntiGrav();
      if (!isRotated && isAntiGrav) {
        this.sprite.FlipY = true;
        this.sprite.Origin.Y -= 6;
        this.sprite.Origin.X -= 1;
      } else if (isRotated && !isAntiGrav) {
        this.sprite.FlipY = false;
        this.sprite.Origin.Y += 6;
        this.sprite.Origin.X += 1;
      }
    }
  }
}