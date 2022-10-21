#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Monocle;
using MonoMod;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TowerFall
{
  public class patch_VersusPlayerMatchResults : VersusPlayerMatchResults
  {
    public static int[] PlayerWins;

    OutlineText winsText;

    #if (EIGHT_PLAYER)
      public patch_VersusPlayerMatchResults(Session session, VersusMatchResults matchResults, int playerIndex, bool small, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards)
        : base(session, matchResults, playerIndex, small, tweenFrom, tweenTo, awards)
      {
        // no-op
      }
    #else
      public patch_VersusPlayerMatchResults(Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards)
        : base(session, matchResults, playerIndex, tweenFrom, tweenTo, awards)
      {
        // no-op
      }
    #endif

    #if (EIGHT_PLAYER)
      public extern void orig_ctor(Session session, VersusMatchResults matchResults, int playerIndex, bool small, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards);
      [MonoModConstructor]
      public void ctor(Session session, VersusMatchResults matchResults, int playerIndex, bool small, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards)
      {
        orig_ctor(session, matchResults, playerIndex, small, tweenFrom, tweenTo, awards);
        this.showWinCount();
      }
    #else
      public extern void orig_ctor(Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards);
      [MonoModConstructor]
      public void ctor(Session session, VersusMatchResults matchResults, int playerIndex, Vector2 tweenFrom, Vector2 tweenTo, List<AwardInfo> awards)
      {
        orig_ctor(session, matchResults, playerIndex, tweenFrom, tweenTo, awards);
        this.showWinCount();
      }
    #endif

    public void showWinCount()
    {
      if (session.MatchStats[playerIndex].Won) {
        PlayerWins[playerIndex]++;
      }

      if (PlayerWins[playerIndex] > 0) {
        winsText = new OutlineText(TFGame.Font, PlayerWins[playerIndex].ToString(), this.gem.Position);
        winsText.Color = Color.White;
        winsText.OutlineColor = Color.Black;
        this.Add(winsText);
      }
    }

    public extern void orig_Render();
    public void patch_Render()
    {
      orig_Render();
      if (winsText != null)
        winsText.Render();
    }
  }
}
