using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public interface IInputHandler {
    public void hanlderInput();
}

public class PlayerManager : MMPersistentSingleton<PlayerManager>,		
        MMEventListener<MMGameEvent>,
        MMEventListener<CorgiEngineEvent>
{
    public Character playerPrefab;

    public int hp;

    [Tooltip("技能蓄力时间长度")]
    public float skillChargeTime;
    [Tooltip("子弹时间长度")]
    public float bulletTime;

    [Tooltip("弹反需求次数")]
    public int skillRequiement;

    public bool canUseSkill => !_isBulletTime && _bounce >= skillRequiement;

    private bool _charging = false;
    private float _skillCharge;
    private bool _isBulletTime = false;
    private float _bulletTime;
    private int _bounce = 0;

    [HideInInspector]
    public Character player {get; private set;}
    
    public void OnMMEvent(MMGameEvent eventType)
    {
    }

    public void OnMMEvent(CorgiEngineEvent eventType)
    {
        switch (eventType.EventType) {
            case CorgiEngineEventTypes.LevelStart: {
                player.GetComponent<Health>().InitialHealth = hp;
                player.GetComponent<Health>().MaximumHealth = hp;

                this.ExitBulletTime();
                return;
            }
            case CorgiEngineEventTypes.LevelComplete: {
                this.ExitBulletTime();
                return;
            }
            case CorgiEngineEventTypes.LevelEnd: {
                this.ExitBulletTime();
                return;
            }
        }
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        player = GameManager.Instance.PersistentCharacter = (Character)Instantiate(playerPrefab);
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (_isBulletTime) {
            _bulletTime -= Time.deltaTime;
            _isBulletTime = _bulletTime > 0;
            if (!_isBulletTime)
                ExitBulletTime();
        }

        HandleInput();
    }

    private void HandleInput() {
        // 检查释放技能
        if (!canUseSkill) {
            _charging = false;
            _skillCharge = 0;
            return;
        }

        bool useSkillInput = player.LinkedInputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed 
                                && player.LinkedInputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed;

        if (useSkillInput) {
            if (!_charging)
                _skillCharge = skillChargeTime;
            else
                _skillCharge -= Time.deltaTime;
            _charging = true;
        } else {
            _charging = false;
            _skillCharge = 0;
            return;
        }
        Debug.LogFormat("skillCharge {0}", _skillCharge);
        if (_skillCharge <= 0)
            UseSkill(); 
    }

    public void UseSkill() {
        if (_bounce >= skillRequiement && !_isBulletTime) {
            EnterBulletTime();
            _bounce = 0;
            _charging = false;
            _skillCharge = 0;
        }
    }

    public bool EnterBulletTime() {
        if (LevelManager.Instance == null)
            return false;
        MMGameEvent.Trigger(GameEventType.FreezeNpc);
        LevelManager.Instance.FreezeCharacters(false);
        _isBulletTime = true;
        _bulletTime = bulletTime;
        return true;
    }

    public void ExitBulletTime() {
        MMGameEvent.Trigger(GameEventType.UnFreeNpc);
        _bulletTime = 0;
        _isBulletTime = false;
        LevelManager.Instance.UnFreezeCharacters();
    }

    public void Stun(float time) {
        player.FindAbility<CharacterStun>().StunFor(time);
    }

}
