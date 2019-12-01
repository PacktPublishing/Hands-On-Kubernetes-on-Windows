namespace VotingApplication
{
    using System.Web.Mvc;

    using VotingApplication.Filters;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new LogExceptionFilter());
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
        }
    }
}
