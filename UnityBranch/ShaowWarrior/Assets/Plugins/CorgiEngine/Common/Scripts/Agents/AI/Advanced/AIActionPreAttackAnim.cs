using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// An AIACtion used to have an AI face its AI Brain's Target
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/AI/Actions/AIActionPreAttackAnim")]
    public class AIActionPreAttackAnim : AIAction
    {
	    /// the ability to pilot
		[FormerlySerializedAs("TargetHandleWeapon")] [Tooltip("the ability to pilot")]
		public CharacterPreAttack PreAttactComp;
	    
		protected Character _character;

		/// <summary>
		/// On init we grab our CharacterHandleWeapon ability
		/// </summary>
		public override void Initialization()
		{
			_character = GetComponentInParent<Character>();
			if (PreAttactComp == null)
			{
				PreAttactComp = _character?.FindAbility<CharacterPreAttack>();    
			}
		}

		/// <summary>
		/// On PerformAction we face and aim if needed, and we shoot
		/// </summary>
		public override void PerformAction()
		{
			PreAttactComp.UpdateAnimator();
		}


		/// <summary>
		/// When entering the state we reset our shoot counter and grab our weapon
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			// 播放
			print("进入预警动作状态");
			PreAttactComp.StartPreAttack();
		}

		/// <summary>
		/// When exiting the state we make sure we're not shooting anymore
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			print("离开预警动作状态");
			// 停止播放
			PreAttactComp.ResetAbility();
		}
	}
}
