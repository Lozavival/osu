// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModRandomSize : Mod, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Random Size";
        public override string Acronym => "RS";
        public override LocalisableString Description => "Every circle has a random static size based on a seed.";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1.0;
        public override IconUsage? Icon => FontAwesome.Solid.Dice;

        #region Settings

        [SettingSource("Min Size", "The minimum possible size of the circles.")]
        public BindableNumber<float> MinScale { get; } = new BindableFloat(0.5f)
        {
            MinValue = 0.1f,
            MaxValue = 1.0f,
            Precision = 0.1f,
        };

        [SettingSource("Max Size", "The maximum possible size of the circles.")]
        public BindableNumber<float> MaxScale { get; } = new BindableFloat(1.5f)
        {
            MinValue = 1.0f,
            MaxValue = 2.5f,
            Precision = 0.1f,
        };

        [SettingSource("Seed", "Change this to generate a different set of random sizes.")]
        public BindableInt Seed { get; } = new BindableInt(0)
        {
            MinValue = 0,
            MaxValue = int.MaxValue,
            Precision = 1,
        };
        #endregion

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            var updater = new OsuModRandomSizeUpdater(
                MinScale,
                MaxScale,
                Seed,
                drawableRuleset.Playfield.HitObjectContainer
            );

            drawableRuleset.PlayfieldAdjustmentContainer.Add(updater);
        }

        private partial class OsuModRandomSizeUpdater : Component
        {
            private readonly Bindable<float> minScale;
            private readonly Bindable<float> maxScale;
            private readonly Bindable<int> seed;
            private readonly IHitObjectContainer hitObjectContainer;

            public OsuModRandomSizeUpdater(Bindable<float> minScale, Bindable<float> maxScale, Bindable<int> seed, IHitObjectContainer hitObjectContainer)
            {
                this.minScale = minScale;
                this.maxScale = maxScale;
                this.seed = seed;
                this.hitObjectContainer = hitObjectContainer;
            }

            protected override void Update()
            {
                base.Update();

                float range = maxScale.Value - minScale.Value;
                if (range < 0) range = 0;

                foreach (var dho in hitObjectContainer.AliveObjects)
                {
                    if (dho.HitObject is OsuHitObject osuObject)
                    {
                        int uniqueObjectSeed = seed.Value + (int)osuObject.StartTime;
                        var random = new Random(uniqueObjectSeed);

                        float randomScale = (float)(random.NextDouble() * range) + minScale.Value;
                        dho.Scale = new Vector2(randomScale);
                    }
                }
            }
        }
    }
}
