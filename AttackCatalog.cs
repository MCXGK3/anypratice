using System.Collections.Generic;

namespace CustomRadAttacks
{
    public enum RadPhase { P1 = 1, P2 = 2 }

    // 一个招式"名字"在两个阶段各自落到哪个 FSM 事件：同名在 P1/P2 不一定同事件
    public sealed class AttackDef
    {
        public readonly string Chinese;
        public readonly string P1Event;   // null = P1 没有这招
        public readonly string P2Event;   // null = P2 没有这招
        public readonly int P2Dir;        // 0 = 不强制, -1 = 左, +1 = 右

        public AttackDef(string chinese, string p1Event, string p2Event, int p2Dir = 0)
        {
            Chinese = chinese;
            P1Event = p1Event;
            P2Event = p2Event;
            P2Dir = p2Dir;
        }

        public string EventFor(RadPhase phase)
        {
            return phase == RadPhase.P1 ? P1Event : P2Event;
        }

        public bool ValidIn(RadPhase phase)
        {
            return EventFor(phase) != null;
        }
    }

    public static class AttackCatalog
    {
        public const string Empty = "";

        public static readonly AttackDef[] All =
        {
            new AttackDef("剑雨",   "NAIL TOP SWEEP", null),
            new AttackDef("横刺",   null,             "NAIL LR SWEEP",  0),
            new AttackDef("左横刺", "NAIL L SWEEP",   "NAIL LR SWEEP", -1),
            new AttackDef("右横刺", "NAIL R SWEEP",   "NAIL LR SWEEP",  1),
            new AttackDef("脸激光", "EYE BEAMS",      "EYE BEAMS"),
            new AttackDef("左光墙", "BEAM SWEEP L",   "BEAM SWEEP L"),
            new AttackDef("右光墙", "BEAM SWEEP R",   "BEAM SWEEP R"),
            new AttackDef("脸刺",   "NAIL FAN",       "NAIL FAN"),
            new AttackDef("光球",   "ORBS",           "ORBS"),
        };

        public static string[] SlotOptions()
        {
            var list = new List<string> { Empty };
            for (int i = 0; i < All.Length; i++) list.Add(All[i].Chinese);
            return list.ToArray();
        }

        public static string[] NamesFor(RadPhase phase)
        {
            var list = new List<string>();
            for (int i = 0; i < All.Length; i++)
                if (All[i].ValidIn(phase)) list.Add(All[i].Chinese);
            return list.ToArray();
        }

        // 空串 / null / 未知名字 一律返回 null
        public static AttackDef Find(string chinese)
        {
            if (string.IsNullOrEmpty(chinese)) return null;
            for (int i = 0; i < All.Length; i++)
                if (All[i].Chinese == chinese) return All[i];
            return null;
        }
    }
}
