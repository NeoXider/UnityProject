using System;
using Postica.BindingSystem.Utility;
using Postica.Common;

namespace Postica.BindingSystem.Modifiers
{
    /// <summary>
    /// Returns the evaluation of a boolean expression where input acts as one of the variables.
    /// </summary>
    [Serializable]
    [HideMember]
    [OneLineModifier]
    [TypeIcon("_bsicons/modifiers/function")]
    public sealed class BoolExpressionModifier : IReadModifier<bool>
    {
        public BoolExpressionValue expression = new("Expression", "x") { expression = "x and true" };

        public bool ModifyRead(in bool value) => expression.Evaluate(value);

        ///<inheritdoc/>
        public string Id => "Bool Expression";

        ///<inheritdoc/>
        public string ShortDataDescription => expression.expression;

        public object Modify(BindMode mode, object value)
        {
            if (value is bool b && mode.CanRead())
            {
                return ModifyRead(b);
            }

            return false;
        }

        public BindMode ModifyMode => BindMode.Read;
    }
}