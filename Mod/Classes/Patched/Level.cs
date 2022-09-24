#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using TowerFall;
using Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace TowerFall
{
  public class patch_Level : Level
  {
    public patch_Level (Session session, XmlElement xml) : base(session, xml)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_Update();
    public void patch_Update()
    {
      orig_Update();
      List<Entity> teamRevivers = base[(GameTags)MyGlobals.GameTags.MyTeamReviver];
      for (int i = 0; i < teamRevivers.Count; i++) {
        MyTeamReviver teamReviver = (MyTeamReviver)(teamRevivers[i]);
        if (teamReviver.Active) {
          teamReviver.ReviveUpdate ();
        }
      }
    }
  }
}
