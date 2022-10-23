#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0114 // 'function1' hides inherited member 'function2'. To make the current method override that implementation, add the override keyword. Otherwise add the new keyword.
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

using MonoMod;
using Monocle;
using Mod;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TowerFall
{
  class patch_ChaliceGhost : ChaliceGhost
  {
    public bool huntsGhosts;
    public bool targetIsGhost;
    public PlayerGhost ghostTarget;
    public Player playerTarget;
    public Actor actorTarget;

    public patch_ChaliceGhost(int ownerIndex, Chalice source) : base(ownerIndex, source)
    {
      // no-op. MonoMod ignores this
    }

    public patch_ChaliceGhost(int ownerIndex, Chalice source, bool huntsGhosts=false) : base(ownerIndex, source)
    {
      // no-op. MonoMod ignores this
    }

    [MonoModLinkTo("Monocle.Entity", "Tag")]
    [MonoModIgnore]
    public extern void base_Tag(GameTags tag);

    [MonoModConstructor]
    public void ctor(int ownerIndex, Chalice source, bool huntsGhosts=false)
    {
      orig_ctor(ownerIndex, source);
      base_Tag(GameTags.PlayerGhostCollider);
      this.huntsGhosts = huntsGhosts;
    }

    public extern void orig_ctor(int ownerIndex, Chalice source);
    [MonoModConstructor]
    public void ctor(int ownerIndex, Chalice source)
    {
      orig_ctor(ownerIndex, source);
      base_Tag(GameTags.PlayerGhostCollider);
    }

    [MonoModIgnore]
    [MonoModPublic]
    public extern void Vanish();

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();
    public void patch_Update ()
    {
      base_Update();

      if (!this.dead) {
        foreach (Arrow item in ((Scene)base.Level)[GameTags.Arrow]) {
          if (this.ArrowCheck(item)) {
            this.OnArrowHit(item);
          }
        }
      }

      if (base.Scene.OnInterval (5)) {
        this.flash = !this.flash;
      }
      if (this.flash) {
        this.cloak.Color = this.colorA * 0.75f;
      } else {
        this.cloak.Color = this.colorB * 0.75f;
      }
      if (this.speed.X != 0f) {
        this.sprite.FlipX = (this.speed.X < 0f);
      }
      this.cloak.Scale = this.sprite.Scale;
      this.cloak.CurrentFrame = this.sprite.CurrentFrame;
      this.cloak.FlipX = this.sprite.FlipX;
      if (this.actorTarget != null) {
        if (this.targetIsGhost && this.ghostTarget.State == 3) {
          this.actorTarget = null;
        } else if (this.playerTarget != null && this.playerTarget.Dead) {
          this.actorTarget = null;
        }
      }
      if (this.canFindTarget) {
        if (this.actorTarget == null) {
          this.actorTarget = this.GetClosestActorTarget();
        } else {
          float num = (this.actorTarget.Position - this.Position).LengthSquared ();
          num -= 400f;
          Actor closestTarget = this.GetClosestActorTarget(num);
          if (closestTarget != null) {
            this.actorTarget = closestTarget;
          }
        }
      }
      if (this.actorTarget != null) {
        this.speed = this.speed.Approach (this.GetTargetSpeed (), MathHelper.Lerp (0.06f, 0.15f, this.lerp) * Engine.TimeMult);
      } else {
        this.speed = this.speed.Approach (Vector2.Zero, 0.1f * Engine.TimeMult);
      }
      this.Position += this.speed * Engine.TimeMult;
      if (!this.dead && this.sprite.CurrentAnimID == "idle") {
        bool flag = this.GetClosestActorTarget() != null;
        if (!flag) {
          this.Vanish ();
        }
      } else if (this.dead && this.sprite.CurrentAnimID == "idle") {
        this.Vanish ();
      }
    }

    public override void OnPlayerGhostCollide (PlayerGhost ghost)
    {
      if (this.CanAttackGhost(ghost)) {
        this.sprite.Play ("attack", true);
        this.speed = (ghost.Position - this.Position).SafeNormalize (2f);
        ghost.Die (this.ownerIndex);
        this.canFindTarget = false;
        this.target = null;
        this.ghostTarget = null;
        this.targetIsGhost = false;
        Alarm.Set (this, 30, delegate {
          this.canFindTarget = true;
        }, Alarm.AlarmMode.Oneshot);
        Sounds.sfx_chaliceGhostKill.Play (210f, 1f);
        this.wiggler.Start ();
      }
    }

    public Vector2 patch_GetTargetSpeed ()
    {
      return Calc.SafeNormalize (WrapMath.Shortest (base.Position, this.actorTarget.Position), MathHelper.Lerp (1.2f, 2.4f, this.lerp));
    }

    public Actor GetClosestActorTarget(float maxDistSq)
    {
      Actor result = null;
      float num = maxDistSq;
      Random rand = new Random();

      if (this.huntsGhosts) {
        // flip a coin to determine whether to check for ghosts first or players first
        if (rand.Next(2) == 0) {
          result = getPlayerTarget(maxDistSq);
          if (result == null) {
            result = getGhostTarget(maxDistSq);
          }
        } else {
          result = getGhostTarget(maxDistSq);
          if (result == null) {
            result = getPlayerTarget(maxDistSq);
          }
        }
      } else {
        return getPlayerTarget(maxDistSq);
      }

      return result;
    }

    public Actor GetClosestActorTarget()
    {
      return this.GetClosestActorTarget(3.40282347E+38f);
    }

    public bool CanAttackGhost (PlayerGhost ghost)
    {
      bool result;
      if (this.team != Allegiance.Neutral) {
        result = (ghost.Allegiance != this.team);
      } else {
        result = (ghost.PlayerIndex != this.ownerIndex);
      }
      return result;
    }

    public Actor getGhostTarget(float maxDistSq)
    {
      PlayerGhost result = null;
      float num = maxDistSq;
      using (List<Entity>.Enumerator enumerator = base.Level [GameTags.PlayerGhost].GetEnumerator ()) {
        while (enumerator.MoveNext ()) {
          PlayerGhost ghost = (PlayerGhost)enumerator.Current;
          if (this.CanAttackGhost (ghost) && ghost.State != 3 /* Ghost not dead */) {
            float num2 = WrapMath.WrapDistanceSquared (this.Position, ghost.Position);
            if (num2 < num) {
              num = num2;
              result = ghost;
            }
          }
        }
      }
      if (result != null) {
        targetIsGhost = true;
        ghostTarget = result;
      }
      return result;
    }

    public Actor getPlayerTarget(float maxDistSq)
    {
      Player result = null;
      float num = maxDistSq;
      using (List<Entity>.Enumerator enumerator = base.Level [GameTags.Player].GetEnumerator ()) {
        while (enumerator.MoveNext ()) {
          Player player = (Player)enumerator.Current;
          if (this.CanAttack (player) && !player.Dead) {
            float num2 = WrapMath.WrapDistanceSquared (this.Position, player.Position);
            if (num2 < num) {
              num = num2;
              result = player;
            }
          }
        }
      }
      if (result != null) {
        targetIsGhost = false;
        playerTarget = result;
      }
      return result;
    }

    public override bool OnArrowHit(Arrow arrow)
    {
      if (arrow.ArrowType == (ArrowTypes)(MyGlobals.ArrowTypes.Ghost)) {
        this.Vanish();
        return true;
      }
      return false;
    }
  }
}