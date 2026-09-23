using System;

namespace CustomRadAttacks
{
    public enum ChoiceMode { Random = 0, LockSingle = 1, LockSequenced = 2 }

    [Serializable]
    public class CustomRadAttacksSettings
    {
        // 总开关：关掉时所有钩子全部委托原版
        public bool Enabled = true;

        public ChoiceMode Mode = ChoiceMode.Random;

        // 锁单招：P1 / P2 各一个下拉（两阶段招池不同，见 design §12）
        public string LockedA1 = "脸刺";
        public string LockedA2 = "脸刺";

        // 锁序列：8 个槽位存招式中文名，null / 空串 = 空槽（跳过、不算一轮）
        public string[] Slots = new string[AttackSequence.SlotCount];
        public bool LoopSequence = true;

        // P2 瞬移
        public bool TeleportAllowRepeat = false;
        public int LockedTelePos = 0;   // 0 = 不锁；1~10 = 锁死第 N 点
    }
}
