using InvulnerabilityIndicator;
using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BM = Satchel.BetterMenus;

namespace anypractice
{
    public class anypractice : Mod, IGlobalSettings<settings>, ICustomMenuMod, ITogglableMod
    {
        internal static anypractice Instance;
        public settings _set = new settings();
        public timerhelp _th;
        public int count;

        private bool _knightIndicator;
        private bool _timerUsed;
        private GameObject _timerObject;
        private BM.Menu _mainMenu;
        private BM.Menu _attackMenu;
        private MenuScreen _mainScreen;
        private MenuScreen _attackScreen;

        public bool ToggleButtonInsideMenu => true;

        public anypractice() : base("anypractice") => Instance = this;
        public override string GetVersion() => "0.0.1.0";

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            On.PlayMakerFSM.OnEnable += FsmOn;
            ChoiceHooks.Install();
            ModHooks.HeroUpdateHook += HeroUpdate;
            ModHooks.GetPlayerIntHook += LegacyCost;
            ModHooks.AfterPlayerDeadHook += ResetCarefree;
            ModHooks.CharmUpdateHook += OnCharmUpdate;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        private int LegacyCost(string name, int original) =>
            _set.on && _set.legacycost && name == "charmCost_32" ? 2 : original;

        private void OnCharmUpdate(PlayerData data, HeroController controller) => ResetCarefree();

        private void SceneChanged(Scene previous, Scene current)
        {
            if (_set.on && _set.carereset && current.name == "GG_Workshop") ResetCarefree();
        }

        private void ResetCarefree()
        {
            if (!_set.on || !_set.carereset || HeroController.instance == null) return;
            Log("carefreeRNG Reset OK");
            ReflectionHelper.SetField<HeroController, int>(HeroController.instance, "hitsSinceShielded", 7);
        }

        private void HeroUpdate()
        {
            if (!_set.on)
            {
                RemoveKnightIndicator();
                if (_timerUsed && _th != null) _th.text.text = "";
                return;
            }

            if (_set.tele && Input.GetKeyDown(KeyCode.Delete))
            {
                if (SceneUtils.getCurrentScene().name == "GG_Radiance")
                    GameManager.instance.ChangeToScene("GG_Workshop", "door1", 0f);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("GG_Radiance");
            }

            if (_set.baldurfix && PlayerData.instance.blockerHits < 4 && Input.GetKeyUp(KeyCode.Backspace))
                PlayerData.instance.blockerHits = 4;

            if (_set.legacycost)
            {
                if (PlayerData.instance.charmCost_32 != 2) PlayerData.instance.charmCost_32 = 2;
            }
            else if (PlayerData.instance.charmCost_32 != 3)
            {
                PlayerData.instance.charmCost_32 = 3;
            }

            if (_set.indicator)
            {
                if (!_knightIndicator)
                {
                    GameObject knight = GameObject.Find("Knight");
                    if (knight != null)
                    {
                        knight.AddComponent<Indicator>();
                        _knightIndicator = true;
                    }
                }
            }
            else RemoveKnightIndicator();

            if (_set.timer == 0)
            {
                if (_timerUsed && _th != null) _th.text.text = "";
            }
            else if (_th != null)
            {
                switch (_set.timerColor)
                {
                    case 0: _th.text.color = Color.black; break;
                    case 1: _th.text.color = Color.white; break;
                    case 2: _th.text.color = Color.red; break;
                    case 3: _th.text.color = Color.blue; break;
                    case 4: _th.text.color = Color.green; break;
                }
            }
        }

        private void FsmOn(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (!_set.on || self.gameObject.name != "Absolute Radiance" || self.FsmName != "Control")
            {
                orig(self);
                return;
            }

            bool callOriginal = !_set.radiance;
            try
            {
                if (_set.timer != 0)
                {
                    if (!_timerUsed)
                    {
                        _timerObject = new GameObject("AnyPractice Timer");
                        UnityEngine.Object.DontDestroyOnLoad(_timerObject);
                        _th = _timerObject.GetAddComponent<timerhelp>();
                        _timerUsed = true;
                    }
                    self.gameObject.AddComponent<Timer>();
                }
                if (_set.cycle != 0) self.gameObject.AddComponent<Cycle>();
                if (_set.beamlock)
                {
                    Log("beamlock ok");
                    self.gameObject.LocateMyFSM("Attack Commands").GetAction<RandomFloat>("Aim", 4).min = 0f;
                    self.gameObject.LocateMyFSM("Attack Commands").GetAction<RandomFloat>("Aim", 4).max = 0f;
                }
                if (_set.orbindicator) self.gameObject.AddComponent<radIndicators>();
                if (_set.abyssremove) self.gameObject.AddComponent<abyssremover>();

            }
            catch (Exception e)
            {
                LogError(e);
                callOriginal = true;
            }

            // “任RUA辐光”通过不启用 Control FSM 让辐光保持静止。
            if (callOriginal) orig(self);
        }

        public void OnLoadGlobal(settings loaded)
        {
            _set = loaded ?? new settings();
            if (_set.crSlots == null || _set.crSlots.Length != AttackSequence.SlotCount)
            {
                string[] slots = new string[AttackSequence.SlotCount];
                if (_set.crSlots != null)
                    Array.Copy(_set.crSlots, slots, Math.Min(_set.crSlots.Length, slots.Length));
                _set.crSlots = slots;
            }
        }

        public settings OnSaveGlobal() => _set;
        public void Persist() => SaveGlobalSettings();

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            _mainMenu ??= new BM.Menu(GetName(), MainElements());
            _mainScreen = BM.Blueprints.GetCachedMenuScreen(_mainMenu, modListMenu);
            _attackMenu ??= new BM.Menu("自定义招式", AttackElements());
            _attackScreen = BM.Blueprints.GetCachedMenuScreen(_attackMenu, _mainScreen);
            return _mainScreen;
        }

        private BM.Element[] MainElements()
        {
            var list = new List<BM.Element>
            {
                Bool("总开关", "总开关开启时，其他才有效", v => _set.on = v, () => _set.on, "on"),
                Bool("巴德尔修复", "按下BackSpace键来修复巴德尔之壳", v => _set.baldurfix = v, () => _set.baldurfix, "baldurfix"),
                Bool("任RUA辐光", "辐光将保持不动，只能通过梦门传出", v => _set.radiance = v, () => _set.radiance, "radiance"),
                Bool("辐光瞬移", "Delete键在辐光场地与诸神堂之间传送", v => _set.tele = v, () => _set.tele, "tele"),
                Bool("激光锁头", "P3的激光不再有偏移角度", v => _set.beamlock = v, () => _set.beamlock, "beamlock"),
                Bool("移除虚空", "P2虚空不再上升", v => _set.abyssremove = v, () => _set.abyssremove, "abyssremove"),
                Bool("光球范围显示", "实时显示光球可能出现的范围", v => _set.orbindicator = v, () => _set.orbindicator, "orbindicator"),
                Bool("重置无忧概率", "每次切换场景都会重置无忧概率", v => _set.carereset = v, () => _set.carereset, "carereset"),
                Bool("两槽快劈", "any挑战中唯一允许的旧护符槽位", v => _set.legacycost = v, () => _set.legacycost, "legacycost"),
                Bool("无敌显示", "处于无敌状态时显示绿圈", v => _set.indicator = v, () => _set.indicator, "indicator"),
                Opt("辐光计时", "记录辐光战斗用时", new[] { "关闭", "第一刀到最后一刀", "标题最后一帧到最后一刀" },
                    v => _set.timer = v, () => _set.timer, "timer"),
                Opt("计时颜色", "计时文字颜色", new[] { "黑", "白", "红", "蓝", "绿" },
                    v => _set.timerColor = v, () => _set.timerColor, "timer_color"),
                BM.Blueprints.NavigateToMenu("自定义招式", "辐光招式控制：锁单招 / 锁序列 / P2瞬移点位", () => _attackScreen)
            };
            return list.ToArray();
        }

        private BM.Element[] AttackElements()
        {
            var list = new List<BM.Element>
            {
                Bool("启用", "", v => _set.crOn = v, () => _set.crOn, "cr_on"),
                Opt("P1 配置", "", new[] { "随机（原版）", "锁单招", "锁序列" },
                    v => _set.crModeA1 = (ChoiceMode)v, () => (int)_set.crModeA1, "cr_mode_a1"),
                Opt("P2 配置", "", new[] { "随机（原版）", "锁单招", "锁序列" },
                    v => _set.crModeA2 = (ChoiceMode)v, () => (int)_set.crModeA2, "cr_mode_a2")
            };

            string[] p1 = AttackCatalog.NamesFor(RadPhase.P1);
            string[] p2 = AttackCatalog.NamesFor(RadPhase.P2);
            list.Add(Opt("P1锁定招", "仅 P1 配置=锁单招时生效", p1,
                v => _set.crA1 = p1[v], () => AttackCatalog.IndexOf(p1, _set.crA1), "cr_a1"));
            list.Add(Opt("P2锁定招", "仅 P2 配置=锁单招时生效", p2,
                v => _set.crA2 = p2[v], () => AttackCatalog.IndexOf(p2, _set.crA2), "cr_a2"));
            list.Add(Opt("轮播序列", "", new[] { "关闭（交还原版）", "开启（8 槽轮播）" },
                v => _set.crLoop = v == 1, () => _set.crLoop ? 1 : 0, "cr_loop"));

            string[] slotOptions = AttackCatalog.SlotOptions();
            for (int i = 0; i < AttackSequence.SlotCount; i++)
            {
                int slot = i;
                list.Add(Opt("槽位 " + (slot + 1), "留空则跳到下个槽位", slotOptions,
                    v => _set.crSlots[slot] = slotOptions[v] == AttackCatalog.Empty ? null : slotOptions[v],
                    () => AttackCatalog.IndexOf(slotOptions, _set.crSlots[slot] ?? AttackCatalog.Empty), "cr_slot" + slot));
            }

            list.Add(Bool("允许相同瞬移点", "", v => _set.crTeleRepeat = v, () => _set.crTeleRepeat, "cr_tele_repeat"));
            string[] teleOptions = TeleOptions();
            list.Add(Opt("指定 P2 瞬移点位", "", teleOptions, v => _set.crTelePos = v,
                () => _set.crTelePos < 0 || _set.crTelePos > 10 ? 0 : _set.crTelePos, "cr_tele_pos"));
            return list.ToArray();
        }

        private BM.HorizontalOption Bool(string name, string description, Action<bool> set, Func<bool> get, string id)
        {
            return BM.Blueprints.HorizontalBoolOption(name, description, v => { set(v); Persist(); }, get,
                Language.Language.Get("MOH_ON", "MainMenu"), Language.Language.Get("MOH_OFF", "MainMenu"), id);
        }

        private BM.HorizontalOption Opt(string name, string description, string[] values, Action<int> set, Func<int> get, string id) =>
            new BM.HorizontalOption(name, description, values, v => { set(v); Persist(); }, get, id);

        private static string[] TeleOptions()
        {
            string[] options = new string[11];
            options[0] = "不锁（原版随机）";
            for (int i = 1; i <= 10; i++) options[i] = "第 " + i + " 点";
            return options;
        }

        private void RemoveKnightIndicator()
        {
            if (!_knightIndicator) return;
            GameObject knight = GameObject.Find("Knight");
            Indicator indicator = knight == null ? null : knight.GetComponent<Indicator>();
            if (indicator != null) UnityEngine.Object.Destroy(indicator);
            _knightIndicator = false;
        }

        public void Unload()
        {
            RemoveKnightIndicator();
            ChoiceHooks.Uninstall();
            On.PlayMakerFSM.OnEnable -= FsmOn;
            ModHooks.HeroUpdateHook -= HeroUpdate;
            ModHooks.GetPlayerIntHook -= LegacyCost;
            ModHooks.AfterPlayerDeadHook -= ResetCarefree;
            ModHooks.CharmUpdateHook -= OnCharmUpdate;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
            if (_timerObject != null) UnityEngine.Object.Destroy(_timerObject);
            _timerObject = null;
            _th = null;
            _timerUsed = false;
        }
    }
}
