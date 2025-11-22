// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModProgressiveSize : Mod, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Progressive Size";
        public override string Acronym => "PS";
        public override LocalisableString Description => "The circles get progressively smaller.";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1.0;
        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAltV;

        #region Settings
        [SettingSource("Max Size", "The inicial and maximum size of the circles.")]
        public BindableNumber<float> MaxScale { get; } = new BindableFloat(1.5f)
        {
            MinValue = 1.0f,
            MaxValue = 5.0f,
            Precision = 0.1f,
        };

        [SettingSource("Min Size", "The minimal size of the circles.")]
        public BindableNumber<float> MinScale { get; } = new BindableFloat(0.5f)
        {
            MinValue = 0.1f,
            MaxValue = 1.0f,
            Precision = 0.1f,
        };

        [SettingSource("Shrink Rate", "Rate at which the circles shrink (scale/second).")]
        public BindableNumber<float> ShrinkRate { get; } = new BindableFloat(0.1f)
        {
            MinValue = 0.01f,
            MaxValue = 0.5f,
            Precision = 0.01f,
        };

        [SettingSource("Miss Recovery", "How much size the circles recover after a miss.")]
        public BindableNumber<float> MissRecovery { get; } = new BindableFloat(0.2f)
        {
            MinValue = 0.0f,
            MaxValue = 1.0f,
            Precision = 0.05f,
        };
        #endregion

        private readonly Bindable<float> currentScaleBindable = new Bindable<float>();

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            currentScaleBindable.Value = MaxScale.Value;

            var updater = new OsuModProgressiveSizeUpdater(
                currentScaleBindable,
                MinScale,
                MaxScale,
                ShrinkRate,
                MissRecovery,
                drawableRuleset.Playfield.HitObjectContainer
            );

            drawableRuleset.PlayfieldAdjustmentContainer.Add(updater);

            drawableRuleset.NewResult += updater.OnNewResult;
        }

        private partial class OsuModProgressiveSizeUpdater : Component
        {
            private readonly Bindable<float> currentScale;
            private readonly Bindable<float> minScale;
            private readonly Bindable<float> maxScale;
            private readonly Bindable<float> shrinkRate;
            private readonly Bindable<float> missRecovery;

            private readonly IHitObjectContainer hitObjectContainer;

            public OsuModProgressiveSizeUpdater(Bindable<float> currentScale, Bindable<float> minScale, Bindable<float> maxScale, Bindable<float> shrinkRate, Bindable<float> missRecovery, IHitObjectContainer hitObjectContainer)
            {
                this.currentScale = currentScale;
                this.minScale = minScale;
                this.maxScale = maxScale;
                this.shrinkRate = shrinkRate;
                this.missRecovery = missRecovery;
                this.hitObjectContainer = hitObjectContainer;
            }

            protected override void Update()
            {
                base.Update();

                float elapsedSeconds = (float)Clock.ElapsedFrameTime / 1000.0f;
                float newScaleValue = currentScale.Value - (shrinkRate.Value * elapsedSeconds);

                newScaleValue = Math.Max(minScale.Value, newScaleValue);
                currentScale.Value = newScaleValue;

                foreach (var dho in hitObjectContainer.AliveObjects)
                {
                    dho.Scale = new Vector2(newScaleValue);
                }
            }

            public void OnNewResult(JudgementResult result)
            {
                if (result.Type.IsMiss())
                {
                    float newScaleValue = currentScale.Value + missRecovery.Value;
                    newScaleValue = Math.Min(maxScale.Value, newScaleValue);
                    currentScale.Value = newScaleValue;
                }
            }
        }
    }
}
