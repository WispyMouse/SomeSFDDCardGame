namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class DamageScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> Damage { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "DAMAGE";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.Damage;
            tokenBuilder.Intensity = Damage;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // [DAMAGE: 5] or [DAMAGE: TARGET_HEALTH] or [DAMAGE: TARGET_HEALTH - 5] are all valid options
            // There should only be one resulting evaluated value out of this
            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> _))
            {
                return false;
            }

            scriptingToken = new DamageScriptingToken()
            {
                Damage = output
            };

            return true;
        }

        public override bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            // Damage is always harmful!
            return true;
        }

        public override bool RequiresTarget()
        {
            return true;
        }
    }
}