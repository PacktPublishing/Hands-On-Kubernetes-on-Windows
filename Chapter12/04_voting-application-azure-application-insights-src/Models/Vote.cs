namespace VotingApplication.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Vote
    {
        public int Id { get; set; }

        [Display(Name = "Create Date")]
        public DateTimeOffset CreateDate { get; set; }

        public int SurveyId { get; set; }

        public virtual Survey Survey { get; set; }

        public int OptionId { get; set; }

        public virtual Option Option { get; set; }
    }
}