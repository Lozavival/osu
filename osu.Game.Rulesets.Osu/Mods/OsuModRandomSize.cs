// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModRandomSize : Mod, IApplicableToDrawableHitObject
    {
        public override string Name => "Random Size";
        public override string Acronym => "RS";
        public override LocalisableString Description => "Every circle spawns with a random size.";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1.0;
        public override IconUsage? Icon => FontAwesome.Solid.Dice;

        #region Settings
        [SettingSource("Min Size", "The minimum possible size of the circles.")]
        public BindableNumber<float> MinScale { get; } = new BindableFloat(0.5f)
        {
            MinValue = 0.1f,
            MaxValue = 1.5f,
            Precision = 0.1f,
        };

        [SettingSource("Max Size", "The maximum possible size of the circles.")]
        public BindableNumber<float> MaxScale { get; } = new BindableFloat(1.5f)
        {
            MinValue = 0.5f,
            MaxValue = 2.5f,
            Precision = 0.1f,
        };

        #endregion

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable.HitObject is not OsuHitObject osuObject)
                return;

            // DETERMINISM:
            // We create a random seed based on the object's StartTime. 
            // This ensures that if you play the map again or watch a replay, 
            // this specific circle will always have the same "random" size.
            int seed = (int)osuObject.StartTime;
            var random = new Random(seed);

            // Calculate the random scale within the range
            float range = MaxScale.Value - MinScale.Value;
            // Ensure Max is actually higher than Min to prevent crashes if settings are weird
            if (range < 0) range = 0;

            float randomScale = (float)(random.NextDouble() * range) + MinScale.Value;

            drawable.ApplyCustomUpdateState += (o, state) =>
            {
                o.Scale = new Vector2(randomScale);
            };
        }
    }
}