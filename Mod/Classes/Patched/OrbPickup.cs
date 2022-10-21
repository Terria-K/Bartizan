#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using TowerFall;
using Monocle;
using MonoMod;
using Microsoft.Xna.Framework;
using System;

namespace TowerFall
{
  public class patch_OrbPickup : OrbPickup
  {
    public patch_OrbPickup(Vector2 position, Vector2 targetPosition, OrbTypes orbType) : base(position, targetPosition, orbType)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Vector2 position, Vector2 targetPosition, OrbTypes orbType);
    [MonoModConstructor]
    public void ctor(Vector2 position, Vector2 targetPosition, OrbTypes orbType)
    {
      orig_ctor(position, targetPosition, orbType);
      base.Tag(GameTags.PlayerGhostCollider);
    }

    public override void OnPlayerGhostCollide(PlayerGhost ghost)
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).GhostItems)
      {
        base.Level.Layers[ghost.LayerIndex].Add(new LightFade().Init(this, null));
        base.DoCollectStats(ghost.PlayerIndex);
        switch (this.orbType)
        {
          case OrbTypes.Dark:
            Sounds.pu_darkOrbCollect.Play(base.X, 1f);
            base.Level.OrbLogic.DoDarkOrb();
            base.Level.Particles.Emit(Particles.DarkOrbCollect, 12, base.Position, Vector2.One * 4f);
            break;
          case OrbTypes.Time:
            Sounds.pu_darkOrbCollect.Play(base.X, 1f);
            base.Level.OrbLogic.DoTimeOrb(false);
            base.Level.Particles.Emit(Particles.TimeOrbCollect, 12, base.Position, Vector2.One * 4f);
            break;
          case OrbTypes.Lava:
            Sounds.pu_lava.Play(base.X, 1f);
            base.Level.OrbLogic.DoLavaOrb(ghost.PlayerIndex);
            base.Level.Particles.Emit(Particles.LavaOrbCollect, 12, base.Position, Vector2.One * 4f);
            break;
          case OrbTypes.Space:
            Sounds.pu_spaceOrb.Stop(true);
            Sounds.pu_spaceOrb.Play(210f, 1f);
            base.Level.OrbLogic.DoSpaceOrb();
            base.Level.Particles.Emit(Particles.SpaceOrbCollect, 12, base.Position, Vector2.One * 4f);
            break;
          case OrbTypes.Chaos:
            Sounds.pu_darkOrbCollect.Play (base.X, 1f);
            base.Level.OrbLogic.DoDarkOrb ();
            base.Level.OrbLogic.DoTimeOrb (true);
            base.Level.OrbLogic.DoLavaOrb (ghost.PlayerIndex);
            if (!SaveData.Instance.Options.RemoveScrollEffects) {
              base.Level.OrbLogic.DoSpaceOrbDelayed ();
            }
            ParticleType type = Calc.GiveMe(this.chaosIndex, Particles.DarkOrbCollect, Particles.TimeOrbCollect, Particles.LavaOrbCollect, Particles.SpaceOrbCollect);
            base.Level.Particles.Emit (type, 12, base.Position, Vector2.One * 4f);
            break;
        }
        base.RemoveSelf();
      }
    }
  }
}
