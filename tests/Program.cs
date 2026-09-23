using System;
using System.Linq;

namespace CustomRadAttacks.Tests
{
    internal static class Program
    {
        private static int _failed;

        private static void Check(bool ok, string what)
        {
            if (ok) { Console.WriteLine("PASS  " + what); }
            else { Console.WriteLine("FAIL  " + what); _failed++; }
        }

        private static int Main()
        {
            CatalogTests();
            SequenceTests();
            Console.WriteLine(_failed == 0 ? "ALL PASS" : (_failed + " FAILED"));
            return _failed == 0 ? 0 : 1;
        }

        private static void SequenceTests()
        {
            // 空槽跳过、不算一轮；循环
            var seq = new AttackSequence();
            seq.Loop = true;
            seq.SetSlot(0, "剑雨");
            seq.SetSlot(2, "脸刺");
            seq.SetSlot(4, "光球");
            Check(seq.Next(RadPhase.P1).Chinese == "剑雨", "轮1 = 剑雨");
            Check(seq.Next(RadPhase.P1).Chinese == "脸刺", "轮2 = 脸刺（槽1 空被跳过）");
            Check(seq.Next(RadPhase.P1).Chinese == "光球", "轮3 = 光球（槽3 空被跳过）");
            Check(seq.Next(RadPhase.P1).Chinese == "剑雨", "轮4 回卷到剑雨");

            // 不循环：走完交还原版（返回 null）
            var seq2 = new AttackSequence();
            seq2.Loop = false;
            seq2.SetSlot(0, "剑雨");
            seq2.SetSlot(1, "脸刺");
            Check(seq2.Next(RadPhase.P1).Chinese == "剑雨", "不循环 轮1 = 剑雨");
            Check(seq2.Next(RadPhase.P1).Chinese == "脸刺", "不循环 轮2 = 脸刺");
            Check(seq2.Next(RadPhase.P1) == null, "不循环 轮3 = null（交还原版）");
            Check(seq2.Next(RadPhase.P1) == null, "不循环 轮4 仍是 null");

            // 当前阶段没这招 → 跳过（P2 没有剑雨）
            var seq3 = new AttackSequence();
            seq3.Loop = false;
            seq3.SetSlot(0, "剑雨");
            seq3.SetSlot(1, AttackCatalog.Empty);
            seq3.SetSlot(2, "光球");
            Check(seq3.Next(RadPhase.P2).Chinese == "光球", "P2 里 剑雨 被跳过，取到 光球");
            Check(seq3.Next(RadPhase.P2) == null, "P2 后续没了 → null");

            // 当前阶段没这招 → 跳过（P1 没有横刺）
            var seq3b = new AttackSequence();
            seq3b.Loop = false;
            seq3b.SetSlot(0, "横刺");
            seq3b.SetSlot(1, "脸刺");
            Check(seq3b.Next(RadPhase.P1).Chinese == "脸刺", "P1 里 横刺 被跳过，取到 脸刺");

            // 改槽位不动指针
            var seq4 = new AttackSequence();
            seq4.Loop = false;
            seq4.SetSlot(0, "剑雨");
            seq4.SetSlot(1, "脸刺");
            Check(seq4.Next(RadPhase.P1).Chinese == "剑雨", "改槽位前 轮1 = 剑雨");
            Check(seq4.Pointer == 1, "轮1 后指针 = 1");
            seq4.SetSlot(0, "光球");   // 改已走过的槽
            Check(seq4.Pointer == 1, "改槽位后指针仍是 1");
            Check(seq4.Next(RadPhase.P1).Chinese == "脸刺", "轮2 = 脸刺（不受改槽影响）");

            // 进 P2 指针打回槽 1
            var seq5 = new AttackSequence();
            seq5.Loop = false;
            seq5.SetSlot(0, "脸刺");
            seq5.SetSlot(1, "光球");
            seq5.SetSlot(2, "脸激光");
            Check(seq5.Next(RadPhase.P1).Chinese == "脸刺", "P1 轮1 = 脸刺");
            Check(seq5.Next(RadPhase.P1).Chinese == "光球", "P1 轮2 = 光球");
            Check(seq5.Next(RadPhase.P2).Chinese == "脸刺", "进 P2 指针回槽 1 = 脸刺");

            // 全空 → 交还原版
            var seq6 = new AttackSequence();
            seq6.Loop = true;
            Check(seq6.Next(RadPhase.P1) == null, "全空槽 → null（不死循环）");

            // LoadSlots 灌入 + 空槽语义
            var seq7 = new AttackSequence();
            seq7.Loop = false;
            seq7.LoadSlots(new[] { "剑雨", null, "", "光球", null, null, null, null });
            Check(seq7.Next(RadPhase.P1).Chinese == "剑雨", "LoadSlots 轮1 = 剑雨");
            Check(seq7.Next(RadPhase.P1).Chinese == "光球", "LoadSlots 轮2 = 光球（null/空 都跳过）");
            Check(seq7.Next(RadPhase.P1) == null, "LoadSlots 轮3 = null");
        }

        private static void CatalogTests()
        {
            // 槽位下拉：空 + 9 个名字 = 10 项
            Check(AttackCatalog.SlotOptions().Length == 10, "SlotOptions 共 10 项");
            Check(AttackCatalog.SlotOptions()[0] == AttackCatalog.Empty, "SlotOptions 第 0 项是空槽");

            // P1 锁定招 8 项，且含剑雨、不含横刺
            string[] p1 = AttackCatalog.NamesFor(RadPhase.P1);
            Check(p1.Length == 8, "P1 锁定招 8 项");
            Check(p1.Contains("剑雨"), "P1 有剑雨");
            Check(!p1.Contains("横刺"), "P1 没有横刺");

            // P2 锁定招 8 项，且含横刺、不含剑雨
            string[] p2 = AttackCatalog.NamesFor(RadPhase.P2);
            Check(p2.Length == 8, "P2 锁定招 8 项");
            Check(p2.Contains("横刺"), "P2 有横刺");
            Check(!p2.Contains("剑雨"), "P2 没有剑雨");

            // 阶段落点（design §8.1）
            Check(AttackCatalog.Find("剑雨").EventFor(RadPhase.P1) == "NAIL TOP SWEEP", "剑雨 P1 = NAIL TOP SWEEP");
            Check(AttackCatalog.Find("剑雨").EventFor(RadPhase.P2) == null, "剑雨 P2 不可用");
            Check(AttackCatalog.Find("左横刺").EventFor(RadPhase.P1) == "NAIL L SWEEP", "左横刺 P1 = NAIL L SWEEP");
            Check(AttackCatalog.Find("左横刺").EventFor(RadPhase.P2) == "NAIL LR SWEEP", "左横刺 P2 = NAIL LR SWEEP");
            Check(AttackCatalog.Find("左横刺").P2Dir == -1, "左横刺 P2 强制左");
            Check(AttackCatalog.Find("右横刺").P2Dir == 1, "右横刺 P2 强制右");
            Check(AttackCatalog.Find("横刺").EventFor(RadPhase.P1) == null, "横刺 P1 不可用");
            Check(AttackCatalog.Find("横刺").P2Dir == 0, "横刺 P2 不强制方向");
            Check(AttackCatalog.Find("脸刺").EventFor(RadPhase.P1) == "NAIL FAN", "脸刺 P1 = NAIL FAN");
            Check(AttackCatalog.Find("脸刺").EventFor(RadPhase.P2) == "NAIL FAN", "脸刺 P2 = NAIL FAN");

            // 空 / 未知名字
            Check(AttackCatalog.Find("") == null, "空槽 Find 返回 null");
            Check(AttackCatalog.Find("不存在的招") == null, "未知名字 Find 返回 null");
            Check(AttackCatalog.Find(null) == null, "null Find 返回 null");
        }
    }
}
