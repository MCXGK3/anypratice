using Galaxy.Api;
using InvulnerabilityIndicator;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace anypratice
{
    public class anypratice : Mod, IGlobalSettings<settings>, IMenuMod
    {
        internal static anypratice Instance;
        private bool test = true;
        private bool test2 = false;
        private bool test3 = false;
        private bool r0 = false;
        private bool r1 = false;
        private int damage=-5;
        public settings _set = new();
        private Texture2D radianceSkin0;
        private Texture2D radianceSkin1;
        public List<GameObject> CH = new();
        public bool ToggleButtonInsideMenu => true;
        public anypratice() : base("anypratice")
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
            ModHooks.AfterPlayerDeadHook += carefreeset1;
            ModHooks.CharmUpdateHook += carefreeset;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += sceneChanged;
            var stream = typeof(anypratice).Assembly.GetManifestResourceStream("anypratice.Rad0.png");
            if (stream != null)
            {
                byte[] array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);
                stream.Dispose();
                radianceSkin0 = new(1, 1);
                radianceSkin0.LoadImage(array, true);
                r0 = true;
            }
            stream = typeof(anypratice).Assembly.GetManifestResourceStream("anypratice.Rad1png");
            if (stream != null)
            {
                byte[] array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);
                stream.Close();
                radianceSkin1 = new(1, 1);
                radianceSkin1.LoadImage(array, true);
                r1 = true;
            }
        }

        private void carefreeset(PlayerData data, HeroController controller)
        {
            carefreeset1();
            return ;
        }

        private void sceneChanged(Scene arg0, Scene arg1)
        {
            if (_set.on)
            {
                if (_set.carereset)
                {
                    bool flag = arg1.name == "GG_Workshop";
                    if (flag) { carefreeset1(); }
                }
            }
        }

        private void carefreeset1()
        {
            if (_set.on)
            {
                if (_set.carereset)
                {
                    HeroController instance = HeroController.instance;
                    bool flag2 = instance != null;
                    if (flag2)
                    {
                        Log("OK");
                        ReflectionHelper.SetField<HeroController, int>(instance, "hitsSinceShielded", 7);
                    }
                }
            }
            return;
        }

        private void baldurfix()
        {
            if (_set.on)
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
                    if (!test2)
                    {
                        test2 = true;
                        GameObject.Find("Knight").AddComponent<Indicator>();
                    }
                }
                else
                {
                    if (test2)
                    {
                        Indicator indicator = GameObject.Find("Knight").GetComponent<Indicator>();
                        if (indicator != null) GameObject.Destroy(indicator);
                        test2 = false;
                    }
                }
            }
        }

        private void fsm_on(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (_set.on)
            {
                if (self.gameObject.name == "Absolute Radiance" && self.FsmName == "Control")
                {
                    /*if (_set.cycle != 0)
                    {
                        Log("cycle ok");
                        self.gameObject.AddComponent<Cycle>();
                    }*/
                    if (_set.beamlock)
                    {
                        Log("beamlock ok");
                        self.gameObject.LocateMyFSM("Attack Commands").GetAction<RandomFloat>("Aim", 4).min = 0f;
                        self.gameObject.LocateMyFSM("Attack Commands").GetAction<RandomFloat>("Aim", 4).max = 0f;
                    }
                    if (_set.skin)
                    {
                        Log("skin ok");
                         /*if (r0 && r1)
                        {
                            //FindChild(self.gameObject);
                            //foreach (GameObject go in CH) 
                            
                                Material[] materials = self.GetComponent<tk2dSprite>().Collection.materials;
                                if (test)
                                {
                                    foreach (Material mat in materials)
                                    {
                                        TextureUtils.WriteTextureToFile(mat.mainTexture, "C:\\Users\\shownyoung\\Desktop\\temp\\" + mat.mainTexture.name + ".png");
                                    }
                                }
                            
                            if (r0) materials[0].mainTexture = radianceSkin0;
                            if (r1) materials[1].mainTexture = radianceSkin1;
                        }*/
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
                /*  if (self.gameObject.name == "Abyss Pit"&&self.FsmName=="Ascend")
                  {
                      if (_set.abyssremove)
                      {
                          self.GetState("Idle").InsertCustomAction(() => { self.gameObject.SetActive(false); }, 0);
                      }
                  }*/

               /* if(self.FsmName == "Superdash")
                {
                    Log("OK");
                }*/
                
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
                Name = "辐光皮肤",
                Description = "皮肤尚无",
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => _set.skin = i == 0,
                Loader = () => _set.skin ? 0 : 1
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
            /*menus.Add(
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
       );*/
            /* menus.Add(
             new()
             {
                 Name = "超冲停滞",
                 Description = "帅",
                 Values = new string[]
                 {
                     Language.Language.Get("MOH_ON", "MainMenu"),
                     Language.Language.Get("MOH_OFF", "MainMenu"),
                 },
                 Saver = i => _set.superdash = i == 0,
                 Loader = () => _set.superdash ? 0 : 1
             }
         );*/
            return menus;
        }

        void FindChild(GameObject child)
        {




            //利用for循环 获取物体下的全部子物体
            for (int c = 0; c < child.transform.childCount; c++)
            {
                //如果子物体下还有子物体 就将子物体传入进行回调查找 直到物体没有子物体为止
                if (child.transform.GetChild(c).childCount > 0)
                {
                    FindChild(child.transform.GetChild(c).gameObject);

                }
                CH.Add(child.transform.GetChild(c).gameObject);


            }
        }
    }
}