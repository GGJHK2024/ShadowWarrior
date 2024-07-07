using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine
{	
	public enum DamageSrcType {A, B};

	/// <summary>
	/// Add this component to an object and it will cause damage to objects that collide with it. 
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Damage/DamageOnTouch")] 
	public class DamageOnTouch : CorgiMonoBehaviour 
	{

		/// the possible ways to add knockback : noKnockback, which won't do nothing, set force, or add force
		public enum KnockbackStyles { NoKnockback, SetForce, AddForce }
		/// the possible knockback directions when causing damage
		public enum CausedKnockbackDirections { BasedOnOwnerPosition, BasedOnSpeed, BasedOnDamageOnTouchPosition }
		/// the possible knockback directions when taking damage
		public enum TakenKnockbackDirections { BasedOnDamagerPosition, BasedOnSpeed, BasedOnDamageOnTouchPosition }

		[Header("Targets")]
		[MMInformation("This component will make your object cause damage to objects that collide with it. Here you can define what layers will be affected by the damage (for a standard enemy, choose Player), how much damage to give, and how much force should be applied to the object that gets the damage on hit. Note that this component will MARK ITS ASSOCIATED COLLIDER AS TRIGGER.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// the layers that will be damaged by this object
		[Tooltip("the layers that will be damaged by this object")]
		public LayerMask TargetLayerMask;
		/// if this is true, the damage will apply on trigger enter
		[Tooltip("if this is true, the damage will apply on trigger enter")]
		public bool ApplyDamageOnTriggerEnter = true;
		/// if this is true, the damage will apply on trigger stay
		[Tooltip("if this is true, the damage will apply on trigger stay")]
		public bool ApplyDamageOnTriggerStay = true;

		[Header("Damage Caused")]
		/// The minimum amount of health to remove from the player's health
		[FormerlySerializedAs("DamageCaused")]
		[Tooltip("The minimum amount of health to remove from the player's health")]
		public float MinDamageCaused = 10f;
		/// The maximum amount of health to remove from the player's health
		[Tooltip("The amount of health to remove from the player's health")]
		public float MaxDamageCaused = 10f;
		/// a list of typed damage definitions that will be applied on top of the base damage
		[Tooltip("a list of typed damage definitions that will be applied on top of the base damage")]
		public List<TypedDamage> TypedDamages;
		
		[Header("Knockback")]
		/// the type of knockback to apply when causing damage
		[Tooltip("the type of knockback to apply when causing damage")]
		public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.SetForce;
		/// The direction to apply the knockback in 
		[Tooltip("The direction to apply the knockback in")]
		public CausedKnockbackDirections DamageCausedKnockbackDirection = CausedKnockbackDirections.BasedOnOwnerPosition;
		/// The force to apply to the object that gets damaged
		[Tooltip("The force to apply to the object that gets damaged")]
		public Vector2 DamageCausedKnockbackForce = new Vector2(10,2);
		
		[Header("Invincibility")]
		/// The duration of the invincibility frames after the hit (in seconds)
		[Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
		public float InvincibilityDuration = 0.5f;
		
		[Header("Damage over time")]
		/// Whether or not this damage on touch zone should apply damage over time
		[Tooltip("Whether or not this damage on touch zone should apply damage over time")]
		public bool RepeatDamageOverTime = false;
		/// if in damage over time mode, how many times should damage be repeated?
		[Tooltip("if in damage over time mode, how many times should damage be repeated?")] 
		[MMCondition("RepeatDamageOverTime", true)]
		public int AmountOfRepeats = 3;
		/// if in damage over time mode, the duration, in seconds, between two damages
		[Tooltip("if in damage over time mode, the duration, in seconds, between two damages")]
		[MMCondition("RepeatDamageOverTime", true)]
		public float DurationBetweenRepeats = 1f;
		/// if in damage over time mode, whether or not it can be interrupted (by calling the Health:InterruptDamageOverTime method
		[Tooltip("if in damage over time mode, whether or not it can be interrupted (by calling the Health:InterruptDamageOverTime method")] 
		[MMCondition("RepeatDamageOverTime", true)]
		public bool DamageOverTimeInterruptible = true;
		/// if in damage over time mode, the type of the repeated damage 
		[Tooltip("if in damage over time mode, the type of the repeated damage")] 
		[MMCondition("RepeatDamageOverTime", true)]
		public DamageType RepeatedDamageType;

		[Header("Damage Taken")]
		[MMInformation("After having applied the damage to whatever it collided with, you can have this object hurt itself. A bullet will explode after hitting a wall for example. Here you can define how much damage it'll take every time it hits something, or only when hitting something that's damageable, or non damageable. Note that this object will need a Health component too for this to be useful.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// The amount of damage taken every time, whether what we collide with is damageable or not
		[Tooltip("The amount of damage taken every time, whether what we collide with is damageable or not")]
		public float DamageTakenEveryTime = 0;
		/// The amount of damage taken when colliding with a damageable object
		[Tooltip("The amount of damage taken when colliding with a damageable object")]
		public float DamageTakenDamageable = 0;
		/// The amount of damage taken when colliding with something that is not damageable
		[Tooltip("The amount of damage taken when colliding with something that is not damageable")]
		public float DamageTakenNonDamageable = 0;
		/// the type of knockback to apply when taking damage
		[Tooltip("the type of knockback to apply when taking damage")]
		public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;
		/// The direction to apply the knockback 
		[Tooltip("The direction to apply the knockback ")]
		public TakenKnockbackDirections DamageTakenKnockbackDirection = TakenKnockbackDirections.BasedOnDamagerPosition;
		/// The force to apply to the object that gets damaged
		[Tooltip("The force to apply to the object that gets damaged")]
		public Vector2 DamageTakenKnockbackForce = Vector2.zero;
		/// The duration of the invincibility frames after the hit (in seconds)
		[Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
		public float DamageTakenInvincibilityDuration = 0.5f;

		[Header("Feedback")]

		/// the feedback to play when applying damage to a damageable
		[Tooltip("the feedback to play when applying damage to a damageable")]
		public MMFeedbacks HitDamageableFeedback;
		/// the feedback to play when applying damage to a non damageable
		[Tooltip("the feedback to play when applying damage to a non damageable")]
		public MMFeedbacks HitNonDamageableFeedback;
		/// the duration of freeze frames on hit (leave it at 0 to ignore)
		[Tooltip("the duration of freeze frames on hit (leave it at 0 to ignore)")]
		public float FreezeFramesOnHitDuration = 0f;

		/// the owner of the DamageOnTouch zone
		[MMReadOnly]
		[Tooltip("the owner of the DamageOnTouch zone")]
		public GameObject Owner;
		
		/// a delegate used to communicate hit events 
		public delegate void OnHitDelegate();
		public OnHitDelegate OnHit;
		public OnHitDelegate OnHitDamageable;
		public OnHitDelegate OnHitNonDamageable;
		public OnHitDelegate OnKill;

		// storage		
		protected Vector2 _lastPosition, _lastDamagePosition, _velocity, _knockbackForce, _damageDirection;
		protected float _startTime = 0f;
		protected Collider2D _collidingCollider;
		protected Health _colliderHealth;
		protected CorgiController _corgiController;
		protected CorgiController _colliderCorgiController;
		protected CharacterJump _characterJump;
		protected Health _health;
		protected List<GameObject> _ignoredGameObjects;
		protected Color _gizmosColor;
		protected Vector3 _gizmoSize;

		protected CircleCollider2D _circleCollider2D;
		protected BoxCollider2D _boxCollider2D;
		protected bool _initializedFeedbacks = false;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Awake()
		{
			if (_ignoredGameObjects == null)
			{
				_ignoredGameObjects = new List<GameObject>();	
			}
			_health = this.gameObject.GetComponent<Health>();
			_corgiController = this.gameObject.GetComponent<CorgiController> ();
			
			_boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
			_circleCollider2D = this.gameObject.GetComponent<CircleCollider2D>();
			if (_boxCollider2D != null) { _boxCollider2D.isTrigger = true; }
			if (_circleCollider2D != null) { _circleCollider2D.isTrigger = true; }
            
			_gizmosColor = Color.red;
			_gizmosColor.a = 0.25f;
			_lastPosition = this.transform.position;
			_lastDamagePosition = this.transform.position;
			InitializeFeedbacks();
		}

		/// <summary>
		/// A public method you can use to set the controller from another class
		/// </summary>
		/// <param name="newController"></param>
		public virtual void SetCorgiController(CorgiController newController)
		{
			_corgiController = newController;
		}
		
		protected virtual void InitializeFeedbacks()
		{
			if (_initializedFeedbacks)
			{
				return;
			}
			HitDamageableFeedback?.Initialization(this.gameObject);
			HitNonDamageableFeedback?.Initialization(this.gameObject);
			
			_initializedFeedbacks = true;
		}

		/// <summary>
		/// OnEnable we set the start time to the current timestamp
		/// </summary>
		protected virtual void OnEnable()
		{
			_startTime = Time.time;
			_lastPosition = this.transform.position;
			_lastDamagePosition = this.transform.position;
		}

		/// <summary>
		/// On Disable we clear our ignore list
		/// </summary>
		protected void OnDisable()
		{
			ClearIgnoreList();
		}

		/// <summary>
		/// During last update, we store the position and velocity of the object
		/// </summary>
		protected virtual void Update () 
		{
			ComputeVelocity();
		}

		/// <summary>
		/// Adds the gameobject set in parameters to the ignore list
		/// </summary>
		/// <param name="newIgnoredGameObject">New ignored game object.</param>
		public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
		{
			if (_ignoredGameObjects == null)
			{
				_ignoredGameObjects = new List<GameObject>();
			}
			_ignoredGameObjects.Add(newIgnoredGameObject);
		}

		/// <summary>
		/// Removes the object set in parameters from the ignore list
		/// </summary>
		/// <param name="ignoredGameObject">Ignored game object.</param>
		public virtual void StopIgnoringObject(GameObject ignoredGameObject)
		{
			_ignoredGameObjects.Remove(ignoredGameObject);
		}

		/// <summary>
		/// Clears the ignore list.
		/// </summary>
		public virtual void ClearIgnoreList()
		{
			if (_ignoredGameObjects != null)
			{
				_ignoredGameObjects.Clear();	
			}
		}

		/// <summary>
		/// Computes the velocity based on the object's last position
		/// </summary>
		protected virtual void ComputeVelocity()
		{
			_velocity = (_lastPosition - (Vector2)transform.position) /Time.deltaTime;

			if (Vector3.Distance(_lastDamagePosition, this.transform.position) > 0.1f)
			{
				_damageDirection = (Vector2)this.transform.position - _lastDamagePosition;
				_lastDamagePosition = this.transform.position;
			}

			_lastPosition = transform.position;
		}
		
		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			if (!ApplyDamageOnTriggerStay)
			{
				return;
			}
			Colliding (collider);
		}

		public virtual void OnTriggerEnter2D(Collider2D collider)
		{	
			if (!ApplyDamageOnTriggerEnter)
			{
				return;
			}

			BounceCheck(collider);
			Colliding (collider);
		}

		/// <summary>
		/// 往layer mask中添加层
		/// </summary>
		/// <param name="layer"></param>
		public void AddLayerMask(int layer)
		{
			TargetLayerMask = TargetLayerMask |= (1 << layer);
		}

		/// <summary>
		/// 往layer mask中移除层
		/// </summary>
		/// <param name="layer"></param>
		public void RemoveLayerMask(int layer)
		{
			TargetLayerMask = TargetLayerMask &= ~(1 << layer);
		}

		/// <summary>
		/// 弹反检测
		/// </summary>
		/// <param name="collider"></param>
		private void BounceCheck(Collider2D collider)
		{
			_collidingCollider = collider;
			Weapon ownerWeapon;
			DamageOnTouch enemyTouch;
			if ( this.gameObject.layer!=12 && Owner != null && Owner.TryGetComponent<Weapon>(out ownerWeapon) && collider.gameObject.layer == 13) {
				print("武器攻击到敌人");
				if (ownerWeapon.Owner.CharacterType == Character.CharacterTypes.Player && ownerWeapon.damageSrcType == DamageSrcType.B)				
				{
					print("是玩家，且攻击类型为B（弹反）");
					if (collider.gameObject.GetComponent<AIBrain>().CurrentState.StateName == "ShowMark") {
						print("敌人处于预攻击（出现感叹号）状态");
						// 如果处于预攻击（出现感叹号）状态，弹反成功
						var hp = collider.gameObject.GetComponent<Health>();
						if (hp!= null) {
							hp.Kill();
							print("弹反成功");
							ownerWeapon.WeaponBounceSuccessNear();	// 近战效果
							MMGameEvent.Trigger(GameEventType.BounceSuccess);
							return;
						}
					}
					
				}
			}
			if ( this.gameObject.layer!=12 && Owner != null && Owner.TryGetComponent<Weapon>(out ownerWeapon) && collider.gameObject.layer == 12) {
				print("武器攻击到子弹");
				if (ownerWeapon.Owner.CharacterType == Character.CharacterTypes.Player && ownerWeapon.damageSrcType == DamageSrcType.B)				
				{
					print("是玩家，且攻击类型为B（弹反）");
					collider.gameObject.GetComponent<Projectile>().SetOwner(Owner);
					collider.gameObject.GetComponent<Projectile>().SetDamage(999);
					// 将player(9)移除出伤害的layermask
					collider.gameObject.GetComponent<DamageOnTouch>().TargetLayerMask &= ~(1 << 9);
					// 将enemy(13)加入伤害的layermask
					collider.gameObject.GetComponent<DamageOnTouch>().TargetLayerMask |= (1 << 13);
					ownerWeapon.WeaponBounceSuccessFar();	// 远程弹反效果
					Vector3 _mousePosition;
					#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
						_mousePosition = Input.mousePosition;
					#else
						_mousePosition = Mouse.current.position.ReadValue();
					#endif
					_mousePosition.z = Camera.main.transform.position.z * -1;

					Vector3 direction = Camera.main.ScreenToWorldPoint(_mousePosition) - Owner.transform.position;
					
					collider.gameObject.GetComponent<Projectile>().SetDirection(Vector3.Normalize(direction)
						, collider.gameObject.transform.rotation);
					
					MMGameEvent.Trigger(GameEventType.BounceSuccess);
					return;

				}
			}
		}

		protected virtual void Colliding(Collider2D collider)
		{

			if (!this.isActiveAndEnabled)
			{
				return;
			}

			// if the object we're colliding with is part of our ignore list, we do nothing and exit
			if (_ignoredGameObjects.Contains(collider.gameObject))
			{
				return;
			}
			

			// if what we're colliding with isn't part of the target layers, we do nothing and exit
			if (!MMLayers.LayerInLayerMask(collider.gameObject.layer,TargetLayerMask))
			{
				return;
			}


			_colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();

			OnHit?.Invoke();
			
			// if what we're colliding with is damageable
			if ((_colliderHealth != null) && (_colliderHealth.enabled))
			{
				if(_colliderHealth.CurrentHealth > 0)
				{
					OnCollideWithDamageable(_colliderHealth);
				}
			}
			// if what we're colliding with can't be damaged
			else
			{
				OnCollideWithNonDamageable();
			}
		}

		/// <summary>
		/// Describes what happens when colliding with a damageable object
		/// </summary>
		/// <param name="health">Health.</param>
		protected virtual void OnCollideWithDamageable(Health health)
		{
			if (health.CanTakeDamageThisFrame())
			{			
				// if what we're colliding with is a CorgiController, we apply a knockback force
				_colliderCorgiController = health.AssociatedController;
	
				float randomDamage = UnityEngine.Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));
				
				ApplyDamageCausedKnockback(randomDamage, TypedDamages);
				
				OnHitDamageable?.Invoke();
	
				HitDamageableFeedback?.PlayFeedbacks(this.transform.position);
	
				if ((FreezeFramesOnHitDuration > 0) && (Time.timeScale > 0))
				{
					MMFreezeFrameEvent.Trigger(Mathf.Abs(FreezeFramesOnHitDuration));
				}
	
				// we apply the damage to the thing we've collided with
				if (RepeatDamageOverTime)
				{
					_colliderHealth.DamageOverTime(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration, _damageDirection, TypedDamages, AmountOfRepeats, DurationBetweenRepeats, DamageOverTimeInterruptible, RepeatedDamageType);	
				}
				else
				{
					_colliderHealth.Damage(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration, _damageDirection, TypedDamages);	
				}
	
				if (_colliderHealth.CurrentHealth <= 0)
				{
					OnKill?.Invoke();
				}
			}
			
			SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
		}

		protected virtual void ApplyDamageCausedKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (!ShouldApplyKnockback(damage, typedDamages))
			{
				return;
			}
			
			_knockbackForce.x = DamageCausedKnockbackForce.x;
			switch (DamageCausedKnockbackDirection)
			{
				case CausedKnockbackDirections.BasedOnOwnerPosition:
					if (Owner == null) { Owner = this.gameObject; }
					Vector2 relativePosition = _colliderCorgiController.transform.position - Owner.transform.position;
					_knockbackForce.x *= Mathf.Sign(relativePosition.x);
					break;
				case CausedKnockbackDirections.BasedOnSpeed:
					Vector2 totalVelocity = _colliderCorgiController.Speed + _velocity;
					_knockbackForce.x *= -1 * Mathf.Sign(totalVelocity.x);
					break;
				case CausedKnockbackDirections.BasedOnDamageOnTouchPosition:
					Vector3 _colliderOffset =
						(_boxCollider2D != null) ? _boxCollider2D.offset : _circleCollider2D.offset;
					_knockbackForce.x *= Mathf.Sign((_colliderCorgiController.transform.position - (this.gameObject.transform.position + _colliderOffset)).x);
					break;
			}
			
			_knockbackForce.y = DamageCausedKnockbackForce.y;

			_knockbackForce = _colliderHealth.ComputeKnockbackForce(_knockbackForce, typedDamages);
			
			switch (DamageCausedKnockbackType)
			{
				case KnockbackStyles.SetForce:
					_colliderCorgiController.SetForce(_knockbackForce);
					_characterJump = _colliderCorgiController.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterJump>();
					if (_characterJump != null)
					{
						_characterJump.SetCanJumpStop(false);
						_characterJump.SetJumpFlags();
					}
					break;
				case KnockbackStyles.AddForce:
					_colliderCorgiController.AddForce(_knockbackForce);
					_characterJump = _colliderCorgiController.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterJump>();
					if (_characterJump != null)
					{
						_characterJump.SetCanJumpStop(false);
						_characterJump.SetJumpFlags();
					}
					break;
			}
		}
		
		/// <summary>
		/// Determines whether or not knockback should be applied
		/// </summary>
		/// <returns></returns>
		protected virtual bool ShouldApplyKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (_colliderHealth.ImmuneToKnockbackIfZeroDamage)
			{
				if (_colliderHealth.ComputeDamageOutput(damage, typedDamages, false) == 0)
				{
					return false;
				}
			}
			
			return (_colliderCorgiController != null)
			       && (DamageCausedKnockbackType != KnockbackStyles.NoKnockback)
			       && (DamageCausedKnockbackForce != Vector2.zero)
			       && !_colliderHealth.Invulnerable
			       && !_colliderHealth.PostDamageInvulnerable
			       && _colliderHealth.CanGetKnockback(typedDamages);
		}
	    
		protected virtual void ApplyDamageTakenKnockback()
		{
			if ((_corgiController != null) && (DamageTakenKnockbackForce != Vector2.zero) && (!_health.Invulnerable) && (!_health.PostDamageInvulnerable) && (!_health.ImmuneToKnockback))
			{
				_knockbackForce.x = DamageCausedKnockbackForce.x;
				switch (DamageTakenKnockbackDirection)
				{
					case TakenKnockbackDirections.BasedOnSpeed:
					{
						Vector2 totalVelocity = _corgiController.Speed + _velocity;
						_knockbackForce.x *= -1 * Mathf.Sign(totalVelocity.x);
						break;
					}
					case TakenKnockbackDirections.BasedOnDamagerPosition:
					{
						Vector2 relativePosition = _corgiController.transform.position - _collidingCollider.bounds.center;
						_knockbackForce.x *= Mathf.Sign(relativePosition.x);
						break;
					}
					case TakenKnockbackDirections.BasedOnDamageOnTouchPosition:
						Vector3 _colliderOffset =
							(_boxCollider2D != null) ? _boxCollider2D.offset : _circleCollider2D.offset;
						_knockbackForce.x *= Mathf.Sign((_colliderCorgiController.transform.position - (this.gameObject.transform.position + _colliderOffset)).x);
						break;
				}

				_knockbackForce.y = DamageCausedKnockbackForce.y;

				switch (DamageTakenKnockbackType)
				{
					case KnockbackStyles.SetForce:
						_corgiController.SetForce(_knockbackForce);
						break;
					case KnockbackStyles.AddForce:
						_corgiController.AddForce(_knockbackForce);
						break;
				}
			}
		}

		/// <summary>
		/// Describes what happens when colliding with a non damageable object
		/// </summary>
		protected virtual void OnCollideWithNonDamageable()
		{
			OnHitNonDamageable?.Invoke();
	        
			if (DamageTakenEveryTime + DamageTakenNonDamageable > 0)
			{
				HitNonDamageableFeedback?.PlayFeedbacks(this.transform.position);
				SelfDamage(DamageTakenEveryTime + DamageTakenNonDamageable);
			}
		}

		/// <summary>
		/// Applies damage to itself
		/// </summary>
		/// <param name="damage">Damage.</param>
		protected virtual void SelfDamage(float damage)
		{
			if (_health != null)
			{
				_damageDirection = Vector3.up;
				_health.Damage(damage,gameObject,0f,DamageTakenInvincibilityDuration, _damageDirection);
			}	

			ApplyDamageTakenKnockback();
		}

		/// <summary>
		/// A method used to draw gizmos
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			Gizmos.color = _gizmosColor;

			if ((_boxCollider2D != null) && _boxCollider2D.enabled)
			{
				_gizmoSize.x =  _boxCollider2D.bounds.size.x ;
				_gizmoSize.y =  _boxCollider2D.bounds.size.y ;
				_gizmoSize.z = 1f;
				Gizmos.DrawCube(_boxCollider2D.bounds.center, _gizmoSize);
			}
			if (_circleCollider2D != null && _circleCollider2D.enabled)
			{
				Gizmos.DrawSphere((Vector2)this.transform.position + _circleCollider2D.offset, _circleCollider2D.radius);                
			}
		}
	}
}