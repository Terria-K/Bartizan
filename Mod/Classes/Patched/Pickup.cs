#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Microsoft.Xna.Framework;
using Mod;

namespace TowerFall
{
  public class patch_Pickup : Pickup
  {
    public patch_Pickup(Vector2 position, Vector2 targetPosition) : base(position, targetPosition)
    {
      // no-op. MonoMod ignores this
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
  }
}