using System;
using Postica.Common;
using UnityEngine;

namespace Postica.BindingSystem.Modifiers
{
    /// <summary>
    /// A modifier which adds each component of a Color.
    /// </summary>
    [Serializable]
    [HideMember]
    [OneLineModifier]
    [TypeIcon("_bsicons/modifiers/add_color")]
    public sealed class AddColorModifier : BaseModifier<Color>
    {
        [Tooltip("The additive to apply to the color.")]
        public ReadOnlyBind<Color> additive = Color.clear.Bind();
        
        ///<inheritdoc/>
        public override string Id => "Color Additive";

        ///<inheritdoc/>
        public override string ShortDataDescription => "";

        protected override Color Modify(Color value)
        {
            return value + additive.Value;
        }
        
        protected override Color InverseModify(Color output)
        {
            return output - additive;
        }
    }
}