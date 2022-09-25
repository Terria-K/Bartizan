#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Monocle;
using Mod;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

// @TODO delete if not used

namespace TowerFall
{
  class patch_TeamReviver : TeamReviver
  {
    public RoundEndCounter roundEndCounter;
    public bool ghostRevives;

    public patch_TeamReviver(PlayerCorpse corpse, TeamReviver.Modes mode, RoundEndCounter roundEndCounter, bool ghostRevives=false) : base(corpse, mode) {
      // no-op. MonoMod ignores this
    }

    public patch_TeamReviver(PlayerCorpse corpse, TeamReviver.Modes mode) : base(corpse, mode) {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(PlayerCorpse corpse, TeamReviver.Modes mode);
    [MonoModConstructor]
    public void ctor(PlayerCorpse corpse, TeamReviver.Modes mode, RoundEndCounter roundEndCounter, bool ghostRevives=false)
    {
      this.roundEndCounter = roundEndCounter;
      this.ghostRevives = ghostRevives;

      orig_ctor(corpse, mode);
    }

    public void patch_HUDRender()
    {
      if (!this.Finished && !base.Level.Ending && !this.Corpse.PrismHit && this.Mode != TeamReviver.Modes.Quest) {
        float num = MathHelper.Lerp(-1f, this.arrowSine.Value, this.reviveCounter / (float)this.ReviveTime) * 2f;
        Draw.OutlineTextureCentered(TFGame.Atlas["versus/playerIndicator"], this.Position + new Vector2 (0f, -18f + num), this.arrowColor);
        Draw.OutlineTextureCentered(TFGame.Atlas["versus/teamRevive"], this.Position + new Vector2 (0f, -28f + num), this.arrowColor);
      }
    }

    extern public Player orig_FinishReviving();
    public Player patch_FinishReviving()
    {
      Player result = orig_FinishReviving();
      // If ghost revives is on, then a revive can cancel a level ending
      if (this.ghostRevives && base.Level.Session.MatchSettings.Mode == TowerFall.Modes.TeamDeathmatch) {
          Allegiance allegiance;
          if (!base.Level.Session.RoundLogic.TeamCheckForRoundOver(out allegiance)) {
              base.Level.Session.CurrentLevel.Ending = false;
              this.roundEndCounter.Reset();
          }
      }
      return result;
    }

    // This is pasted code from the original class, except where it calls functions defined in this class
    private void ReviveUpdateOriginalWithAdditions()
    {
      this.LightAlpha = Calc.Approach (this.LightAlpha, this.targetLightAlpha, 0.1f * Engine.TimeMult);
      base.Update ();
      if (this.levitateCorpse) {
        float num = this.targetPosition.Y + this.sine.Value * 2f;
        Vector2 zero = Vector2.Zero;
        zero.Y = MathHelper.Clamp (num - this.Corpse.ActualPosition.Y, -0.6f, 0.6f);
        if (!this.Finished && !this.AutoRevive) {
          Player player = base.Level.GetPlayer (this.reviver);
          if (player != null) {
            if (Math.Abs (player.X - base.X) > 10f) {
              zero.X = (float)Math.Sign (player.X - base.X) * 0.2f;
            }
            if (Math.Abs (player.Y - base.Y) > 14f) {
              zero.Y = (float)Math.Sign (player.Y - base.Y) * 0.6f;
            }
          }
        }
        if (this.Corpse.Squished == Vector2.Zero) {
          this.Corpse.Speed = this.Corpse.Speed.Approach (zero, 4f * Engine.TimeMult);
        }
      }
      if (!this.Finished) {
        if (this.Corpse.Scene == null || this.Corpse.MarkedForRemoval) {
          LightFade lightFade = Cache.Create<LightFade> ();
          lightFade.Init (this, null);
          base.Level.Add<LightFade> (lightFade);
          base.RemoveSelf ();
        } else {
          this.Position = this.Corpse.BottomCenter;
          if (base.Scene.OnInterval (3)) {
            if (this.arrowColor == this.colorA) {
              this.arrowColor = this.colorB;
            } else {
              this.arrowColor = this.colorA;
            }
          }
          if (this.reviving) {
            this.reviveCounter -= Engine.TimeMult;
            if (this.Corpse.Squished != Vector2.Zero) {
              this.StopReviving ();
            } else if (this.reviveCounter <= 0f) {
              this.Finished = true;
              this.Corpse.Revived = true;
              base.Add (new Coroutine (this.ReviveSequence ()));
            } else if (!this.Corpse.PrismHit && this.CanReviveAtThisPosition ()) {
              if (!this.AutoRevive && this.PlayerCanRevive) {
                bool flag = false;
                int num2 = -1;
                using (List<Entity>.Enumerator enumerator = base.Level.Players.GetEnumerator ()) {
                  while (enumerator.MoveNext ()) {
                    Player player2 = (Player)enumerator.Current;
                    if (player2.Allegiance == this.Corpse.Allegiance && base.CollideCheck (player2)) {
                      flag = true;
                      if (num2 != this.reviver) {
                        if (player2.PlayerIndex == this.reviver) {
                          num2 = this.reviver;
                        } else if (num2 == -1) {
                          num2 = player2.PlayerIndex;
                        }
                      }
                    }
                  }
                }

                ReviveUpdateGhostRevivesBlock1(ref flag, ref num2);

                if (num2 != this.reviver && num2 != -1) {
                  this.reviver = num2;
                }
                if (!flag) {
                  this.StopReviving ();
                }
              }
              TFGame.PlayerInputs [this.Corpse.PlayerIndex].Rumble (0.5f, 2);
              if (this.reviver != -1) {
                TFGame.PlayerInputs [this.reviver].Rumble (0.5f, 2);
              }
            } else {
              this.StopReviving ();
            }
          } else {
            this.ResetCounter ();
            this.LightAlpha = Calc.Approach (this.LightAlpha, 0f, 0.1f * Engine.TimeMult);
            if (this.canRevive && !this.Corpse.PrismHit && this.Corpse.Squished == Vector2.Zero && this.CanReviveAtThisPosition ()) {
              if (this.AutoRevive) {
                this.StartReviving ();
              } else if (this.PlayerCanRevive) {
                using (List<Entity>.Enumerator enumerator = base.Level.Players.GetEnumerator ()) {
                  while (enumerator.MoveNext ()) {
                    Player player2 = (Player)enumerator.Current;
                    if (player2.Allegiance == this.Corpse.Allegiance && !player2.Dead && base.CollideCheck (player2)) {
                      this.reviver = player2.PlayerIndex;
                      this.StartReviving ();
                      break;
                    }
                  }
                }
                ReviveUpdateGhostRevivesBlock2();
              }
            }
          }
        }
      }
    }

    private void ReviveUpdateGhostRevivesBlock1(ref bool flag, ref int num2)
    {
      if (this.ghostRevives) {
        using (List<Entity>.Enumerator enumerator = base.Level[GameTags.PlayerGhost].GetEnumerator ()) {
          while (enumerator.MoveNext ()) {
            PlayerGhost ghost = (PlayerGhost)enumerator.Current;
            if (ghost.Allegiance == this.Corpse.Allegiance && base.CollideCheck (ghost) && ghost.PlayerIndex != this.Corpse.PlayerIndex) {
              flag = true;
              if (num2 != this.reviver) {
                if (ghost.PlayerIndex == this.reviver) {
                  num2 = this.reviver;
                } else if (num2 == -1) {
                  num2 = ghost.PlayerIndex;
                }
              }
            }
          }
        }
      }
    }

    private void ReviveUpdateGhostRevivesBlock2()
    {
      if (this.ghostRevives) {
        using (List<Entity>.Enumerator enumerator = base.Level[GameTags.PlayerGhost].GetEnumerator ()) {
          while (enumerator.MoveNext ()) {
            PlayerGhost ghost = (PlayerGhost)enumerator.Current;
            if (ghost.Allegiance == this.Corpse.Allegiance && ghost.State != 3 && base.CollideCheck (ghost) && ghost.PlayerIndex != this.Corpse.PlayerIndex) {
              this.reviver = ghost.PlayerIndex;
              this.StartReviving ();
              break;
            }
          }
        }
      }
    }

    public void patch_ReviveUpdate()
    {
      ReviveUpdateOriginalWithAdditions();
    }
  }
}