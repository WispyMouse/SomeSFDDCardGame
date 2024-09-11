namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class FoeTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "Foe";
        }

        public override bool TryEvaluateValue(CentralGameStateController gameStateController, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            if (TryGetFoe(gameStateController, currentBuilder.User, out evaluatedValue))
            {
                return true;
            }

            evaluatedValue = null;
            return false;
        }

        private bool TryGetFoe(CentralGameStateController gameStateController, ICombatantTarget user, out ICombatantTarget foe)
        {
            if (user is Enemy)
            {
                foe = gameStateController.CurrentPlayer;
                return true;
            }

            if (user is Player)
            {
                if (gameStateController.CurrentRoom.Enemies.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, gameStateController.CurrentRoom.Enemies.Count);
                    foe = gameStateController.CurrentRoom.Enemies[randomIndex];
                    return true;
                }
            }

            foe = null;
            return false;
        }
    }
}