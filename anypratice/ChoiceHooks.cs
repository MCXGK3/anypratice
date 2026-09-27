namespace anypractice
{
    // 辐光招式选择的三处钩子。钩子是全局的，靠 FSM 身份过滤，只处理辐光那一个。
    // 设计上只发/拦事件，不读转移表、不碰动作索引，因此不影响任何按索引读该 FSM 的 mod。
    internal static class ChoiceHooks
    {
        private const string RadianceGoName = "Absolute Radiance";
        private const string ChoicesFsmName = "Attack Choices";
        private const string ControlFsmName = "Control";

        private static readonly AttackSequence Sequence = new AttackSequence();

        // 本轮强制招在 P2 的方向：0 不强制 / -1 左 / +1 右（供 L or R Choice 取用）
        private static int _pendingDir;

        internal static void Install()
        {
            On.HutongGames.PlayMaker.Actions.SendRandomEventV3.OnEnter += HookChoice;
            On.HutongGames.PlayMaker.Actions.SendRandomEvent.OnEnter += HookNailLr;
            On.HutongGames.PlayMaker.Actions.SendRandomEvent.OnEnter += HookTeleport;
        }

        internal static void Uninstall()
        {
            On.HutongGames.PlayMaker.Actions.SendRandomEventV3.OnEnter -= HookChoice;
            On.HutongGames.PlayMaker.Actions.SendRandomEvent.OnEnter -= HookNailLr;
            On.HutongGames.PlayMaker.Actions.SendRandomEvent.OnEnter -= HookTeleport;
        }

        private static settings Set { get { return anypractice.Instance._set; } }

        private static void HookChoice(On.HutongGames.PlayMaker.Actions.SendRandomEventV3.orig_OnEnter orig,
                                       SendRandomEventV3 self)
        {
            Fsm fsm = self.Fsm;
            if (fsm == null || fsm.GameObjectName != RadianceGoName || fsm.Name != ChoicesFsmName)
            {
                orig(self);
                return;
            }

            RadPhase phase;
            if (!TryPhase(self, out phase)) { orig(self); return; }

            settings s = Set;
            ChoiceMode mode = phase == RadPhase.P1 ? s.crModeA1 : s.crModeA2;
            if (!s.on || !s.crOn || mode == ChoiceMode.Random) { _pendingDir = 0; orig(self); return; }

            AttackDef def;
            if (mode == ChoiceMode.LockSingle)
            {
                def = AttackCatalog.Find(phase == RadPhase.P1 ? s.crA1 : s.crA2);
            }
            else
            {
                Sequence.Loop = s.crLoop;
                Sequence.LoadSlots(s.crSlots);
                def = Sequence.Next(phase);
            }

            string evt = def == null ? null : def.EventFor(phase);
            if (evt == null) { _pendingDir = 0; orig(self); return; }   // 空槽 / 本阶段没这招 → 交还原版

            _pendingDir = def.P2Dir;

            // 不调 orig：原版随机被完全抑制（SendRandomEventV3 的防重复计数也随之冻住，切回随机后自行恢复）
            Send(fsm, evt);
        }

        // 阶段只认状态名，不认 HP —— AnyRadiance 2 会整体改血量阈值
        private static bool TryPhase(FsmStateAction self, out RadPhase phase)
        {
            phase = RadPhase.P1;
            string st = self.State != null ? self.State.Name : self.Fsm.ActiveStateName;
            if (st == "A1 Choice") { phase = RadPhase.P1; return true; }
            if (st == "A2 Choice") { phase = RadPhase.P2; return true; }
            return false;
        }

        // P2 的 L or R Choice：NAIL LR SWEEP 之后的左右二选一
        private static void HookNailLr(On.HutongGames.PlayMaker.Actions.SendRandomEvent.orig_OnEnter orig,
                                       SendRandomEvent self)
        {
            Fsm fsm = self.Fsm;
            if (fsm == null || fsm.GameObjectName != RadianceGoName || fsm.Name != ChoicesFsmName)
            {
                orig(self);
                return;
            }
            string st = self.State != null ? self.State.Name : fsm.ActiveStateName;
            if (st != "L or R Choice") { orig(self); return; }

            if (!Set.on || !Set.crOn) { orig(self); return; }

            // 方向只由本轮选中的招式名决定（左横刺 = -1 / 右横刺 = +1）；
            // 「横刺」不带方向 → 交还原版左右随机
            int dir = _pendingDir;
            if (dir == 0) { orig(self); return; }

            Send(fsm, dir < 0 ? "NAIL L SWEEP" : "NAIL R SWEEP");   // 不调 orig
        }

        // P2 瞬移点位：Control FSM 的 A2 Tele Choice
        private static void HookTeleport(On.HutongGames.PlayMaker.Actions.SendRandomEvent.orig_OnEnter orig,
                                         SendRandomEvent self)
        {
            Fsm fsm = self.Fsm;
            if (fsm == null || fsm.GameObjectName != RadianceGoName || fsm.Name != ControlFsmName)
            {
                orig(self);
                return;
            }
            string st = self.State != null ? self.State.Name : fsm.ActiveStateName;
            if (st != "A2 Tele Choice") { orig(self); return; }

            settings s = Set;
            if (!s.on || !s.crOn) { orig(self); return; }
            if (!s.crTeleRepeat && s.crTelePos == 0) { orig(self); return; }   // 两个都关 = 原版

            // Tele N 里是 IntCompare(Last Tele Pos == N) → NEXT 的防重复链；
            // 清零后整条链落空：既实现「允许重复」，也是「锁死第 N 点」的前提
            SetLastTelePos(fsm, 0);

            if (s.crTelePos > 0)
            {
                Send(fsm, s.crTelePos.ToString());   // 事件名就是 "1".."10"
                return;
            }
            orig(self);   // 允许重复：仍按原版权重随机挑点，只是防重复链已清零
        }

        private static void SetLastTelePos(Fsm fsm, int value)
        {
            FsmInt v = fsm.GetFsmInt("Last Tele Pos");
            if (v == null)
            {
                anypractice.Instance.LogError("找不到 Control FSM 的 Last Tele Pos");
                return;
            }
            v.Value = value;
        }

        private static void Send(Fsm fsm, string eventName)
        {
            FsmEvent e = FindEvent(fsm, eventName);
            if (e == null)
            {
                anypractice.Instance.LogError("事件找不到: " + eventName);
                return;
            }
            fsm.Event(e);
        }

        private static FsmEvent FindEvent(Fsm fsm, string name)
        {
            if (fsm.Events != null)
            {
                for (int i = 0; i < fsm.Events.Length; i++)
                {
                    FsmEvent e = fsm.Events[i];
                    if (e != null && e.Name == name) return e;
                }
            }
            return FsmEvent.GetFsmEvent(name);
        }
    }
}
