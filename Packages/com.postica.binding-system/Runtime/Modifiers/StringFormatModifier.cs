using System;
using System.Text.RegularExpressions;
using Postica.Common;
using UnityEngine;

namespace Postica.BindingSystem.Modifiers
{
    /// <summary>
    /// Formats the input string.
    /// </summary>
    [Serializable]
    [HideMember]
    [OneLineModifier]
    public sealed class StringFormatModifier : BaseModifier<string>
    {
        [SerializeField]
        [HideInInspector]
        private string _format = "{0}";
        
        [Bind]
        [Multiline(3)]
        public ReadOnlyBind<string> format = "{0}".Bind();

        private Regex _reverseFormatRegex;

        ///<inheritdoc/>
        public override string ShortDataDescription
        {
            get
            {
                if(_format != "{0}" && format.Value == "{0}")
                {
                    format.UnboundValue = _format;
                }

                var formatValue = format.Value;
                return formatValue == "{0}" ? "Not Set".RT().Bold().Color(BindColors.Primary) : $"[{formatValue}]";
            }
        }

        protected override string Modify(string value)
        {
            return string.IsNullOrEmpty(format) ? value : string.Format(format, value);
        }

        protected override string InverseModify(string output)
        {
            if(_reverseFormatRegex == null)
            {
                var pattern = GetPattern(format);
                _reverseFormatRegex = new Regex(pattern);
            }

            var match = _reverseFormatRegex.Match(output);
            return match.Success ? match.Groups[1].Value : output;
        }

        private static string GetPattern(string format)
        {
            return Regex.Replace(format, @"\{0.*\}", "(.*)");
        }
    }
}