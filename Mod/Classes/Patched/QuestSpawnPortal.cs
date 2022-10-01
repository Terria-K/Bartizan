using Microsoft.Xna.Framework;
using Monocle;

namespace TowerFall
{
  public class patch_QuestSpawnPortal : QuestSpawnPortal
  {
    public patch_QuestSpawnPortal (Vector2 position, Vector2[] nodes)
      : base (position, nodes)
    {
      // no-op. MonoMod ignores this
    }

    public void patch_SpawnEnemy (string enemy)
    {
      if (((patch_MatchVariants)base.Level.Session.MatchSettings.Variants).MeanerMonsters) {
        string[] choices = {
          "Mole",
          "TechnoMage",
          "FlamingSkull",
          "Birdman",
          "DarkBirdman",
          "Slime",
          "RedSlime",
          "BlueSlime",
          "Bat",
          "BombBat",
          "SuperBombBat",
          "Crow",
          "Cultist",
          "ScytheCultist",
          "BossCultist"

          // The following enemies require a Nodes value passed to their construtor.
          // In VS mode, QuestSpawnPortals.Nodes is null.

          // "Exploder",
          // "EvilCrystal",
          // "BlueCrystal",
          // "BoltCrystal",
          // "PrismCrystal",
          // "Ghost",
          // "GreenGhost",
          // "Elemental",
          // "GreenElemental",

          // Exclude these skeleton enemies that shoot arrows

          // "Skeleton",
          // "BombSkeleton",
          // "LaserSkeleton",
          // "MimicSkeleton",
          // "DrillSkeleton",
          // "BoltSkeleton",
          // "Jester",
          // "BossSkeleton",
          // "BossWingSkeleton",
          // "WingSkeleton",
          // "TriggerSkeleton",
          // "PrismSkeleton"
        };
        enemy = choices[Calc.Random.Next(choices.Length)];
      }

      if (this.toSpawn.Count == 0) {
        this.sprite.Play (1, true);
      }
      this.toSpawn.Enqueue(enemy);
      this.autoDisappear = false;
    }
  }
}