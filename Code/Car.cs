using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public partial class Car : Component
{
	[Property, Group("Tires")] public float TireRestDistance { get; set; } = 38;
	[Property, Group("Tires")] public float SpringStrength { get; set; } = 50;
	[Property, Group("Tires")] public float SpringDamping { get; set; } = 5;
	[Property, Group("Tires"), Range(0, 1)] public float TireGrip { get; set; } = 200;
	[Property, Group("Tires")] public float TireMass { get; set; } = 10;
	[Property, Group("Tires")] public List<GameObject> FrontTires { get; set; }
	[Property, Group("Tires")] public List<GameObject> BackTires { get; set; }
	
	[Property] public float MaxSpeedMPH { get; set; } = 90;
	public float MaxSpeed { get { return MaxSpeedMPH * 17.6f; } }
	[Property, Range(0, 10)] public float TurnSpeed { get; set; } = 5;
	[Property] public float JumpForce { get; set; } = 20;
	[Property] public Curve TorqueCurve { get; set; } = Curve.EaseOut;
	[RequireComponent] public Rigidbody Rb { get; set; }
	private SceneTraceResult TireTrace { get; set; }
	
	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		
		UpdateInteractions();
		
		foreach ( var i in FrontTires )
		{
			ApplySuspension( i );
			ApplyTorque( i );
				
			i.LocalRotation = Rotation.Identity.Angles().WithYaw( i.LocalRotation.Yaw().LerpTo(Input.AnalogMove.y * TurnSpeed, 1) );
			i.LocalRotation = i.LocalRotation.Clamp( Rotation.FromYaw( 0 ), 45 );
			
			DebugOverlay.ScreenText( new Vector2( 20, 20 ), (Rb.Velocity.Length / 17.6f).Floor().ToString() + " mph", 40,
					TextFlag.Absolute, Color.Orange );
		}
		
		foreach ( var i in BackTires )
		{
			ApplySuspension( i );
		}
		
		WorldRotation = WorldRotation.SlerpTo( Scene.Camera.WorldRotation.Angles().WithPitch(
			TireTrace.Hit ? WorldRotation.Angles().pitch : Scene.Camera.WorldRotation.Pitch() ), 0.25f );
	}

	public void ApplySuspension( GameObject tire )
	{
		TireTrace = Scene.Trace.Ray( tire.WorldPosition + tire.WorldRotation.Up * 20,
				tire.WorldPosition + tire.WorldRotation.Down * TireRestDistance )
			.IgnoreGameObjectHierarchy(GameObject)
			.Run();
		//DebugOverlay.Trace(TireTrace);

		if ( TireTrace.Hit )
		{
			var tireWorldVelocity = Rb.GetVelocityAtPoint( tire.WorldPosition );
			var offset = TireRestDistance - TireTrace.Distance;
			if ( offset < 0 ) return;
			
			var springDir = tire.WorldRotation.Up;
			var springVel = Vector3.Dot( springDir, tireWorldVelocity );
			var springForce = (offset * (SpringStrength * Rb.Mass)) - (springVel * SpringDamping * Rb.Mass);
			
			var slipDir = tire.WorldRotation.Left;
			var slipVel = Vector3.Dot( slipDir, tireWorldVelocity );
			var slipForce = -slipVel * TireGrip * Rb.Mass;

			Rb.ApplyForceAt( tire.WorldPosition, springDir * springForce );
			Rb.ApplyForceAt( tire.WorldPosition, slipDir * TireMass * slipForce );

			var model = tire.Children.FirstOrDefault();
			if ( model.IsValid() ) model.WorldPosition = TireTrace.HitPosition;
			
			//DebugOverlay.Line(tire.WorldPosition, tire.WorldPosition + springDir * springForce, Color.Blue);
			//DebugOverlay.Line(tire.WorldPosition, tire.WorldPosition + slipDir * TireMass * slipForce, Color.Red);
		}
	}

	public void ApplyTorque( GameObject tire )
	{
		if ( TireTrace.Hit )
		{
			var tireWorldVelocity = Rb.GetVelocityAtPoint( tire.WorldPosition );
			var driveDir = tire.WorldRotation.Forward;
			var driveVel = Vector3.Dot( driveDir, tireWorldVelocity );
			
			var driveVelNormal = float.Clamp(MathF.Abs( driveVel ) / MaxSpeed, 0, 1);
			var driveForce = TorqueCurve.Evaluate( driveVelNormal ) * Input.AnalogMove.x * Rb.Mass * 500;
			
			if ( Input.AnalogMove.x.AlmostEqual( 0 ) )
			{
				driveForce = -driveVel * Rb.Mass * 2;
			}
			if ( driveVel >= MaxSpeed ) driveForce = 0;
		
			Rb.ApplyForceAt( tire.WorldPosition, driveDir * driveForce );
			//DebugOverlay.Line(tire.WorldPosition, tire.WorldPosition + driveDir * driveForce, Color.Green);
		}
	}
}
