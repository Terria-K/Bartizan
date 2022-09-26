#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall
{
  public class patch_MirrorPickup : MirrorPickup
  {
    public patch_MirrorPickup(Vector2 position, Vector2 targetPosition)
      : base(position, targetPosition)
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
        if (!g.Invisible)
        {
          base.Level.Layers[g.LayerIndex].Add(new LightFade().Init(this, null));
          g.Invisible = true;
          Sounds.pu_invisible.Play(base.X, 1f);
          base.RemoveSelf();
        }
      }
    }
  }
}
