using FactoryX.Web.Services.Abstracts;

namespace FactoryX.Web.Services.Concretes;

public class FirstVisitService : IFirstVisitService
{
	private const string FIRST_VISIT_KEY = "__FirstVisit";
	private readonly ILogger<FirstVisitService> _logger;

	public FirstVisitService(ILogger<FirstVisitService> logger)
	{
		_logger = logger;
	}

	public bool IsFirstVisit(HttpContext context)
	{
		try
		{
			var sessionValue = context.Session.GetString(FIRST_VISIT_KEY);
			if (sessionValue != null)
				return false;

			var cookieValue = context.Request.Cookies[FIRST_VISIT_KEY];
			return cookieValue == null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking first visit status");
			return false; 
		}
	}

	public void MarkAsVisited(HttpContext context)
	{
		try
		{
			context.Session.SetString(FIRST_VISIT_KEY, DateTime.UtcNow.ToString("O"));

			var cookieOptions = new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddDays(30),
				HttpOnly = true,
				Secure = context.Request.IsHttps,
				SameSite = SameSiteMode.Strict,
				IsEssential = true
			};

			context.Response.Cookies.Append(FIRST_VISIT_KEY,
				DateTime.UtcNow.ToString("O"), cookieOptions);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error marking as visited");
		}
	}
}
