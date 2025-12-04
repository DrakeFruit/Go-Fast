using System;
using Sandbox;
using Sandbox.Movement;

public partial class Car : Component
{
	public RealTimeSince Timer { get; set; }
	public float FastestTime { get; set; }
	public bool TimerActive { get; set; }
	public Angles EyeAngles { get; set; }
	[Property] public Vector3 Offset { get; set; }
	
	public void UpdateInteractions()
	{
		if ( TireTrace.Hit && TireTrace.GameObject.Tags.Has( "kill" ) || Input.Pressed( "reload" ) )
		{
			Rb.Velocity = 0;
			GameObject.WorldPosition = Vector3.Zero;
		}

		var time = TimerActive ? Utilities.FormatTime( Timer.Relative ) : Utilities.FormatTime( 0 );
		DebugOverlay.ScreenText( new Vector2(Screen.Width / 2, Screen.Height - 50), time, 40, TextFlag.Center, Color.White );
		DebugOverlay.ScreenText( new Vector2(Screen.Width / 2, Screen.Height - 20), Utilities.FormatTime(FastestTime), 20, TextFlag.Center, Color.White );
		DebugOverlay.ScreenText( new Vector2(Screen.Width - 100, Screen.Height - 25), "Press 'r' to restart", 20, TextFlag.Center, Color.Yellow );
	}

	protected override void OnUpdate()
	{
		Scene.Camera.FieldOfView = Scene.Camera.FieldOfView.LerpTo( Preferences.FieldOfView * Rb.Velocity.Length
			.Remap(0, 1000, 1, 1.2f), Time.Delta * 5);

		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( EyeAngles.pitch.Clamp( -90, 90 ) );
		var offset = EyeAngles.ToRotation().Right * Offset.x + EyeAngles.ToRotation().Up * Offset.z + EyeAngles.ToRotation().Backward * Offset.y;
		var tr = Scene.Trace.Ray( WorldPosition + WorldRotation.Up * 20, WorldPosition + offset ).IgnoreGameObjectHierarchy(GameObject).Run();
		Scene.Camera.WorldPosition = tr.Hit ? tr.HitPosition + -tr.Direction * 5 : tr.EndPosition;
		Scene.Camera.WorldRotation = EyeAngles;
	}
}
