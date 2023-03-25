#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Monocle;
using Mod;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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

    [MonoModLinkTo("Monocle.Entity", "Tag")]
    [MonoModIgnore]
    public extern void base_Tag(GameTags tag);

    [MonoModLinkTo("TowerFall.LevelEntity", "System.Void .ctor(Microsoft.Xna.Framework.Vector2)")]
    [MonoModRemove]
    public extern void base_ctor(Vector2 position);

    // Manually pasting in origial constructor because of error: System.MissingMethodException: Method not found: 'TowerFall.TeamReviver.orig_ctor'.
    public void OriginalConstructor(PlayerCorpse corpse, TeamReviver.Modes mode)
    {
      base_ctor(corpse.BottomCenter);
      this.Mode = mode;
      this.Corpse = corpse;
      this.ScreenWrap = true;
      base_Tag(GameTags.LightSource);
      base_Tag(GameTags.TeamReviver);
      this.LightRadius = 60f;
      this.LightAlpha = 1f;
      base.Collider = (this.normalHitbox = new WrapHitbox (24f, 25f, -12f, -20f));
      this.revivingHitbox = new WrapHitbox (40f, 46f, -20f, -30f);
      this.playerNomralHitbox = new WrapHitbox (8f, 14f, -4f, -6f);
      this.reviveCounter = (float)this.ReviveTime;
      base.Add(this.sine = new SineWave (90));
      base.Add(this.arrowSine = new SineWave (20));
      switch (this.Mode) {
        case TeamReviver.Modes.TeamDeathmatch:
          this.arrowColor = (this.colorA = ArcherData.Get (corpse.TeamColor).ColorA);
          this.colorB = ArcherData.Get (corpse.TeamColor).ColorB;
          this.PlayerCanRevive = true;
          break;
        case TeamReviver.Modes.DarkWorld: {
          ArcherData archerData = ArcherData.Get (TFGame.Characters [corpse.PlayerIndex], TFGame.AltSelect [corpse.PlayerIndex]);
          this.arrowColor = (this.colorA = archerData.ColorA);
          this.colorB = archerData.ColorB;
          this.PlayerCanRevive = true;
          break;
        }
        case TeamReviver.Modes.Quest: {
          ArcherData archerData = ArcherData.Get (TFGame.Characters [corpse.PlayerIndex], TFGame.AltSelect [corpse.PlayerIndex]);
          this.arrowColor = (this.colorA = archerData.ColorA);
          this.colorB = archerData.ColorB;
          this.PlayerCanRevive = false;
          break;
        }
      }
      Alarm.Set(this, 60, delegate {
        this.canRevive = true;
      }, Alarm.AlarmMode.Oneshot);
      this.targetLightAlpha = 1f;
    }

    [MonoModConstructor]
    public void ctor(PlayerCorpse corpse, TeamReviver.Modes mode, RoundEndCounter roundEndCounter, bool ghostRevives=false)
    {
      this.roundEndCounter = roundEndCounter;
      this.ghostRevives = ghostRevives;

      OriginalConstructor(corpse, mode);
    }

    public void patch_HUDRender()
    {
      if (!this.Finished && !base.Level.Ending && !this.Corpse.PrismHit && this.Mode != TeamReviver.Modes.Quest) {
        float num = MathHelper.Lerp(-1f, this.arrowSine.Value, this.reviveCounter / (float)this.ReviveTime) * 2f;
        MyDraw.OutlineTextureCentered(
          TFGame.Atlas["versus/playerIndicator"],
          this.Position + new Vector2(0f, patch_Level.IsAntiGrav() ? 10f + num : -18f + num),
          this.arrowColor,
          patch_Level.IsAntiGrav() ? 3.1415926536f : 0f
        );
        MyDraw.OutlineTextureCentered(
          TFGame.Atlas["versus/teamRevive"],
          this.Position + new Vector2(0f, patch_Level.IsAntiGrav() ? 20f + num : -28f + num),
          this.arrowColor,
          patch_Level.IsAntiGrav() ? 3.1415926536f : 0f
        );
      }
    }

    extern public Player orig_FinishReviving();
    public Player patch_FinishReviving()
    {
      Player player = orig_FinishReviving();
      // If ghost revives is on, then a revive can cancel a level ending
      if (this.ghostRevives && base.Level.Session.MatchSettings.Mode == TowerFall.Modes.TeamDeathmatch) {
          Allegiance allegiance;
          if (!base.Level.Session.RoundLogic.TeamCheckForRoundOver(out allegiance)) {
              base.Level.Session.CurrentLevel.Ending = false;
              this.roundEndCounter.Reset();
          }
      }
      return player;
    }

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    // This is pasted code from the original class, except where it calls functions defined in this class
    // (plus a few inline edits for the reverse gravity effect)
    private void ReviveUpdateOriginalWithAdditions()
    {
      this.LightAlpha = Calc.Approach (this.LightAlpha, this.targetLightAlpha, 0.1f * Engine.TimeMult);
      base_Update ();
      if (this.levitateCorpse) {
        float num = patch_Level.IsAntiGrav()
          ? this.targetPosition.Y - this.sine.Value * 2f
          : this.targetPosition.Y + this.sine.Value * 2f;
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
            if (this.canRevive && !this.Corpse.PrismHit && this.Corpse.Squished == Vector2.Zero && this.CanReviveAtThisPosition()) {
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

    public extern void orig_StartReviving();
    public void patch_StartReviving()
    {
      orig_StartReviving();
      if (patch_Level.IsAntiGrav()) {
        this.targetPosition = this.Corpse.Position - Vector2.UnitY * -6f;
      }
    }

    public bool patch_CanReviveAtThisPosition (ref Vector2 revivePoint)
    {
      Collider collider = base.Collider;
      Vector2 position = this.Position;
      base.Collider = this.playerNomralHitbox;
      base.BottomCenter = this.Corpse.BottomCenter;
      Vector2 position2 = this.Position;
      bool result;
      for (int i = 0; i < 10; i += 2) {
        this.Position = position2 + Vector2.UnitY * (float)i;
        if (!base.CollideCheck (GameTags.Solid)) {
          revivePoint = this.Position;
          if (patch_Level.IsAntiGrav()) {
            float added = 8f;
            revivePoint.Y = revivePoint.Y + added;
          }
          base.Collider = collider;
          this.Position = position;
          result = true;
          return result;
        }
      }
      base.Collider = collider;
      this.Position = position;
      result = false;
      return result;
    }
  }
}