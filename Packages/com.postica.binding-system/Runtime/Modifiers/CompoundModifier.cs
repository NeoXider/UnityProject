using Postica.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postica.BindingSystem
{
    /// <summary>
    /// A modifier which may contain other modifiers.
    /// </summary>
    [Serializable]
    [HideMember]
    public sealed class CompoundModifier : ISmartModifier, IObjectModifier, IRequiresAutoUpdate
    {
        private IBind _bindOwner;
        private Type _bindType;
        private List<IModifier> _modifiers;
        private List<IModifier> _readModifiers;
        private List<IModifier> _writeModifiers;
        
        ///<inheritdoc/>
        public string Id => "Compound Modifier";

        ///<inheritdoc/>
        public string ShortDataDescription => string.Empty;

        ///<inheritdoc/>
        public BindMode ModifyMode { get; set; }

        public CompoundModifier(IEnumerable<IModifier> modifiers)
        {
            _readModifiers = new List<IModifier>();
            _writeModifiers = new List<IModifier>();
            _modifiers = new List<IModifier>(modifiers);

            foreach (var modifier in modifiers)
            {
                if (modifier is IRequiresAutoUpdate requiresAutoUpdate)
                {
                    ShouldAutoUpdate |= requiresAutoUpdate.ShouldAutoUpdate;
                    UpdateOnEnable |= requiresAutoUpdate.UpdateOnEnable;
                }
                
                if (modifier.ModifyMode.CanRead())
                {
                    _readModifiers.Add(modifier);
                }
                if (modifier.ModifyMode.CanWrite())
                {
                    _writeModifiers.Add(modifier);
                }
            }

            if (_readModifiers.Count > 0 && _writeModifiers.Count > 0)
            {
                ModifyMode = BindMode.ReadWrite;
            }
            else if (_writeModifiers.Count > 0)
            {
                ModifyMode = BindMode.Write;
            }
            else
            {
                ModifyMode = BindMode.Read;
            }

            _writeModifiers.Reverse();
        }

        ///<inheritdoc/>
        public object Modify(BindMode mode, object value)
        {
            return mode switch
            {
                BindMode.ReadWrite => Modify(mode, _modifiers, value),
                BindMode.Write => Modify(mode, _writeModifiers, value),
                _ => Modify(mode, _readModifiers, value)
            };
        }

        private object Modify(BindMode mode, List<IModifier> modifiers, object value)
        {
            var returnValue = value;
            foreach(var modifier in modifiers)
            {
                returnValue = modifier.Modify(mode, returnValue);
            }
            return returnValue;
        }

        public IBind BindOwner
        {
            get => _bindOwner;
            set
            {
                if (_bindOwner == value)
                {
                    return;
                }
                
                _bindOwner = value;
                foreach (var modifier in _modifiers)
                {
                    if (modifier is ISmartModifier smartModifier)
                    {
                        smartModifier.BindOwner = value;
                    }
                }
            }
        }

        public Type TargetType 
        {
            get => _bindType;
            set
            {
                if (_bindType == value)
                {
                    return;
                }
                
                _bindType = value;
                foreach (var modifier in _modifiers)
                {
                    if (modifier is IObjectModifier objectModifier)
                    {
                        objectModifier.TargetType = value;
                    }
                }
            }
        }
        
        void ISmartModifier.SetSetValueCallback<T>(Action<ISmartModifier, T> callback)
        {
            foreach (var modifier in _modifiers)
            {
                if (modifier is ISmartModifier smartModifier)
                {
                    smartModifier.SetSetValueCallback(callback);
                }
            }
        }

        public bool ShouldAutoUpdate { get; private set; }
        public bool UpdateOnEnable { get; private set; }
    }

    /// <summary>
    /// A modifier which may contain other modifiers of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of data to modify</typeparam>
    public sealed class CompoundModifier<T> : IReadModifier<T>, IWriteModifier<T>, ISmartValueModifier<T>, IObjectModifier, IRequiresAutoUpdate
    {
        private IBind _bindOwner;
        private Type _bindType;

        private List<IModifier> _modifiers;
        private ModifyDelegate<T> _readModifiers;
        private ModifyDelegate<T> _writeModifiers;
        private Dictionary<ISmartModifier, ModifyDelegate<T>> _smartModifiers;

        ///<inheritdoc/>
        public string Id => "Compound of " + typeof(T).GetAliasName();

        ///<inheritdoc/>
        public string ShortDataDescription => string.Empty;

        ///<inheritdoc/>
        public BindMode ModifyMode { get; set; }

        /// <summary>
        /// Creates a modifier containing the given <paramref name="modifiers"/> set.
        /// </summary>
        /// <param name="owner">The <see cref="IBind"/> to use this modifier</param>
        /// <param name="modifiers">The modifiers to handle.</param>
        public CompoundModifier(IBind owner, IEnumerable<IModifier> modifiers)
        {
            _modifiers = new List<IModifier>();
            
            _smartModifiers = new Dictionary<ISmartModifier, ModifyDelegate<T>>();
            
            foreach (var modifier in modifiers)
            {
                _modifiers.Add(modifier);

                if (modifier is IRequiresAutoUpdate requiresAutoUpdate)
                {
                    ShouldAutoUpdate |= requiresAutoUpdate.ShouldAutoUpdate;
                    UpdateOnEnable |= requiresAutoUpdate.UpdateOnEnable;
                }
                
                if (modifier is ISmartModifier smartModifier)
                {
                    var smartFunc = _writeModifiers;
                    _smartModifiers.Add(smartModifier, smartFunc);
                }

                var (readFunc, writeFunc) = modifier.GetBothFunc<T>(owner, InnerSmartSetValue);
                
                if(readFunc != null)
                {
                    if (_readModifiers == null)
                    {
                        _readModifiers = readFunc;
                    }
                    else
                    {
                        var prevFunc = _readModifiers;
                        _readModifiers = (in T v) => readFunc(prevFunc(v));
                    }
                }
                if(writeFunc != null)
                {
                    if (_writeModifiers == null)
                    {
                        _writeModifiers = writeFunc;
                    }
                    else
                    {
                        var prevFunc = _writeModifiers;
                        _writeModifiers = (in T v) => prevFunc(writeFunc(v));
                    }
                }
            }

            ModifyMode = (_readModifiers != null, _writeModifiers != null) switch
            {
                (true, false) => BindMode.Read,
                (false, true) => BindMode.Write,
                _ => BindMode.ReadWrite
            };
        }

        private void InnerSmartSetValue(ISmartModifier modifier, T value)
        {
            if (SetValue == null)
            {
                return;
            }
            
            if(_smartModifiers.TryGetValue(modifier, out var modifyFunc)
               && modifyFunc != null)
            {
                value = modifyFunc(value);
            }
            
            SetValue(this, value);
        }

        ///<inheritdoc/>
        public object Modify(BindMode mode, object value) => mode.CanRead() ? ModifyRead((T)value) : ModifyWrite((T)value);

        ///<inheritdoc/>
        public T ModifyRead(in T value) => _readModifiers != null ? _readModifiers(value) : value;

        ///<inheritdoc/>
        public T ModifyWrite(in T value) => _writeModifiers != null ? _writeModifiers(value) : value;

        public IBind BindOwner
        {
            get => _bindOwner;
            set
            {
                if (_bindOwner == value)
                {
                    return;
                }
                
                _bindOwner = value;
                foreach (var smartModifier in _smartModifiers)
                {
                    smartModifier.Key.BindOwner = value;
                }
            }
        }
        
        public Type TargetType 
        {
            get => _bindType;
            set
            {
                if (_bindType == value)
                {
                    return;
                }
                
                _bindType = value;
                foreach (var modifier in _modifiers)
                {
                    if (modifier is IObjectModifier objectModifier)
                    {
                        objectModifier.TargetType = value;
                    }
                }
            }
        }

        public Action<ISmartModifier, T> SetValue { get; set; }
        public bool ShouldAutoUpdate { get; private set; }
        public bool UpdateOnEnable { get; private set; }
    }
}