#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Monocle;
using MonoMod;
using Microsoft.Xna.Framework;

namespace TowerFall
{
  public class patch_SpeedBootsPickup : SpeedBootsPickup
  {
    public patch_SpeedBootsPickup(Vector2 position, Vector2 targetPosition)
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
        if (!g.HasSpeedBoots)
        {
          Sounds.pu_speedBoots.Play(base.X, 1f);
          g.HasSpeedBoots = true;
          base.Level.Layers[g.LayerIndex].Add(new LightFade().Init(this, null));
          for (int i = 0; i < 30; i++)
          {
            base.Level.Particles.Emit(Particles.SpeedBootsPickup, 1, base.Position, Vector2.One * 3f, 6.28318548f * (float)i / 30f);
          }
          base.Level.Particles.Emit(Particles.SpeedBootsPickup2, 18, base.Position, Vector2.One * 4f);
          base.DoCollectStats(g.PlayerIndex);
          base.RemoveSelf();
        }
      }
    }
  }
}
