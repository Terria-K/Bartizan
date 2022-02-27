using TowerFall;
using Patcher;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mod
{
    [Patch]
    public class MyMatchVariants : MatchVariants
    {
        public static List<string> ModVariants = new List<string> {
            "NoHeadBounce",
            "NoLedgeGrab",
            "AwfullySlowArrows",
            "AwfullyFastArrows",
            "InfiniteArrows",
            "NoDodgeCooldowns",
            "VarietyPack",
            "GottaBustGhosts",
            "CrownSummonsChaliceGhost",
            "ChaliceGhostsHuntGhosts",
            "FastGhosts",
            "GhostRevives",
            "GhostItems",
            "GhostJoust",
            "CalvinFall",
            "MeanerMonsters"
        };

        [Header("MODS")]
        [PerPlayer, CanRandom]
        public Variant NoHeadBounce;
        [PerPlayer, CanRandom]
        public Variant NoLedgeGrab;
        [CanRandom]
        public Variant AwfullySlowArrows;
        [CanRandom]
        public Variant AwfullyFastArrows;
        [PerPlayer, CanRandom]
        public Variant InfiniteArrows;
        [PerPlayer, CanRandom]
        public Variant NoDodgeCooldowns;

        [PerPlayer, CanRandom, Description ("START WITH ONE OF EVERY ARROW TYPE")]
        public Variant VarietyPack;

        [CanRandom, Description ("THE ROUND WON'T END UNTIL GHOSTS ARE DEAD")]
        public Variant GottaBustGhosts;

        [CanRandom, Description ("PUTTING ON A CROWN SUMMONS THE CHALICE GHOST")]
        public Variant CrownSummonsChaliceGhost;

        [CanRandom, Description ("THE CHALICE GHOST WILL GO AFTER GHOSTS")]
        public Variant ChaliceGhostsHuntGhosts;

        [CanRandom, Description ("GHOSTS ARE REALLY FAST")]
        public Variant FastGhosts;

        [CanRandom, Description ("GHOSTS CAN REVIVE")]
        public Variant GhostRevives;

        [CanRandom, Description ("GHOSTS CAN OPEN NON-BOMB CHESTS AND COLLECT SOME ITEMS")]
        public Variant GhostItems;

        [CanRandom, Description ("GHOSTS CAN KILL OTHER GHOSTS BY DASHING INTO THEM")]
        public Variant GhostJoust;

        [Description ("NEW RANDOM VARIANTS EVERY ROUND")]
        public Variant CalvinFall;

        [CanRandom, Description ("MORE TYPES OF MONSTERS SPAWN FROM PORTAL")]
        public Variant MeanerMonsters;

        public MyMatchVariants(bool noPerPlayer = false) : base(noPerPlayer)
        {
            // mutually exclusive variants
            this.CreateLinks(NoHeadBounce, NoTimeLimit);
            this.CreateLinks(NoDodgeCooldowns, ShowDodgeCooldown);
            this.CreateLinks(AwfullyFastArrows, AwfullySlowArrows);
        }

        new public static Subtexture GetVariantIconFromName (string variantName)
        {
            bool isModVariant = MyMatchVariants.ModVariants.Contains(variantName);

            if (isModVariant) {
                return MyTFGame.ModAtlas ["variants/" + variantName [0].ToString ().ToLower (CultureInfo.InvariantCulture) + variantName.Substring (1)];
            } else {
                return TFGame.MenuAtlas ["variants/" + variantName [0].ToString ().ToLower (CultureInfo.InvariantCulture) + variantName.Substring (1)];
            }
        }
    }
}
