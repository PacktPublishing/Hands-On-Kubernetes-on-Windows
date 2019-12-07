namespace VotingApplication.Interfaces
{
    using VotingApplication.Models;

    public interface IVoteLogManager
    {
        void Append(Vote vote);

        string Get();
    }
}