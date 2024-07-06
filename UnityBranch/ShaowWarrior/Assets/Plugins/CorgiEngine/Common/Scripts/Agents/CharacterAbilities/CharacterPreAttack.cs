using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class CharacterPreAttack : CharacterAbility
{
    // animation parameters
    protected const string _preAttackAnimationParameterName = "PreAttack";
    protected int _preAttackAnimationParameter;
    
    /// <summary>
    /// initializes all parameters prior to a dash and triggers the pre dash feedbacks
    /// </summary>
    public virtual void InitiatePreAttack()
    {
        // we set its dashing state to true
        _movement.ChangeState(CharacterStates.MovementStates.PreAttack);
    }
    
    /// <summary>
    /// Adds required animator parameters to the animator parameters list if they exist
    /// </summary>
    protected override void InitializeAnimatorParameters()
    {
        RegisterAnimatorParameter(_preAttackAnimationParameterName, AnimatorControllerParameterType.Bool, out _preAttackAnimationParameter);
    }

    /// <summary>
    /// At the end of the cycle, we update our animator's PreAttack state 
    /// </summary>
    public override void UpdateAnimator()
    {
        MMAnimatorExtensions.UpdateAnimatorBool(_animator, _preAttackAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.PreAttack), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
    }
    
    /// <summary>
    /// At the end of the cycle, we update our animator's PreAttack state 
    /// </summary>
    public void PlayAnimator()
    {
        MMAnimatorExtensions.UpdateAnimatorBool(_animator, _preAttackAnimationParameter, true, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
    }
    
    /// <summary>
    /// At the end of the cycle, we update our animator's PreAttack state 
    /// </summary>
    public void StopAnimator()
    {
        MMAnimatorExtensions.UpdateAnimatorBool(_animator, _preAttackAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
        print(_movement.CurrentState.ToString());
    }
    
    /// <summary>
    /// On reset ability, we cancel all the changes made
    /// </summary>
    public override void ResetAbility()
    {
        base.ResetAbility();

        if (_animator != null)
        {
            _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _preAttackAnimationParameter, (_movement.CurrentState != CharacterStates.MovementStates.PreAttack), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
        }
    }
    
    /// <summary>
    /// Causes the character to dash or dive (depending on the vertical movement at the start of the dash)
    /// </summary>
    public virtual void StartPreAttack()
    {
        InitiatePreAttack();
    }
}
