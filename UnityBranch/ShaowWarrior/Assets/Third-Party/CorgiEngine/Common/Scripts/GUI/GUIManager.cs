using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using TMPro;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Handles all GUI effects and changes
	/// </summary>
	public class GUIManager : MMSingleton<GUIManager>, MMEventListener<LevelNameEvent>, MMEventListener<ControlsModeEvent>
	{
		[Header("Bindings")]

		/// the game object that contains the heads up display (avatar, health, points...)
		[Tooltip("the game object that contains the heads up display (avatar, health, points...)")]
		public GameObject HUD;
		/// the health bar
		[Tooltip("the health bar")]
		public MMProgressBar[] HealthBars;
		///  the bounce bar
		[Tooltip("大招准备条")]
		public Slider bounceBar;
		[Tooltip("头像")]
		public Image avater;
		[Tooltip("头像遮罩")]
		public Image avaterMusk;
		[Tooltip("金币文本")] 
		public TextMeshProUGUI money;
		/// the panels and bars used to display current weapon ammo
		[Tooltip("the panels and bars used to display current weapon ammo")]
		public AmmoDisplay[] AmmoDisplays;
		/// the pause screen game object
		[Tooltip("the pause screen game object")]
		public GameObject PauseScreen;
		/// the time splash gameobject
		[Tooltip("the time splash gameobject")]
		public GameObject TimeSplash;
		/// The mobile buttons
		[Tooltip("The mobile buttons")]
		public CanvasGroup Buttons;
		/// The mobile arrows
		[Tooltip("The mobile arrows")]
		public CanvasGroup Arrows;
		/// The mobile movement joystick
		[Tooltip("The mobile movement joystick")]
		public CanvasGroup Joystick;
		/// the points counter
		[Tooltip("the points counter")]
		public Text PointsText;
		/// the level display
		[Tooltip("the level display")]
		public Text LevelText;
		
		[Header("LevelUpTexts")]
		/// 通关升级界面
		[Tooltip("升级界面")] 
		public GameObject LevelUpScreen;
		[Tooltip("LU界面姓名")] 
		public TextMeshProUGUI player_text;
		[Tooltip("LU界面攻击力")]
		public TextMeshProUGUI attack_text;
		[Tooltip("LU界面金币")] 
		public TextMeshProUGUI money_text;
		[Tooltip("LU界面幸运值")] 
		public TextMeshProUGUI lucky_text;
		[Tooltip("LU界面闪避cd值")] 
		public TextMeshProUGUI cd_text;
		[Tooltip("LU界面血条")] 
		public Slider hp_bar;
		[Tooltip("LU界面Skill Pic 2")] 
		public Image skill_2;
		[Tooltip("LU界面Skill Pic 3")] 
		public Image skill_3;
		[Tooltip("LU界面升级A")] 
		public Button optionA;
		[Tooltip("LU界面升级B")] 
		public Button optionB;
		
		[Header("ShopTexts")]
		/// 商店界面
		[Tooltip("商店界面")] 
		public GameObject ShopScreen;
		[Tooltip("技能升级图标，如果升满了这里替换成售罄")]
		public Button SkillUpdate;
		[Tooltip("大招图标，如果已经有了这里替换成售罄")]
		public Button BigSkill;
		[Tooltip("当前购物需要的金钱文本")]
		public TextMeshProUGUI neededMoneyText;
		[Tooltip("当前购物需要的金钱")]
		private int neededMoney = 0;
		[Tooltip("检查当前是否已经购买过技能升级")]
		private bool boughtSKillUp = false;
		
		[Header("SpecialEventTexts")]
		/// 奇遇界面
		[Tooltip("奇遇界面")] 
		public GameObject SepcialEventScreen;
		public bool hadSpecialEvent = false;
		[Tooltip("奇遇子事件界面")] 
		public GameObject Sp_Good1;
		public GameObject Sp_Good2;
		public GameObject Sp_Good3;
		public GameObject Sp_Normal1;
		public GameObject Sp_Normal2;
		public GameObject Sp_Bad;
		private GameObject curPanel;
		private GameObject curSubPanel;

		[Header("Settings")]

		/// the pattern to apply when displaying the score
		[Tooltip("the pattern to apply when displaying the score")]
		public string PointsPattern = "000000";

		protected float _initialJoystickAlpha;
		protected float _initialArrowsAlpha;
		protected float _initialButtonsAlpha;
		protected Object[] sprites;	//all ui sliced sprites
		[SerializeField]
		protected List<Button> buyingItems;	// 待确认的购买物
		
		private Character player {get; set;}
		
		/// <summary>
		/// Statics initialization to support enter play modes
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			player = GameManager.Instance.PersistentCharacter;
			sprites = Resources.LoadAll ("ui");

			if (Joystick != null)
			{
				_initialJoystickAlpha = Joystick.alpha;
			}
			if (Arrows != null)
			{
				_initialArrowsAlpha = Arrows.alpha;
			}
			if (Buttons != null)
			{
				_initialButtonsAlpha = Buttons.alpha;
			}
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start()
		{
			player = GameManager.Instance.PersistentCharacter;
			RefreshPoints();
		}

		/// <summary>
		/// Sets the HUD active or inactive
		/// </summary>
		/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
		public virtual void SetHUDActive(bool state)
		{
			if (HUD!= null)
			{ 
				HUD.SetActive(state);
			}
			if (PointsText!= null)
			{ 
				PointsText.enabled = state;
			}
			if (LevelText!= null)
			{ 
				LevelText.enabled = state;
			}
		}

		/// <summary>
		/// Sets the avatar active or inactive
		/// </summary>
		/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
		public virtual void SetAvatarActive(bool state)
		{
			if (HUD != null)
			{
				HUD.SetActive(state);
			}
		}

		/// <summary>
		/// Called by the input manager, this method turns controls visible or not depending on what's been chosen
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		/// <param name="movementControl">Movement control.</param>
		public virtual void SetMobileControlsActive(bool state, InputManager.MovementControls movementControl = InputManager.MovementControls.Joystick)
		{
			if (Joystick != null)
			{
				Joystick.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Joystick)
				{
					Joystick.alpha = _initialJoystickAlpha;
				}
				else
				{
					Joystick.alpha = 0;
					Joystick.gameObject.SetActive(false);
				}
			}

			if (Arrows != null)
			{
				Arrows.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Arrows)
				{
					Arrows.alpha = _initialArrowsAlpha;
				}
				else
				{
					Arrows.alpha = 0;
					Arrows.gameObject.SetActive(false);
				}
			}

			if (Buttons != null)
			{
				Buttons.gameObject.SetActive(state);
				if (state)
				{
					Buttons.alpha=_initialButtonsAlpha;
				}
				else
				{
					Buttons.alpha=0;
					Buttons.gameObject.SetActive (false);
				}
			}
		}

		/// <summary>
		/// 更新GUI金币文本
		/// </summary>
		/// <param name="i"></param>
		public void UpdateMoneyText(int i)
		{
			money.text = i.ToString();
		}

		/// <summary>
		/// Sets the pause.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetPause(bool state)
		{
			if (PauseScreen!= null)
			{ 
				PauseScreen.SetActive(state);
				EventSystem.current.sendNavigationEvents = state;
			}
		}

		/// <summary>
		/// 打开简历界面
		/// </summary>
		public virtual void OpenPause()
		{
			Time.timeScale = 0;
			if (PauseScreen != null)
			{ 
				PauseScreen.SetActive(true);
				UpdateLUManuel();
				// 隐藏LU界面的按钮
				ShowLUButton(false);
			}
		}

		/// <summary>
		/// 关闭简历界面
		/// </summary>
		public virtual void ClosePause()
		{
			Time.timeScale = 1.0f;
			if (PauseScreen != null)
			{ 
				PauseScreen.SetActive(false);
			}
		}
		
		/// <summary>
		/// Open 简历页面
		/// </summary>
		public virtual void OpenCV()
		{
			Time.timeScale = 0;
			if (LevelUpScreen!= null)
			{ 
				LevelUpScreen.SetActive(true);
				// 显示LU界面的按钮
				ShowLUButton(true);
				UpdateLUManuel();
			}
		}
		
		/// <summary>
		/// Sets the Level Up.
		/// </summary>
		public virtual void OpenLevelUp()
		{
			if (LevelUpScreen!= null)
			{ 
				Invoke("InvokOpenLU", 1.5f);
			}
		}

		public void InvokOpenLU()
		{
			Time.timeScale = 0;
			LevelUpScreen.SetActive(true);
			// 显示LU界面的按钮
			ShowLUButton(true);
			UpdateLUManuel();
		}
		
		/// <summary>
		/// Sets the Level Up.
		/// </summary>
		public virtual void CloseLevelUp()
		{
			Time.timeScale = 1.0f;
			if (LevelUpScreen!= null)
			{ 
				LevelUpScreen.SetActive(false);
			}
		}

		/// <summary>
		/// 设置LU界面按钮显隐状态
		/// </summary>
		/// <param name="state"></param>
		public void ShowLUButton(bool state)
		{
			optionA.gameObject.SetActive(state);
			optionB.gameObject.SetActive(state);
		}
		
		/// <summary>
		/// Sets the Shop.
		/// </summary>
		public virtual void OpenShop()
		{
			Time.timeScale = 0;
			UpdateNeedMoneyText(0);
			if (ShopScreen != null)
			{ 
				ShopScreen.SetActive(true);
				player = GameManager.Instance.PersistentCharacter;
				string curName = player.GetComponent<CharacterHandleWeapon>().InitialWeapon.name;
				int curStage = int.Parse(curName.Substring(curName.Length - 1, 1));
				// 如果已经连招升级满了，则替换为售罄，按钮无法购买
				if (curStage == 3 || boughtSKillUp)
				{
					SkillUpdate.gameObject.GetComponent<Image>().color = new Color(1,1,1,0);	// 售罄
					SkillUpdate.enabled = false;
				}
				else
				{
					SkillUpdate.gameObject.GetComponent<Image>().color = new Color(1,1,1,1);
					SkillUpdate.enabled = true;
				}
				// 如果已经有大招了，则替换为售罄，按钮无法购买
				if (PlayerManager.Instance.hasBigSkill == true)
				{
					BigSkill.gameObject.GetComponent<Image>().color = new Color(1,1,1,0);	// 售罄
					BigSkill.enabled = false;
				}
				else
				{
					BigSkill.gameObject.GetComponent<Image>().color = new Color(1,1,1,1);	
					BigSkill.enabled = true;
				}
			}
		}

		/// <summary>
		/// 确认购物
		/// 如果钱够则执行购买决策，否则播放无法购买的音效
		/// </summary>
		public void ConfirmBuying()
		{
			if (PlayerManager.Instance.money < neededMoney)
			{
				// 播放无法购买的音效
				print("购买失败");
			}
			else
			{
				PlayerManager.Instance.money -= neededMoney;
				foreach (var b in buyingItems)
				{
					if (b.name == "LU_SkillUp")
					{
						boughtSKillUp = true;
						SkillUpdate.gameObject.GetComponent<Image>().color = new Color(1,1,1,0);	// 售罄
						SkillUpdate.enabled = false;
					}

					if (b.name == "S_BigSkill")
					{
						// 已经购买大招了，则替换为售罄，按钮无法购买
						BigSkill.gameObject.GetComponent<Image>().color = new Color(1,1,1,0);	// 售罄
						BigSkill.enabled = false;
					}
					Invoke(b.name,0.02f);
					b.gameObject.GetComponent<Image>().material = null;
				}
				// 清空所需金币数，更新文本
				buyingItems.Clear();
				neededMoney = 0;
				UpdateNeedMoneyText(neededMoney);
				UpdateMoneyText(PlayerManager.Instance.money);
				// 播放购买的音效
				print("购买成功");
			}
		}
		
		/// <summary>
		/// Sets the Shop.
		/// </summary>
		public virtual void CLoseShop()
		{
			Time.timeScale = 1.0f;
			if (ShopScreen!= null)
			{
				foreach (var b in buyingItems)
				{
					b.gameObject.GetComponent<Image>().material = null;	// 移除选择描边效果
				}
				buyingItems.Clear();
				ShopScreen.SetActive(false);
			}
		}

		/// <summary>
		/// 打开奇遇界面
		/// </summary>
		public virtual void OpenSpecialEvent()
		{
			if (SepcialEventScreen != null && !hadSpecialEvent)
			{ 
				Time.timeScale = 0;
				SepcialEventScreen.SetActive(true);
				// roll奇遇事件
				RollSpecialEvent();
				hadSpecialEvent = true;
			}
		}

		/// <summary>
		/// 退出奇遇
		/// </summary>
		public virtual void CloseSpecialEvent()
		{
			Time.timeScale = 1;
			if (SepcialEventScreen != null)
			{ 
				if(curSubPanel)
				{
					curSubPanel.SetActive(false);
					curPanel.transform.GetChild(0).gameObject.SetActive(true);
					curSubPanel = null;
				}
				curPanel.SetActive(false);
				SepcialEventScreen.SetActive(false);
			}
		}

		/// <summary>
		/// 商品选择函数（作用与按钮上）
		/// </summary>
		/// <param name="b"></param>
		public void ShopItemFunc(Button b)
		{
			if (CheckButton(b))
			{
				b.gameObject.GetComponent<Image>().material = null;	// 移除选择描边效果
				buyingItems.Remove(b);
				neededMoney -= int.Parse(b.gameObject.transform.GetChild(transform.childCount).GetComponent<TextMeshProUGUI>()
					.text);
				UpdateNeedMoneyText(neededMoney);
			}
			else
			{
				b.gameObject.GetComponent<Image>().material = Resources.Load<Material>("Materials/Custom_Outline");
				buyingItems.Add(b);
				neededMoney += int.Parse(b.gameObject.transform.GetChild(transform.childCount).GetComponent<TextMeshProUGUI>()
					.text);
				UpdateNeedMoneyText(neededMoney);
			}
			print("money: " + neededMoney);
		}

		/// <summary>
		/// 检查该商品是否已添加
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool CheckButton(Button b)
		{
			return buyingItems.Contains(b);
		}

		/// <summary>
		/// 根据幸运值roll特殊事件
		/// </summary>
		public void RollSpecialEvent()
		{
			float rd = Random.Range(0.0f, 1.0f);
			float curLucky = PlayerManager.Instance.lucky / 100.0f;
			if (rd < curLucky)
			{
				// 好事件中roll一个
				print("good event");
				RollGood();
			}
			else
			{
				float nb_rd = Random.Range(0.0f, 1.0f);
				if (nb_rd < 0.667f)
				{
					// 一般事件中roll一个
					print("normal event");
					RollNormal();
				}
				else
				{
					// 坏事件
					print("bad event");
					RollBad();
				}
			}
		}

		/// <summary>
		/// roll 好事件
		/// </summary>
		private void RollGood()
		{
			float rd = Random.Range(0.0f, 1.0f);
			if (rd <= 0.3333f)
			{
				Sp_Good1.SetActive(true);
				curPanel = Sp_Good1;
			}else if (rd <= 0.6667f)
			{
				Sp_Good2.SetActive(true);
				curPanel = Sp_Good2;
			}
			else
			{
				Sp_Good3.SetActive(true);
				curPanel = Sp_Good3;
			}
		}

		/// <summary>
		/// 顶级offer
		/// </summary>
		public void Good1B()
		{
			CharacterHandleWeapon curWeapon = player.GetComponent<CharacterHandleWeapon>();
			string curName = curWeapon.InitialWeapon.name;
			int curStage = int.Parse(curName.Substring(curName.Length - 1, 1));
			if (curStage < 3)	// 未升满
			{
				LU_SkillUp();
			}
			else
			{
				// 攻击力+50
				LU_AddAttack(50);
			}
			CloseSpecialEvent();
		}
		
		/// <summary>
		/// 升职加薪
		/// </summary>
		public void Good2B()
		{
			LU_AddXMoney(100);
			CloseSpecialEvent();
		}
		
		/// <summary>
		/// 天降馅饼
		/// A选项
		/// </summary>
		public void Good3BA()
		{
			LU_SubXDashCD(player.GetComponent<CharacterDash>().DashCooldown);	// 闪避cd为0
			CloseSpecialEvent();
		}
		
		/// <summary>
		/// 天降馅饼
		/// B选项
		/// </summary>
		public void Good3BB()
		{
			// 攻击力+75
			LU_AddAttack(75);
			CloseSpecialEvent();
		}
		
		/// <summary>
		/// 天降馅饼
		/// C选项
		/// </summary>
		public void Good3BC()
		{
			// 最大+3
			LU_AddXHP(3);
			// 回满血量
			player.GetComponent<Health>().GetHealth(PlayerManager.Instance.hp, gameObject);
			CloseSpecialEvent();
		}

		/// <summary>
		/// roll 中等事件
		/// </summary>
		private void RollNormal()
		{
			float rd = Random.Range(0.0f, 1.0f);
			if (rd <= 0.5f)
			{
				Sp_Normal1.SetActive(true);
				curPanel = Sp_Normal1;
				curSubPanel = Sp_Normal1.transform.GetChild(0).gameObject;
				curSubPanel.SetActive(true);
			}
			else
			{
				Sp_Normal2.SetActive(true);
				curPanel = Sp_Normal2;
				curSubPanel = Sp_Normal2.transform.GetChild(0).gameObject;
				curSubPanel.SetActive(true);
			}
		}

		/// <summary>
		/// offer选择？
		/// A公司（B公司通用）
		/// 是否跳槽
		/// 选项A：留下
		/// </summary>
		public void Normal1BA(GameObject sub)
		{
			curSubPanel = sub;
			sub.SetActive(true);
			sub.transform.parent.GetChild(0).gameObject.SetActive(false);
		}

		/// <summary>
		/// offer选择
		/// A公司结局
		/// </summary>
		public void Normal1BAB()
		{
			float curhp = player.GetComponent<Health>().CurrentHealth;
			player.GetComponent<Health>().GetHealth(curhp * 0.3f, gameObject);
			// 降低25点攻击
			LU_AddAttack(-25);
			CloseSpecialEvent();
		}

		/// <summary>
		/// offer选择
		/// B公司结局
		/// </summary>
		public void Normal1BBB()
		{
			float curhp = player.GetComponent<Health>().CurrentHealth;
			LU_AddXMoney(150);	// +150金币
			player.GetComponent<Health>().GetHealth(-curhp * 0.5f, gameObject);	// -50%当前血量
			CloseSpecialEvent();
		}

		/// <summary>
		/// 是否跳槽
		/// 选项B：离开
		/// </summary>
		public void Normal2BB(GameObject myself)
		{
			Transform father = myself.transform.parent;
			// 随机roll一个可能的
			float rd = Random.Range(0.0f, 1.0f);
			if (rd <= 0.5f)
			{
				curSubPanel = father.GetChild(2).gameObject;
				curSubPanel.SetActive(true);
			}
			else
			{
				curSubPanel = father.GetChild(3).gameObject;
				curSubPanel.SetActive(true);
			}
			father.GetChild(0).gameObject.SetActive(false);	// 关闭自己
		}

		/// <summary>
		/// 是否跳槽
		/// 选项A：留下
		/// 的子A：继续留下
		/// </summary>
		public void Normal2BA_A()
		{
			player = GameManager.Instance.PersistentCharacter;
			// -40%
			float curhp = player.GetComponent<Health>().CurrentHealth;
			player.GetComponent<Health>().GetHealth(-curhp * 0.4f, gameObject);
			// cd-1
			LU_SubXDashCD(1);
			CloseSpecialEvent();
		}

		/// <summary>
		/// 是否跳槽
		/// 选项A：留下
		/// 的子B：离开
		/// </summary>
		public void Normal2BA_B()
		{
			player = GameManager.Instance.PersistentCharacter;
			float curhp = player.GetComponent<Health>().CurrentHealth;
			player.GetComponent<Health>().GetHealth(curhp * 0.3f, gameObject);	// 回复当前的30%
			CloseSpecialEvent();
		}

		/// <summary>
		/// 是否跳槽
		/// 选项B：离开
		/// 结局1
		/// </summary>
		public void Normal2BB_()
		{
			player = GameManager.Instance.PersistentCharacter;
			float curhp = player.GetComponent<Health>().CurrentHealth;
			player.GetComponent<Health>().GetHealth(-curhp * 0.3f, gameObject);	// 扣除当前的30%
			LU_AddXMoney(150);	//+150 money
			CloseSpecialEvent();
		}

		/// <summary>
		/// 是否跳槽
		/// 选项B：离开
		/// 结局2
		/// </summary>
		public void Normal2BB__()
		{
			// 攻击力+50
			LU_AddAttack(50);
			LU_AddXMoney(-50);	// -50 money
			CloseSpecialEvent();
		}

		/// <summary>
		/// roll 坏事件
		/// </summary>
		private void RollBad()
		{
			Sp_Bad.SetActive(true);
			curPanel = Sp_Bad;
		}

		/// <summary>
		/// 坏事件
		/// 结局
		/// </summary>
		public void BadBA()
		{
			player = GameManager.Instance.PersistentCharacter;
			float curhp = player.GetComponent<Health>().CurrentHealth;
			player.GetComponent<Health>().GetHealth(-curhp * 0.8f, gameObject);	// 扣除当前的80%
			// 攻击-25
			CloseSpecialEvent();
		}

		/// <summary>
		/// Sets the ammo displays active or not
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void SetAmmoDisplays(bool state, string playerID, int ammoDisplayID)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay != null)
				{ 
					if ((ammoDisplay.PlayerID == playerID) && (ammoDisplay.AmmoDisplayID == ammoDisplayID))
					{
						ammoDisplay.gameObject.SetActive(state);
					}					
				}
			}
		}

		/// <summary>
		/// Sets the time splash.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, turns the timesplash on.</param>
		public virtual void SetTimeSplash(bool state)
		{
			if (TimeSplash != null)
			{
				TimeSplash.SetActive(state);
			}
		}
		
		/// <summary>
		/// Sets the text to the game manager's points.
		/// </summary>
		public virtual void RefreshPoints()
		{
			if (PointsText!= null)
			{ 
				PointsText.text = GameManager.Instance.Points.ToString(PointsPattern);
			}
		}

		/// <summary>
		/// Updates the health bar.
		/// </summary>
		/// <param name="currentHealth">Current health.</param>
		/// <param name="minHealth">Minimum health.</param>
		/// <param name="maxHealth">Max health.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void UpdateHealthBar(float currentHealth,float minHealth,float maxHealth,string playerID)
		{
			if (HealthBars == null) { return; }
			if (HealthBars.Length <= 0)	{ return; }

			foreach (MMProgressBar healthBar in HealthBars)
			{
				if (healthBar == null) { continue; }
				if (healthBar.PlayerID == playerID)
				{
					healthBar.UpdateBar(currentHealth,minHealth,maxHealth);
				}
			}

		}

		/// <summary>
		/// 随血量设置头像遮罩
		/// </summary>
		/// <param name="a"></param>
		public void SetAvaterMusk(float a)
		{
			avaterMusk.color = new Color(255,255,255,Mathf.Abs(a));
		}

		/// <summary>
		/// 切换头像。1：初始；2：死亡；3：开心
		/// </summary>
		/// <param name="idx"></param>
		public void SwitchAvater(int idx)
		{
			avater.sprite = (Sprite)sprites[idx + 1];
		}
		
		/// <summary>
		/// 设置弹反条
		/// </summary>
		/// <param name="v"></param>
		public void SetBounceBar(float v)
		{
			bounceBar.value = v;
		}

		/// <summary>
		/// Updates the (optional) ammo displays.
		/// </summary>
		/// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
		/// <param name="totalAmmo">Total ammo.</param>
		/// <param name="maxAmmo">Max ammo.</param>
		/// <param name="ammoInMagazine">Ammo in magazine.</param>
		/// <param name="magazineSize">Magazine size.</param>
		/// <param name="playerID">Player I.</param>
		/// <param name="displayTotal">If set to <c>true</c> display total.</param>
		public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, string playerID, int ammoDisplayID, bool displayTotal)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay == null) { return; }
				if ((ammoDisplay.PlayerID == playerID) && (ammoDisplay.AmmoDisplayID == ammoDisplayID))
				{
					ammoDisplay.UpdateAmmoDisplays (magazineBased, totalAmmo, maxAmmo, ammoInMagazine, magazineSize, displayTotal);
				}    
			}
		}

		/// <summary>
		/// Sets the level name in the HUD
		/// </summary>
		public virtual void SetLevelName(string name)
		{
			if (LevelText != null)
			{ 
				LevelText.text=name;
			}
		}

		/// <summary>
		/// 每次打开升级面板时更新数值
		/// </summary>
		public void UpdateLUManuel()
		{
			money_text.text = PlayerManager.Instance.money.ToString();
			lucky_text.text = PlayerManager.Instance.lucky.ToString();
			attack_text.text = PlayerManager.Instance.attack.ToString();
			player_text.text = PlayerManager.Instance.playerName;
			cd_text.text = player.GetComponent<CharacterDash>().DashCooldown.ToString();
			hp_bar.value = PlayerManager.Instance.hp;
			// buttons
			optionA.onClick.RemoveAllListeners();
			optionB.onClick.RemoveAllListeners();
			optionA.onClick.AddListener(CloseLevelUp);
			optionB.onClick.AddListener(CloseLevelUp);
			Sprite[] btsA;
			Sprite[] btsB;
			SpriteState ssA = new SpriteState();
			SpriteState ssB = new SpriteState();
			TextMeshProUGUI bttA = optionA.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI bttB = optionB.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			// 技能图标：可以用三元表达式判断替换
			switch (LevelChooseManager.Instance.stage)
			{
				case 1:
					btsA = Resources.LoadAll<Sprite>("LU/血量加1");
					btsB = Resources.LoadAll<Sprite>("LU/攻击加");
					optionA.gameObject.GetComponent<Image>().sprite = btsA[0];
					optionB.gameObject.GetComponent<Image>().sprite = btsB[0];
					ssA.highlightedSprite = btsA[1];
					ssB.highlightedSprite = btsB[1];
					optionA.spriteState = ssA;
					optionB.spriteState = ssB;
					bttA.text = "提升1格血条";
					bttB.text = "攻击升级";
					optionA.onClick.AddListener(()=>LU_AddXHP(1));
					optionB.onClick.AddListener(LU_SkillUp);
					break;
				case 2:
					btsB = Resources.LoadAll<Sprite>("LU/血量加1");
					btsA = Resources.LoadAll<Sprite>("LU/闪避");
					optionA.gameObject.GetComponent<Image>().sprite = btsA[0];
					optionB.gameObject.GetComponent<Image>().sprite = btsB[0];
					ssA.highlightedSprite = btsA[1];
					ssB.highlightedSprite = btsB[1];
					optionA.spriteState = ssA;
					optionB.spriteState = ssB;
					bttA.text = "闪避CD减少1S";
					bttB.text = "提升1格血条";
					optionA.onClick.AddListener(()=>LU_SubXDashCD(1));
					optionB.onClick.AddListener(()=>LU_AddXHP(1));
					break;
				case 3:
					btsA = Resources.LoadAll<Sprite>("LU/血量加1");
					btsB = Resources.LoadAll<Sprite>("LU/奇遇");
					optionA.gameObject.GetComponent<Image>().sprite = btsA[0];
					optionB.gameObject.GetComponent<Image>().sprite = btsB[0];
					ssA.highlightedSprite = btsA[1];
					ssB.highlightedSprite = btsB[1];
					optionA.spriteState = ssA;
					optionB.spriteState = ssB;
					bttA.text = "提升1格血条";
					bttB.text = "获得一次额外奇遇机会";
					optionA.onClick.AddListener(()=>LU_AddXHP(1));
					optionB.onClick.AddListener(LU_OpenSpecialEvent);
					break;
				case 4:
					btsA = Resources.LoadAll<Sprite>("LU/吸血");
					btsB = Resources.LoadAll<Sprite>("LU/渴血");
					optionA.gameObject.GetComponent<Image>().sprite = btsA[0];
					optionB.gameObject.GetComponent<Image>().sprite = btsB[0];
					ssA.highlightedSprite = btsA[1];
					ssB.highlightedSprite = btsB[1];
					optionA.spriteState = ssA;
					optionB.spriteState = ssB;
					bttA.text = "【大招】血魔流-吸干你的血。击杀敌人触发吸血技能。击杀成功恢复X%血量，累计杀害敌人血量达N，触发S秒无敌状态。";
					bttB.text = "【大招】低血流-打不倒的小强。当血量低于X%时，闪避无CD且主动攻击伤害翻倍。累计杀害敌人血量达N，S秒主动攻击一击毙命。";
					// 大招-血魔流
					// 大招-低血流
					break;
				case 5:
					btsA = Resources.LoadAll<Sprite>("LU/攻击加");
					btsB = Resources.LoadAll<Sprite>("LU/血量加2");
					optionA.gameObject.GetComponent<Image>().sprite = btsA[0];
					optionB.gameObject.GetComponent<Image>().sprite = btsB[0];
					ssA.highlightedSprite = btsA[1];
					ssB.highlightedSprite = btsB[1];
					optionA.spriteState = ssA;
					optionB.spriteState = ssB;
					bttA.text = "攻击升级";
					bttB.text = "提升2格血条";
					optionA.onClick.AddListener(LU_SkillUp);
					optionB.onClick.AddListener(()=>LU_AddXHP(2));
					break;
				case 6:
					btsA = Resources.LoadAll<Sprite>("LU/加货币");
					btsB = Resources.LoadAll<Sprite>("LU/奇遇");
					optionA.gameObject.GetComponent<Image>().sprite = btsA[0];
					optionB.gameObject.GetComponent<Image>().sprite = btsB[0];
					ssA.highlightedSprite = btsA[1];
					ssB.highlightedSprite = btsB[1];
					optionA.spriteState = ssA;
					optionB.spriteState = ssB;
					bttA.text = "发财了，获得5货币";
					bttB.text = "获得一次额外奇遇机会";
					optionA.onClick.AddListener(()=>LU_AddXMoney(5));
					optionB.onClick.AddListener(LU_OpenSpecialEvent);
					break;
			}
		}

		/// <summary>
		/// Level Up 事件
		/// HP+x
		/// </summary>
		public void LU_AddXHP(int x)
		{
			PlayerManager.Instance.AddHP(x);
		}

		/// <summary>
		/// Level Up 事件
		/// 闪避cd-x
		/// </summary>
		/// <param name="x"></param>
		public void LU_SubXDashCD(float x)
		{
			player.GetComponent<CharacterDash>().DashCooldown = (player.GetComponent<CharacterDash>().DashCooldown - x) >= 0 ? 
				player.GetComponent<CharacterDash>().DashCooldown - x : player.GetComponent<CharacterDash>().DashCooldown;
		}

		/// <summary>
		/// Level Up 事件
		/// 触发一次奇遇
		/// </summary>
		public void LU_OpenSpecialEvent()
		{
			OpenSpecialEvent();
		}

		/// <summary>
		/// Level Up 事件
		/// 货币+5 (暂时)
		/// </summary>
		public void LU_AddXMoney(int x)
		{
			PlayerManager.Instance.AddMoney(x);
		}

		/// <summary>
		/// Level Up 事件 | 商店函数
		/// 拿到Handle Weapon的武器名字，根据名字判断升级
		/// </summary>
		public void LU_SkillUp()
		{
			CharacterHandleWeapon curWeapon = player.GetComponent<CharacterHandleWeapon>();
			string curName = curWeapon.InitialWeapon.name;
			int curStage = int.Parse(curName.Substring(curName.Length - 1, 1));
			PlayerManager.Instance.attack += 25;
			switch (curStage)
			{
				case 1:
					curWeapon.InitialWeapon = Resources.Load<MeleeWeapon>("Prefabs/MeleeWeapon_2");
					curWeapon.Setup();
					break;
				case 2:
					curWeapon.InitialWeapon = Resources.Load<MeleeWeapon>("Prefabs/MeleeWeapon_3");
					curWeapon.Setup();
					break;
				case 3:
					break;
			}
		}

		/// <summary>
		/// 增加攻击力
		/// </summary>
		public void LU_AddAttack(int x)
		{
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
					a.MinDamageCaused = (a.MinDamageCaused + x > 25) ? a.MinDamageCaused + x : 25;
					a.MaxDamageCaused = (a.MaxDamageCaused + x > 25) ? a.MaxDamageCaused + x : 25;
				}
			}
		}

		/// <summary>
		/// 商店事件
		/// 更新购物所需金币
		/// </summary>
		public void UpdateNeedMoneyText(int i)
		{
			neededMoneyText.text = i.ToString();
		}

		/// <summary>
		/// 商店函数
		/// 血包效果
		/// </summary>
		public void S_Add3HP()
		{
			player.GetComponent<Health>().GetHealth(3 * 20, gameObject);
			// PlayerManager.Instance.AddHP(3);
		}

		/// <summary>
		/// 商店函数
		/// 开启玩家的主动大招
		/// </summary>
		public void S_BigSkill()
		{
			PlayerManager.Instance.hasBigSkill = true;
		}

		/// <summary>
		/// 商店函数
		/// 增加5点幸运（待定
		/// </summary>
		public void S_Add1Lucky()
		{
			PlayerManager.Instance.AddLcuky(5);
		}

		/// <summary>
		/// When we catch a level name event, we change our level's name in the GUI
		/// </summary>
		/// <param name="levelNameEvent"></param>
		public virtual void OnMMEvent(LevelNameEvent levelNameEvent)
		{
			SetLevelName(levelNameEvent.LevelName);
		}

		public virtual void OnMMEvent(ControlsModeEvent controlsModeEvent)
		{
			SetMobileControlsActive(controlsModeEvent.Status, controlsModeEvent.MovementControl);
		}

		/// <summary>
		/// On enable, we start listening to events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<LevelNameEvent>();
			this.MMEventStartListening<ControlsModeEvent>();
		}

		/// <summary>
		/// On disable, we stop listening to events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<LevelNameEvent>();
			this.MMEventStopListening<ControlsModeEvent>();
		}
	}
}