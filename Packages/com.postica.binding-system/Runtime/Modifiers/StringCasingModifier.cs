using System;
using UnityEngine;

namespace Postica.BindingSystem
{
    /// <summary>
    /// A modifier that changes the casing of a string.
    /// </summary>
    [OneLineModifier]
    [HideMember]
    public class StringCasingModifier : BaseModifier<string>
    {
        public enum Casing
        {
            ToLower,
            ToLowerInvariant,
            ToUpper,
            ToUpperInvariant,
        }

        [SerializeField]
        [Tooltip("What casing to use")]
        private Casing _casing;

        public override string Id => "Change Casing";
        public override string ShortDataDescription => _casing.ToString();

        protected override string Modify(string value)
        {
            return _casing switch
            {
                Casing.ToLower => value.ToLower(),
                Casing.ToLowerInvariant => value.ToLowerInvariant(),
                Casing.ToUpper => value.ToUpper(),
                Casing.ToUpperInvariant => value.ToUpperInvariant(),
                _ => value
            };
        }
    }
}