using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace anypractice
{
    internal class abyssremover:MonoBehaviour
    {
        PlayMakerFSM _choice;
        PlayMakerFSM _asc;
        GameObject pit;
        bool removed=false;
        private void Awake()
        {
            _choice = base.gameObject.LocateMyFSM("Attack Choices");
        }


        private void Update()
        {
            if (!removed&&_choice.ActiveStateName=="A2 End" && gameObject.transform.position.y >= 150f)
            {
                pit = GameObject.Find("Abyss Pit");
                _asc = pit.LocateMyFSM("Ascend");
                _asc.RemoveTransition("Idle", "ASCEND");
                removed= true;
                Modding.Logger.Log("Have Removed Abyss Pit");
            } 
        }
        private void Unload()
        {
            removed= false;
            if (pit != null) _asc.AddTransition("Idle", "ASCEND", "Ascend"); 
        }
    }
}
