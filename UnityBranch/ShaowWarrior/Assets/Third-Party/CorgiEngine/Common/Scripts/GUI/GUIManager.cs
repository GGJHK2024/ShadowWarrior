using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using TMPro;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

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

		[Header("Settings")]

		/// the pattern to apply when displaying the score
		[Tooltip("the pattern to apply when displaying the score")]
		public string PointsPattern = "000000";

		protected float _initialJoystickAlpha;
		protected float _initialArrowsAlpha;
		protected float _initialButtonsAlpha;
		protected Object[] sprites;	//all ui sliced sprites
		
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
		/// Sets the Level Up.
		/// </summary>
		public virtual void OpenLevelUp()
		{
			if (LevelUpScreen!= null)
			{ 
				LevelUpScreen.SetActive(true);
			}
		}
		
		/// <summary>
		/// Sets the Level Up.
		/// </summary>
		public virtual void CloseLevelUp()
		{
			if (LevelUpScreen!= null)
			{ 
				LevelUpScreen.SetActive(false);
			}
		}
		
		/// <summary>
		/// Sets the Shop.
		/// </summary>
		public virtual void OpenShop()
		{
			if (ShopScreen!= null)
			{ 
				ShopScreen.SetActive(true);
			}
		}
		
		/// <summary>
		/// Sets the Shop.
		/// </summary>
		public virtual void CLoseShop()
		{
			if (ShopScreen!= null)
			{ 
				ShopScreen.SetActive(false);
			}
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
			if (LevelText!= null)
			{ 
				LevelText.text=name;
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
		/// 移速+x
		/// </summary>
		/// <param name="x"></param>
		public void LU_AddXSpeed(int x)
		{
			player.GetComponent<CharacterHorizontalMovement>().WalkSpeed += x;
		}

		/// <summary>
		/// Level Up 事件
		/// 闪避cd-x
		/// </summary>
		/// <param name="x"></param>
		public void LU_SubXDashCD(int x)
		{
			player.GetComponent<CharacterDash>().DashCooldown -= x;
		}

		/// <summary>
		/// Level Up 事件
		/// 触发一次奇遇
		/// </summary>
		public void LU_OpenSpecialEvent()
		{
			//
		}

		/// <summary>
		/// Level Up 事件
		/// 货币+x
		/// </summary>
		public void LU_AddXMoney(int x)
		{
			player.GetComponent<PlayerManager>().AddMoney(x);
		}

		/// <summary>
		/// 拿到Handle Weapon的武器名字，根据名字判断升级
		/// </summary>
		public void LU_SkillUp()
		{
			CharacterHandleWeapon curWeapon = player.GetComponent<CharacterHandleWeapon>();
			string curName = curWeapon.InitialWeapon.name;
			int curStage = int.Parse(curName.Substring(curName.Length - 1, curName.Length));
			switch (curStage)
			{
				case 1:
					curWeapon.InitialWeapon = Resources.Load<MeleeWeapon>("Prefabs/MeleeWeapon_2");
					break;
				case 2:
					curWeapon.InitialWeapon = Resources.Load<MeleeWeapon>("Prefabs/MeleeWeapon_3");
					break;
				case 3:
					break;
			}
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