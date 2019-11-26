namespace VotingApplication.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Survey
    {
        public int Id { get; set; }

        [Display(Name = "Create Date")]
        public DateTimeOffset CreateDate { get; set; }

        [Display(Name = "Survey Name")]
        public string Name { get; set; }

        [Display(Name = "Is Survey Featured?")]
        public bool IsDefault { get; set; }

        public virtual ICollection<Option> Options { get; set; }

        public virtual ICollection<Vote> Votes { get; set; }
    }
}