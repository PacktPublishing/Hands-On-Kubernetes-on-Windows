namespace VotingApplication.Interfaces
{
    using System;

    public interface IDateTimeProvider
    {
        DateTimeOffset GetCurrentTime();
    }
}