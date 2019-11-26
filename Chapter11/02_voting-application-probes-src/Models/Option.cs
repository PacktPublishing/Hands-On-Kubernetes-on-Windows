namespace VotingApplication.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Option
    {
        public int Id { get; set; }

        [Display(Name = "Create Date")]
        public DateTimeOffset CreateDate { get; set; }

        [Display(Name = "Option Name (Title)")]
        public string Name { get; set; }

        [Display(Name = "Which Survey?")]
        public virtual Survey Survey { get; set; }

        public int SurveyId { get; set; }
    }
}