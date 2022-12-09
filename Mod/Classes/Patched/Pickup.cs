#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Microsoft.Xna.Framework;
using Monocle;
using Mod;

namespace TowerFall
{
  public class patch_Pickup : Pickup
  {
    public patch_Pickup(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
    {
      // no-op. MonoMod ignores this
    }

    private static bool IsAntiGrav()
    {
      return patch_Level.IsAntiGrav();
    }

    public extern static Pickup orig_CreatePickup(Vector2 position, Vector2 targetPosition, Pickups type, int playerIndex);
    public static Pickup patch_CreatePickup(Vector2 position, Vector2 targetPosition, Pickups type, int playerIndex)
    {
      if (type == (Pickups)(MyGlobals.Pickups.GhostArrows)) {
        Pickup pickup;
        pickup = new ArrowTypePickup(position, targetPosition, (ArrowTypes)(MyGlobals.ArrowTypes.Ghost));
        pickup.PickupType = type;
        return pickup;
      } else {
        return orig_CreatePickup(position, targetPosition, type, playerIndex);
      }
    }

    public static Vector2 patch_GetTargetPositionFromChest(Level level, Vector2 position)
    {
      Vector2 vector = position;
      for (int i = 0; i < 4; i++) {
        Rectangle rect = patch_Pickup.IsAntiGrav()
          ? new Rectangle ((int)vector.X - 2, (int)vector.Y + 8 + 4, 4, 8)
          : new Rectangle ((int)vector.X - 2, (int)vector.Y - 8 - 4, 4, 8);
        if (level.CollideCheck(rect, GameTags.Solid)) {
          break;
        }
        if (patch_Pickup.IsAntiGrav()) {
          vector += Vector2.UnitY * 8f;
        } else {
          vector -= Vector2.UnitY * 8f;
        }
      }
      return vector;
    }
  }
}