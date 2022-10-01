#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod;

namespace TowerFall
{
  public class patch_MatchSettings : MatchSettings
  {
    public patch_MatchSettings(LevelSystem levelSystem, Modes mode, MatchSettings.MatchLengths matchLength)
      : base(levelSystem, mode, matchLength)
    {
      // no-op. MonoMod ignores this
    }

    public extern int orig_get_GoalScore();
    public new int GoalScore {
      get {
        switch (this.Mode) {
          case RespawnRoundLogic.Mode:
          case MobRoundLogic.Mode:
            #if (EIGHT_PLAYER)
              int goals = this.PlayerGoals(5, 8, 10, 10, 10, 10, 10);
            #else
              int goals = this.PlayerGoals(5, 8, 10);
            #endif
            return (int)Math.Ceiling(((float)goals * MatchSettings.GoalMultiplier[(int)this.MatchLength]));
          default:
            return orig_get_GoalScore();
        }
      }
    }
  }
}
