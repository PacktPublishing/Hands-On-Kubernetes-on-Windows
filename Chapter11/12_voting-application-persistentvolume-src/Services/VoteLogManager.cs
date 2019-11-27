namespace VotingApplication.Services
{
    using System;
    using System.IO;

    using VotingApplication.Interfaces;
    using VotingApplication.Models;

    public class VoteLogManager : IVoteLogManager
    {
        public const string LogPath = @"C:\data\voting.log";

        private readonly object logLock = new object();

        public void Append(Vote vote)
        {
            lock (this.logLock)
            {
                var logDirectory = Path.GetDirectoryName(LogPath);

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                File.AppendAllText(LogPath, $"{vote.CreateDate}: Received vote, SurveyId={vote.SurveyId} for OptionId={vote.OptionId}{Environment.NewLine}");
            }
        }

        public string Get()
        {
            lock (this.logLock)
            {
                if (!File.Exists(LogPath))
                {
                    return string.Empty;
                }

                return File.ReadAllText(LogPath);
            }
        }
    }
}