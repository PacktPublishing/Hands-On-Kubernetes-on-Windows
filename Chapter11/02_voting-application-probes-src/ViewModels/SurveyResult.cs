namespace VotingApplication.ViewModels
{
    using System.Collections.Generic;

    public class SurveyResult
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<OptionResult> OptionResults { get; set; } = new List<OptionResult>();

        public int Total { get; set; }
    }
}