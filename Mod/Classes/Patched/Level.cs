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
    private bool reverseGravEnabled;

    public patch_Level(Session session, XmlElement xml) : base (session, xml)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Session session, XmlElement xml);
    [MonoModConstructor]
    public void ctor(Session session, XmlElement xml)
    {
      orig_ctor(session, xml);
      this.reverseGravEnabled = false;
    }

    public bool ToggleGravity()
    {
      this.reverseGravEnabled = !reverseGravEnabled;

      foreach (patch_Player player in this.Players) {
        player.InitHead();
        player.InitBody();
      }

      return reverseGravEnabled;
    }

    public bool IsReverseGravEnabled()
    {
      return this.reverseGravEnabled;
    }

    public static void SetReverseGravity(bool enabled)
    {
      patch_Level level = (Engine.Instance.Scene as patch_Level);
      if (level.IsReverseGravEnabled() == enabled) {
        return;
      }
      level.ToggleGravity();
    }

    public static bool IsReverseGrav()
    {
      if (Engine.Instance.Scene is Level) {
        return (Engine.Instance.Scene as patch_Level).IsReverseGravEnabled();
      }
      return false;
    }
  }
}