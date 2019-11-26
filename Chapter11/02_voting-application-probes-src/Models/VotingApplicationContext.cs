namespace VotingApplication.Models
{
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;

    public class VotingApplicationContext : DbContext
    {
        public VotingApplicationContext(string connectionString) : base(connectionString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<VotingApplicationContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<VotingApplicationContext>());
        }

        public VotingApplicationContext() : this("name=DatabaseContext")
        {
        }

        public DbSet<Option> Options { get; set; }

        public DbSet<Vote> Votes { get; set; }

        public DbSet<Survey> Surveys { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Survey>()
                .HasMany(s => s.Options)
                .WithRequired(o => o.Survey)
                .HasForeignKey(o => o.SurveyId)
                .WillCascadeOnDelete();
            
            modelBuilder.Entity<Survey>()
                .HasMany(s => s.Votes)
                .WithRequired(v => v.Survey)
                .HasForeignKey(v => v.SurveyId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Vote>()
                .HasRequired(v => v.Option)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
