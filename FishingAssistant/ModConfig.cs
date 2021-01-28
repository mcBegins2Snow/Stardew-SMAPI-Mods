﻿using StardewModdingAPI;

namespace FishingAssistant
{
    internal class ModConfig
    {
        /// <summary>Button for toggle max or free fishing rod cast power</summary>
        public SButton EnableModButton { get; set; } = SButton.F5;

        /// <summary>Button for toggle max or free fishing rod cast power</summary>
        public SButton CastPowerButton { get; set; } = SButton.F6;

        /// <summary>Button for toggle catch or ignore treasure in fishing minigame</summary>
        public SButton CatchTreasureButton { get; set; } = SButton.F7;

        /// <summary>Button for roload config file</summary>
        public SButton ReloadConfigButton { get; set; } = SButton.F8;

        /// <summary>Whether the game should consider every catch to be perfectly executed, even if it wasn't.</summary>
        public bool AlwaysPerfect { get; set; } = false;

        /// <summary>Whether to always find treasure.</summary>
        public bool AlwaysFindTreasure { get; set; } = false;

        /// <summary>Make fish to bite  instantly.</summary>
        public bool InstantFishBite { get; set; } = false;

        /// <summary>Whether to catch fish instantly.</summary>
        public bool InstantCatchFish { get; set; } = false;

        /// <summary>Whether to catch treasure instantly.</summary>
        public bool InstantCatchTreasure { get; set; } = false;

        /// <summary>Whether fishing tackles last forever.</summary>
        public bool InfiniteTackle { get; set; } = true;

        /// <summary>Whether fishing bait lasts forever.</summary>
        public bool InfiniteBait { get; set; } = true;

        /// <summary>A multiplier applied to the fish difficulty. This can a number between 0 and 1 to lower difficulty, or more than 1 to increase it.</summary>
        public float FishDifficultyMultiplier { get; set; } = 1;

        /// <summary>A value added to the fish difficulty. This can be less than 0 to decrease difficulty, or more than 0 to increase it.</summary>
        public float FishDifficultyAdditive { get; set; } = 0;
    }
}