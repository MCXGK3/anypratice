using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anypractice
{
    public class settings
    {
        public bool on = true;
        public bool baldurfix = true;
        public bool beamlock=true;
        public bool abyssremove=true;
        public bool orbindicator=true;
        public bool carereset=true;
        public bool legacycost=true;
        public int skin=0;
        public bool indicator=true;
        public int cycle=0;
        public int timer=0;
        public int timerColor = 0;
        public bool radiance = false;
        public bool tele=false;
       //public bool superdash=true;

        // ---- 辐光招式控制 ----
        public bool crOn = true;
        // 模式：P1 / P2 各自独立选（随机 / 锁单招 / 锁序列）
        public ChoiceMode crModeA1 = ChoiceMode.Random;
        public ChoiceMode crModeA2 = ChoiceMode.Random;
        public string crA1 = "脸刺";
        public string crA2 = "脸刺";
        public string[] crSlots = new string[AttackSequence.SlotCount];
        public bool crLoop = true;
        public bool crTeleRepeat = false;
        public int crTelePos = 0;
    }
}
