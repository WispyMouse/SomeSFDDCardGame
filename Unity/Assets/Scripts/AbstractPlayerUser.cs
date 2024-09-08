namespace SFDDCards
{
    public class AbstractPlayerUser : ICombatantTarget
    {
        public string Name => "Player";

        public void ApplyDelta(DeltaEntry deltaEntry)
        {
            // No-op
        }
    }
}