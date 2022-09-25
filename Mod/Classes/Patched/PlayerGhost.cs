#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;
using Mod;
using System.Collections.Generic;

namespace TowerFall
{
  public class patch_PlayerGhost : PlayerGhost
  {
    public PlayerCorpse corpse;
    public bool HasSpeedBoots;
    public bool Invisible;
    public bool dodging;
    public PlayerGhostShield shield;
    public WrapHitbox shieldHitbox;
    public Counter shieldRegenCounter;
    public float InvisOpacity = 1f;

    public Scheduler scheduler;

    public patch_PlayerGhost(PlayerCorpse corpse) : base(corpse)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(PlayerCorpse corpse);
    [MonoModConstructor]
    public void ctor(PlayerCorpse corpse)
    {
      orig_ctor(corpse);
      this.corpse = corpse;
      this.Allegiance = corpse.Allegiance;
      this.shield = new PlayerGhostShield(this);
      this.shieldHitbox = new WrapHitbox(16f, 18f, -8f, -10f);
      base.Add(this.shield);
      this.shieldRegenCounter = new Counter(240);
    }

    public extern void orig_Die(int killerIndex, Arrow arrow, Explosion explosion, ShockCircle circle);
    public void patch_Die(int killerIndex, Arrow arrow, Explosion explosion, ShockCircle circle)
    {
      orig_Die(killerIndex, arrow, explosion, circle);
      ((patch_PlayerCorpse)(this.corpse)).hasGhost = false;

      List<Entity> players = Level.Session.CurrentLevel[GameTags.Player];
      for (int i = 0; i < players.Count; i++)
      {
        patch_Player player = (patch_Player)players[i];
        if (player.PlayerIndex == this.PlayerIndex)
        {
          i = players.Count;
        }
      }

      var mobLogic = this.Level.Session.RoundLogic as MobRoundLogic;
      if (mobLogic != null) {
        // Ghosts treated as players in crawl mode
        mobLogic.OnPlayerDeath(
          null, this.corpse, this.PlayerIndex, DeathCause.Arrow, // FIXME
          this.Position, killerIndex
        );
      }
      if (((patch_MatchVariants)base.Level.Session.MatchSettings.Variants).GottaBustGhosts) {
        ((patch_Session)base.Level.Session).OnPlayerGhostDeath(this, this.corpse);
      }
    }

    public extern void orig_OnPlayerGhostCollide(PlayerGhost ghost);
    public void patch_OnPlayerGhostCollide(PlayerGhost ghost)
    {
      if (((patch_MatchVariants)base.Level.Session.MatchSettings.Variants).GhostJoust)
      {
        if (this.dodging && (base.Allegiance == Allegiance.Neutral || ghost.Allegiance != base.Allegiance))
        {
          Vector2 value = Calc.SafeNormalize (ghost.Position - base.Position);
          if (((patch_PlayerGhost)ghost).dodging)
          {
            if (this.HasSpeedBoots && !((patch_PlayerGhost)ghost).HasSpeedBoots)
            {
            }
            else
            {
              this.Hurt(-value * 4f, 1, ghost.PlayerIndex, null, null, null);
            }
          }
          ghost.Hurt(value * 4f, 1, this.PlayerIndex, null, null, null);
        }
        else
        {
          orig_OnPlayerGhostCollide(ghost);
        }
      }
      else
      {
        orig_OnPlayerGhostCollide(ghost);
      }
    }

    public extern bool orig_OnArrowHit(Arrow arrow);
    public bool patch_OnArrowHit(Arrow arrow)
    {
      if (base.State == 0)
      {
        return false;
      }
      if ((bool)base.Level.Session.MatchSettings.Variants.NoFriendlyFire && ((base.Allegiance != Allegiance.Neutral && arrow.Allegiance == base.Allegiance) || arrow.PlayerIndex == this.PlayerIndex))
      {
        return false;
      }
      if (this.HasShield)
      {
        this.HasShield = false;
        this.Speed = arrow.Speed;
        arrow.EnterFallMode(true, false, true);
        if (arrow.PlayerIndex != -1)
        {
          base.Level.Session.MatchStats[arrow.PlayerIndex].ShieldsBroken += 1u;
        }
        return false;
      }
      else
      {
        return orig_OnArrowHit(arrow);
      }
    }

    public extern void orig_OnPlayerBounce(Player player);
    public void patch_OnPlayerBounce(Player player)
    {
      if (base.State != 0)
      {
        if (base.Allegiance != Allegiance.Neutral && player.Allegiance == base.Allegiance)
        {
          base.Speed.Y = 3f;
          this.sprite.Scale.X = 1.5f;
          this.sprite.Scale.Y = 0.5f;
        }
        else
        {
          if (this.HasShield)
          {
            this.HasShield = false;
            base.Speed.Y = 3f;
            this.sprite.Scale.X = 1.5f;
            this.sprite.Scale.Y = 0.5f;
            base.Level.Session.MatchStats[player.PlayerIndex].ShieldsBroken += 1u;
          }
          else
          {
            orig_OnPlayerBounce(player);
          }
        }
      }
    }

    public override void Hurt(Vector2 force, int damage, int killerIndex, Arrow arrow = null, Explosion explosion = null, ShockCircle shock = null)
    {
      if (shock && killerIndex == this.PlayerIndex) {
        // ShockCircle shouldn't kill friendly ghosts
        return;
      }

      if (this.HasShield)
      {
        this.HasShield = false;
        if (explosion && explosion.PlayerIndex != -1)
        {
          base.Level.Session.MatchStats[explosion.PlayerIndex].ShieldsBroken += 1u;
        }
      }
      else
      {
        base.Hurt(force, damage, killerIndex, arrow, explosion, shock);
      }
      this.Speed = force;
    }

    public extern void orig_Added();
    public void patch_Added()
    {
      orig_Added();

      ((patch_PlayerCorpse)(this.corpse)).spawningGhost = false;
      ((patch_PlayerCorpse)(this.corpse)).hasGhost = true;

      List<Entity> players = Level.Session.CurrentLevel[GameTags.Player];
      for (int i = 0; i < players.Count; i++)
      {
      patch_Player player = (patch_Player)players[i];
        if (player.PlayerIndex == this.PlayerIndex)
        {
        player.spawningGhost = false;
        i = players.Count;
        }
      }
    }

    public extern void orig_DodgeEnter();
    public void patch_DodgeEnter ()
    {
      this.dodging = true;
      orig_DodgeEnter();
    }

    public extern void orig_DodgeLeave();
    public void patch_DodgeLeave ()
    {
      this.dodging = false;
      orig_DodgeLeave();
    }

    public extern void orig_Update();
    public void patch_Update()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).RegeneratingShields[this.PlayerIndex])
      {
        if (this.HasShield)
        {
          this.shieldRegenCounter.Set(240);
        }
        else
        {
          this.shieldRegenCounter.Update();
          if (!(bool)this.shieldRegenCounter)
          {
            this.HasShield = true;
          }
        }
      }

      if (this.Invisible)
      {
        this.InvisOpacity = Math.Max(this.InvisOpacity - 0.02f * Engine.TimeMult, 0.2f);
      }
      else
      {
        this.InvisOpacity = Math.Min(this.InvisOpacity + 0.05f * Engine.TimeMult, 1f);
      }

      float ghostSpeed = 1f;
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).FastGhosts)
      {
        ghostSpeed *= 1.5f;
      }
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).GhostItems && this.HasSpeedBoots)
      {
        ghostSpeed *= 1.5f;
      }

      if (ghostSpeed > 1f)
      {
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * ghostSpeed, null);
        orig_Update();
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / ghostSpeed, null);
      } else {
        orig_Update();
      }
    }

    public bool HasShield
    {
      get
      {
        return this.shield.Visible;
      }
      set
      {
        if (this.shield.Visible != value)
        {
          if (value)
          {
            base.TargetCollider = this.shieldHitbox;
            this.shield.Gain();
          }
          else
          {
            base.TargetCollider = null;
            this.shield.Lose();
            base.Flash(30, null);
            TFGame.PlayerInputs[this.PlayerIndex].Rumble(0.5f, 20);
          }
        }
      }
    }

    public extern void orig_Render();
    public void patch_Render()
    {
      orig_Render();
      this.sprite.Color = this.blendColor * (0.9f + this.alphaSine.Value * 0.1f) * this.InvisOpacity;
      this.halo.Color = Color.White * this.InvisOpacity;

      // I looked at this code and thought "why not just call base first and then do the code" so trying that instead
      // this.sprite.Color = this.blendColor * (0.9f + this.alphaSine.Value * 0.1f) * this.InvisOpacity;
      // this.sprite.Scale.X = Math.Abs (this.sprite.Scale.X) * (float)this.Facing;
      // this.halo.Color = Color.White * this.InvisOpacity;
      // // From LevelEntity::Render(), base of base of base...
      // this.DoWrapRender ();
      // if (this.ScreenWrap) {
      //   int num;
      //   if (base.X > 210f) {
      //     base.X -= 420f;
      //     this.DoWrapRender ();
      //     base.X += 420f;
      //     num = -1;
      //   } else {
      //     base.X += 420f;
      //     this.DoWrapRender ();
      //     base.X -= 420f;
      //     num = 1;
      //   }
      //   int num2;
      //   if (base.Y > 120f) {
      //     base.Y -= 240f;
      //     this.DoWrapRender ();
      //     base.Y += 240f;
      //     num2 = -1;
      //   } else {
      //     base.Y += 240f;
      //     this.DoWrapRender ();
      //     base.Y -= 240f;
      //     num2 = 1;
      //   }
      //   Vector2 position = base.Position;
      //   base.Position += new Vector2 ((float)(420 * num), (float)(240 * num2));
      //   this.DoWrapRender ();
      //   base.Position = position;
      // }
    }
  }
}
