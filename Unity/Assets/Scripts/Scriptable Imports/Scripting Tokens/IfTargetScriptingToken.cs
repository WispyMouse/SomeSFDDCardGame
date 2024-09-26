using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;
using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class IfTargetScriptingToken : BaseScriptingToken, IRequirement
    {
        public override string ScriptingTokenIdentifier => "IFTARGET";

        public CombatantTargetEvaluatableValue Target;

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Requirements.Add(this);
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // The only expected argument is the targeting value
            if (arguments.Count != 1)
            {
                return false;
            }

            string firstArgument = arguments[0].ToLower();

            switch (firstArgument)
            {
                case "self":
                    scriptingToken = new IfTargetScriptingToken() { Target = new SelfTargetEvaluatableValue() };
                    return true;
                case "foe":
                    scriptingToken = new IfTargetScriptingToken() { Target = new FoeTargetEvaluatableValue() };
                    return true;
                case "allfoes":
                    scriptingToken = new IfTargetScriptingToken() { Target = new AllFoeTargetEvaluatableValue() };
                    return true;
                case "original":
                    scriptingToken = new IfTargetScriptingToken() { Target = new OriginalTargetEvaluatableValue() };
                    return true;
            }

            return false;
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        public bool MeetsRequirement(TokenEvaluatorBuilder builder, CampaignContext context)
        {
            if (!this.Target.TryEvaluateValue(context, builder, out ICombatantTarget targetEvaluated))
            {
                return false;
            }

            return builder.Target.OverlapsTarget(builder.User, targetEvaluated) || targetEvaluated.OverlapsTarget(builder.User, builder.Target);        }
    }
}