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
    public string playerName = "哈哈";
    public int hp = 5;
    public int attack = 25;
    public float cd = 5;
    public int money = 0;
    public int lucky = 50;
    public bool hasBigSkill = false;

    protected float _cd;
    protected int _attack;
    /// <summary>
    /// 【大招】血魔流-吸干你的血。击杀敌人触发吸血技能。击杀成功恢复X%血量，累计杀害敌人血量达N，触发S秒无敌状态。
    /// </summary>
    public bool passiveSkill1 = false;
    /// <summary>
    /// 【大招】低血流-打不倒的小强。当血量低于X%时，闪避无CD且主动攻击伤害翻倍。累计杀害敌人血量达N，S秒主动攻击一击毙命。
    /// </summary>
    public bool passiveSkill2 = false;

    /// <summary>
    /// 统计击杀敌人的血量
    /// </summary>
    [SerializeField]
    private int _enemyBlood = 0;

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
            case GameEventType.PassiveSkill2Effect:
            {
                if (cd == 0 && attack != _attack) return;
                // 当血量低于10%时，闪避无CD且主动攻击伤害翻倍
                player.GetComponent<CharacterDash>().DashCooldown = (player.GetComponent<CharacterDash>().DashCooldown - cd) >= 0 ? 
                    player.GetComponent<CharacterDash>().DashCooldown - cd : player.GetComponent<CharacterDash>().DashCooldown;
                cd = 0;
                attack *= 2;
                break;
            }
            case GameEventType.ExitPassiveSkill2Effect:
            {
                if (cd == _cd) return;
                cd = _cd;
                player.GetComponent<CharacterDash>().DashCooldown =
                    player.GetComponent<CharacterDash>().DashCooldown + cd;
                attack = _attack;
                break;
            }
            case GameEventType.BounceSuccess: {
                if (hasBigSkill)
                {
                    _bounce++;
                    GUIManager.Instance.SetBounceBar(_bounce);
                }
                return;
            }
            case GameEventType.Dead: {
                _bounce = 0;
                GUIManager.Instance.SetBounceBar(_bounce);
                _enemyBlood = 0;
                GUIManager.Instance.SetBloodBar(_enemyBlood);
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
                    for (int i = 0; i < mw.Length; i++)
                    {
                        mw[i].MinDamageCaused = (i + 1) * attack;
                        mw[i].MaxDamageCaused = (i + 1) * attack;
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
                GoToLevel();
                break;
            }
        }
    }

    /// <summary>
    /// 十秒钟无敌
    /// </summary>
    /// <returns></returns>
    IEnumerator UnHittableFor10Second()
    {
        player.gameObject.GetComponent<Health>().Invulnerable = true;
        yield return new WaitForSeconds(10.0f);
        player.gameObject.GetComponent<Health>().Invulnerable = false;
        yield return null;
    }

    /// <summary>
    /// 十五秒一刀毙命
    /// </summary>
    /// <returns></returns>
    IEnumerator OneHit999For15Second()
    {
        int temp = attack;
        attack = 999;
        yield return new WaitForSeconds(15.0f);
        attack = temp;
        yield return null;
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
        _cd = cd;
        _attack = attack;
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

    /// <summary>
    /// 是否允许使用被动大招1
    /// </summary>
    public void EnablePSkill1(bool state)
    {
        passiveSkill1 = state;
    }

    /// <summary>
    /// 是否允许使用被动大招2
    /// </summary>
    public void EnablePSkill2(bool state)
    {
        passiveSkill2 = state;
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
    /// 更新累计击杀血量
    /// </summary>
    /// <param name="v"></param>
    public void UpdateBloodBar(int v)
    {
        if(!passiveSkill1 && !passiveSkill2)    return;
        _enemyBlood += v;
        if (_enemyBlood >= 2000)
        {
            if (passiveSkill1)
            {
                // 触发10秒无敌状态
                StartCoroutine("UnHittableFor10Second");
            }
            if (passiveSkill2)
            {
                // 触发15秒主动攻击一击毙命效果
                StartCoroutine("OneHit999For15Second");
            }
            _enemyBlood = 0;
            GUIManager.Instance.SetBloodBar(_enemyBlood);
        }
        else
        {
            GUIManager.Instance.SetBloodBar(_enemyBlood);
        }
    }

    /// <summary>
    /// 玩家设置名字
    /// </summary>
    /// <param name="n"></param>
    public void SetName(string n)
    {
        playerName = n;
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
    /// 减少is cd
    /// </summary>
    /// <param name="i"></param>
    public void SubCD(float i)
    {
        cd -= i;
        _cd = cd;
        player.GetComponent<CharacterDash>().DashCooldown = (player.GetComponent<CharacterDash>().DashCooldown - i) >= 0 ? 
            player.GetComponent<CharacterDash>().DashCooldown - i : player.GetComponent<CharacterDash>().DashCooldown;
    }

    /// <summary>
    /// 增加 i 点金币
    /// </summary>
    /// <param name="i"></param>
    public void AddMoney(int i)
    {
        money = (money + i < 0) ? 0 : money + i;
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
    /// 增加 i 点攻击
    /// </summary>
    /// <param name="i"></param>
    public void AddAttack(int i)
    {
        attack += i;
        _attack = attack;
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
        _cd = cd;
        _attack = attack;
        DisableBigSKill();
        EnablePSkill1(false);
        EnablePSkill2(false);
    }

}
