#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;

namespace TowerFall
{
  public class patch_ReplayFrame : ReplayFrame
  {
    public extern void orig_Record(float timeSinceLastFrame, long ticksSinceLastFrame);
    public void patch_Record (float timeSinceLastFrame, long ticksSinceLastFrame)
    {
      orig_Record(timeSinceLastFrame, ticksSinceLastFrame);
      #if (EIGHT_PLAYER)
        // Undo screen offset to fix glitched out frames during screen shake or offset effects
        // Not an ideal solution, but an improvement
        this.ScreenOffset = new Vector2(0, 0);
        this.ScreenOffsetAdd = new Vector2(0, 0);
      #endif
    }
  }
}
