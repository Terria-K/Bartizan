using TowerFall;

namespace Mod
{
  class GhostArrow : DefaultArrow
  {
    public override ArrowTypes ArrowType {
      get {
        return (ArrowTypes)MyGlobals.ArrowTypes.Ghost;
      }
    }
  }
}