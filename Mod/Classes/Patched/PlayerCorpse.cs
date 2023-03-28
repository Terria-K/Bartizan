#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Mod;
using Monocle;
using System.Collections.Generic;
using System;
using MonoMod;

namespace TowerFall
{
  public class patch_PlayerCorpse : PlayerCorpse
  {
    public bool reviverAdded;
    public bool spawningGhost;
    public bool hasGhost;

    public patch_PlayerCorpse (PlayerCorpse.EnemyCorpses enemyCorpse, Vector2 position, Facing facing, int killerIndex) : base (enemyCorpse.ToString (), Allegiance.Neutral, position, facing, -1, killerIndex)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(PlayerCorpse.EnemyCorpses enemyCorpse, Vector2 position, Facing facing, int killerIndex);
    [MonoModConstructor]
    public void ctor(PlayerCorpse.EnemyCorpses enemyCorpse, Vector2 position, Facing facing, int killerIndex)
    {
      orig_ctor(enemyCorpse, position, facing, killerIndex);

      this.sprite.FlipX = (this.Facing == Facing.Left) && !IsAntiGrav();
      this.flashSprite.FlipX = (this.Facing == Facing.Left) && !IsAntiGrav();
    }

    public extern void orig_DoWrapRender();
    public override void DoWrapRender()
    {
      orig_DoWrapRender();

      // Uncomment to see hitboxes
      // this.DebugRender();
    }

    public void FlipSprite()
    {
      bool isRotated = this.sprite.Rotation != 0;
      if (IsAntiGrav() && !isRotated) {
        this.sprite.FlipX = this.Facing == Facing.Right;
        this.sprite.Rotation = 3.1415926536f;
        this.sprite.Position.Y -= 8f;
        if (this.flashSprite != null) {
          this.flashSprite.FlipX = this.Facing == Facing.Right;
          this.flashSprite.Rotation = 3.1415926536f;
          this.flashSprite.Position.Y -= 8f;
        }
        while (base.CollideCheck(GameTags.Solid, base.Position + Vector2.UnitY)) {
          base.Position -= Vector2.UnitY;
        }

        SetBuriedForAnimID(this.sprite.CurrentAnimID);

      } else if (!IsAntiGrav() && isRotated) {
        this.sprite.FlipX = this.Facing == Facing.Left;
        this.sprite.Rotation = 0;
        this.sprite.Position.Y += 8f;
        if (this.flashSprite != null) {
          this.flashSprite.FlipX = this.Facing == Facing.Left;
          this.flashSprite.Rotation = 0;
          this.flashSprite.Position.Y += 8f;
        }

        SetBuriedForAnimID(this.sprite.CurrentAnimID);

        while (base.CollideCheck(GameTags.Solid, base.Position + Vector2.UnitY)) {
          base.Position += Vector2.UnitY;
        }
      }
    }

    private void SetBuriedForAnimID(string animID)
    {
      switch (animID) {
        case "ground":
          this.SetBuried(new Vector2(7f, 17f), IsAntiGrav() ? Calc.ReflectAngle(1.57079637f, 0f) : 1.57079637f);
          break;
        case "pinned":
          this.SetBuried(new Vector2(4f, 13f), IsAntiGrav() ? Calc.ReflectAngle(3.14159274f, 0f) : 3.14159274f);
          break;
        case "ledge":
          this.SetBuried(new Vector2(9f, 12f), IsAntiGrav() ? Calc.ReflectAngle(0.7853982f, 0f) : 0.7853982f);
          break;
        case "flying":
          this.SetBuried(new Vector2(4f, 13f), IsAntiGrav() ? Calc.ReflectAngle(3.14159274f, 0f) : 3.14159274f);
          break;
        case "slouched":
          this.SetBuried(new Vector2(6f, 16f), IsAntiGrav() ? Calc.ReflectAngle(2.3561945f, 0f) : 2.3561945f);
          break;
        case "fall":
          this.SetBuried(new Vector2(6f, 14f), IsAntiGrav() ? Calc.ReflectAngle(2.3561945f, 0f) : 2.3561945f);
          break;
      }
    }

    public extern void orig_Added();
    public void patch_Added()
    {
      this.spawningGhost = true;
      this.hasGhost = false;
      orig_Added();

      if (this.PlayerIndex != -1) {
        if (base.Level.Session.MatchSettings.Mode == Modes.TeamDeathmatch && base.Level.Session.MatchSettings.Variants.TeamRevive) {
          reviverAdded = true;
        }
      }

      FlipSprite();
    }

    public extern void orig_DieByArrow(Arrow arrow, int ledge);
    public void patch_DieByArrow(Arrow arrow, int ledge)
    {
      if (this.CanDoPrismHit (arrow)) {
        this.spawningGhost = false;
      }
      orig_DieByArrow(arrow, ledge);

      if (IsAntiGrav() && arrow.Speed.X != 0f) {
        this.sprite.FlipX = (this.Facing == Facing.Right);
      }
    }

    public extern void orig_DieByExplosion(Explosion explosion, Vector2 normal);
    public void patch_DieByExplosion(Explosion explosion, Vector2 normal)
    {
      orig_DieByExplosion(explosion, normal);
      if (IsAntiGrav() && this.Speed.X != 0f) {
        this.sprite.FlipX = (this.Facing == Facing.Right);
      }
    }

    public extern void orig_DieBySquish(Vector2 direction, bool ducking);
    public void patch_DieBySquish(Vector2 direction, bool ducking)
    {
      orig_DieBySquish(direction, ducking);
      this.sprite.FlipX = (this.Facing == Facing.Left) && !IsAntiGrav();
    }

    public void patch_DieByJumpedOn ()
    {
      this.Speed.X = 0f;
      this.Speed.Y = IsAntiGrav() ? -2.8f : 2.8f;
      this.DeathAngle = 1.57079637f;
    }

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    public void patch_Update()
    {
      bool flag = false;
      this.sprite.FlipY = false;
      this.inMud = base.CollideCheck (GameTags.Mud);
      if (this.PrismHit && base.Level.OnInterval (1)) {
        base.Level.ParticlesFG.Emit (Particles.PrismCorpse, 2, this.Position, Vector2.One * 6f);
      }
      if (this.isPlayer && this.drawOpacity < 1f) {
        this.drawOpacity = Math.Min (this.drawOpacity + 0.02f * Engine.TimeMult, 1f);
      }
      if (this.Pinned && this.ArrowCushion.Count == 0) {
        this.Pinned = false;
      }
      if (this.Reviving) {
        if (base.Level.OnInterval (4)) {
          this.reviveDraw = !this.reviveDraw;
        }
      } else {
        this.reviveDraw = false;
      }
      if (this.dodgeTooLateCounter) {
        this.dodgeTooLateCounter.Update ();
        if (this.PlayerIndex != -1 && TFGame.PlayerInputs [this.PlayerIndex] != null && TFGame.PlayerInputs [this.PlayerIndex].GetState ().DodgePressed) {
          MatchStats[] expr_18E_cp_0 = base.Level.Session.MatchStats;
          int expr_18E_cp_1 = this.PlayerIndex;
          expr_18E_cp_0 [expr_18E_cp_1].DodgesTooLate = expr_18E_cp_0 [expr_18E_cp_1].DodgesTooLate + 1u;
          this.dodgeTooLateCounter.Set (0);
        }
      }
      if (base.CollideCheck (GameTags.HotCoals, this.Position + Vector2.UnitY)) {
        this.fire.Start ();
      }
      if (this.PlayerIndex != -1 && this.CanExplode && !this.PrismHit) {
        int num = 60;
        bool flag2;
        if (base.Level.Session.MatchSettings.Variants.TriggerCorpses [this.PlayerIndex]) {
          num = 40;
          InputState state = TFGame.PlayerInputs [this.PlayerIndex].GetState ();
          flag2 = (state.ShootCheck || state.DodgeCheck || state.JumpCheck);
          if (flag2 && this.explodingCounter < 0.75f) {
            TFGame.PlayerInputs [this.PlayerIndex].Rumble (0.2f, 2);
          }
        } else {
          flag2 = true;
        }
        if (!flag2 || this.Squished != Vector2.Zero) {
          if (BombPickup.SFXNewest == this) {
            BombPickup.SFXNewest = null;
            Sounds.sfx_bombChestLoop.Stop (true);
          }
          this.flashSprite.Visible = false;
          this.explodingCounter = Math.Max (0f, this.explodingCounter - 1f / (float)num * Engine.TimeMult);
        } else {
          if (base.Scene.OnInterval (5)) {
            this.flashSprite.Visible = !this.flashSprite.Visible;
          }
          this.explodingCounter = Math.Min (1f, this.explodingCounter + 1f / (float)num * Engine.TimeMult);
          if (this.explodingCounter >= 1f) {
            this.Explode (null);
          } else if (BombPickup.SFXNewest != this) {
            BombPickup.SFXNewest = this;
            Sounds.sfx_bombChestLoop.Play (base.X, 1f);
          }
        }
      }
      if (this.Squished != Vector2.Zero) {
        if (this.Squished == Vector2.UnitY) {
          this.sprite.Play ("ground", false);
          SetBuriedForAnimID("ground");
        } else if (this.Squished == -Vector2.UnitY) {
          this.sprite.Play ("ground", false);
          this.sprite.FlipY = true;
          SetBuriedForAnimID("ground");
        } else {
          this.sprite.Play ("pinned", false);
          SetBuriedForAnimID("pinned");
        }
        this.Speed = Vector2.Zero;
        if (base.CollideCheck (GameTags.Solid)) {
          this.squishedCounter.Set (30);
          if (this.Squished.X != 0f) {
            this.sprite.Scale = new Vector2 (0.6f, 1.2f);
          } else {
            this.sprite.Scale = new Vector2 (1.6f, 0.6f);
          }
        } else {
          if (this.Squished.X != 0f) {
            this.sprite.Scale.Y = Calc.Approach (this.sprite.Scale.Y, 1f, 0.02f * Engine.TimeMult);
          } else {
            this.sprite.Scale.X = Calc.Approach (this.sprite.Scale.X, 1f, 0.02f * Engine.TimeMult);
          }
          this.squishedCounter.Update ();
          if (!this.squishedCounter || !base.CollideCheck (GameTags.Solid, this.Position + this.Squished)) {
            this.squishedCounter.Set (0);
            if (this.Squished.X != 0f) {
              this.Speed = -this.Squished * 0.6f;
              this.sprite.Scale = new Vector2 (1.4f, 0.6f);
            } else {
              this.sprite.Scale = new Vector2 (0.6f, 1.4f);
            }
            this.Collidable = (this.Pushable = true);
            this.Squished = Vector2.Zero;
            Sounds.char_squishPop.Play (base.X, 1f);
          }
        }
      } else if (this.Ledge != 0) {
        this.sprite.Play ("ledge", false);
        SetBuriedForAnimID("ledge");
        this.ArrowCushion.Update ();
        if (!base.CollideCheck (GameTags.Solid, base.X + (float)this.Ledge, base.Y)) {
          this.Ledge = 0;
        }
      } else {
        bool flag3 = base.CheckBelow ();
        bool flag4 = flag3 && !base.Level.Session.MatchSettings.Variants.NoSlipping [this.PlayerIndex] && (base.Level.Session.MatchSettings.Variants.SlipperyFloors [this.PlayerIndex] || base.CollideCheck (GameTags.Ice, this.Position + Vector2.UnitY));
        if (this.againstWall && !base.CollideCheck (GameTags.Solid, this.Position + Vector2.UnitX * (float)(-(float)this.Facing))) {
          this.Pinned = (this.againstWall = false);
        }
        if (flag3) {
          this.fallSpriteCounter = 2f;
          float target = 0f;
          if (this.PlayerIndex != -1) {
            int num2 = 0;
            using (List<Entity>.Enumerator enumerator = base.Level [GameTags.Corpse].GetEnumerator ()) {
              while (enumerator.MoveNext ()) {
                PlayerCorpse playerCorpse = (PlayerCorpse)enumerator.Current;
                if (playerCorpse.PlayerIndex != -1 && base.CollideCheck (playerCorpse)) {
                  if (playerCorpse.ActualPosition.X == base.ActualPosition.X) {
                    if (playerCorpse.actualDepth > this.actualDepth) {
                      num2 = 1;
                    } else {
                      num2 = -1;
                    }
                  } else if (playerCorpse.ActualPosition.X < base.ActualPosition.X) {
                    num2 = 1;
                  } else {
                    num2 = -1;
                  }
                  if (!this.CanSlip (num2)) {
                    num2 = 0;
                  }
                  break;
                }
              }
            }
            target = (float)num2 * 0.5f;
          }
          if (!this.prismFall) {
            if (this.inMud) {
              this.Speed.X = Calc.Approach (this.Speed.X, target, 0.4f * Engine.TimeMult);
            } else if (flag4) {
              this.Speed.X = Calc.Approach (this.Speed.X, target, 0.02f * Engine.TimeMult);
            } else {
              this.Speed.X = Calc.Approach (this.Speed.X, target, 0.2f * Engine.TimeMult);
            }
          }
          if (Math.Abs (this.Speed.X) <= 0.5f) {
            if (!base.CheckBelow (4) || !base.CheckBelow (2)) {
              this.Speed.X = 0.5f;
              flag = true;
            } else if (!base.CheckBelow (-4) || !base.CheckBelow (-2)) {
              this.Speed.X = -0.5f;
              flag = true;
            }
          }
        } else if (!this.Pinned) {
          if (this.fallSpriteCounter > 0f) {
            this.fallSpriteCounter -= Engine.TimeMult;
          }
          if (!this.prismFall) {
            this.Speed.X = Calc.Approach (this.Speed.X, 0f, 0.02f * Engine.TimeMult);
          }
          if (!this.Reviving) {
            float num3 = this.againstWall ? 0.6f : 1f;
            if (this.prismFall) {
              num3 *= 0.3f;
            }
            if (IsAntiGrav()) {
              this.Speed.Y = Math.Max(this.Speed.Y + GetGravity() * ((Math.Abs (this.Speed.Y) < 0.5f) ? 0.5f : 1f) * num3 * Engine.TimeMult, GetMaxFall() * num3);
            } else {
              this.Speed.Y = Math.Min(this.Speed.Y + GetGravity() * ((Math.Abs (this.Speed.Y) < 0.5f) ? 0.5f : 1f) * num3 * Engine.TimeMult, GetMaxFall() * num3);
            }
          }
        }
        base.MoveH (this.Speed.X * Engine.TimeMult, this.onCollideH);
        base.MoveV (this.Speed.Y * Engine.TimeMult, this.onCollideV);
        if (flag) {
          base.MoveV (1f, null);
        }
        base_Update();
        if (this.Squished == Vector2.Zero && Math.Abs (this.Speed.X) <= 1f) {
          this.sprite.Scale.X = Calc.Approach (this.sprite.Scale.X, 1f, 0.01f * Engine.TimeMult);
          this.sprite.Scale.Y = Calc.Approach (this.sprite.Scale.Y, 1f, 0.01f * Engine.TimeMult);
        }
        if (Math.Abs (this.Speed.X) >= 2.5f) {
          this.sprite.Play ("flying", false);
          SetBuriedForAnimID("flying");
        } else if (flag3 && !this.Reviving) {
          if (this.againstWall) {
            this.sprite.Play ("slouched", false);
            SetBuriedForAnimID("slouched");
          } else {
            this.sprite.Play ("ground", false);
            SetBuriedForAnimID("ground");
          }
        } else if (this.againstWall || this.Reviving) {
          this.sprite.Play ("pinned", false);
          SetBuriedForAnimID("pinned");
        } else if (this.fallSpriteCounter > 0f) {
          this.sprite.Play ("ground", false);
          SetBuriedForAnimID("ground");
        } else {
          this.sprite.Play ("fall", false);
          SetBuriedForAnimID("fall");
        }
      }
      if (this.Hair != null) {
        this.UpdateHair ();
      }
      if (this.ghostCoroutine != null && this.ghostCoroutine.Active) {
        this.ghostCoroutine.Update ();
      }

      // Mod stuff
      if (this.reviverAdded == true) {
        this.reviverAdded = false;
        List<Entity> teamRevivers = base.Level[GameTags.TeamReviver];
        for (int i = 0; i < teamRevivers.Count; i++) {
          TeamReviver teamReviver = (TeamReviver)(teamRevivers[i]);
          if (teamReviver.Corpse.PlayerIndex == this.PlayerIndex) {
            base.Level.Layers[teamReviver.LayerIndex].Remove(teamReviver);
            patch_TeamReviver myTeamReviver = new patch_TeamReviver(
              this,
              TeamReviver.Modes.TeamDeathmatch,
              ((patch_TeamDeathmatchRoundLogic)(base.Level.Session.RoundLogic)).GetRoundEndCounter(),
              ((patch_MatchVariants)(base.Level.Session.MatchSettings.Variants)).GhostRevives
            );
            base.Level.Layers[myTeamReviver.LayerIndex].Add(myTeamReviver, false);
          }
        }
      }
    }

    private bool IsAntiGrav()
    {
      return patch_Level.IsAntiGrav();
    }

    private float GetGravity()
    {
      return IsAntiGrav() ? -0.3f : 0.3f;
    }

    private float GetMaxFall()
    {
      return IsAntiGrav() ? -2.8f : 2.8f;
    }
  }
}
