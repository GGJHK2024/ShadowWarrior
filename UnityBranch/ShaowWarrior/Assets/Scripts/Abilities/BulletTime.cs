using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class BulletTime : CharacterAbility,
        MMEventListener<MMGameEvent>,
        MMEventListener<CorgiEngineEvent>
{
    private PlayerManager _playerManager;

    private bool _charging = false;
    private float _skillCharge;
    private bool _isBulletTime = false;
    private float _bulletTime;

    void Awake()
    {
        _playerManager = PlayerManager.Instance;        
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        this.MMEventStartListening<MMGameEvent>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        this.MMEventStopListening<MMGameEvent>();

    }
    protected override void HandleInput()
    {
        if (!_playerManager.canUseSkill)
            return;
        bool useSkillInput = _inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed 
                                && _inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed;

        if (useSkillInput) {
            Focus();
        } else {
            if (_charging)
            {
                StopStartFeedbacks();
                resetBulletTime();    
            }
        }
    }

    public override void ProcessAbility()
    {
        if (_isBulletTime) {
            _bulletTime -= Time.deltaTime;
            _isBulletTime = _bulletTime > 0;
            if (!_isBulletTime)
                ExitBulletTime();
        }
    }

    // 蓄力
    private void Focus() {
        if (!_charging) {
            _skillCharge = _playerManager.skillChargeTime;
            PlayAbilityStartFeedbacks();
            _charging = true;
        }
        else {
            _skillCharge -= Time.deltaTime;
            _charging = true;
        }
        if (_skillCharge <= 0) {
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
            _playerManager.UseSkill();
        }
        else
            PlayAbilityStartFeedbacks();
    }

    private void resetBulletTime() {
        _charging = false;
        _skillCharge = 0;
    }

    public bool EnterBulletTime() {
        if (LevelManager.Instance == null)
            return false;
        MMGameEvent.Trigger(GameEventType.FreezeNpc);
        LevelManager.Instance.FreezeCharacters(false);
        _isBulletTime = true;
        _bulletTime = _playerManager.bulletTime;
        return true;
    }

    public void ExitBulletTime() {
        MMGameEvent.Trigger(GameEventType.UnFreeNpc);
        _bulletTime = 0;
        _isBulletTime = false;
        LevelManager.Instance.UnFreezeCharacters();
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName == GameEventType.UseSkill)
            EnterBulletTime();
    }

    public void OnMMEvent(CorgiEngineEvent eventType)
    {
        switch (eventType.EventType) {
            case CorgiEngineEventTypes.LevelStart: {
                ExitBulletTime();
                return;
            }
            case CorgiEngineEventTypes.LevelComplete: {
                ExitBulletTime();
                return;
            }
            case CorgiEngineEventTypes.LevelEnd: {
                ExitBulletTime();
                return;
            }
        }
    }
}
