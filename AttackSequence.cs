namespace CustomRadAttacks
{
    // 8 槽固定招式组的指针逻辑（纯 C#，无 Unity 依赖）
    public sealed class AttackSequence
    {
        public const int SlotCount = 8;

        private readonly string[] _slots = new string[SlotCount];
        private int _pointer;
        private bool _hasPhase;
        private RadPhase _phase;

        public AttackSequence()
        {
            for (int i = 0; i < SlotCount; i++) _slots[i] = AttackCatalog.Empty;
        }

        // 8 槽走完是否轮播；关 = 交还原版随机
        public bool Loop { get; set; }

        public int Pointer { get { return _pointer; } }

        public string SlotAt(int index) { return _slots[index]; }

        // 只改内容，指针不动
        public void SetSlot(int index, string chineseName)
        {
            if (index < 0 || index >= SlotCount) return;
            _slots[index] = string.IsNullOrEmpty(chineseName) ? AttackCatalog.Empty : chineseName;
        }

        // 从设置整体灌入，指针不动
        public void LoadSlots(string[] names)
        {
            for (int i = 0; i < SlotCount; i++)
                SetSlot(i, names != null && i < names.Length ? names[i] : null);
        }

        // 返回要强制的招式；null = 交还原版随机
        public AttackDef Next(RadPhase phase)
        {
            // 进 P2 那一次把指针打回槽 1
            if (_hasPhase && _phase == RadPhase.P1 && phase == RadPhase.P2) _pointer = 0;
            _hasPhase = true;
            _phase = phase;

            int at;
            AttackDef hit = Scan(phase, _pointer, SlotCount, out at);

            // 后面没货了：循环就从头再扫一遍
            if (hit == null && Loop) hit = Scan(phase, 0, _pointer, out at);

            if (hit == null) return null;

            _pointer = at + 1;
            if (_pointer >= SlotCount) _pointer = 0;
            return hit;
        }

        // 在 [from, to) 里找第一个非空、且当前阶段可用的槽
        private AttackDef Scan(RadPhase phase, int from, int to, out int at)
        {
            for (int i = from; i < to; i++)
            {
                AttackDef def = AttackCatalog.Find(_slots[i]);
                if (def == null || !def.ValidIn(phase)) continue;
                at = i;
                return def;
            }
            at = -1;
            return null;
        }
    }
}
