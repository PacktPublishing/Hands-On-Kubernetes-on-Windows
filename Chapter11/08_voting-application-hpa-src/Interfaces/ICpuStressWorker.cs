namespace VotingApplication.Interfaces
{
    public interface ICpuStressWorker
    {
        void Enable(int value);

        void Disable();
    }
}