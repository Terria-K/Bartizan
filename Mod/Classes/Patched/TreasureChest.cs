using Microsoft.Xna.Framework;

namespace TowerFall
{
  public class patch_TreasureChest : TreasureChest
  {
    public patch_TreasureChest(Vector2 position, Types graphic, AppearModes mode, Pickups pickup, int timer = 0) : base(position, graphic, mode, pickup, timer)
    {
      // no-op
    }

    public patch_TreasureChest(Vector2 position, Types graphic, AppearModes mode, Pickups[] pickups, int timer = 0) : base(position, graphic, mode, pickups, timer)
    {
      // no-op
    }

    public void patch_OnPlayerGhostCollide(PlayerGhost ghost)
    {
      if (!base.Flashing && this.type != Types.Large && this.type != Types.Bottomless)
      {
        if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).GhostItems)
        {
          patch_PlayerGhost g = (patch_PlayerGhost)ghost;
          if (this.pickups[0].ToString() == "SpeedBoots" && !g.HasSpeedBoots ||
            this.pickups[0].ToString() == "Shield" && !g.HasShield ||
            this.pickups[0].ToString() == "Mirror" && !g.Invisible ||
            this.pickups[0].ToString().Contains("Orb"))
          {
            this.OpenChest(ghost.PlayerIndex);
          } else
          {
            this.OpenChestForceBomb(ghost.PlayerIndex);
          }
        }
        else
        {
          this.OpenChestForceBomb(ghost.PlayerIndex);
        }

        TFGame.PlayerInputs[ghost.PlayerIndex].Rumble(0.5f, 12);
      }
    }
  }
}
