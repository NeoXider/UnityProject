using System;
using UnityEngine;

namespace Postica.BindingSystem
{
    /// <summary>
    /// A modifier that returns a substring of a string.
    /// </summary>
    [OneLineModifier]
    [HideMember]
    public class SubstringModifier : BaseModifier<string>
    {
        [SerializeField]
        [Tooltip("The index of the first character of the substring.")]
        private Bind<int> _startIndex;
        [SerializeField]
        [Tooltip("The length of the substring. If length is 0, the substring will go to the end of the string. " +
            "Negative values are accepted and will count the number of characters from the end")]
        private Bind<int> _length;

        public override string ShortDataDescription => $"({_startIndex.ToString("x")}, {_length.ToString("y")})";

        protected override string Modify(string value)
        {
            var length = _length.Value;
            return length switch
            {
                0 => value[_startIndex.Value..],
                < 0 => value[_startIndex.Value..^(-length)],
                _ => value.Substring(_startIndex.Value, length),
            };
        }
    }
}