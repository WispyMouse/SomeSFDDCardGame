namespace SFDDCards
{
    public class FoeTarget : ICombatantTarget
    {
        public string Name => "Foe";

        public void ApplyDelta(DeltaEntry deltaEntry)
        {
            // No-op
        }
    }
}