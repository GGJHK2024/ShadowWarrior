using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class BulletTime : CharacterAbility,
        MMEventListener<MMGameEvent>,
        MMEventListener<CorgiEngineEvent>
{

    [Tooltip("选中要击杀的敌人")] 
    private List<Health> enemies = new List<Health>();
    
    private PlayerManager _playerManager;

    private int _bulletTimeCanKillEnemyNumber;
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
        _bulletTimeCanKillEnemyNumber = _playerManager.bulletTimeCanKillEnemyNumber;
        enemies.Clear();
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
        if (!_playerManager.canUseSkill || !_playerManager.hasBigSkill)
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
            // 原先的计时大招功能，现在改为选择敌人
            /*_bulletTime -= Time.deltaTime;
            _isBulletTime = _bulletTime > 0;*/

            Vector2 ray = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(ray, ray);
            if (hit.collider != null && hit.collider.gameObject.layer is 13 or 29)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Health currentEne = hit.collider.gameObject.GetComponent<Health>();
                    if (enemies.Contains(currentEne))
                    {
                        enemies.Remove(currentEne);
                        currentEne.GetComponent<Character>().CancelOutlineCharacter();
                        // Debug.Log("移除：" + hit.collider.gameObject.name);
                    }
                    else
                    {
                        enemies.Add(currentEne);
                        currentEne.GetComponent<Character>().OutlineCharacter();
                        // Debug.Log("选中：" + hit.collider.gameObject.name);
                    }
                }
            }
            /*if (!_isBulletTime)
                ExitBulletTime();*/
            if (enemies.Count >= ((EnemyManager.Instance.RemainingEnemies()>_bulletTimeCanKillEnemyNumber)?
                    _bulletTimeCanKillEnemyNumber:EnemyManager.Instance.RemainingEnemies()))
            {
                ExitBulletTime();
            }
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

    /// <summary>
    /// 开启大招（进入子弹时间）
    /// </summary>
    /// <returns></returns>
    public bool EnterBulletTime() {
        if (LevelManager.Instance == null)
            return false;
        // MMGameEvent.Trigger(GameEventType.FreezeNpc);
        MMEventManager.TriggerEvent(new MMGameEvent("FreezeNpc"));
        LevelManager.Instance.FreezeCharacters(false);
        _isBulletTime = true;
        enemies.Clear();
        // _bulletTime = _playerManager.bulletTime;
        return true;
    }

    /// <summary>
    /// 结束大招（离开子弹时间）
    /// </summary>
    public void ExitBulletTime() {
        // MMGameEvent.Trigger(GameEventType.UnFreezeNpc);
        MMEventManager.TriggerEvent(new MMGameEvent("UnFreezeNpc"));
        // 一击必杀所有选中的敌人
        foreach (var e in enemies)
        {
            e.Kill();
        }
        // _bulletTime = 0;
        enemies.Clear();
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
            case CorgiEngineEventTypes.PlayerDeath:
            {
                ExitBulletTime();
                return;
            }
        }
    }
}
