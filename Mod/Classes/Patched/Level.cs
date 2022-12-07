#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Mod;
using System.Xml;

namespace TowerFall
{
  public class patch_Level : Level
  {
    public patch_Level(Session session, XmlElement xml) : base (session, xml)
    {
      // no-op. MonoMod ignores this
    }

    public bool IsAntiGrav()
    {
      return MyGlobals.IsAntiGrav;
    }
  }
}