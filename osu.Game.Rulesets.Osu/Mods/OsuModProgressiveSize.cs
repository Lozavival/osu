// Em osu.Game.Rulesets.Osu/Mods/OsuModProgressiveSize.cs
using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers; // Adicione este using, ele é necessário para o PlayfieldAdjustmentContainer
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

// O namespace correto
namespace osu.Game.Rulesets.Osu.Mods
{
    public partial class OsuModProgressiveSize : Mod, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Progressive Size";
        public override string Acronym => "PS";
        public override LocalisableString Description => "O tamanho dos círculos muda durante o mapa.";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1.0;
        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAltV;

        #region Configurações
        [SettingSource("Tamanho Máximo", "O tamanho inicial e máximo dos círculos.")]
        public BindableNumber<float> MaxScale { get; } = new BindableFloat(1.5f)
        {
            MinValue = 1.0f,
            MaxValue = 5.0f,
            Precision = 0.1f,
        };

        [SettingSource("Tamanho Mínimo", "O tamanho mínimo que os círculos podem atingir.")]
        public BindableNumber<float> MinScale { get; } = new BindableFloat(0.5f)
        {
            MinValue = 0.1f,
            MaxValue = 1.0f,
            Precision = 0.1f,
        };

        [SettingSource("Velocidade de Diminuição", "Velocidade que o tamanho diminui (escala/segundo).")]
        public BindableNumber<float> ShrinkRate { get; } = new BindableFloat(0.1f)
        {
            MinValue = 0.01f,
            MaxValue = 0.5f,
            Precision = 0.01f,
        };

        [SettingSource("Recuperação por Miss", "O quanto o tamanho recupera ao errar.")]
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

            // A CORREÇÃO FINAL!
            // Baseado no DrawableOsuRuleset.cs, este é o container correto.
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
