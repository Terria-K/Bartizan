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
      this.antiGravEnabled = false;
    }

    public bool ToggleGravity()
    {
      this.antiGravEnabled = !antiGravEnabled;

      foreach (patch_Player player in this.Players) {
        player.InitHead();
        player.InitBody();
      }

      return antiGravEnabled;
    }

    public bool IsAntiGravEnabled()
    {
      return this.antiGravEnabled;
    }

    public static void SetReverseGravity(bool enabled)
    {
      patch_Level level = (Engine.Instance.Scene as patch_Level);
      if (level.IsAntiGravEnabled() == enabled) {
        return;
      }
      level.ToggleGravity();
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