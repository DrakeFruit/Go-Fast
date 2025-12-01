using System;
using Sandbox;
using Sandbox.Movement;

public partial class Car : Component
{
	public RealTimeSince Timer { get; set; }
	public bool TimerActive { get; set; }
	public Angles EyeAngles { get; set; }
	[Property] public Vector3 Offset { get; set; }
	
	public void UpdateInteractions()
	{
		if ( TireTrace.Hit && TireTrace.GameObject.Tags.Has( "kill" ) || Input.Pressed( "reload" ) )
		{
			GameObject.WorldPosition = Vector3.Zero;
		}

		var seconds = Timer.Relative % 60;
		var minutes = Timer.Relative / 60;
		var milliseconds = (int)(Timer.Relative % 1 * 1000);
		var time = TimerActive ? $"{minutes:00}:{seconds:00}:{milliseconds:000}" : "00:00:000";
		DebugOverlay.ScreenText( new Vector2(Screen.Width / 2, Screen.Height - 50), time, 40, TextFlag.Center, Color.White );
		DebugOverlay.ScreenText( new Vector2(Screen.Width - 100, Screen.Height - 25), "Press 'r' to restart", 20, TextFlag.Center, Color.Yellow );
	}

	protected override void OnUpdate()
	{
		Scene.Camera.FieldOfView = Scene.Camera.FieldOfView.LerpTo( Preferences.FieldOfView * Rb.Velocity.Length
			.Remap(0, 1000, 1, 1.2f), Time.Delta * 5);

		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
		var offset = EyeAngles.ToRotation().Right * Offset.x + EyeAngles.ToRotation().Up * Offset.z + EyeAngles.ToRotation().Backward * Offset.y;
		Scene.Camera.WorldPosition = WorldPosition + offset;
		Scene.Camera.WorldRotation = EyeAngles;
	}
}
