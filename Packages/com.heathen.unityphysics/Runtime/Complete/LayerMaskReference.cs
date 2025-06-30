#if HE_SYSCORE

using UnityEngine;
using System;

namespace HeathenEngineering
{
    [Serializable]
    public class LayerMaskReference : VariableReference<LayerMask>
    {
        public LayerMaskVariable Variable;
        public override IDataVariable<LayerMask> m_variable => Variable;

        public LayerMaskReference(LayerMask value) : base(value)
        { }
    }
}

#endif
