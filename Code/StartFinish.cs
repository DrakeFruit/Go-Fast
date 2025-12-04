using Sandbox;

[Title( "Start / Finish Line" )]
public sealed class StartFinish : Component, Component.ITriggerListener
{
	[Property] public bool Start { get; set; } = true;

	public void OnTriggerExit( GameObject other )
	{
		if ( other.Tags.Has( "player" ) )
		{
			var player = other.GetComponentInParent<Car>();
			if ( Start )
			{
				player?.Timer = 0;
				player?.TimerActive = true;
			}
		}
	}
	
	public void OnTriggerEnter( GameObject other )
	{
		if ( other.Tags.Has( "player" ) )
		{
			var player = other.GetComponentInParent<Car>();
			if ( !Start )
			{
				player.FastestTime = player.Timer.Relative;
			}
			player.TimerActive = false;
			player.Timer = 0;
		}
	}
}
