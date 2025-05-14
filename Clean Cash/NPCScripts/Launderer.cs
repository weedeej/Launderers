#if IL2CPP
using Il2CppScheduleOne.Persistence;
using Il2CppInterop.Runtime;
#else
using ScheduleOne.Persistence;
#endif
using UnityEngine;

namespace Clean_Cash.NPCScripts
{
    public class Launderer : MonoBehaviour
    {
#if IL2CPP
        public Launderer(IntPtr pointer) : base(pointer) { }
#endif
    }
}
