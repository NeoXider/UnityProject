
using System;
using Postica.Common;
using UnityEngine;

namespace Postica.BindingSystem
{
    /// <summary>
    /// The main class of the Binding System to access important information.
    /// </summary>
    public partial class BindSystem
    {
        private static BindMetaValues _metaValues;
        
        /// <summary>
        /// Represents the current version of the Binding System with the format "Major.Minor.Patch".
        /// </summary>
        public const string Version = "2.2.6";
        
        /// <summary>
        /// Represents the name of the Binding System.
        /// </summary>
        public const string ProductName = "Binding System";
        
        /// <summary>
        /// Represents the full product text with the version.
        /// </summary>
        public const string FullProductText = ProductName + " v" + Version;
        
        /// <summary>
        /// Used to prefix debug messages.
        /// </summary>
        internal const string DebugPrefix = "[" + FullProductText + "] - ";
        
        /// <summary>
        /// True if the current architecture is ARM64, false otherwise.
        /// </summary>
        public static bool IsARM64Architecture => SystemInfo.processorType.Contains("ARM", StringComparison.OrdinalIgnoreCase) ||
                                                  SystemInfo.processorType.Contains("Apple", StringComparison.OrdinalIgnoreCase);
        
        internal static BindMetaValues MetaValues
        {
            get
            {
                if (_metaValues) return _metaValues;
                
                _metaValues = Resources.Load<BindMetaValues>("bind-meta-values");
                
                if (!_metaValues)
                {
                    _metaValues = ScriptableObject.CreateInstance<BindMetaValues>();
                }
                else
                {
                    _metaValues.Sanitize();
                }

                return _metaValues;
            }
            set => _metaValues = value;
        }

        public static bool RerouteBoundField(Type type, string fieldName, string newFieldOrProperty,
            bool overwrite = false)
        {
            return ReflectionExtensions.RerouteFieldPath(type, fieldName, newFieldOrProperty, overwrite);
        }
        
        public static bool RerouteBoundFieldOf<T>(string fieldName, string newFieldOrProperty, bool overwrite = false)
        {
            return ReflectionExtensions.RerouteFieldPath(typeof(T), fieldName, newFieldOrProperty, overwrite);
        }
        
        public static bool UnrouteBoundField(Type type, string fieldName)
        {
            return ReflectionExtensions.UnRerouteFieldPath(type, fieldName);
        }
    }
}
