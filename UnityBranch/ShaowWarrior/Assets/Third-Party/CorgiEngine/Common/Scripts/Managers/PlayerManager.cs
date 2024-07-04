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

    public int hp = 5;
    public int attack = 25;
    public int cd = 5;
    public int money = 0;
    public int lucky = 50;
    public bool hasBigSkill = false;
    /// <summary>
    /// 【大招】血魔流-吸干你的血。击杀敌人触发吸血技能。击杀成功恢复X%血量，累计杀害敌人血量达N，触发S秒无敌状态。
    /// </summary>
    public bool passiveSkill1 = false;
    /// <summary>
    /// 【大招】低血流-打不倒的小强。当血量低于X%时，闪避无CD且主动攻击伤害翻倍。累计杀害敌人血量达N，S秒主动攻击一击毙命。
    /// </summary>
    public bool passiveSkill2 = false;

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
                // set health
                var health = player.GetComponent<Health>(); 
                health.InitialHealth = hp * 20;
                health.MaximumHealth = hp * 20;
                // health.SetHealth(health.CurrentHealth, null);
                // set attack
                GameObject weapon = null;
                for (int i = 0; i < player.gameObject.transform.childCount; i++)
                {
                    if (player.gameObject.transform.GetChild(i).name.Contains("MeleeWeapon"))
                    {
                        weapon = player.gameObject.transform.GetChild(i).gameObject;
                        break;
                    }
                }
                if (weapon)
                {
                    MeleeWeapon[] mw = weapon.GetComponents<MeleeWeapon>();
                    foreach (var a in mw)
                    {
                        a.MinDamageCaused = attack;
                        a.MaxDamageCaused = attack;
                    }
                }
                // set cd
                player.GetComponent<CharacterDash>().DashCooldown = cd;
                return;
            }
            case CorgiEngineEventTypes.LevelComplete: {
                return;
            }
            case CorgiEngineEventTypes.LevelEnd: {
                return;
            }
            case CorgiEngineEventTypes.GameOver:
            {
                print("reset in PM");
                GoToLevel();
                break;
            }
        }
    }
    
    /// <summary>
    /// Loads the level specified in the inspector
    /// </summary>
    public virtual void GoToLevel()
    {
        // 由于目前这个函数仅用于不保存回到标题的功能，因此需要重置角色和关卡进度。
        CharacterHandleWeapon curWeapon = player.GetComponent<CharacterHandleWeapon>();
        curWeapon.InitialWeapon = Resources.Load<MeleeWeapon>("Prefabs/MeleeWeapon_1");
        curWeapon.Setup();
        ResetPlayerParams();
        // 关卡进度
        LevelChooseManager.Instance.ResetLevelProgress();
        LevelManager.Instance.GotoLevel("CG");
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
        // hasBigSkill = false;    // 初始无法使用双键大招
        var health = player.GetComponent<Health>(); 
        health.SetHealth(health.InitialHealth, null);
        MMEventManager.AddListener<CorgiEngineEvent>(this);
        MMEventManager.AddListener<MMGameEvent>(this);

    }

    /// <summary>
    /// 允许使用主动大招
    /// </summary>
    public void EnableBigSkill()
    {
        hasBigSkill = true;
    }

    /// <summary>
    /// 禁止使用主动大招
    /// </summary>
    public void DisableBigSKill()
    {
        hasBigSkill = false;
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

    /// <summary>
    /// 增加 i 点幸运值
    /// </summary>
    /// <param name="i"></param>
    public void AddLcuky(int i)
    {
        lucky += i;
        // GUIManager.Instance. //好像目前没有表达幸运值的ui
    }

    /// <summary>
    /// 重置所有玩家属性（这里没有连招
    /// </summary>
    public void ResetPlayerParams()
    {
        hp = 5;
        attack = 25;
        cd = 5;
        money = 0;
        lucky = 50;
        DisableBigSKill();
        passiveSkill1 = false;
        passiveSkill2 = false;
    }

}
