using anypractice;
using System.EnterpriseServices;

namespace anypractice
{
    internal class Cycle:MonoBehaviour
    {
        private bool set = false;
        HealthManager hm;
        PlayMakerFSM pc;
        private void Awake()
        {
            hm = base.GetComponent<HealthManager>();
            pc = base.gameObject.LocateMyFSM("Phase Control");
            switch (anypractice.Instance._set.cycle)
            {
                case 0: break;
                case 1:
                    pc.ChangeTransition("Init", "FINISHED", "Set Phase 3");
                    //pc.ChangeTransition("Check 3", "FINISHED","Check 3");
                    break;
                case 2:
                    pc.ChangeTransition("Init", "FINISHED", "Set Phase 3");
                    //pc.ChangeTransition("Check 4", "FINISHED","Check 4");
                    break;
                default: break;
            }
        }
        private void Update()
        {
            if (!set)
            {
                switch (anypractice.Instance._set.cycle)
                {
                    case 0: break;
                    case 1:
                        hm.hp = base.gameObject.LocateMyFSM("Phase Control").FsmVariables.GetFsmInt("P4 Stun1").Value; 
                        break;
                    case 2:
                        hm.hp = base.gameObject.LocateMyFSM("Phase Control").FsmVariables.GetFsmInt("P5 Acend").Value;
                        break;
                    default: break;
                }
                set = true;
            }
        }
        private void Undestroy()
        {
            set = false;
        }
    }
}