namespace Sandbox;

public class Utilities
{
	public static string FormatTime( float relativeTime )
	{
		var seconds = relativeTime % 60;
		var minutes = relativeTime / 60;
		var milliseconds = (int)(relativeTime % 1 * 1000);
		var time = $"{minutes:00}:{seconds:00}:{milliseconds:000}";
		return time;
	}
}
