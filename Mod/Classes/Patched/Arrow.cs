#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Monocle;

namespace TowerFall
{
  public abstract class patch_Arrow : Arrow
  {
    const float AwfullySlowArrowMult = 0.2f;
    const float AwfullyFastArrowMult = 3.0f;

    public extern void orig_Added();
    public void patch_Added()
    {
      orig_Added();

      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullyFastArrows) {
        this.NormalHitbox = new WrapHitbox(6f, 3f, -1f, -1f);
        this.otherArrowHitbox = new WrapHitbox(12f, 4f, -2f, -2f);
      }
    }

    public extern void orig_ArrowUpdate();
    public void patch_ArrowUpdate()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullySlowArrows) {
        // Engine.TimeMult *= AwfullySlowArrowMult;
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullySlowArrowMult, null);
        orig_ArrowUpdate();
        // Engine.TimeMult /= AwfullySlowArrowMult;
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullySlowArrowMult, null);
      } else if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).AwfullyFastArrows) {
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult * AwfullyFastArrowMult, null);
        orig_ArrowUpdate();
        typeof(Engine).GetProperty("TimeMult").SetValue(null, Engine.TimeMult / AwfullyFastArrowMult, null);
      } else
        orig_ArrowUpdate();
    }
  }
}