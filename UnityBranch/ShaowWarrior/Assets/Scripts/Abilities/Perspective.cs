using System;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;


public class Perspective : CharacterAbility
{
    [Tooltip("透视上下界位置")] 
    public float topPos;
    public float groundPos;

    public float topScale;
    public float downScale;

    protected float _verticalMovement;
    protected float mapVal;
    protected float remap;
    protected float curScale;
    
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        mapVal = topPos - groundPos;
    }

    public override void ProcessAbility()
    {
        _verticalMovement = _verticalInput;
        if (_verticalMovement != 0 && (_movement.CurrentState == CharacterStates.MovementStates.Walking ||
                                       _movement.CurrentState == CharacterStates.MovementStates.Running))
        {
            // when move vertically, change scale.
            // y = ax + b
            remap = Math.Abs((gameObject.transform.position.y - groundPos)/mapVal);
            curScale = Mathf.Lerp(topScale, downScale, remap);
            gameObject.transform.localScale = new Vector3(curScale,curScale,curScale);
        }
    }
}
