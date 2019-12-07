namespace VotingApplication.Filters
{
    using System.Web.Mvc;

    using Serilog;

    public class LogExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Log.Logger.Fatal(context.Exception, "Unhandled exception occurred");
        }
    }
}