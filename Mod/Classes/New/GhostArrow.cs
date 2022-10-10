using TowerFall;
using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Mod
{
  class GhostArrow : Arrow
  {
    public Image shaftImage;

    public Image buriedShaftImage;

    public Image featherImage;

    public bool wasMiracled = false;

    public override ArrowTypes ArrowType {
      get {
        return (ArrowTypes)MyGlobals.ArrowTypes.Ghost;
      }
    }

    // public override void OnPlayerCatch(Player player)
    // {
    //   // On regular catch
    //   Console.WriteLine("Caught ghost arrow");

    //   // Need to wait to see if miracle happens
    //   Alarm.Set(this, 60, delegate {
    //     Console.WriteLine("Done Waiting");
    //     if (this.wasMiracled && ((patch_Player)player).lastArrowCaught == this) {
    //       Console.WriteLine("WAS MIRACLED");
    //     }
    //     this.wasMiracled = false;
    //   }, Alarm.AlarmMode.Oneshot);
    // }

    public override void CreateGraphics()
    {
      this.shaftImage = new Image (patch_TFGame.ModAtlas["arrows/ghost"], null);
      this.shaftImage.Origin = new Vector2 (11f, 3f);
      this.buriedShaftImage = new Image (patch_TFGame.ModAtlas["arrows/ghostBuried"], null);
      this.buriedShaftImage.Origin = new Vector2 (11f, 3f);
      this.featherImage = new Image (patch_TFGame.ModAtlas["arrows/ghostFeather"], null);
      this.featherImage.Origin = new Vector2 (11f, 3f);
      base.Graphics = new Image[3] {
        this.shaftImage,
        this.buriedShaftImage,
        this.featherImage
      };
      base.Add(base.Graphics);
    }

    public override void InitGraphics ()
    {
      this.shaftImage.Visible = true;
      this.buriedShaftImage.Visible = false;
      this.featherImage.Color = ArcherData.GetColorB(base.PlayerIndex, base.TeamColor);
    }

    public override void SwapToBuriedGraphics ()
    {
      this.shaftImage.Visible = false;
      this.buriedShaftImage.Visible = true;
    }

    public override void SwapToUnburiedGraphics ()
    {
      this.shaftImage.Visible = true;
      this.buriedShaftImage.Visible = false;
    }
  }
}