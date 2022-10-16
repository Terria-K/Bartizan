using TowerFall;
using Monocle;
using MonoMod;
using Microsoft.Xna.Framework;
using System;

namespace Mod
{
  class GhostArrow : Arrow
  {
    public FlashingImage headImage;

    public FlashingImage shaftImage;

    public FlashingImage featherImage;

    public FlashingImage outlineImage;

    public bool wasMiracled = false;

    public override ArrowTypes ArrowType {
      get {
        return (ArrowTypes)MyGlobals.ArrowTypes.Ghost;
      }
    }

    // Too ghostly to be flammable
    public override bool CanCatchFire {
      get {
        return false;
      }
    }

    public override void Init(LevelEntity owner, Vector2 position, float direction)
    {
      base.Init(owner, position, direction);
      base.LightVisible = true;
      base.StopFlashing ();
    }

    public override void CreateGraphics()
    {
      this.headImage = new FlashingImage(patch_TFGame.ModAtlas["arrows/ghostHead"], null);
      this.headImage.Origin = new Vector2 (11f, 3f);
      this.shaftImage = new FlashingImage(patch_TFGame.ModAtlas["arrows/ghostShaft"], null);
      this.shaftImage.Origin = new Vector2 (11f, 3f);
      this.featherImage = new FlashingImage (patch_TFGame.ModAtlas["arrows/ghostFeather"], null);
      this.featherImage.Origin = new Vector2 (11f, 3f);
      this.outlineImage = new FlashingImage (patch_TFGame.ModAtlas["arrows/ghostOutline"], null);
      this.outlineImage.Origin = new Vector2 (11f, 3f);
      base.Graphics = new Image[4] {
        this.headImage,
        this.shaftImage,
        this.featherImage,
        this.outlineImage
      };
      base.Add(base.Graphics);
    }

    public override void InitGraphics ()
    {
      this.headImage.Visible = true;
      this.shaftImage.Visible = true;
      this.outlineImage.Visible = true;
      this.outlineImage.Color = ArcherData.GetColorB(base.PlayerIndex, base.TeamColor);

      base.LightColor = Calc.Invert(ArcherData.GetColorB(base.PlayerIndex, base.TeamColor));
    }

    public override void SwapToBuriedGraphics ()
    {
      this.headImage.Visible = false;
      this.outlineImage.Visible = true;
    }

    public override void SwapToUnburiedGraphics ()
    {
      this.headImage.Visible = true;
      this.outlineImage.Visible = false;
    }


    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    public override void Update()
    {
      // Pasted from Arrow.Update() with one short block removed
      if (this.squished && !base.CollideCheck (GameTags.Solid)) {
        this.squished = false;
        base.Collidable = true;
      }
      if (this.BuriedIn != null && this.State != ArrowStates.Buried) {
        this.State = ArrowStates.Buried;
      }
      Entity entity = null;
      bool flag = this.Speed.Y < 0f && (entity = base.CollideFirst (GameTags.JumpThru)) != null;
      if (flag) {
        (entity as JumpThru).OnArrowPop (base.X);
        if (!this.didPopThroughJumpThru) {
          Sounds.env_gplatArrowPierce.Play (base.X, 1f);
        }
      }
      this.didPopThroughJumpThru = flag;
      if (this.Speed != Vector2.Zero && base.Scene.OnInterval (1) && base.CollideCheck (GameTags.Moonglass)) {
        base.Level.Particles.Emit (Particles.ArrowInMoonglass, 1, base.Position, Vector2.One * 2f);
      }
      if ((bool)this.cannotCatchCounter) {
        this.cannotCatchCounter.Update ();
      }
      if (this.CanCatchFire) {
        if (this.Fire.OnFire) {
          base.LightVisible = true;
          this.Fire.Offset = Calc.AngleToVector (this.Direction, -8f) + new Vector2 (0f, 0.5f);
          if (this.State < ArrowStates.Stuck) {
            entity = base.CollideFirst (GameTags.Brambles);
            if ((bool)entity) {
              (entity as Brambles).Fire.Start ();
            }
          }
        } else {
          base.LightVisible = false;
        }
      }
      this.EnableSolids ();
      base_Update();
      this.ArrowUpdate ();
      // Removed code here that prevented non-laser-or-bramble arrows from disappearing
      if ((bool)this.cannotPickupCounter) {
        this.cannotPickupCounter.Update ();
      }
      if ((bool)this.cannotHitEnemiesCounter) {
        this.cannotHitEnemiesCounter.Update ();
      }
      if (this.Dangerous) {
        if (this.CheckForTargetCollisions ()) {
          this.DisableSolids ();
          return;
        }
        if (this.CannotHit != null && WrapMath.WrapDistanceSquared (base.Position, this.CannotHit.Position) >= 400f) {
          this.CannotHit = null;
        }
        base.Collider = this.otherArrowHitbox;
        Arrow arrow = base.CollideFirst (GameTags.Arrow) as Arrow;
        base.Collider = this.NormalHitbox;
        if (arrow != null && arrow.Dangerous && Vector2.Dot (this.Speed, arrow.Speed) <= -0.4f) {
          this.EnterFallMode (true, false, true);
          arrow.EnterFallMode (true, false, true);
          this.DisableSolids ();
          return;
        }
      }
      this.DisableSolids ();

      // Disappear like a laser arrow
      if (!base.Flashing && base.State >= ArrowStates.Stuck) {
        base.Flash(60, base.RemoveSelf);
      }
    }
  }
}