namespace VotingApplication.Services
{
    using System;
    using VotingApplication.Interfaces;

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset GetCurrentTime()
        {
            return DateTimeOffset.Now;
        }
    }
}