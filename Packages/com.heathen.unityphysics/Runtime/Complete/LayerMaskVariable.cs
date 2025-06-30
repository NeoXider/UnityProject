#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering
{
    [CreateAssetMenu(menuName = "Variables/LayerMask")]
    public class LayerMaskVariable : DataVariable<LayerMask>
    { }
}

#endif