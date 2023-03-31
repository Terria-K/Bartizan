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
    public enum MyOrbTypes {
      ReverseGravity = 5
    }

    public patch_OrbPickup(Vector2 position, Vector2 targetPosition, OrbTypes orbType) : base(position, targetPosition, orbType)
    {
      // no-op. MonoMod ignores this
    }

    [MonoModLinkTo("TowerFall.Pickup", "System.Void .ctor(Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Vector2)")]
    [MonoModRemove]
    public extern void base_ctor(Vector2 position, Vector2 targetPosition);

    public extern void orig_ctor(Vector2 position, Vector2 targetPosition, OrbTypes orbType);
    [MonoModConstructor]
    public void ctor(Vector2 position, Vector2 targetPosition, OrbTypes orbType)
    {
      if ((MyOrbTypes)orbType == MyOrbTypes.ReverseGravity) {
        Sprite<int> sprite = new Sprite<int> (patch_TFGame.ModAtlas["pickups/reverseGravityOrb"], 16, 16, 0);
        sprite.Origin = new Vector2(8f, 8f);
        sprite.Position = new Vector2(0f, 0f);
        // sprite.Add(0, 0); // Add 1 frame animation with id 0
        this.sprite = sprite;

        base_ctor(position, targetPosition);
        this.orbType = orbType;
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        base.Tag(GameTags.PlayerCollectible);
        this.border = new Image(TFGame.Atlas["pickups/orbBorder"], null);
        this.border.CenterOrigin();
        base.Add(this.border);
        // this.sprite.Play(0, false);
        base.Add(this.sprite);
        base.LightColor = Calc.Invert(OrbPickup.BorderColors[3]); // Hardcode white light
        this.border.Color = OrbPickup.BorderColors[3]; // Hardcode white border
        base.Tag(GameTags.PlayerGhostCollider);
      } else {
        orig_ctor(position, targetPosition, orbType);
        base.Tag(GameTags.PlayerGhostCollider);
      }
    }

    public void DoOrb(int playerIndex)
    {
      base.Level.Add(Cache.Create<LightFade> ().Init (this, null));
      base.DoCollectStats(playerIndex);
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
          base.Level.OrbLogic.DoLavaOrb(playerIndex);
          base.Level.Particles.Emit(Particles.LavaOrbCollect, 12, base.Position, Vector2.One * 4f);
          break;
        case OrbTypes.Space:
          Sounds.pu_spaceOrb.Stop(true);
          Sounds.pu_spaceOrb.Play(210f, 1f);
          base.Level.OrbLogic.DoSpaceOrb();
          base.Level.Particles.Emit(Particles.SpaceOrbCollect, 12, base.Position, Vector2.One * 4f);
          break;
        case (OrbTypes)MyOrbTypes.ReverseGravity:
          Sounds.pu_spaceOrb.Stop(true);
          Sounds.pu_spaceOrb.Play(210f, 1f);
          ((patch_OrbLogic)(base.Level.OrbLogic)).DoReverseGravityOrb();
          base.Level.Particles.Emit(Particles.SpaceOrbCollect, 12, base.Position, Vector2.One * 4f);
          break;
        case OrbTypes.Chaos:
          Sounds.pu_darkOrbCollect.Play (base.X, 1f);
          base.Level.OrbLogic.DoDarkOrb ();
          base.Level.OrbLogic.DoTimeOrb (true);
          base.Level.OrbLogic.DoLavaOrb (playerIndex);
          if (!SaveData.Instance.Options.RemoveScrollEffects) {
            base.Level.OrbLogic.DoSpaceOrbDelayed ();
          }
          ParticleType type = Calc.GiveMe(this.chaosIndex, Particles.DarkOrbCollect, Particles.TimeOrbCollect, Particles.LavaOrbCollect, Particles.SpaceOrbCollect);
          base.Level.Particles.Emit (type, 12, base.Position, Vector2.One * 4f);
          break;
      }
      base.RemoveSelf();
    }

    public override void OnPlayerGhostCollide(PlayerGhost ghost)
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).GhostItems) {
        this.DoOrb(ghost.PlayerIndex);
      }
    }

    public override void OnPlayerCollide (Player player)
    {
      this.DoOrb(player.PlayerIndex);
    }
  }
}
