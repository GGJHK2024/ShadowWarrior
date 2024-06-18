﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this ability to a Character to have it handle horizontal movement (walk, and potentially run, crawl, etc)
	/// Animator parameters : Speed (float), Walking (bool)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Horizontal Movement")] 
	public class CharacterHorizontalMovement : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component handles basic left/right movement, friction, and ground hit detection. Here you can define standard movement speed, walk speed, and what effects to use when the character hits the ground after a jump/fall."; }

		/// the current reference movement speed
		public float MovementSpeed { get; set; }

		[Header("Speed")]

		/// the speed of the character when it's walking
		[Tooltip("the speed of the character when it's walking")]
		public float WalkSpeed = 6f;
		/// press this debug button if you decide to change the WalkSpeed at runtime, to reset its cache and have it impact your character in real time
		[MMInspectorButton("ResetHorizontalSpeed")]
		public bool ResetHorizontalSpeedBtn;
		/// the multiplier to apply to the horizontal movement
		[MMReadOnly]
		[Tooltip("the multiplier to apply to the horizontal movement")]
		public float MovementSpeedMultiplier = 1f;
		/// the multiplier to apply to the horizontal movement, dedicated to abilities
		[MMReadOnly]
		[Tooltip("the multiplier to apply to the horizontal movement, dedicated to abilities")]
		public float AbilityMovementSpeedMultiplier = 1f;
		/// the multiplier to apply when pushing
		[MMReadOnly]
		[Tooltip("the multiplier to apply when pushing")]
		public float PushSpeedMultiplier = 1f;
		/// the multiplier that gets set and applied by CharacterSpeed
		[MMReadOnly]
		[Tooltip("the multiplier that gets set and applied by CharacterSpeed")]
		public float StateSpeedMultiplier = 1f;
		/// if this is true, the character will automatically flip to face its movement direction
		[Tooltip("if this is true, the character will automatically flip to face its movement direction")]
		public bool FlipCharacterToFaceDirection = true;


		/// the current horizontal movement force
		public virtual float HorizontalMovementForce { get { return _horizontalMovementForce; }}
		/// if this is true, movement will be forbidden (as well as flip)
		public virtual bool MovementForbidden { get; set; }

		[Header("Input")]

		/// if this is true, will get input from an input source, otherwise you'll have to set it via SetHorizontalMove()
		[Tooltip("if this is true, will get input from an input source, otherwise you'll have to set it via SetHorizontalMove()")]
		public bool ReadInput = true;
		/// if this is true, no acceleration will be applied to the movement, which will instantly be full speed (think Megaman movement). Attention : a character with instant acceleration won't be able to get knockbacked on the x axis as a regular character would, it's a tradeoff
		[Tooltip("if this is true, no acceleration will be applied to the movement, which will instantly be full speed (think Megaman movement). Attention : a character with instant acceleration won't be able to get knockbacked on the x axis as a regular character would, it's a tradeoff")]
		public bool InstantAcceleration = false;
		/// the threshold after which input is considered (usually 0.1f to eliminate small joystick noise)
		[Tooltip("the threshold after which input is considered (usually 0.1f to eliminate small joystick noise)")]
		public float InputThreshold = 0.1f;
		/// how much air control the player has
		[Range(0f, 1f)]
		[Tooltip("how much air control the player has")]
		public float AirControl = 1f;
		/// whether or not the player can flip in the air
		[Tooltip("whether or not the player can flip in the air")]
		public bool AllowFlipInTheAir = true;
		/// whether or not this ability should keep taking care of horizontal movement after death
		[Tooltip("whether or not this ability should keep taking care of horizontal movement after death")]
		public bool ActiveAfterDeath = false;

		[Header("Walls")]
		/// Whether or not the state should be reset to Idle when colliding laterally with a wall
		[Tooltip("Whether or not the state should be reset to Idle when colliding laterally with a wall")]
		public bool StopWalkingWhenCollidingWithAWall = false;
		
		public Stack<float> ContextSpeedStack = new Stack<float>();
		public float ContextSpeedMultiplier => ContextSpeedStack.Count > 0 ? ContextSpeedStack.Peek() : 1;
                
		protected float _horizontalMovement;
		protected float _verticalMovement;
		protected float _horizontalMovementForce;
		protected float _verticalMovementForce;
		protected float _normalizedHorizontalSpeed;
		protected float _normalizedVerticalSpeed;
		protected float _initialMovementSpeedMultiplier;

		// animation parameters
		protected const string _speedAnimationParameterName = "Speed";
		protected const string _relativeSpeedAnimationParameterName = "xSpeedFacing";
		protected const string _walkingAnimationParameterName = "Walking";
		protected int _speedAnimationParameter;
		protected int _walkingAnimationParameter;
		protected int _relativeSpeedAnimationParameter;

		/// <summary>
		/// On Initialization, we set our movement speed to WalkSpeed.
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization ();
			MovementSpeed = WalkSpeed;
			MovementSpeedMultiplier = 1f;
			AbilityMovementSpeedMultiplier = 1f;
			MovementForbidden = false;
			_initialMovementSpeedMultiplier = MovementSpeedMultiplier;
		}

		/// <summary>
		/// The second of the 3 passes you can have in your ability. Think of it as Update()
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleHorizontalMovement();
			DetectWalls(true);
		}

		/// <summary>
		/// Called at the very start of the ability's cycle, and intended to be overridden, looks for input and calls
		/// methods if conditions are met
		/// </summary>
		protected override void HandleInput()
		{
			if (!ReadInput)
			{
				return;
			}

			_horizontalMovement = _horizontalInput;
			_verticalMovement = _verticalInput;
		}

		/// <summary>
		/// Sets the horizontal move value.
		/// </summary>
		/// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
		public virtual void SetHorizontalMove(float value)
		{
			_horizontalMovement = value;
		}
		public virtual void SetVerticalMove(float value)
		{
			_verticalMovement = value;
		}

		/// <summary>
		/// Called at Update(), handles horizontal movement
		/// </summary>
		protected virtual void HandleHorizontalMovement()
		{	
			// if we're not walking anymore, we stop our walking sound
			if ((_movement.CurrentState != CharacterStates.MovementStates.Walking) && _startFeedbackIsPlaying)
			{
				print("you shall die");
				StopStartFeedbacks();
			}

			// if movement is prevented, or if the character is dead/frozen/can't move, we exit and do nothing
			if (!ActiveAfterDeath)
			{
				if (!AbilityAuthorized 
				    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) 
				    || (_movement.CurrentState == CharacterStates.MovementStates.Gripping))
				{
					return;
				}
			}

			bool canFlip = true;

			if (MovementForbidden)
			{
				_horizontalMovement = _character.Airborne ? _controller.Speed.x * Time.deltaTime : 0f;
				_verticalMovement = _character.Airborne ? _controller.Speed.y * Time.deltaTime : 0f;
				canFlip = false;
			}

			// If the value of the horizontal axis is positive, the character must face right.
			if (_horizontalMovement > InputThreshold)
			{
				_normalizedHorizontalSpeed = _horizontalMovement;
				if (!_character.IsFacingRight && canFlip && FlipCharacterToFaceDirection)
				{
					_character.Flip();
				}					
			}		
			// If it's negative, then we're facing left
			else if (_horizontalMovement < -InputThreshold)
			{
				_normalizedHorizontalSpeed = _horizontalMovement;
				if (_character.IsFacingRight && canFlip && FlipCharacterToFaceDirection)
				{
					_character.Flip();
				}					
			}
			else
			{
				_normalizedHorizontalSpeed = 0;
			}
			// If the value of the vertical axis is positive, the character must face right.
			if (_verticalMovement > InputThreshold)
			{
				_normalizedVerticalSpeed = _verticalMovement;
				/*if (!_character.IsFacingRight && canFlip && FlipCharacterToFaceDirection)
				{
					_character.Flip();
				}	*/				
			}		
			// If it's negative, then we're facing left
			else if (_verticalMovement < -InputThreshold)
			{
				_normalizedVerticalSpeed = _verticalMovement;
				/*if (_character.IsFacingRight && canFlip && FlipCharacterToFaceDirection)
				{
					_character.Flip();
				}*/					
			}
			else
			{
				_normalizedVerticalSpeed = 0;
			}

			/// if we're dashing, we stop there
			if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
			{
				return;
			}

			// if we're grounded and moving, and currently Idle, Dangling or Falling, we become Walking
			if ( (_normalizedHorizontalSpeed != 0 || _normalizedVerticalSpeed != 0) && 
			     ( _movement.CurrentState == CharacterStates.MovementStates.Idle 
			       || _movement.CurrentState == CharacterStates.MovementStates.Dangling 
			       || _movement.CurrentState == CharacterStates.MovementStates.Falling))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Walking);
				
				if (!DetectWalls(false))
				{
					PlayAbilityStartFeedbacks();
				}
			}

			// if we're grounded, jumping but not moving up, we become idle
			if (_movement.CurrentState == CharacterStates.MovementStates.Jumping
			    && _controller.TimeAirborne >= _character.AirborneMinimumTime)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}

			// if we're walking and not moving anymore, we go back to the Idle state
			if (_movement.CurrentState == CharacterStates.MovementStates.Walking
			    && _normalizedHorizontalSpeed == 0 && _normalizedVerticalSpeed == 0)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
				PlayAbilityStopFeedbacks();
			}

			// we apply instant acceleration if needed
			if (InstantAcceleration)
			{
				if (_normalizedHorizontalSpeed > 0f) { _normalizedHorizontalSpeed = 1f; }
				if (_normalizedHorizontalSpeed < 0f) { _normalizedHorizontalSpeed = -1f; }
				if (_normalizedVerticalSpeed > 0f) { _normalizedVerticalSpeed = 1f; }
				if (_normalizedVerticalSpeed < 0f) { _normalizedVerticalSpeed = -1f; }
			}


			float airAcceleration = _controller.Parameters.SpeedAccelerationInAir;
			if (_controller.Parameters.UseSeparateDecelerationInAir && (Mathf.Abs(_horizontalMovement) < InputThreshold) && (Mathf.Abs(_verticalMovement) < InputThreshold))
			{
				airAcceleration = _controller.Parameters.SpeedDecelerationInAir;
			}
			
			float movementFactor = airAcceleration;
			float horizontalMovementSpeed = _normalizedHorizontalSpeed * MovementSpeed * _controller.Parameters.SpeedFactor * MovementSpeedMultiplier * ContextSpeedMultiplier * AbilityMovementSpeedMultiplier * StateSpeedMultiplier * PushSpeedMultiplier;
			float verticalMovementSpeed = _normalizedVerticalSpeed * MovementSpeed * _controller.Parameters.SpeedFactor * MovementSpeedMultiplier * ContextSpeedMultiplier * AbilityMovementSpeedMultiplier * StateSpeedMultiplier * PushSpeedMultiplier;
                        
			if (InstantAcceleration && (_movement.CurrentState != CharacterStates.MovementStates.WallJumping))
			{
				// if we are in instant acceleration mode, we just apply our movement speed
				_horizontalMovementForce = horizontalMovementSpeed;
				_verticalMovementForce = verticalMovementSpeed;

				// and any external forces that may be active right now
				if (Mathf.Abs(_controller.ExternalForce.x) > 0)
				{
					_horizontalMovementForce += _controller.ExternalForce.x;
					_verticalMovementForce += _controller.ExternalForce.y;
				}                
			}
			else
			{
				// if we are not in instant acceleration mode, we lerp towards our movement speed
				_horizontalMovementForce = Mathf.Lerp(_controller.Speed.x, horizontalMovementSpeed, Time.deltaTime * movementFactor);
				_verticalMovementForce = Mathf.Lerp(_controller.Speed.y, verticalMovementSpeed, Time.deltaTime * movementFactor);
			}			
						
			// we handle friction
			_horizontalMovementForce = HandleFriction(_horizontalMovementForce);
			_verticalMovementForce = HandleFriction(_verticalMovementForce);

			// we set our newly computed speed to the controller
			_controller.SetHorizontalForce(_horizontalMovementForce);
			_controller.SetVerticalForce(_verticalMovementForce);
		}

		/// <summary>
		/// This method will return true if a wall is detected in front of the character, false otherwise
		/// </summary>
		/// <param name="changeState">Whether or not this method should change state to idle if a wall is found</param>
		/// <returns></returns>
		protected virtual bool DetectWalls(bool changeState)
		{
			if (!StopWalkingWhenCollidingWithAWall)
			{
				return false;
			}

			if ((_movement.CurrentState == CharacterStates.MovementStates.Walking) || (_movement.CurrentState == CharacterStates.MovementStates.Running))
			{
				if ((_controller.State.IsCollidingLeft) || (_controller.State.IsCollidingRight))
				{
					if (changeState) { _movement.ChangeState(CharacterStates.MovementStates.Idle); }
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Handles surface friction.
		/// </summary>
		/// <returns>The modified current force.</returns>
		/// <param name="force">the force we want to apply friction to.</param>
		protected virtual float HandleFriction(float force)
		{
			// if we have a friction above 1 (mud, water, stuff like that), we divide our speed by that friction
			if (_controller.Friction>1)
			{
				force = force/_controller.Friction;
			}

			// if we have a low friction (ice, marbles...) we lerp the speed accordingly
			if (_controller.Friction<1 && _controller.Friction > 0)
			{
				force = Mathf.Lerp(_controller.Speed.x, force, Time.deltaTime * _controller.Friction * 10);
			}

			return force;
		}

		/// <summary>
		/// A public method to reset the horizontal speed
		/// </summary>
		public virtual void ResetHorizontalSpeed()
		{
			MovementSpeed = WalkSpeed;
		}

		/// <summary>
		/// Resets the current movement multiplier to its initial value
		/// </summary>
		public virtual void ResetMovementSpeedMultiplier()
		{
			MovementSpeedMultiplier = _initialMovementSpeedMultiplier;
		}
		
		/// <summary>
		/// Applies a movement multiplier for the specified duration
		/// </summary>
		/// <param name="movementMultiplier"></param>
		/// <param name="duration"></param>
		public virtual void ApplyContextSpeedMultiplier(float movementMultiplier, float duration)
		{
			StartCoroutine(ApplyContextSpeedMultiplierCo(movementMultiplier, duration));
		}

		/// <summary>
		/// A coroutine used to apply a movement multiplier for a certain duration only
		/// </summary>
		/// <param name="movementMultiplier"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected virtual IEnumerator ApplyContextSpeedMultiplierCo(float movementMultiplier, float duration)
		{
			SetContextSpeedMultiplier(movementMultiplier);
			yield return MMCoroutine.WaitFor(duration);
			ResetContextSpeedMultiplier();
		}

		/// <summary>
		/// Stacks a new context speed multiplier
		/// </summary>
		/// <param name="newMovementSpeedMultiplier"></param>
		public virtual void SetContextSpeedMultiplier(float newMovementSpeedMultiplier)
		{
			ContextSpeedStack.Push(newMovementSpeedMultiplier);
		}

		/// <summary>
		/// Revers the context speed multiplier to its previous value
		/// </summary>
		public virtual void ResetContextSpeedMultiplier()
		{
			ContextSpeedStack.Pop();
		}


		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_speedAnimationParameterName, AnimatorControllerParameterType.Float, out _speedAnimationParameter);
			RegisterAnimatorParameter (_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
			RegisterAnimatorParameter(_relativeSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeSpeedAnimationParameter);
		}

		/// <summary>
		/// Sends the current speed and the current value of the Walking state to the animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, Mathf.Max(Mathf.Abs(_normalizedHorizontalSpeed),Mathf.Abs(_normalizedVerticalSpeed)), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Walking), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeSpeedAnimationParameter, _character.IsFacingRight ? _controller.Speed.x : -_controller.Speed.x, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}
        
		/// <summary>
		/// When the character gets revived we reinit it again
		/// </summary>
		protected virtual void OnRevive()
		{
			Initialization ();
		}

		/// <summary>
		/// When the player respawns, we reinstate this agent.
		/// </summary>
		/// <param name="checkpoint">Checkpoint.</param>
		/// <param name="player">Player.</param>
		protected override void OnEnable ()
		{
			base.OnEnable ();
			if (gameObject.GetComponentInParent<Health>() != null)
			{
				gameObject.GetComponentInParent<Health>().OnRevive += OnRevive;
			}
		}
		
		/// <summary>
		/// On disable, we stop listening for OnRevive events
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable ();
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
			}			
		}
	}
}