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
    public int money = 0;
    public int lucky = 0;

    [Tooltip("击杀敌人上限")] 
    public int bulletTimeCanKillEnemyNumber;
    [Tooltip("技能蓄力时间长度")]
    public float skillChargeTime;
    [Tooltip("子弹时间长度")]
    public float bulletTime;
    [Tooltip("弹反需求次数")]
    public int skillRequiement;

    public bool canUseSkill => !_isBulletTime && _bounce >= skillRequiement;

    private bool _isBulletTime = false;
    
    [SerializeReference]
    private int _bounce = 0;

    [HideInInspector]
    public Character player {get; private set;}
    
    public void OnMMEvent(MMGameEvent eventType)
    {
        switch (eventType.EventName) {
            case GameEventType.BounceSuccess: {
                _bounce++;
                GUIManager.Instance.SetBounceBar(_bounce);
                return;
            }
            case GameEventType.Dead: {
                _bounce = 0;
                GUIManager.Instance.SetBounceBar(_bounce);
                return;
            }
        }
    }

    public void OnMMEvent(CorgiEngineEvent eventType)
    {
        switch (eventType.EventType) {
            case CorgiEngineEventTypes.LevelStart: {
                var health = player.GetComponent<Health>(); 
                health.InitialHealth = hp;
                health.MaximumHealth = hp;
                health.SetHealth(hp, null);
                return;
            }
            case CorgiEngineEventTypes.LevelComplete: {
                return;
            }
            case CorgiEngineEventTypes.LevelEnd: {
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

    void Start()
    {
        MMEventManager.AddListener<CorgiEngineEvent>(this);
        MMEventManager.AddListener<MMGameEvent>(this);

    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
    }

    public void UseSkill() {
        if (_bounce >= skillRequiement && !_isBulletTime) {
            _bounce = 0;
            GUIManager.Instance.SetBounceBar(_bounce);
            MMGameEvent.Trigger(GameEventType.UseSkill);
        }
    }

    public void Stun(float time) {
        player.FindAbility<CharacterStun>().StunFor(time);
    }

    /// <summary>
    /// 增加 i 滴血
    /// </summary>
    /// <param name="i"></param>
    public void AddHP(int i)
    {
        hp += i;
    }

    /// <summary>
    /// 增加 i 点金币
    /// </summary>
    /// <param name="i"></param>
    public void AddMoney(int i)
    {
        money += i;
        GUIManager.Instance.UpdateMoneyText(money);
    }

}
