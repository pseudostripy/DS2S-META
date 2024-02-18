namespace DS2S_META
{
    public class State
    {
        public record PlayerState(int HP, int Stamina, float[] StablePos, float[] Ang, float[] Cam);
    }
}
