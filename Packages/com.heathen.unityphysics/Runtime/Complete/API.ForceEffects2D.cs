#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics.API
{
    public static class ForceEffects2D
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            forceEffectFields = new List<ForceEffectSource2D>();
        }

        public static List<ForceEffectSource2D> GlobalEffects => forceEffectFields;

        private static List<ForceEffectSource2D> forceEffectFields = new();
    }
}
#endif