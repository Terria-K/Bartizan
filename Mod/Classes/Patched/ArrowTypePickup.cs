#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Microsoft.Xna.Framework;
using Monocle;
using Mod;

using System;

namespace TowerFall
{
  class patch_ArrowTypePickup : ArrowTypePickup
  {
    public patch_ArrowTypePickup(Vector2 position, Vector2 targetPosition, ArrowTypes type)
      : base (position, targetPosition, type)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Vector2 position, Vector2 targetPosition, ArrowTypes type);
    [MonoModConstructor]
    public void ctor(Vector2 position, Vector2 targetPosition, ArrowTypes type)
    {
      orig_ctor(position, targetPosition, type);
      if (type == (ArrowTypes)(MyGlobals.ArrowTypes.Ghost)) {
        this.graphic = new Image(patch_TFGame.ModAtlas["pickups/ghostArrows"], null);
        this.graphic.CenterOrigin ();
        base.Add (this.graphic);
      }
    }

    public extern void orig_PlaySound();
    public void patch_PlaySound ()
    {
      if (this.arrowType == (ArrowTypes)(MyGlobals.ArrowTypes.Ghost)) {
        Sounds.en_ghostDodge.Play(base.X, 1f);
      } else {
        orig_PlaySound();
      }
    }

    public void patch_OnPlayerCollide (Player player)
    {
      // Original code but call static methods that know about mod arrows
      if (player.CollectArrows (this.arrowType, this.arrowType)) {
        base.DoCollectStats(player.PlayerIndex);
        base.RemoveSelf();
        Color color = patch_Arrow.GetColor((int)this.arrowType);
        Color color2 = patch_Arrow.GetColorB((int)this.arrowType);
        base.Level.Add(new FloatText (base.Position + new Vector2 (0f, -10f), patch_Arrow.GetArrowName((int)this.arrowType), color, color2, 1f, 1f, false));
        base.Level.Add(new FloatText (base.Position + new Vector2 (0f, -3f), "ARROWS", color, color2, 1f, 1f, false));
        base.Level.Add(Cache.Create<LightFade> ().Init (this, null));
        this.PlaySound();
      }
    }
  }
}