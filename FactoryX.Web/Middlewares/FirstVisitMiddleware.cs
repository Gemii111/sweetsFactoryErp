using FactoryX.Web.Services.Abstracts;

namespace FactoryX.Web.Middlewares;

public class FirstVisitMiddleware
{
	private readonly RequestDelegate _next;
	
	private readonly ILogger<FirstVisitMiddleware> _logger;

	public FirstVisitMiddleware(RequestDelegate next,ILogger<FirstVisitMiddleware> logger)
	{
		_next = next;
		
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			var _firstVisitService = context.RequestServices.GetRequiredService<IFirstVisitService>();
			if (ShouldCheckFirstVisit(context))
			{
				if (_firstVisitService.IsFirstVisit(context))
				{
					_logger.LogInformation("First visit detected for IP: {IP}",
						context.Connection.RemoteIpAddress?.ToString());

					_firstVisitService.MarkAsVisited(context);

					context.Response.Redirect("/Login");
					return;
				}
			}

			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred in FirstVisitMiddleware");
			throw;
		}
	}

	private bool ShouldCheckFirstVisit(HttpContext context)
	{
		if (context.User?.Identity?.IsAuthenticated == true)
			return false;

		// Sadece GET istekleri
		if (context.Request.Method != HttpMethods.Get)
			return false;

		// Sadece ana sayfa
		if (context.Request.Path != "/")
			return false;

		// API istekleri hariç
		if (context.Request.Path.StartsWithSegments("/api"))
			return false;

		// AJAX istekleri hariç
		if (context.Request.Headers.ContainsKey("X-Requested-With"))
			return false;

		return true;
	}

}
