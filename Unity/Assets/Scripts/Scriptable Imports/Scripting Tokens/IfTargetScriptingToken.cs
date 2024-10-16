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
        public bool NotTarget = false;

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Requirements.Add(this);
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // This should contain either just the target, or the word NOT and then the target
            // The only expected argument is the targeting value
            if (arguments.Count != 1 && arguments.Count != 2)
            {
                return false;
            }

            bool notTarget = false;

            if (arguments.Count == 2)
            {
                if (arguments[0].ToLower() == "not")
                {
                    notTarget = true;
                }
                else
                {
                    return false;
                }
            }

            string firstArgument = arguments[arguments.Count - 1].ToLower();

            switch (firstArgument)
            {
                case "self":
                    scriptingToken = new IfTargetScriptingToken() { Target = new SelfTargetEvaluatableValue(), NotTarget = notTarget };
                    return true;
                case "foe":
                    scriptingToken = new IfTargetScriptingToken() { Target = new FoeTargetEvaluatableValue(), NotTarget = notTarget };
                    return true;
                case "allfoes":
                    scriptingToken = new IfTargetScriptingToken() { Target = new AllFoeTargetEvaluatableValue(), NotTarget = notTarget };
                    return true;
                case "original":
                    scriptingToken = new IfTargetScriptingToken() { Target = new OriginalTargetEvaluatableValue(), NotTarget = notTarget };
                    return true;
            }

            return false;
        }

        public bool MeetsRequirement(TokenEvaluatorBuilder builder, CampaignContext context)
        {
            if (!this.Target.TryEvaluateValue(context, builder, out ICombatantTarget targetEvaluated))
            {
                return false;
            }

            if (builder.Target.OverlapsTarget(builder.User, targetEvaluated) != this.NotTarget)
            {
                return true;
            }

            return false;
        }

        public string DescribeRequirement()
        {
            return $"the target is {(this.NotTarget == true ? "not " : "")}{this.Target.DescribeEvaluation()}";
        }
    }
}