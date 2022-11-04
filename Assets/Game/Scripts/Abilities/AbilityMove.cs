using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityMove : CharacterAbility<Vector2>
{
   /// <summary>
   /// Последнее направление ввода
   /// </summary>
   protected Vector2 curInputDir = Vector2.zero;
   protected Rigidbody2D rigidbody;
   [SerializeField]
   protected float runMaxSpeed; //Target speed we want the player to reach.
   [SerializeField]
   protected float runAcceleration; //The speed at which our player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all
   
   protected float runAccelAmount; //The actual force (multiplied with speedDiff) applied to the player.
   [SerializeField]
   protected float runDecceleration; //The speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all
   protected float runDeccelAmount; //Actual force (multiplied with speedDiff) applied to the player .
   [Space(5)]
   [SerializeField]
   [Range(0f, 1)] protected float accelInAir; //Multipliers applied to acceleration rate when airborne.
   [SerializeField]
   [Range(0f, 1)] protected float deccelInAir;
   [Space(5)]
   public bool doConserveMomentum = true;
   protected override void PreInitialize()
   {
      base.PreInitialize();
      Debug.Log("Ability");
      rigidbody = owner.RigidBody;
   }

   protected override void Initialize()
   {
      base.Initialize();
   }

   private void FixedUpdate()
   {
      Walk();
   }

   private void Walk()
   {
      //Calculate the direction we want to move in and our desired velocity
		float targetSpeed = curInputDir.x * runMaxSpeed;
		//We can reduce are control using Lerp() this smooths changes to are direction and speed
		//targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (owner.IsOnGround)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * accelInAir : runDeccelAmount * deccelInAir;
		#endregion

		/*#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion*/

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if(doConserveMomentum && Mathf.Abs(rigidbody.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(rigidbody.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && !owner.IsOnGround)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0; 
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - rigidbody.velocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;
		//Convert this to a vector and apply to rigidbody
		rigidbody.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
   }

   public override void ProcessInput(Vector2 input)
   {
      base.ProcessInput(input);
      curInputDir = input;
   }

   protected void OnValidate()
   {
	   runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
	   runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;
	   
	   #region Variable Ranges
	   runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
	   runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
	   #endregion
   }
}