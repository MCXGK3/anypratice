using Galaxy.Api;
using InvulnerabilityIndicator;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace anypractice
{
    public class anypractice : Mod, IGlobalSettings<settings>, IMenuMod
    {
        internal static anypractice Instance;
        private bool knightIndicator = false;
        public settings _set = new();
        private int i = 0;
        public timerhelp _th;
        private bool timerused=false;
        public bool ToggleButtonInsideMenu => true;
        public anypractice() : base("anypractice")
        {
            Instance = this;
        }
        public override string GetVersion()
        {
            return "0.0.0.1";
        }


        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            
            On.PlayMakerFSM.OnEnable += fsm_on;
            ModHooks.HeroUpdateHook += baldurfix;
            ModHooks.GetPlayerIntHook += LegacyCost;
            ModHooks.AfterPlayerDeadHook += carefreeset1;
            ModHooks.CharmUpdateHook += carefreeset;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += sceneChanged;
        }

        private int LegacyCost(string name, int orig)
        {
            if(name== "charmCost_32")
            {
                if (_set.legacycost)
                {
                    orig = 2;
                }
            }
            return orig;
        }

        private void carefreeset(PlayerData data, HeroController controller)
        {
            carefreeset1();
            return ;
        }

        private void sceneChanged(Scene arg0, Scene arg1)
        {
                if (_set.carereset)
                {
                    bool flag = arg1.name == "GG_Workshop";
                    if (flag) { carefreeset1(); }
                }
        }

        private void carefreeset1()
        {
                if (_set.carereset)
                {
                    HeroController instance = HeroController.instance;
                    bool flag2 = instance != null;
                    if (flag2)
                    {
                        Log("carefreeRNG Reset OK");
                        ReflectionHelper.SetField<HeroController, int>(instance, "hitsSinceShielded", 7);
                    }
                }
            return;
        }

        private void baldurfix()
        {

                if (_set.baldurfix)
                {
                    if (global::PlayerData.instance.blockerHits < 4)
                    {
                        if (Input.GetKeyUp(KeyCode.Backspace))
                        {
                            global::PlayerData.instance.blockerHits = 4;
                        }
                    }
                }
                if (_set.legacycost)
                {
                    if (global::PlayerData.instance.charmCost_32 != 2) PlayerData.instance.charmCost_32 = 2;
                }
                else
                {
                    if (PlayerData.instance.charmCost_32 != 3) PlayerData.instance.charmCost_32 = 3;
                }
                if (_set.indicator)
                {
                    if (!knightIndicator)
                    {
                        knightIndicator = true;
                        GameObject.Find("Knight").AddComponent<Indicator>();
                    }
                }
                else
                {
                    if (knightIndicator)
                    {
                        Indicator indicator = GameObject.Find("Knight").GetComponent<Indicator>();
                        if (indicator != null) GameObject.Destroy(indicator);
                        knightIndicator = false;
                    }
                }
                if (!_set.timer)
                {
                    _th.text.text = "";
                }
        }

        private void fsm_on(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {

                if (self.gameObject.name == "Absolute Radiance" && self.FsmName == "Control")
                {
                    if (_set.timer)
                    {
                        if (!timerused)
                        {
                            GameObject timer = new GameObject();
                            UnityEngine.Object.DontDestroyOnLoad(timer);
                            _th = timer.GetAddComponent<timerhelp>();
                            timerused = true;
                        }
                        self.gameObject.AddComponent<Timer>();
                    }
                    if (_set.cycle != 0)
                    {
                        Log("cycle ok");
                        self.gameObject.AddComponent<Cycle>();
                    }
                    if (_set.beamlock)
                    {
                        Log("beamlock ok");
                        self.gameObject.LocateMyFSM("Attack Commands").GetAction<RandomFloat>("Aim", 4).min = 0f;
                        self.gameObject.LocateMyFSM("Attack Commands").GetAction<RandomFloat>("Aim", 4).max = 0f;
                    }
                    if (_set.orbindicator)
                    {
                        self.gameObject.AddComponent<radIndicators>();
                    }
                    if (_set.abyssremove)
                    {
                        self.gameObject.AddComponent<abyssremover>();
                    }      
                }
            orig(self);
            
        }
        public void OnLoadGlobal(settings settings) => _set = settings;
        public settings OnSaveGlobal() => _set;

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> menus = new();
            menus.Add(
            new()
            {
                Name = "总开关",
                Description = "总开关开启时，其他才有效",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.on = i == 0,
                Loader = () => _set.on ? 0 : 1
            }
            );
            menus.Add(
            new()
            {
                Name = "巴德尔修复",
                Description = "按下BackSpace键来修复巴德尔之壳",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.baldurfix = i == 0,
                Loader = () => _set.baldurfix ? 0 : 1
            }
        );
            menus.Add(
            new()
            {
                Name = "激光锁头",
                Description = "P3的激光不再有偏移角度",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.beamlock = i == 0,
                Loader = () => _set.beamlock ? 0 : 1
            }
        );
            menus.Add(
            new()
            {
                Name = "移除虚空",
                Description = "P2虚空不再上升",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.abyssremove = i == 0,
                Loader = () => _set.abyssremove ? 0 : 1
            }
        );
            menus.Add(
            new()
            {
                Name = "光球范围显示",
                Description = "实时显示光球可能出现的范围",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.orbindicator = i == 0,
                Loader = () => _set.orbindicator ? 0 : 1
            }
        );
            menus.Add(
            new()
            {
                Name = "重置无忧概率",
                Description = "每次切换场景都会重置无忧概率",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.carereset = i == 0,
                Loader = () => _set.carereset ? 0 : 1
            }
        );
            menus.Add(
            new()
            {
                Name = "两槽快劈",
                Description = "any挑战中唯一允许的旧护符槽位",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.legacycost = i == 0,
                Loader = () => _set.legacycost ? 0 : 1
            }
        );
            menus.Add(
            new()
            {
                Name = "无敌显示",
                Description = "处于无敌状态时显示绿圈",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.indicator = i == 0,
                Loader = () => _set.indicator ? 0 : 1
            }
        );
            menus.Add(
           new()
           {
               Name = "阶段选择",
               Description = "辐光初始将有该阶段血量",
               Values = new string[]
               {
                    //Language.Language.Get("MOH_ON", "MainMenu"),
                    //Language.Language.Get("MOH_OFF", "MainMenu"),
                    "关闭",
                    "P2血量",
                    "P3血量"
               },
               Saver = i => _set.cycle= i ,
               Loader = () => _set.cycle
           }
       );
             menus.Add(
             new()
             {
                 Name = "辐光计时",
                 Description = "会记录从第一刀到最后一刀的时间",
                 Values = new string[]
                 {
                     Language.Language.Get("MOH_ON", "MainMenu"),
                     Language.Language.Get("MOH_OFF", "MainMenu"),
                 },
                 Saver = i => _set.timer = i == 0,
                 Loader = () => _set.timer ? 0 : 1
             }
         );
            return menus;
        }

        }
    }
