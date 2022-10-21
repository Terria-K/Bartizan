#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall
{
  public class patch_ShieldPickup : ShieldPickup
  {
    public patch_ShieldPickup(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Vector2 position, Vector2 targetPosition);
    [MonoModConstructor]
    public void ctor(Vector2 position, Vector2 targetPosition)
    {
      orig_ctor(position, targetPosition);
      base.Tag(GameTags.PlayerGhostCollider);
    }


    public override void OnPlayerGhostCollide(PlayerGhost ghost)
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).GhostItems)
      {
        patch_PlayerGhost g = (patch_PlayerGhost)ghost;
        if (!g.HasShield)
        {
          base.Level.Layers[g.LayerIndex].Add(new LightFade().Init(this, null));
          base.DoCollectStats(g.PlayerIndex);
          g.HasShield = true;
          base.RemoveSelf();
        }
      }
    }
  }
}
