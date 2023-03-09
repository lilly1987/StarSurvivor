using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BepInPluginSample
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.0")]
    public class Sample : BaseUnityPlugin
    {
        #region GUI
        public static ManualLogSource logger;

        static Harmony harmony;

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isGUIOnKey;
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> isOpenKey;

        private ConfigEntry<bool> isGUIOn;
        private ConfigEntry<bool> isOpen;
        private ConfigEntry<float> uiW;
        private ConfigEntry<float> uiH;

        public int windowId = 542;
        public Rect windowRect;

        public string title = "";
        public string windowName = ""; // 변수용 
        public string FullName = "Plugin"; // 창 펼쳤을때
        public string ShortName = "P"; // 접었을때

        GUILayoutOption h;
        GUILayoutOption w;
        public Vector2 scrollPosition;
        #endregion

        #region 변수
        // =========================================================

        // private static ConfigEntry<bool> hpNotChg;
        // private static ConfigEntry<float> uiW;
        // private static ConfigEntry<float> xpMulti;
        private static ConfigEntry<bool> noPlayerHealthTakeDamage;
        private static ConfigEntry<bool> isPickup;
        private static ConfigEntry<bool> noDeductGold;
        private static ConfigEntry<bool> rerolluses;
        private static ConfigEntry<bool> upGainExp;
        private static ConfigEntry<bool> isUpgradeable;
        private static ConfigEntry<int> upGainExpVal;
        // =========================================================
        #endregion

        public void Awake()
        {
            #region GUI
            logger = Logger;
            Logger.LogMessage("Awake");

            isGUIOnKey = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키
            isOpenKey = Config.Bind("GUI", "isOpenKey", new KeyboardShortcut(KeyCode.KeypadPeriod));// 이건 단축키

            isGUIOn = Config.Bind("GUI", "isGUIOn", true);
            isOpen = Config.Bind("GUI", "isOpen", true);
            isOpen.SettingChanged += IsOpen_SettingChanged;
            uiW = Config.Bind("GUI", "uiW", 300f);
            uiH = Config.Bind("GUI", "uiH", 600f);

            if (isOpen.Value)
                windowRect = new Rect(Screen.width - 65, 0, uiW.Value, 800);
            else
                windowRect = new Rect(Screen.width - uiW.Value, 0, uiW.Value, 800);

            IsOpen_SettingChanged(null, null);
            #endregion

            #region 변수 설정
            // =========================================================

            noPlayerHealthTakeDamage = Config.Bind("game", "noPlayerHealthTakeDamage", true);
            isPickup = Config.Bind("game", "isPickup", true);
            noDeductGold = Config.Bind("game", "noDeductGold", true);
            rerolluses = Config.Bind("game", "rerolluses", true);
            upGainExp = Config.Bind("game", "upGainExp", true);
            isUpgradeable = Config.Bind("game", "isUpgradeable", true);
            upGainExpVal = Config.Bind("game", "upGainExpVal", 10);

            // =========================================================
            #endregion
        }

        #region GUI
        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{windowRect.x} ");
            if (isOpen.Value)
            {
                title = isGUIOnKey.Value.ToString() + "," + isOpenKey.Value.ToString();
                h = GUILayout.Height(uiH.Value);
                w = GUILayout.Width(uiW.Value);
                windowName = FullName;
                windowRect.x -= (uiW.Value - 64);
            }
            else
            {
                title = "";
                h = GUILayout.Height(40);
                w = GUILayout.Width(60);
                windowName = ShortName;
                windowRect.x += (uiW.Value - 64);
            }
        }
        #endregion

        public void OnEnable()
        {
            Logger.LogWarning("OnEnable");
            // 하모니 패치
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(Sample));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public void Update()
        {
            #region GUI
            if (isGUIOnKey.Value.IsUp())// 단축키가 일치할때
            {
                isGUIOn.Value = !isGUIOn.Value;
            }
            if (isOpenKey.Value.IsUp())// 단축키가 일치할때
            {
                if (isGUIOn.Value)
                {
                    isOpen.Value = !isOpen.Value;
                }
                else
                {
                    isGUIOn.Value = true;
                    isOpen.Value = true;
                }
            }
            #endregion
        }

        #region GUI
        public void OnGUI()
        {
            if (!isGUIOn.Value)
                return;

            // 창 나가는거 방지
            windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + 4, Screen.width - 4);
            windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + 4, Screen.height - 4);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, windowName, w, h);
        }
        #endregion

        public virtual void WindowFunction(int id)
        {
            #region GUI
            GUI.enabled = true; // 기능 클릭 가능

            GUILayout.BeginHorizontal();// 가로 정렬
                                        // 라벨 추가
                                        //GUILayout.Label(windowName, GUILayout.Height(20));
                                        // 안쓰는 공간이 생기더라도 다른 기능으로 꽉 채우지 않고 빈공간 만들기
            if (isOpen.Value) GUILayout.Label(title);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { isOpen.Value = !isOpen.Value; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { isGUIOn.Value = false; }
            GUI.changed = false;

            GUILayout.EndHorizontal();// 가로 정렬 끝

            if (!isOpen.Value) // 닫혔을때
            {
            }
            else // 열렸을때
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
                #endregion
                
                #region 여기에 GUI 항목 작성
                // =========================================================

                if (GUILayout.Button($"noPlayerHealthTakeDamage {noPlayerHealthTakeDamage.Value}")) { noPlayerHealthTakeDamage.Value = !noPlayerHealthTakeDamage.Value; }
                if (GUILayout.Button($"isPickup {isPickup.Value}")) { isPickup.Value = !isPickup.Value; }
                if (GUILayout.Button($"noDeductGold {noDeductGold.Value}")) { noDeductGold.Value = !noDeductGold.Value; }
                if (GUILayout.Button($"rerolluses {rerolluses.Value}")) { rerolluses.Value = !rerolluses.Value; }
                if (GUILayout.Button($"isUpgradeable {isUpgradeable.Value}")) { isUpgradeable.Value = !isUpgradeable.Value; }
                if (GUILayout.Button($"campaign.gold=1000000")) { Singleton<SaveController>.i.data.campaign.gold=1000000; }
                if (GUILayout.Button($"select upgrade")) { Singleton<UIManager>.i.menus.ShowMenu("select upgrade", true); }
                
                if (GUILayout.Button($"upGainExp {upGainExp.Value}")) { upGainExp.Value = !upGainExp.Value; }

                if (GUILayout.Button($"UpgradeAll")) { Singleton<SaveController>.i.data.UpgradeAll(); }
                if (GUILayout.Button($"UnlockAll")) { Singleton<SaveController>.i.data.UnlockAll(); }
                if (GUILayout.Button($"card.level = 1 all decks")) { 

                    foreach (var deck in Singleton<SaveController>.i.data.decks)
                    {
                        foreach (var card in deck.cards)
                        {
                            card.level = 1;
                        }
                    }
                    


                }

                GUILayout.BeginHorizontal();
                GUILayout.Label($"ammoMulti {upGainExpVal.Value}");
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { upGainExpVal.Value += 1; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { upGainExpVal.Value -= 1; }
                GUILayout.EndHorizontal();

                // =========================================================
                #endregion

                #region GUI
                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
            #endregion
        }


        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
        }

        #region Harmony
        // ====================== 하모니 패치 샘플 ===================================
        //public override void TakeDamage(float damage)
        [HarmonyPatch(typeof(PlayerHealth), "TakeDamage", typeof(float))]
        [HarmonyPrefix]
        public static void TakeDamage(ref float damage)
        {
            if (!noPlayerHealthTakeDamage.Value)
            {
                return ;
            }
            damage = 0f;
           
        }

        [HarmonyPatch(typeof(PickupVacuum), "Refresh")]
        [HarmonyPostfix]
        public static void Refresh(PickupVacuum __instance)
        {
            if (!isPickup.Value)
            {
                return;
            }
            logger.LogWarning($"PickupVacuum");
            __instance.pickupRange = 99f;
        }
        
        [HarmonyPatch(typeof(UIUpgrades), "DeductGold")]
        [HarmonyPostfix]
        public static void DeductGold(ref int value)
        {
            if (!noDeductGold.Value)
            {
                return;
            }
            logger.LogWarning($"PickupVacuum");
            value = 0;
        }

        
        [HarmonyPatch(typeof(UIUpgrades), "SetRerollCost")]
        [HarmonyPrefix]
        public static void SetRerollCost(UIUpgrades __instance)
        {
            if (!rerolluses.Value)
            {
                return;
            }
            logger.LogWarning($"SetRerollCost");
            __instance.rerolluses = 0;
        }
        
        
        [HarmonyPatch(typeof(PlayerStats), "GainExp")]
        [HarmonyPrefix]
        public static void GainExp(ref float amount)
        {
            if (!upGainExp.Value)
            {
                return;
            }
            logger.LogWarning($"SetRerollCost");
            amount*= upGainExpVal.Value;
        }
        
        
        [HarmonyPatch(typeof(UIDeckViewer), "SelectCard")]
        [HarmonyPrefix]
        public static void SelectCard(UIDeckViewer __instance, UIDeckCard card)
        {
            if (!isUpgradeable.Value)
            {
                return;
            }
            logger.LogWarning($"SelectCard");
            if (card .inst.isUpgradeable())
            {
                card.inst.data.level++;
            }
        }
        

        /*
        [HarmonyPatch(typeof(PickupExp), "Setup")]
        [HarmonyPostfix]
        public static void Setup(PickupExp __instance)
        {
            if (!isPickup.Value)
            {
                return ;
            }
            logger.LogWarning($"PickupExp");
            __instance.Pickup();
        }
        */
        /*
        [HarmonyPatch(typeof(PickupGold), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable(PickupGold __instance)
        {
            if (!isPickup.Value)
            {
                return;
            }
            logger.LogWarning($"PickupGold");
            __instance.Pickup();
        }        
        
        [HarmonyPatch(typeof(PickupFreeUpgrade), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable(PickupFreeUpgrade __instance)
        {
            if (!isPickup.Value)
            {
                return;
            }
            logger.LogWarning($"PickupFreeUpgrade");
            __instance.Pickup();
        }
        */
        /*
        [HarmonyPatch(typeof(PickupObject), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable(PickupObject __instance)
        {
            if (!isPickup.Value)
            {
                return;
            }
            logger.LogWarning($"PickupObject");
            __instance.Pickup();
        }

         
        [HarmonyPatch(typeof(XPPicker), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void XPPickerCtor(XPPicker __instance, ref float ___pickupRadius)
        {
            //logger.LogWarning($"XPPicker.ctor {___pickupRadius}");
            ___pickupRadius = pickupRadius.Value;
        }
                 
        [HarmonyPatch(typeof(XPPicker), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void XPPickerCtor(XPPicker __instance, ref float ___pickupRadius)
        {
            //logger.LogWarning($"XPPicker.ctor {___pickupRadius}");
            ___pickupRadius = pickupRadius.Value;
        }

        [HarmonyPatch(typeof(AEnemy), "DamageMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetDamageMult(ref float __0)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            __0 *= eDamageMult.Value;
        }
        */
        // =========================================================
        #endregion
    }
}
