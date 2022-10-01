#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Mod;
using System.Collections.Generic;

namespace TowerFall
{
  public class patch_VersusRoundResults : VersusRoundResults
  {
    private Modes _oldMode;

    public patch_VersusRoundResults(Session session, List<EventLog> events)
      : base(session, events)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Session session, List<EventLog> events);
    [MonoModConstructor]
    public void ctor(Session session, List<EventLog> events)
    {
      orig_ctor(session, events);
      this._oldMode = session.MatchSettings.Mode;
      if (
        this._oldMode == RespawnRoundLogic.Mode ||
        this._oldMode == MobRoundLogic.Mode
      ) {
        session.MatchSettings.Mode = Modes.HeadHunters;
      }
    }

    public extern void orig_TweenOut();
    public void patch_TweenOut()
    {
      this.session.MatchSettings.Mode = this._oldMode;
      orig_TweenOut();
    }
  }
}
