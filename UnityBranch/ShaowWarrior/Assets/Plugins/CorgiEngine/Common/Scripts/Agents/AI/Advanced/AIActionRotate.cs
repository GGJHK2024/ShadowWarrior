using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This action directs the CharacterHorizontalMovement ability to move around the target.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Rotate")]
    public class AIActionRotate : AIAction
    {
        /// The minimum distance to the target that this Character can reach
        [Tooltip("The minimum distance to the target that this Character can reach")]
        public float MinimumDistance = 1f;
        
        protected CharacterHorizontalMovement _characterHorizontalMovement;
        
        /// <summary>
        /// On init we grab our CharacterHorizontalMovement ability
        /// </summary>
        public override void Initialization()
        {
            _characterHorizontalMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHorizontalMovement>();
        }
        
        /// <summary>
        /// Moves the character in the decided direction
        /// </summary>
        protected virtual void Move()
        {
            if (_brain.Target == null)
            {
                _characterHorizontalMovement.SetHorizontalMove(0f);
                return;
            }
            var rotation = transform.rotation;
            transform.RotateAround(_brain.Target.transform.position, Vector3.forward, 100 * Time.deltaTime);
            transform.rotation = rotation;
        }

        public override void PerformAction()
        {
            Move();
        }
    }
}