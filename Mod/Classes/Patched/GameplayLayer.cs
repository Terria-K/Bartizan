#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using TowerFall;
using Monocle;
using Mod;
using System;
using System.Collections.Generic;

namespace TowerFall
{
  public class patch_GameplayLayer : GameplayLayer
  {
    public extern void orig_BatchedRender();
    public void patch_BatchedRender ()
    {
      orig_BatchedRender();

      List<Entity> teamRevivers = base.Scene[(GameTags)MyGlobals.GameTags.MyTeamReviver];
      for (int i = 0; i < teamRevivers.Count; i++) {
        MyTeamReviver teamReviver = (MyTeamReviver)teamRevivers[i];
        teamReviver.HUDRender ();
      }
    }
  }
}
