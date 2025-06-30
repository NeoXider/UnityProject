using System;
using Postica.BindingSystem.Accessors;
using UnityEngine.Scripting;

using Object = UnityEngine.Object;

namespace Postica.BindingSystem
{
    [Preserve]
    internal class UnityObjectToBoolAccessor : IAccessor<bool>, IConcurrentAccessor<bool>
    {
        private object _requestor;
        public UnityObjectToBoolAccessor(object requestor)
        {
            _requestor = requestor;
        }

        public bool GetValue(object target) => (Object)target;

        public IConcurrentAccessor<bool> MakeConcurrent() => this;

        public void SetValue(object target, in bool value)
        {
            throw new InvalidOperationException($"{_requestor.GetType().Name}<{typeof(bool).Name}> cannot convert to bool from self reference of {target}.");
        }
    }
}
