#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Microsoft.Xna.Framework;
using Mod;
using MonoMod;
using Monocle;
using System.Xml;

namespace TowerFall
{
  public class patch_Player : Player
  {
    private string lastHatState;
    public bool spawningGhost;
    public bool diedFromPrism = false;
    public Arrow lastArrowCaught;

    ChaliceGhost summonedChaliceGhost;

    public patch_Player(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatStates hatState, bool frozen, bool flash, bool indicator)
      : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatStates hatState, bool frozen, bool flash, bool indicator);
    [MonoModConstructor]
    public void ctor(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatStates hatState, bool frozen, bool flash, bool indicator)
    {
      orig_ctor(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator);
      lastHatState = "UNSET";
    }

    public extern void orig_InitHead();
    [MonoModPublic]
    public void patch_InitHead()
    {
      orig_InitHead();

      XmlElement headDataXml;

      if (IsAntiGrav()) {
        switch (this.HatState) {
          case HatStates.Normal:
            headDataXml = TFGame.SpriteData.GetXML(this.ArcherData.Sprites.HeadNormal);
            break;
          case HatStates.NoHat:
            headDataXml = TFGame.SpriteData.GetXML(this.ArcherData.Sprites.HeadNoHat);
            break;
          case HatStates.Crown:
            headDataXml = TFGame.SpriteData.GetXML(this.ArcherData.Sprites.HeadCrown);
            break;
          default:
            throw new Exception("Unknown HatState: " + this.HatState.ToString());
        }

        this.headSprite.Rotation = 3.1415926536f;
        if (Calc.HasChild(headDataXml, "Y")) {
          this.headSprite.Position.Y = Calc.ChildInt(headDataXml, "Y") * -1;
        }
        this.headSprite.Origin.X = this.headSprite.Width / 2f;
        this.headSprite.FlipX = true;
      }
    }

    public void InitBody()
    {
      XmlElement bodySpriteData = TFGame.SpriteData.GetXML(this.ArcherData.Sprites.Body);
      XmlElement bowSpriteData = TFGame.SpriteData.GetXML (this.ArcherData.Sprites.Bow);
      if (IsAntiGrav()) {
        this.bodySprite.Rotation = 3.1415926536f;
        if (Calc.HasChild(bodySpriteData, "Y")) {
          this.bodySprite.Position.Y = Calc.ChildInt(bodySpriteData, "Y") * -1;
          this.bodySprite.Position.Y += 2;
        }
        this.bodySprite.Origin.X = this.bodySprite.Width / 2f;
        this.bodySprite.FlipX = true;

        this.hatHitbox.Top = base.Collider.Bottom;
        this.duckingHitbox.Top = base.Collider.Top;
      } else {
        this.bodySprite.Rotation = 0;
        if (Calc.HasChild(bodySpriteData, "Y")) {
          this.bodySprite.Position.Y = Calc.ChildInt(bodySpriteData, "Y");
        }
        if (Calc.HasChild(bodySpriteData, "OriginX")) {
          Console.WriteLine("Updating OriginX");
          this.bodySprite.Origin.X = Calc.ChildInt(bodySpriteData, "OriginX");
        }
        this.bodySprite.FlipX = false;
        this.hatHitbox.Bottom = base.Collider.Top;
        this.duckingHitbox.Bottom = base.Collider.Bottom;
      }
    }

    public extern void orig_Added();
    public void patch_Added()
    {
      orig_Added();
      InitBody();
      InitHead();
      this.spawningGhost = false;
      this.diedFromPrism = false;
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).VarietyPack[this.PlayerIndex]) {
        this.Arrows.Clear();
        this.Arrows.SetMaxArrows(10);
        var arrows = new ArrowTypes[] {
          ArrowTypes.Bomb,
          ArrowTypes.SuperBomb,
          ArrowTypes.Laser,
          ArrowTypes.Bramble,
          ArrowTypes.Drill,
          ArrowTypes.Bolt,
          ArrowTypes.Toy,
          ArrowTypes.Feather,
          ArrowTypes.Trigger,
          ArrowTypes.Prism
        };

        // Randomize. Couldn't get static method to work.
        Random rand = new Random();
        // For each spot in the array, pick
        // a random item to swap into that spot.
        for (int i = 0; i < arrows.Length - 1; i++)
        {
          int j = rand.Next(i, arrows.Length);
          ArrowTypes temp = arrows[i];
          arrows[i] = arrows[j];
          arrows[j] = temp;
        }
        this.Arrows.AddArrows(arrows);
      }
    }

    public extern bool orig_CanGrabLedge(int targetY, int direction);
    public bool patch_CanGrabLedge(int targetY, int direction)
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).NoLedgeGrab[this.PlayerIndex]) {
        return false;
      }
      return IsAntiGrav() ? !base.CollideCheck(
        GameTags.Solid,
        base.X,
        (float)(targetY - 2)
      ) && !base.CollideCheck(
        GameTags.Solid,
        base.X,
        base.Y - 5f
      ) && !base.Scene.CollideCheck(
        WrapMath.Vec(base.X + (float)(6 * direction), (float)(targetY + 1)),
        GameTags.Solid
      ) && base.Scene.CollideCheck(
        WrapMath.Vec(base.X + (float)(6 * direction), (float)targetY), GameTags.Solid
      ) : orig_CanGrabLedge(targetY, direction);
    }

    public extern int orig_GetDodgeExitState();
    public int patch_GetDodgeExitState()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).NoDodgeCooldowns[this.PlayerIndex]) {
        this.DodgeCooldown();
      }
      return orig_GetDodgeExitState();
    }

    public extern void orig_ShootArrow();
    public void patch_ShootArrow()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).InfiniteArrows[this.PlayerIndex]) {
        var arrow = this.Arrows.Arrows[0];
        orig_ShootArrow();
        this.Arrows.AddArrows(arrow);
      } else {
        orig_ShootArrow();
      }
    }

    public extern void orig_HurtBouncedOn(int bouncerIndex);
    public void patch_HurtBouncedOn(int bouncerIndex)
    {
      if (!((patch_MatchVariants)Level.Session.MatchSettings.Variants).NoHeadBounce[this.PlayerIndex]) {
        orig_HurtBouncedOn(bouncerIndex);
      }
    }

    public extern void orig_Die(Arrow arrow);
    public void patch_Die(Arrow arrow)
    {
      this.diedFromPrism = arrow is PrismArrow;
      orig_Die(arrow);
    }

    public extern PlayerCorpse orig_Die(DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false);
    public PlayerCorpse patch_Die (DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false)
    {
      if (summonedChaliceGhost) {
          summonedChaliceGhost.Vanish();
          summonedChaliceGhost = null;
      }

      if (Level.Session.MatchSettings.Variants.ReturnAsGhosts[this.PlayerIndex] && !this.diedFromPrism)
      {
        this.spawningGhost = true;
      }

      return orig_Die(deathCause, killerIndex, brambled, laser);
    }

    public extern int orig_DodgingUpdate();
    public int patch_DodgingUpdate()
    {
      if (this.input.DodgePressed) {
        this.canHyper = true;
        if ((bool)this.dodgeCatchCounter && this.Speed.LengthSquared () >= 3.6f) {
          if (this.lastArrowCaught.ArrowType == (ArrowTypes)MyGlobals.ArrowTypes.Ghost) {
            ((GhostArrow)(this.lastArrowCaught)).wasMiracled = true;
          }
          ((patch_Session)(Level.Session)).MyMatchStats[this.PlayerIndex].MiracleCatches += 1u;
        }
      }
      return orig_DodgingUpdate();
    }

    // Commented out because we've overwritten below. Will need to add chalice ghost code later
    // public extern void orig_Update();
    // public void patch_Update()
    // {
    //   orig_Update();
    //   if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).CrownSummonsChaliceGhost) {
    //     if (lastHatState == "UNSET") {
    //       lastHatState = HatState.ToString();
    //     } else if (lastHatState != HatState.ToString()) {
    //       if (lastHatState != "Crown" && HatState.ToString() == "Crown") {
    //         ChalicePad chalicePad = new ChalicePad(ActualPosition, 40);
    //         Chalice chalice = new Chalice(chalicePad);
    //         summonedChaliceGhost = new patch_ChaliceGhost(
    //           PlayerIndex,
    //           chalice,
    //           ((patch_MatchVariants)Level.Session.MatchSettings.Variants).ChaliceGhostsHuntGhosts
    //         );
    //         Level.Layers[summonedChaliceGhost.LayerIndex].Add(summonedChaliceGhost, false);
    //       } else if (summonedChaliceGhost && lastHatState == "Crown" && HatState.ToString() != "Crown") {
    //         // Ghost vanishes when player loses the crown
    //         summonedChaliceGhost.Vanish();
    //         summonedChaliceGhost = null;
    //       }
    //       lastHatState = HatState.ToString();
    //     }
    //   }
    // }

    public void patch_CatchArrow(Arrow arrow)
    {
      this.lastArrowCaught = arrow;

      // Mostly original, except where ghost arrow handled
      if (arrow.CanCatch (this) && !arrow.IsCollectible && arrow.CannotHit != this && (!this.HasShield || !arrow.Dangerous) && arrow != this.lastCaught) {
        Color colorB = ArcherData.GetColorB (this.PlayerIndex, this.TeamColor);
        arrow.OnPlayerCatch (this);
        this.dodgeCatchCounter.Set (10);
        base.Level.Add (Cache.Create<CatchShine> ().Init (colorB, arrow.Position));
        this.ArcherData.SFX.ArrowGrab.Play (base.X, 1f);
        if (!(bool)this.dodgeEndCounter && this.State == PlayerStates.Dodging) {
          Sounds.char_dodgeStallGrab.Play (base.X, 1f);
        }
        TFGame.PlayerInputs [this.PlayerIndex].Rumble (0.8f, 12);
        base.Level.Session.MatchStats [this.PlayerIndex].ArrowsCaught += 1u;
        if (arrow.PlayerIndex == this.PlayerIndex) {
          base.Level.Session.MatchStats [this.PlayerIndex].OwnArrowsCaught += 1u;
        }
        if (arrow.PlayerIndex != this.PlayerIndex && base.Allegiance != Allegiance.Neutral && arrow.Allegiance == base.Allegiance) {
          base.Level.Session.MatchStats [this.PlayerIndex].TeamArrowCatches += 1u;
        }
        SaveData.Instance.Stats.ArrowsCaught++;
        this.dodgeCurseSatisfied = true;
        if (arrow.Fire.OnFire) {
          this.Fire.Start ();
        }
        if (arrow.ArrowType == (ArrowTypes)MyGlobals.ArrowTypes.Ghost && arrow.Owner != this && arrow.Dangerous) {
          Alarm.Set(this, 10, delegate {
            if (((GhostArrow)(arrow)).wasMiracled) {
              arrow.OnPlayerCollect(this, true);
              this.HasShield = true;
            } else if (arrow.PlayerIndex > -1) {
              Player player = base.Level.GetPlayer(arrow.PlayerIndex);
              if (player != null) {
                arrow.OnPlayerCollect(player, true);
              }
            }
          }, Alarm.AlarmMode.Oneshot);

          ShockCircle shockCircle = Cache.Create<ShockCircle>();
          shockCircle.Init (this.Position, this.PlayerIndex, this, ShockCircle.ShockTypes.BoltCatch);
          this.Level.Add (shockCircle);
          Sounds.sfx_boltArrowExplode.Play(base.X, 1f);
          arrow.RemoveSelf();
        } else if (arrow.PrismCheck ()) {
          arrow.Collidable = false;
          this.StartPrism (arrow.PlayerIndex);
          arrow.RemoveSelf ();
        } else if (this.Arrows.CanAddArrow(arrow.ArrowType)) {
          arrow.OnPlayerCollect(this, true);
        } else {
          this.lastCaught = arrow;
          arrow.Drop ((int)this.Facing);
        }
      } else {
        arrow.Active = (arrow.Collidable = true);
      }
    }

    public void patch_UpdateAnimation ()
    {
      this.aimer.Rotation = this.AimDirection;
      this.SetBowRotation ();
      if (this.State == PlayerStates.Dodging) {
        if (this.DodgeSliding) {
          switch (this.HatState) {
          case HatStates.Normal:
            this.bodySprite.Play ("slide_normal", false);
            break;
          case HatStates.NoHat:
            this.bodySprite.Play ("slide_nohat", false);
            break;
          case HatStates.Crown:
            this.bodySprite.Play ("slide_crown", false);
            break;
          }
        } else {
          this.bodySprite.Play ("dodge", false);
        }
        this.bowSprite.Visible = false;
      } else if (this.State == PlayerStates.Ducking) {
        this.bodySprite.Play ("duck", false);
        this.bowSprite.Visible = false;
      } else if (this.State == PlayerStates.LedgeGrab) {
        this.bodySprite.Play ("ledge", false);
        this.bowSprite.Visible = false;
      } else {
        this.bowSprite.Visible = (!this.hideBow || this.Aiming);
        if (this.AimDirection == 1.57079637f) {
          this.bowSprite.Y = (float)this.bowDownY;
        } else {
          this.bowSprite.Y = this.bowPosition.Y;
        }
        this.bowSprite.X = this.bowPosition.X;
        if (this.bowXOffsets != null && this.bodySprite.CurrentFrame < this.bowXOffsets.Length) {
          this.bowSprite.X += (float)(this.bowXOffsets [this.bodySprite.CurrentFrame] * (int)this.Facing);
        }
        if (this.bowYOffsets != null && this.bodySprite.CurrentFrame < this.bowYOffsets.Length) {
          this.bowSprite.Y += (float)this.bowYOffsets [this.bodySprite.CurrentFrame];
        }
        if (this.Cling != 0 && (IsAntiGrav() ? this.Speed.Y <= 0f : this.Speed.Y >= 0f)) {
          this.bowSprite.Visible = false;
          this.bodySprite.Play ("ledge", false);
        } else if (this.gliding) {
          if (this.Aiming && this.bodySprite.ContainsAnimation ("glide_aim")) {
            this.bodySprite.Play ("glide_aim", false);
          } else {
            this.bodySprite.Play ("glide", false);
          }
        } else if (this.OnGround) {
          if (Math.Abs (this.Speed.X) <= 0.390000015f && (this.Aiming || this.input.MoveX == 0 || this.State == PlayerStates.Frozen)) {
            if (this.hideBow) {
              if (this.Aiming && this.bodySprite.ContainsAnimation ("stand_aim")) {
                this.bodySprite.Play ("stand_aim", false);
              } else {
                this.bodySprite.Play ("stand", false);
              }
            } else {
              this.bowSprite.Visible = true;
              if (this.bodySprite.ContainsAnimation ("stand_aim")) {
                this.bodySprite.Play ("stand_aim", false);
              } else {
                this.bodySprite.Play ("stand", false);
              }
            }
          } else if (this.Aiming && this.bodySprite.ContainsAnimation ("run_aim")) {
            this.bodySprite.Play ("run_aim", false);
          } else {
            this.bodySprite.Play ("run", false);
          }
        } else if (this.Speed.Y < 0f) {
          if (this.Aiming && this.bodySprite.ContainsAnimation ("jump_aim")) {
            this.bodySprite.Play ("jump_aim", false);
          } else {
            this.bodySprite.Play ("jump", false);
          }
        } else if (this.Aiming && this.bodySprite.ContainsAnimation ("fall_aim")) {
          this.bodySprite.Play ("fall_aim", false);
        } else {
          this.bodySprite.Play ("fall", false);
        }
        if (this.Aiming) {
          this.bowSprite.Play ((this.Arrows.Count > 0) ? "drawn" : "drawnEmpty", false);
        } else {
          this.bowSprite.Play ("idle", false);
        }
      }
    }
    public int patch_DuckingUpdate()
    {
      if (this.headSprite.CurrentFrame == this.ArcherData.SleepHeadFrame) {
        if (base.Scene.OnInterval (60)) {
          if (this.snoreIndex == 0) {
            UnlockData.UnlockAchievement ("SLEEPY_MASTER");
            this.ArcherData.SFX.Sleep.Play (base.X, 1f);
          }
          this.snoreIndex = (this.snoreIndex + 1) % 4;
          base.Level.ParticlesFG.Emit (Particles.YellowSleeping, base.Position + new Vector2 (3f, -3f));
        }
      } else if (this.snoreIndex > 0) {
        this.snoreIndex = 0;
        this.ArcherData.SFX.Sleep.Stop (true);
      }
      base.Level.Session.MatchStats [this.PlayerIndex].DuckFrames += Engine.TimeMult;
      this.wings.Normal ();
      bool flag = this.CanUnduck ();
      if (flag && !this.dodgeCooldown && this.input.DodgePressed && !base.Level.Session.MatchSettings.Variants.NoDodging [this.PlayerIndex]) {
        this.DodgeSliding = true;
        return 3;
      }
      if (this.input.JumpPressed && base.CollideCheck (GameTags.JumpThru, base.Position + Vector2.UnitY) && !base.CollideCheck (GameTags.Solid, base.Position + Vector2.UnitY * 3f)) {
        this.jumpBufferCounter.Set (0);
        this.jumpGraceCounter.Set (0);
        base.Y += 3f;
        Sounds.env_gplatPlayerDrop.Play (base.X, 1f);
        return 0;
      }
      if (flag) {
        if (this.onHotCoals) {
          this.HotCoalsBounce ();
          return 0;
        }
        if (this.input.JumpPressed && (bool)this.jumpGraceCounter) {
          this.Jump (true, true, false, 0, false);
          return 0;
        }
        if (!this.OnGround || (IsAntiGrav() ? this.moveAxis.Y != -1f : this.moveAxis.Y != 1f)) {
          return 0;
        }
      }
      if (!Player.ShootLock) {
        if (this.input.AltShootPressed) {
          if (this.triggerArrows.Count > 0) {
            this.DetonateTriggerArrows ();
          } else if (this.CanUnduck ()) {
            this.Aiming = true;
            this.startAimingDown = true;
            return 0;
          }
        } else if (this.input.ShootPressed && this.CanUnduck ()) {
          this.Aiming = true;
          this.startAimingDown = true;
          return 0;
        }
      }
      float target = flag ? 0f : (0.8f * (float)this.input.MoveX);
      this.Speed.X = Calc.Approach (this.Speed.X, target, DUCK_FRICTION * Engine.TimeMult);
      if (!this.OnGround) {
        this.Speed.Y = Calc.Approach (this.Speed.Y, GetMaxFall(), GetGravity() * Engine.TimeMult);
      }
      base.MoveH (this.Speed.X * Engine.TimeMult, this.onCollideH);
      base.MoveV (this.Speed.Y * Engine.TimeMult, this.onCollideV);
      if (Math.Sign (this.moveAxis.X) == 0 - this.Facing) {
        this.Facing = (Facing)(0 - this.Facing);
        this.bodySprite.Scale = new Vector2 (1.2f, 0.8f);
      }
      if (this.duckSlipCounter > 0f) {
        this.duckSlipCounter -= Engine.TimeMult;
      } else if (this.OnGround) {
        if (!base.CheckBelow (-3)) {
          base.MoveH (-1f * Engine.TimeMult, null);
          if (!base.CheckBelow ()) {
            base.MoveV (1f, null);
          }
        } else if (!base.CheckBelow (3)) {
          base.MoveH (1f * Engine.TimeMult, null);
          if (!base.CheckBelow ()) {
            base.MoveV (1f, null);
          }
        }
      }
      return 2;
    }

    public int patch_NormalUpdate()
    {
      if (this.OnGround && (bool)this.inMud && this.Speed.X != 0f && base.Level.OnInterval (10)) {
        Sounds.env_mudPlayerMove.Play (base.X, 1f);
      }
      if (!this.onHotCoals && this.InputDucking) {
        if (!this.startAimingDown) {
          return 2;
        }
      } else if (this.startAimingDown) {
        this.startAimingDown = false;
      }
      float num = MathHelper.Lerp (0.35f, 1f, this.slipperyControl);
      if (this.inMud != null && this.OnGround && this.moveAxis.X != (float)Math.Sign (this.Speed.X)) {
        this.Speed.X = Calc.Approach (this.Speed.X, 0f, 0.1f);
      } else if ((this.Aiming && this.OnGround) || (!this.Aiming && this.slipperyControl == 1f && this.moveAxis.X != (float)Math.Sign (this.Speed.X))) {
        float maxMove = (!this.HasWings) ? (((this.OnGround || this.HasWings) ? 0.2f : 0.14f) * num * Engine.TimeMult) : (((Math.Abs (this.Speed.X) > this.MaxRunSpeed) ? 0.14f : 0.2f) * num * Engine.TimeMult);
        this.Speed.X = Calc.Approach (this.Speed.X, 0f, maxMove);
      }
      if (!this.Aiming && this.moveAxis.X != 0f) {
        if (this.OnGround && num == 1f && this.inMud == null) {
          if (Math.Sign (this.moveAxis.X) == -Math.Sign (this.Speed.X) && base.Level.OnInterval (1)) {
            base.Level.Particles.Emit (this.DustParticleType, 2, base.Position + new Vector2 ((float)(-4 * Math.Sign (this.moveAxis.X)), 6f), Vector2.One * 2f);
          } else if (this.moveAxis.X != 0f && this.HasSpeedBoots && Math.Abs (this.Speed.X) >= 1.5f && base.Level.OnInterval (2)) {
            base.Level.Particles.Emit (this.DustParticleType, 1, base.Position + new Vector2 ((float)(-4 * Math.Sign (this.moveAxis.X)), 6f), Vector2.One * 2f);
          }
        }
        if (Math.Abs (this.Speed.X) > this.MaxRunSpeed && (float)Math.Sign (this.Speed.X) == this.moveAxis.X) {
          this.Speed.X = Calc.Approach (this.Speed.X, this.MaxRunSpeed * this.moveAxis.X, 0.03f * Engine.TimeMult);
        } else {
          float num2 = this.OnGround ? 0.15f : 0.1f;
          num2 *= num;
          if (this.dodgeCooldown) {
            num2 *= 0.8f;
          }
          if (this.HasSpeedBoots) {
            num2 *= 1.4f;
          }
          this.Speed.X = Calc.Approach (this.Speed.X, this.MaxRunSpeed * this.moveAxis.X, num2 * Engine.TimeMult);
        }
      }
      if (IsAntiGrav() ? this.Speed.Y > GetJump() : this.Speed.Y < GetJump()) {
        if (this.canPadParticles && base.Level.OnInterval (1)) {
          base.Level.Particles.Emit (Particles.JumpPadTrail, Calc.Range (Calc.Random, base.Position, Vector2.One * 4f));
        }
      } else {
        this.canPadParticles = false;
      }
      this.Cling = 0;
      if (this.OnGround) {
        this.wings.Normal ();
      } else {
        this.flapGravity = Calc.Approach (this.flapGravity, 1f, ((this.flapGravity < 0.5f) ? 0.012f : 0.048f) * Engine.TimeMult);
        if (this.autoBounce && (IsAntiGrav() ? this.Speed.Y < 0f : this.Speed.Y > 0f)) {
          this.autoBounce = false;
        }
        float num3 = (
          (IsAntiGrav() ? this.Speed.Y >= -1f : this.Speed.Y <= 1f)
          && (this.input.JumpCheck || this.autoBounce)
          && this.canVarJump
        ) ? 0.15f : 0.3f;
        num3 *= this.flapGravity;
        float target = IsAntiGrav() ? -2.8f : 2.8f;
        if (this.moveAxis.X != 0f && this.CanWallSlide ((Facing)(int)this.moveAxis.X)) {
          this.wings.Normal ();
          target = this.wallStickMax;
          this.wallStickMax = Calc.Approach (this.wallStickMax, IsAntiGrav() ? -1.6f : 1.6f, 0.01f * Engine.TimeMult);
          this.Cling = (int)this.moveAxis.X;
          if (IsAntiGrav() ? this.Speed.Y < 0f : this.Speed.Y > 0f) {
            this.ArcherData.SFX.WallSlide.Play (base.X, 1f);
          }
          if (base.Level.OnInterval (3)) {
            base.Level.Particles.Emit (this.DustParticleType, 1, base.Position + new Vector2 ((float)(3 * this.Cling), 0f), new Vector2 (1f, 3f));
          }
        } else if (this.input.MoveY == 1 && (IsAntiGrav() ? this.Speed.Y < 0f : this.Speed.Y > 0f)) {
          this.wings.FallFast ();
          target = IsAntiGrav() ? -3.5f : 3.5f;
          base.Level.Session.MatchStats [this.PlayerIndex].FastFallFrames += Engine.TimeMult;
        } else if (this.input.JumpCheck && this.HasWings && (IsAntiGrav() ? this.Speed.Y <= 1f : this.Speed.Y >= -1f)) {
          this.wings.Glide ();
          this.gliding = true;
          target = IsAntiGrav() ? -0.8f : 0.8f;
        } else {
          this.wings.Normal ();
        }
        if (this.Cling == 0 || (IsAntiGrav() ? this.Speed.Y >= 0f : this.Speed.Y <= 0f)) {
          this.ArcherData.SFX.WallSlide.Stop (true);
        }
        this.Speed.Y = Calc.Approach (this.Speed.Y, target, num3 * Engine.TimeMult);
      }
      if (!this.dodgeCooldown && this.input.DodgePressed && !base.Level.Session.MatchSettings.Variants.NoDodging [this.PlayerIndex]) {
        if (this.moveAxis.X != 0f) {
          this.Facing = (Facing)(int)this.moveAxis.X;
        }
        return 3;
      }
      if (this.onHotCoals) {
        this.HotCoalsBounce ();
      } else if (this.input.JumpPressed || (bool)this.jumpBufferCounter) {
        if ((bool)this.jumpGraceCounter) {
          int num4 = this.graceLedgeDir;
          if (this.input.MoveX != num4) {
            num4 = 0;
          }
          this.Jump (true, true, false, num4, false);
        } else if (this.CanWallJump (Facing.Left)) {
          this.WallJump (1);
        } else if (this.CanWallJump (Facing.Right)) {
          this.WallJump (-1);
        } else if (this.canDoubleJump) {
          this.canDoubleJump = false;
          this.Jump (true, false, false, 0, true);
        } else if (this.HasWings && !(bool)this.flapBounceCounter) {
          this.WingsJump ();
        }
      }
      if (!Player.ShootLock) {
        if (this.triggerArrows.Count > 0) {
          if (this.input.AltShootPressed) {
            this.DetonateTriggerArrows ();
          }
          if (this.Aiming && !this.input.ShootCheck) {
            this.ShootArrow ();
          } else if (!this.Aiming && this.input.ShootPressed) {
            this.Aiming = true;
          }
        } else if (this.Aiming) {
          if (!this.input.AltShootCheck && !this.input.ShootCheck) {
            this.ShootArrow ();
          }
        } else if (this.didDetonate) {
          if (!this.input.AltShootCheck) {
            this.didDetonate = false;
          }
          if (this.input.ShootPressed) {
            this.Aiming = true;
          }
        } else if (this.input.AltShootPressed || this.input.ShootPressed) {
          this.Aiming = true;
        }
      }
      float num5 = 0f;
      Entity entity = null;
      bool flag = (
        (IsAntiGrav() ? this.Speed.Y > 1f : this.Speed.Y < -1f) &&
        (IsAntiGrav() ? this.Speed.Y < GetJump() : this.Speed.Y > GetJump()) &&
        this.input.JumpCheck &&
        (entity = base.CollideFirst (GameTags.JumpThru)) != null
      );
      if (flag) {
        num5 = -2f;
        (entity as JumpThru).OnPlayerPop (base.X);
        if (!this.didPopThroughJumpThru) {
          Sounds.env_gplatPlayerJump.Play (base.X, 1f);
        }
      }
      this.didPopThroughJumpThru = flag;
      if (this.moveAxis.X != 0f) {
        this.Facing = (Facing)(int)this.moveAxis.X;
      }
      base.MoveH (this.Speed.X * Engine.TimeMult, this.onCollideH);
      base.MoveV ((this.Speed.Y + num5) * Engine.TimeMult, this.onCollideV);
      if (
        this.Prism == null &&
        !this.OnGround &&
        !this.Aiming &&
        (IsAntiGrav() ? this.Speed.Y <= 0f : this.Speed.Y >= 0f) &&
        this.moveAxis.X != 0f &&
        (IsAntiGrav() ? this.moveAxis.Y >= 0f : this.moveAxis.Y <= 0f) &&
        base.CollideCheck(GameTags.Solid, base.Position + Vector2.UnitX * this.moveAxis.X * 2f)
      ) {
        int direction = Math.Sign (this.moveAxis.X);
        for (int i = 0; i < 10; i++) {
          if (this.CanGrabLedge ((int)base.Y - i, direction)) {
            return this.GrabLedge ((int)base.Y - i, direction);
          }
        }
      }
      return 0;
    }

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    public void patch_Update()
    {
      if ((bool)this.spamShotCounter) {
        this.spamShotCounter.Update ();
      }
      base.Level.Session.MatchStats [this.PlayerIndex].FramesAlive += Engine.TimeMult;
      this.EnableSolids ();
      if (base.Level.Session.MatchSettings.Variants.RegeneratingArrows [this.PlayerIndex] && this.State != PlayerStates.Frozen) {
        if (this.Arrows.HasArrows) {
          this.arrowRegenCounter.Set (90);
        } else {
          this.arrowRegenCounter.Update ();
          if (!(bool)this.arrowRegenCounter) {
            Sounds.sfx_regenArrow.Play (base.X, 1f);
            this.Arrows.AddArrows (base.Level.Session.GetRegenArrow (this.PlayerIndex));
          }
        }
      }
      if (base.Level.Session.MatchSettings.Variants.RegeneratingShields [this.PlayerIndex] && this.State != PlayerStates.Frozen) {
        if (this.HasShield) {
          this.shieldRegenCounter.Set (240);
        } else {
          this.shieldRegenCounter.Update ();
          if (!(bool)this.shieldRegenCounter) {
            this.HasShield = true;
          }
        }
      }
      if (this.Invisible) {
        if (this.Aiming) {
          this.InvisOpacity = 1f;
        } else {
          this.InvisOpacity = Math.Max (this.InvisOpacity - 0.02f * Engine.TimeMult, this.TargetInvisibleOpacity);
        }
      } else {
        this.InvisOpacity = Math.Min (this.InvisOpacity + 0.05f * Engine.TimeMult, 1f);
      }
      if (this.State != PlayerStates.Dodging) {
        if (Math.Abs (this.Speed.Y) > 2.8f) {
          this.bodySprite.Scale.X = Calc.Approach (this.bodySprite.Scale.X, 0.8f, 0.04f * Engine.TimeMult);
          this.bodySprite.Scale.Y = Calc.Approach (this.bodySprite.Scale.Y, 1.2f, 0.04f * Engine.TimeMult);
        } else {
          this.bodySprite.Scale.X = Calc.Approach (this.bodySprite.Scale.X, 1f, 0.04f * Engine.TimeMult);
          this.bodySprite.Scale.Y = Calc.Approach (this.bodySprite.Scale.Y, 1f, 0.04f * Engine.TimeMult);
        }
      }
      if (this.State == PlayerStates.Ducking && !base.Level.Session.MatchSettings.SoloMode) {
        base.LightAlpha = Calc.Approach (base.LightAlpha, 0f, 0.05f * Engine.TimeMult);
      } else {
        base.LightAlpha = Calc.Approach (base.LightAlpha, 1f, 0.05f * Engine.TimeMult);
      }
      Platform below = base.GetBelow ();
      this.OnGround = (below != null);
      if (this.OnGround) {
        this.slipperyControl = (float)((!base.Level.Session.MatchSettings.Variants.SlipperyFloors [this.PlayerIndex] && (base.Level.Session.MatchSettings.Variants.NoSlipping [this.PlayerIndex] || !base.CollideCheck (GameTags.Ice, base.Position + Vector2.UnitY))) ? 1 : 0);
        this.lastPlatform = below;
      } else {
        this.slipperyControl = Math.Min (this.slipperyControl + 0.1f, 1f);
      }
      if (this.OnGround) {
        this.onHotCoals = base.CollideCheck (GameTags.HotCoals, base.Position + Vector2.UnitY);
      } else {
        this.onHotCoals = false;
      }
      if (TFGame.PlayerInputs [this.PlayerIndex] != null) {
        this.input = TFGame.PlayerInputs [this.PlayerIndex].GetState ();
      }
      this.moveAxis = new Vector2 ((float)this.input.MoveX, (float)this.input.MoveY);
      this.AimDirection = (PlayerInput.GetAimDirection (this.input.AimAxis, !base.Level.Session.MatchSettings.Variants.FreeAiming [this.PlayerIndex]) ?? ((this.Facing == Facing.Right) ? 0f : 3.14159274f));
      if (this.inMud == null) {
        this.inMud = (base.CollideFirst (GameTags.Mud) as Mud);
        if (this.inMud != null) {
          this.inMud.SplashDown (base.X);
          Sounds.env_mudPlayerLand.Play (base.X, 1f);
        }
      } else {
        this.inMud = (base.CollideFirst (GameTags.Mud) as Mud);
        if (this.inMud != null && this.Speed.X != 0f) {
          this.inMud.ParticleSplash (base.X + this.Speed.X * 4f);
        }
      }
      if (this.IsHyper && base.Level.OnInterval (1)) {
        base.Level.Particles.Emit (this.HyperJumpParticleType, 2, base.Position, Vector2.One * 4f);
      }
      if (this.hyperDir != 0 && !this.IsHyper) {
        float value = (this.hyperDir != 1) ? ((!(base.X > this.hyperStartX)) ? (this.hyperStartX - base.X) : (this.hyperStartX + (320f - base.X))) : ((!(base.X < this.hyperStartX)) ? (base.X - this.hyperStartX) : (320f - this.hyperStartX + base.X));
        value = Math.Abs (value);
        this.hyperDir = 0;
        if (value >= 100f) {
          UnlockData.UnlockAchievement ("SPEED_OF_LIGHT");
        }
      }
      if (base.Level.Session.MatchSettings.Variants.DoubleJumping [this.PlayerIndex] && (this.OnGround || this.State == PlayerStates.LedgeGrab)) {
        this.canDoubleJump = true;
      }
      if (this.State == PlayerStates.Frozen) {
        base_Update ();
        if (this.moveAxis.X != 0f) {
          this.Facing = (Facing)Math.Sign (this.moveAxis.X);
        }
        this.UpdateAnimation ();
        this.DisableSolids ();
      } else {
        this.jumpBufferCounter.Update ();
        if (this.input.JumpPressed) {
          this.jumpBufferCounter.Set (6);
        }
        if (this.Aiming) {
          if (this.lastAimDirection != -1f && this.lastAimDirection != this.AimDirection) {
            this.ArcherData.SFX.AimDir.Play (base.X, 1f);
          }
          this.lastAimDirection = this.AimDirection;
        }
        if (this.autoMove != 0) {
          this.moveAxis.X = (float)this.autoMove;
        }
        if ((bool)this.canDetonateCounter) {
          this.canDetonateCounter.Update ();
        }
        if (this.OnGround && (IsAntiGrav() ? this.Speed.Y < 0.02f : this.Speed.Y > -0.02f)) {
          this.jumpGraceCounter.SetMax (6);
          this.wallStickMax = IsAntiGrav() ? -0.5f : 0.5f;
          this.flapGravity = 1f;
          this.graceLedgeDir = 0;
        } else {
          this.jumpGraceCounter.Update ();
        }
        if ((bool)this.flapBounceCounter) {
          if (IsAntiGrav() ? this.Speed.Y >= 0f : this.Speed.Y <= 0f) {
            this.flapBounceCounter.Set (0);
          } else {
            this.flapBounceCounter.Update ();
          }
        }
        this.gliding = false;
        if (this.input.ArrowsPressed && this.Arrows.ToggleSort ()) {
          Sounds.sfx_arrowToggle.Play (base.X, 1f);
        }
        base_Update ();
        if (this.canHyper && !this.IsHyper) {
          this.canHyper = false;
        }
        Collider collider = base.Collider;
        base.CollideDo (GameTags.PlayerCollectible, this.playerColliderDo);
        if (base.TargetCollider != null) {
          base.Collider = base.TargetCollider;
        }
        base.CollideDo (GameTags.PlayerCollider, this.playerColliderDo);
        base.Collider = collider;
        base.Collider = this.arrowPickupHitbox;
        Entity entity = base.CollideFirst (GameTags.Arrow);
        if (entity != null) {
          (entity as Arrow).OnPlayerCollect (this, false);
        }
        base.Collider = collider;
        if (!this.Dead && !this.HasShield && this.HatState != 0) {
          if (IsAntiGrav()) {
            this.hatHitbox.Top = base.Collider.Bottom;
          } else {
            this.hatHitbox.Bottom = base.Collider.Top;
          }
          base.Collider = this.hatHitbox;
          foreach (Arrow item in ((Scene)base.Level) [GameTags.Arrow]) {
            if (item.CannotHit != this && item.Dangerous && (IsAntiGrav() ? item.Speed.Y > -2f : this.Speed.Y < 2f) && base.CollideCheck (item)) {
              if (item.PlayerIndex != -1) {
                base.Level.Session.MatchStats [item.PlayerIndex].HatsShotOff += 1u;
              }
              this.LoseHat (item, true);
              break;
            }
          }
          base.Collider = collider;
        }
        if (this.Fire.OnFire && (bool)this.wingsFireCounter) {
          this.wingsFireCounter.Update ();
          if (!(bool)this.wingsFireCounter) {
            Sounds.sfx_burn.Play (base.X, 1f);
            this.HasWings = false;
          }
        }
        this.UpdateAnimation ();
        this.DisableSolids ();
        if ((bool)this.Hair) {
          this.Hair.Update ();
        }
        if (this.ArcherData.PurpleParticles && this.InvisOpacity >= 1f) {
          if (this.State == PlayerStates.Ducking) {
            if (base.Level.OnInterval (5)) {
              Vector2 positionRange = new Vector2 (7f, 6f);
              base.Level.ParticlesBG.Emit (Particles.PurpleAmbience, 1, base.Position + Vector2.UnitY * 4f, positionRange);
            }
          } else if (base.Level.OnInterval (3)) {
            Vector2 positionRange = new Vector2 (7f, 11f);
            base.Level.ParticlesBG.Emit (Particles.PurpleAmbience, 1, base.Position, positionRange);
          }
        }
      }
    }

    public extern void orig_DoWrapRender();
    public override void DoWrapRender()
    {
      orig_DoWrapRender();

      // Uncomment to see ducking hitbox
      // Collider collider = base.Collider;
      // base.Collider = this.duckingHitbox;
      // this.duckingHitbox.Render(Color.Purple);
      // base.Collider = collider;

      // Uncomment to see hitboxes
      // this.DebugRender();
    }

    public void patch_Jump (bool particles, bool canSuper, bool forceSuper, int ledgeDir, bool doubleJump)
    {
      this.autoBounce = false;
      this.jumpBufferCounter.Set (0);
      this.jumpGraceCounter.Set (0);
      if (this.IsHyper && this.Speed.X != 0f) {
        this.hyperDir = Math.Sign (this.Speed.X);
        this.hyperStartX = base.X;
      }
      if (this.inMud != null) {
        this.inMud.SplashUp (base.X);
        Sounds.env_mudPlayerJump.Play (base.X, 1f);
      }
      if (forceSuper) {
        this.Speed.Y = GetJumpOnPad();
      } else {
        JumpPad jumpPad = null;
        if (canSuper) {
          jumpPad = ((!(this.lastPlatform is JumpPad)) ? (base.CollideFirst (GameTags.JumpPad, base.Position + Vector2.UnitY) as JumpPad) : (this.lastPlatform as JumpPad));
        }
        if ((bool)jumpPad) {
          this.Speed.Y = GetJumpOnPad();
          this.canPadParticles = true;
          jumpPad.Launch (base.X);
        } else {
          this.Speed.Y = GetJump();
        }
      }
      this.ArcherData.SFX.Jump.Play (base.X, 1f);
      this.bodySprite.Scale.X = 0.7f;
      this.bodySprite.Scale.Y = 1.3f;
      if (ledgeDir != 0) {
        this.Facing = (Facing)ledgeDir;
        this.Speed.X = (float)ledgeDir * 1.5f;
        this.autoMove = ledgeDir;
        this.scheduler.ScheduleAction (this.FinishAutoMove, 6, false);
      }
      if (this.moveAxis.X != 0f && !this.Aiming) {
        this.Speed.X += (doubleJump ? 1f : 0.5f) * this.moveAxis.X;
      }
      if (this.moveAxis.X != 0f && doubleJump && (Math.Sign (this.Speed.X) != (int)this.moveAxis.X || Math.Abs (this.Speed.X) < 2f)) {
        this.Speed.X = 2f * this.moveAxis.X;
      }
      this.canVarJump = true;
      if (this.Prism != null) {
        this.Prism.CaptiveForceShatter ();
      }
      if (particles) {
        if (ledgeDir == 0) {
          base.Level.Particles.Emit (this.DustParticleType, 5, base.Position + new Vector2 (0f, 6f), Vector2.One * 5f);
        } else {
          base.Level.Particles.Emit (this.DustParticleType, 5, base.Position + new Vector2 ((float)(4 * -ledgeDir), 4f), new Vector2 (2f, 5f), Calc.Angle (new Vector2 ((float)ledgeDir, -0.5f)));
        }
      }
      base.Level.Session.MatchStats [this.PlayerIndex].Jumps += 1u;
      SaveData.Instance.Stats.Jumps++;
    }

    public void patch_WallJump (int dir)
    {
      this.autoBounce = false;
      this.jumpBufferCounter.Set (0);
      this.jumpGraceCounter.Set (0);
      this.ArcherData.SFX.Jump.Play (base.X, 1f);
      this.Speed.Y = GetJump();
      this.Speed.X = (float)dir * 2f;
      this.canVarJump = true;
      this.bodySprite.Scale.X = 0.7f;
      this.bodySprite.Scale.Y = 1.3f;
      this.wallStickMax = IsAntiGrav() ? -0.5f : 0.5f;
      this.flapGravity = 1f;
      this.Facing = (Facing)dir;
      this.autoMove = dir;
      this.scheduler.ScheduleAction (this.FinishAutoMove, 12, false);
      base.Level.Particles.Emit (this.DustParticleType, 5, base.Position + new Vector2 ((float)(4 * -dir), 4f), new Vector2 (2f, 5f), Calc.Angle (new Vector2 ((float)dir, -0.5f)));
      base.Level.Session.MatchStats [this.PlayerIndex].Jumps += 1u;
      SaveData.Instance.Stats.Jumps++;
    }

    public void patch_HotCoalsBounce ()
    {
      if (IsAntiGrav() ? this.Speed.Y <= 0f : this.Speed.Y >= 0f) {
        Sounds.sfx_coalBurn.Play (base.X, 1f);
        if (this.input.JumpCheck) {
          this.Speed.Y = IsAntiGrav() ? 2.87999988f : -2.87999988f;
        } else {
          this.Speed.Y = IsAntiGrav() ? 2.24f : -2.24f;
        }
        if (this.input.MoveX != 0) {
          this.Speed.X += (float)this.input.MoveX * 0.3f;
        }
        this.Speed.X = MathHelper.Clamp (this.Speed.X, -3f, 3f);
        this.autoBounce = true;
        this.canVarJump = true;
        this.jumpGraceCounter.Set (0);
        this.Fire.Start ();
        this.bodySprite.Scale.X = 0.8f;
        this.bodySprite.Scale.Y = 1.2f;
      }
    }

    public void patch_OnCollideV (Platform platform)
    {
      if (IsAntiGrav() ? this.Speed.Y < 0f : this.Speed.Y > 0f) {
        this.ArcherData.SFX.Land.Play (base.X, 1f);
        this.bodySprite.Scale.X = 1f + this.Speed.Y / GetMaxFall() * 0.5f;
        this.bodySprite.Scale.Y = 1f - this.Speed.Y / GetMaxFall() * 0.5f;
        this.Speed.X = MathHelper.Lerp (this.Speed.X, 0f, 0.6f * (this.Speed.Y / GetMaxFall()));
        if (this.Speed.Y > GetMaxFall()) {
          base.Level.Particles.Emit (this.DustParticleType, 5, base.Position + new Vector2 (-4f, 8f), Vector2.One * 3f);
          base.Level.Particles.Emit (this.DustParticleType, 5, base.Position + new Vector2 (4f, 8f), Vector2.One * 3f);
        }
        if (IsAntiGrav() ? this.Speed.Y <= -1f : this.Speed.Y >= 1f) {
          this.ColdBreath ();
        }
      } else if (
        IsAntiGrav()
          ? (this.flapGravity > -1f && this.Speed.Y > 0f)
          : (this.flapGravity < 1f && this.Speed.Y < 0f)
        ) {
        this.Speed.Y = IsAntiGrav() ? -1.5f : 1.5f;
        this.flapBounceCounter.Set (4);
        return;
      }
      this.Speed.Y = 0f;
    }

    public void patch_BouncedOn (Player bouncer)
    {
      this.bodySprite.Scale.X = 1.3f;
      this.bodySprite.Scale.Y = 0.7f;
      this.Speed.Y = GetMaxFall();
      if (this.IsEnemy (bouncer)) {
        this.HurtBouncedOn (bouncer.PlayerIndex);
      }
    }

    public static void PlayerOnPlayer (patch_Player a, patch_Player b)
    {
      bool isAntiGrav = a.IsAntiGrav() && b.IsAntiGrav();
      a.InvisOpacity = (b.InvisOpacity = 1f);
      float num = (isAntiGrav ? a.Bottom + a.Speed.Y : a.Top - a.Speed.Y);
      float num2 = (isAntiGrav ? b.Bottom + b.Speed.Y : b.Top - b.Speed.Y);
      if (Math.Abs (num - num2) > 200f) {
        if (num > num2) {
          num -= 240f;
        } else {
          num2 -= 240f;
        }
      }
      if (Math.Abs (num - num2) >= 10f) {
        patch_Player player;
        patch_Player player2;
        if (isAntiGrav ? num > num2 : num < num2) {
          player = a;
          player2 = b;
        } else {
          player = b;
          player2 = a;
        }
        if (
          (isAntiGrav
            ? player.Speed.Y <= 0f
            : player.Speed.Y >= 0f
          ) || (isAntiGrav
            ? player2.Speed.Y > 0f
            : player2.Speed.Y < 0f
          )
        ) {
          player2.BouncedOn (player);
          player.Bounce (player2.Top, player2.State == PlayerStates.Ducking && !player.IsEnemy (player2));
          if (a.Allegiance == b.Allegiance && a.Allegiance != Allegiance.Neutral) {
            a.TradeArrow (b);
          }
          TFGame.PlayerInputs [player.PlayerIndex].Rumble (0.4f, 15);
          if (player.State == PlayerStates.Dodging) {
            player.dodgeCurseSatisfied = true;
            player.Level.Session.MatchStats [player.PlayerIndex].DodgeStomps += 1u;
          } else if (player.IsHyper) {
            player.Level.Session.MatchStats [player.PlayerIndex].HyperStomps += 1u;
          }
        }
      } else {
        if (Player.collideCounter <= 0f) {
          Sounds.sfx_collide.Play (a.X, 1f);
          Player.collideCounter = 5f;
        }
        a.SideBouncePlayer (b);
        b.SideBouncePlayer (a);
        if (a.Allegiance == b.Allegiance && a.Allegiance != Allegiance.Neutral) {
          a.TradeArrow (b);
        } else if (a.Arrows.Count == 0 && b.Arrows.Count > 0 && a.input.MoveX == Math.Sign (WrapMath.DiffX (a.X, b.X))) {
          a.StealArrow (b);
        } else if (a.Arrows.Count > 0 && b.Arrows.Count == 0 && b.input.MoveX == Math.Sign (WrapMath.DiffX (b.X, a.X))) {
          b.StealArrow (a);
        }
      }
    }

    public override bool InputDucking {
      get {
        return this.OnGround && this.moveAxis.X == 0f && (IsAntiGrav() ? this.moveAxis.Y == -1f : this.moveAxis.Y == 1f);
      }
    }

    public bool IsAntiGrav()
    {
      return patch_Level.IsAntiGrav();
    }

    public float GetGravity()
    {
      return IsAntiGrav() ? -0.3f : 0.3f;
    }

    public float GetJump()
    {
      return IsAntiGrav() ? 3.2f : -3.2f;
    }

    public float GetJumpOnPad()
    {
      return IsAntiGrav() ? 4.3f : -4.3f;
    }

    public float GetMaxFall()
    {
      return IsAntiGrav() ? -2.8f : 2.8f;
    }

    public float GetFastFall()
    {
      return IsAntiGrav() ? -3.5f : 3.5f;
    }

    public float GetWingsMaxFall()
    {
      return IsAntiGrav() ? -0.8f : 0.8f;
    }
  }
}
