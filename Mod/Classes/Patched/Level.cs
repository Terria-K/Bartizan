#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Monocle;
using MonoMod;
using Mod;
using System;
using System.Xml;

namespace TowerFall
{
  public class patch_Level : Level
  {
    private bool antiGravEnabled;

    public patch_Level(Session session, XmlElement xml) : base (session, xml)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Session session, XmlElement xml);
    [MonoModConstructor]
    public void ctor(Session session, XmlElement xml)
    {
      orig_ctor(session, xml);
      antiGravEnabled = false;
      this.ToggleGravity();
    }

    public bool ToggleGravity()
    {
      antiGravEnabled = !antiGravEnabled;

      foreach (patch_Player player in this.Players) {
        player.InitHead();
        player.InitBody();
      }

      foreach(patch_TreasureChest chest in ((Scene)this)[GameTags.TreasureChest]) {
        chest.FlipSprite();
      }

      foreach(patch_PlayerCorpse corpse in ((Scene)this)[GameTags.Corpse]) {
        corpse.FlipSprite();
      }

      return antiGravEnabled;
    }

    public bool IsAntiGravEnabled()
    {
      return this.antiGravEnabled;
    }

    public static bool IsAntiGrav()
    {
      if (Engine.Instance.Scene is Level) {
        return (Engine.Instance.Scene as patch_Level).IsAntiGravEnabled();
      }
      return false;
    }
  }
}