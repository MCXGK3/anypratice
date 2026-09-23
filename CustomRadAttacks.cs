using System;
using System.Collections.Generic;
using System.IO;
using Modding;
using Newtonsoft.Json;

namespace CustomRadAttacks
{
    public class CustomRadAttacks : Mod, IMenuMod
    {
        internal static CustomRadAttacks Instance;
        internal static CustomRadAttacksSettings Settings = new CustomRadAttacksSettings();

        public override string GetVersion() { return "0.1.0.0"; }

        public CustomRadAttacks() : base("自定义辐光招式")
        {
            Instance = this;
            LoadSettings();
        }

        public override void Initialize(Dictionary<string, Dictionary<string, UnityEngine.GameObject>> preloadedObjects)
        {
            CheckConflicts();
            Log("CustomRadAttacks v" + GetVersion() + " init, enabled=" + Settings.Enabled + ", mode=" + Settings.Mode
                + ", conflicted=" + Conflicted);
            On.HutongGames.PlayMaker.Actions.SendRandomEventV3.OnEnter += ChoiceHooks.HookChoice;
            On.HutongGames.PlayMaker.Actions.SendRandomEvent.OnEnter += ChoiceHooks.HookNailLr;
            On.HutongGames.PlayMaker.Actions.SendRandomEvent.OnEnter += ChoiceHooks.HookTeleport;
        }

        // 互斥判据（design §7.1）不是"也碰了辐光 FSM"，而是"是否争夺同一个决策点"——
        // 即是否改写 SendRandomEventV3 的权重/事件表。本 mod 只发事件，从不读转移表与动作索引，
        // 所以只改转移目标、增删动作、改字段值的 mod 都不冲突。
        internal static bool Conflicted;

        // 2026-09-22：三个候选全部排除，名单刻意留空，不是漏填。
        //   天国余晖 —— 源码核对 + 同日共存实测，无一项偏差
        //   OrbRadiance —— 判定不冲突
        //   AbsRadConfigurableAttacks —— 用户自行处理，本 mod 不介入
        // 机制保留为扩展点：将来真出现改写权重的 mod，把别名填进来即可。
        private static readonly string[][] ConflictingMods =
        {
        };

        private void CheckConflicts()
        {
            try
            {
                // onlyEnabled: 装了但关着的 mod 不算冲突 —— 那种情况它不会碰 FSM
                foreach (IMod mod in ModHooks.GetAllMods(true, false))
                {
                    string name = mod.GetName();
                    string asm = mod.GetType().Assembly.GetName().Name;
                    for (int i = 0; i < ConflictingMods.Length; i++)
                    {
                        if (!Matches(ConflictingMods[i], name) && !Matches(ConflictingMods[i], asm)) continue;
                        Conflicted = true;
                        Settings.Enabled = false;
                        SaveSettings();
                        LogError("检测到冲突 mod「" + name + "」——已自动禁用自定义辐光招式。"
                                 + "两者都接管辐光的招式选择，同时开启行为不可预期。请只留一个。");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("冲突检测失败: " + ex.Message);
            }
        }

        private static bool Matches(string[] aliases, string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            for (int i = 0; i < aliases.Length; i++) if (aliases[i] == value) return true;
            return false;
        }

        // 配置文件放 mod 自己目录（Mods\自定义辐光招式\Settings.json），整包自包含
        private static string SettingsPath
        {
            get
            {
                string dir = Path.GetDirectoryName(typeof(CustomRadAttacks).Assembly.Location);
                return dir == null ? null : Path.Combine(dir, "Settings.json");
            }
        }

        private static void LoadSettings()
        {
            try
            {
                string path = SettingsPath;
                if (path != null && File.Exists(path))
                {
                    Settings = JsonConvert.DeserializeObject<CustomRadAttacksSettings>(File.ReadAllText(path))
                        ?? new CustomRadAttacksSettings();
                }
            }
            catch (Exception ex)
            {
                Instance.LogError("Settings load failed: " + ex.Message);
                Settings = new CustomRadAttacksSettings();
            }
            if (Settings.Slots == null) Settings.Slots = new string[AttackSequence.SlotCount];
        }

        internal static void SaveSettings()
        {
            try
            {
                string path = SettingsPath;
                if (path == null) return;
                string dir = Path.GetDirectoryName(path);
                if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Instance.LogError("Settings save failed: " + ex.Message);
            }
        }

        public bool ToggleButtonInsideMenu { get { return true; } }

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? menu)
        {
            var list = new List<IMenuMod.MenuEntry>
            {
                new IMenuMod.MenuEntry(
                    "启用",
                    new[] { "Off", "On" },
                    Conflicted
                        ? "⚠ 检测到抢占招式选择的 mod\n已自动禁用，名字见 ModLog"
                        : "总开关：关掉时全部交还原版辐光",
                    value => { Settings.Enabled = value == 1; SaveSettings(); },
                    () => Settings.Enabled ? 1 : 0),
                new IMenuMod.MenuEntry(
                    "模式",
                    new[] { "随机（原版）", "锁单招", "锁序列" },
                    "随机 = 完全原版；\n锁单招 = 每轮都出指定的那一招；\n锁序列 = 按 8 个槽位逐轮出",
                    value => { Settings.Mode = (ChoiceMode)value; SaveSettings(); },
                    () => (int)Settings.Mode),
                new IMenuMod.MenuEntry(
                    "P1 锁定招",
                    AttackCatalog.NamesFor(RadPhase.P1),
                    "只在「锁单招」模式下生效",
                    value => { Settings.LockedA1 = AttackCatalog.NamesFor(RadPhase.P1)[value]; SaveSettings(); },
                    () => IndexOf(AttackCatalog.NamesFor(RadPhase.P1), Settings.LockedA1)),
                new IMenuMod.MenuEntry(
                    "P2 锁定招",
                    AttackCatalog.NamesFor(RadPhase.P2),
                    "只在「锁单招」模式下生效",
                    value => { Settings.LockedA2 = AttackCatalog.NamesFor(RadPhase.P2)[value]; SaveSettings(); },
                    () => IndexOf(AttackCatalog.NamesFor(RadPhase.P2), Settings.LockedA2)),
                new IMenuMod.MenuEntry(
                    "走完循环",
                    new[] { "Off（走完交还原版）", "On（8 槽轮播）" },
                    "只在「锁序列」模式下生效",
                    value => { Settings.LoopSequence = value == 1; SaveSettings(); },
                    () => Settings.LoopSequence ? 1 : 0)
            };
            list.AddRange(SlotEntries());
            list.Add(new IMenuMod.MenuEntry(
                "P2 瞬移允许重复",
                new[] { "Off", "On" },
                "On = 允许连续去同一个点\nOff = 原版防重复限制",
                value => { Settings.TeleportAllowRepeat = value == 1; SaveSettings(); },
                () => Settings.TeleportAllowRepeat ? 1 : 0));
            list.Add(new IMenuMod.MenuEntry(
                "P2 瞬移锁死点",
                BuildTeleportOptions(),
                "选一个点 = 每次瞬移都去该点\n与「允许重复」可叠加\n都关 = 原版随机",
                value => { Settings.LockedTelePos = value; SaveSettings(); },
                () => Settings.LockedTelePos < 0 || Settings.LockedTelePos > 10 ? 0 : Settings.LockedTelePos));
            return list;
        }

        private static string[] BuildTeleportOptions()
        {
            var opts = new string[11];
            opts[0] = "不锁（原版随机）";
            for (int i = 1; i <= 10; i++) opts[i] = "第 " + i + " 点";
            return opts;
        }

        private static IEnumerable<IMenuMod.MenuEntry> SlotEntries()
        {
            string[] options = AttackCatalog.SlotOptions();
            for (int i = 0; i < AttackSequence.SlotCount; i++)
            {
                int slot = i;   // 闭包捕获：必须复制到局部变量
                yield return new IMenuMod.MenuEntry(
                    "槽位 " + (slot + 1),
                    options,
                    "空 = 这一槽跳过（不算一轮）",
                    value => { Settings.Slots[slot] = options[value] == AttackCatalog.Empty ? null : options[value]; SaveSettings(); },
                    () => IndexOf(options, Settings.Slots[slot] ?? AttackCatalog.Empty));
            }
        }

        private static int IndexOf(string[] arr, string value)
        {
            for (int i = 0; i < arr.Length; i++) if (arr[i] == value) return i;
            return 0;
        }
    }
}
