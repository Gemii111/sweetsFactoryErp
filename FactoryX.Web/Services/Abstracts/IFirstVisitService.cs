namespace FactoryX.Web.Services.Abstracts;

public interface IFirstVisitService
{
	/// <summary>
	/// Checks if the user is visiting for the first time.
	/// </summary>
	/// <returns>True if it's the first visit, otherwise false.</returns>
	bool IsFirstVisit(HttpContext  context);

	/// <summary>
	/// Marks the user as having visited.
	/// </summary>
	void MarkAsVisited(HttpContext context);
}
