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

    // Update position when player ducks
    public void patch_PlayerUpdate (float yPos)
    {
      this.sprite.Y = patch_Level.IsReverseGrav() ? yPos * -1 : yPos;
    }

    public void FlipSprite()
    {
      bool isRotated = this.sprite.FlipY;
      bool IsReverseGrav = patch_Level.IsReverseGrav();
      if (!isRotated && IsReverseGrav) {
        this.sprite.FlipY = true;
        this.sprite.Origin.Y -= 2;
        this.sprite.Origin.X += 1f;
      } else if (isRotated && !IsReverseGrav) {
        this.sprite.FlipY = false;
        this.sprite.Origin.Y += 2;
        this.sprite.Origin.X -= 1f;
      }
    }
  }
}