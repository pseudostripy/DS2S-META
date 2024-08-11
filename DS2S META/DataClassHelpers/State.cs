namespace DS2S_META.DataClassHelpers
{
    public class State
    {
        public record PlayerState(int HP, int Stamina, float[] StablePos, float[] Ang, float[] Cam);
    }
}
