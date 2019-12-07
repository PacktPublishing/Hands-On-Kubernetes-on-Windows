namespace VotingApplication
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Ninject.Extensions.Logging;

    using Prometheus;

    using VotingApplication.Interfaces;
    using VotingApplication.Models;
    using VotingApplication.ViewModels;

    public class SurveysController : Controller
    {
        private static readonly Counter DbAddedVotesCount = Metrics
            .CreateCounter("votingapplication_db_added_votes", "Number of votes added to the database.");

        private static readonly Histogram GetSurveyResultOperationDuration = Metrics
            .CreateHistogram("votingapplication_getsurveyresult_duration_seconds", "Histogram for duration of GetSurveyResult operation.");

        private readonly VotingApplicationContext db;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IVoteLogManager voteLogManager;
        private readonly ILogger log;

        public SurveysController(VotingApplicationContext db, IDateTimeProvider dateTimeProvider, IVoteLogManager voteLogManager, ILogger log)
        {
            this.db = db;
            this.dateTimeProvider = dateTimeProvider;
            this.voteLogManager = voteLogManager;
            this.log = log;
        }

        // GET: Surveys
        public async Task<ActionResult> Index()
        {
            return this.View(await db.Surveys.ToListAsync());
        }

        // GET: Surveys/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Survey survey = await this.db.Surveys.FindAsync(id);
            if (survey == null)
            {
                return this.HttpNotFound();
            }

            return this.View(survey);
        }

        // GET: Surveys/Create
        public ActionResult Create()
        {
            return this.View();
        }

        // POST: Surveys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,IsDefault")] Survey survey)
        {
            if (this.ModelState.IsValid)
            {
                if (survey.IsDefault)
                {
                    this.DisableExistingDefault();
                }

                survey.CreateDate = this.dateTimeProvider.GetCurrentTime();
                this.db.Surveys.Add(survey);
                await this.db.SaveChangesAsync();
                this.log.Info("Created Survey with ID={0}", survey.Id);
                return this.RedirectToAction("Index");
            }

            return View(survey);
        }

        // GET: Surveys/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Survey survey = await db.Surveys.FindAsync(id);
            if (survey == null)
            {
                return this.HttpNotFound();
            }

            return this.View(survey);
        }

        // POST: Surveys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,CreateDate,Name,IsDefault")] Survey survey)
        {
            if (this.ModelState.IsValid)
            {
                if (survey.IsDefault)
                {
                    this.DisableExistingDefault();
                }

                this.db.Entry(survey).State = EntityState.Modified;
                await this.db.SaveChangesAsync();
                this.log.Info("Updated Survey with ID={0}", survey.Id);
                return this.RedirectToAction("Index");
            }

            return this.View(survey);
        }

        // GET: Surveys/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Survey survey = await this.db.Surveys.FindAsync(id);
            if (survey == null)
            {
                return this.HttpNotFound();
            }

            return this.View(survey);
        }

        // POST: Surveys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Survey survey = await db.Surveys.FindAsync(id);
            this.db.Surveys.Remove(survey);
            await this.db.SaveChangesAsync();
            this.log.Info("Deleted Survey with ID={0}", survey.Id);
            return this.RedirectToAction("Index");
        }

        public async Task<ActionResult> Vote(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Survey survey = await this.db.Surveys.FindAsync(id);
            if (survey == null)
            {
                return this.HttpNotFound();
            }

            await this.db.Entry(survey).Collection(s => s.Options).LoadAsync();
            return this.View(survey);
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Vote(int? id, ICollection<int> optionIds)
        {
            if (id == null || !optionIds.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Survey survey = await this.db.Surveys.FindAsync(id);
            if (survey == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            foreach (var optionId in optionIds)
            {
                var vote = new Vote 
                { 
                    CreateDate = this.dateTimeProvider.GetCurrentTime(),
                    SurveyId = id.Value,
                    OptionId = optionId
                };

                this.voteLogManager.Append(vote);
                this.db.Votes.Add(vote);
                DbAddedVotesCount.Inc();
            }

            await this.db.SaveChangesAsync();
            this.log.Info("Vote(s) for Survey with ID={0} have been added", survey.Id);

            return this.RedirectToAction("Results", new { id });
        }

        public async Task<ActionResult> Results(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Survey survey = await db.Surveys.FindAsync(id);
            if (survey == null)
            {
                return this.HttpNotFound();
            }

            SurveyResult result;
            using (GetSurveyResultOperationDuration.NewTimer())
            {
                result = this.GetSurveyResult(survey);
            }

            return this.View(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }

        private SurveyResult GetSurveyResult(Survey survey)
        {
            var result = new SurveyResult
            {
                Id = survey.Id,
                Name = survey.Name,
                Total = survey.Votes.Count,
            };

            if (result.Total > 0)
            {
                result.OptionResults = survey.Votes
                    .GroupBy(v => v.Option)
                    .Select(g => new OptionResult { Id = g.Key.Id, Name = g.Key.Name, Value = 100.0f * g.Count() / result.Total })
                    .OrderByDescending(r => r.Value)
                    .ToList();
            }
            else
            {
                result.OptionResults = new List<OptionResult>();
            }

            var optionsWithVotes = new HashSet<int>(result.OptionResults.Select(r => r.Id));            
            foreach (var option in survey.Options)
            {
                if (!optionsWithVotes.Contains(option.Id))
                {
                    result.OptionResults.Add(new OptionResult { Id = option.Id, Name = option.Name, Value = 0 });
                }
            }

            return result;
        }

        private void DisableExistingDefault()
        {
            foreach (var survey in this.db.Surveys.Where(s => s.IsDefault))
            {
                survey.IsDefault = false;
            }
        }
    }
}
