namespace VotingApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    using VotingApplication.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<VotingApplicationContext>
    {
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.ContextKey = "VotingApplication.Models.VotingApplicationContext";
        }

        protected override void Seed(VotingApplicationContext context)
        {
            var now = DateTimeOffset.Now;
            
            context.Surveys.AddOrUpdate(
                x => x.Id,
                new Survey { Id = 1, CreateDate = now, IsDefault = true, Name = "Cats or dogs?" });

            context.Options.AddOrUpdate(
                x => x.Id,
                new Option { Id = 1, CreateDate = now, Name = "Cats", SurveyId = 1 },
                new Option { Id = 2, CreateDate = now, Name = "Dogs", SurveyId = 1 });
        }
    }
}
