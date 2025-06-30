using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Postica.BindingSystem.Accessors;
using UnityEngine;
using Object = UnityEngine.Object;
using Postica.Common;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Postica.BindingSystem
{
    [Serializable]
    [HideMember]
    internal class BindProxy : IBind, IBindData<BindData>
    {
        internal delegate ref BindData GetBindDataDelegate();
        
        internal static event Action<BindProxy> OnInvalidBindProxy;
        
        [Flags]
        public enum Options
        {
            None = 0,
            MaterialProperty = 1,
        }
        
        [NonSerialized]
        private int _id;
        [SerializeField]
        private Object _proxySource;
        [SerializeField]
        private SerializedType _proxySourceType;
        [SerializeField]
        private string _proxyPath;
        [SerializeField]
        private string _runtimeProxyPath;
        [SerializeField]
        private SerializedType _proxyType;
        [SerializeField]
        private bool _isBound;
        [SerializeField]
        private Options _options;

        [SerializeField] 
        [BindTypeSource(nameof(ValueType))]
        [MultiUpdate]
        private BindData _bindData = new(BindingSystem.BindData.BitFlags.UpdateOnUpdate);

        [NonSerialized]
        private BindPair _proxyBind;

        public int Id
        {
            get
            {
                if (_id == 0)
                {
                    // The id is the combined hash code of all the properties of the object
                    _id = HashCode.Combine(_proxySource, _proxyPath, _proxyType, _options);
                }
                return _id;
            }
        }
        
        public Object Source { get => _proxySource; set => _proxySource = value; }
        
        public IBindProxyProvider Provider { get; set; }

        public string Path
        {
            get => _proxyPath;
            set
            {
                if (_proxyPath == value)
                {
                    return;
                }
                _proxyPath = value;
                UpdateRuntimePath();
            }
        }
        
        public string RuntimePath => string.IsNullOrEmpty(_runtimeProxyPath) ? _proxyPath : _runtimeProxyPath;
        
        public Type SourceType { get => _proxySourceType; set => _proxySourceType = value; }
        public string SourceTypeFullName => _proxySourceType?.AssemblyQualifiedName;
        public Type ValueType { get => _proxyType; set => _proxyType = value; }
        public string ValueTypeFullName => _proxyType?.AssemblyQualifiedName;
        internal Options OptionsValue
        {
            get => _options;
            set => _options = value;
        }

        public BindData? BindData => _bindData;

        public Object Context => _proxySource ? _proxySource : _bindData.Context;
        internal Object ActualContext => _bindData.Context;
        internal string ContextPath => _bindData.ContextPath;

        /// <summary>
        /// Whether this object should be bound to another object value or not
        /// </summary>
        public bool IsBound
        {
            get => _isBound;
            set
            {
                if (_isBound == value) return;
                
                _isBound = value;
                if (_isBound)
                {
                    BindProxyPair.UpdateRead();
                }
                else
                {
                    BindProxyPair.RestoreUnboundValue();
                }
            }
        }

        internal BindPair BindProxyPair
        {
            get
            {
                if (_proxyBind != null) return _proxyBind;
                
                PreInitialize();
                _proxyBind = GenerateBindPair(ValueType);
                _proxyBind.Initialize(this, GetBindData);
                return _proxyBind;
            }
        }

        private ref BindData GetBindData() => ref _bindData;

        private void PreInitialize()
        {
            if (Application.isPlaying && Source is Renderer)
            {
                if (string.IsNullOrEmpty(_runtimeProxyPath))
                {
                    _proxyPath = _proxyPath.Replace("sharedMaterial", "material");
                }
                else
                {
                    _runtimeProxyPath = _runtimeProxyPath.Replace("sharedMaterial", "material");
                }
            }
        }

        public bool IsValid => _proxySource && !string.IsNullOrEmpty(_proxyPath);

        public void OnValidate()
        {
            if (!Application.isEditor)
            {
                Debug.LogError("OnValidate should only be called in editor mode");
                return;
            }
            
            RefreshProxy();

            if (Application.isPlaying && !IsBound)
            {
                _proxyBind?.RestoreUnboundValue();
            }
        }

        internal void RefreshProxy(bool updateRuntimePath = true, bool resetId = false)
        {
            if (resetId)
            {
                _id = 0;
            }
            
            if(_proxySource && _proxySourceType.Get() != _proxySource.GetType())
            {
                _proxySourceType = _proxySource.GetType();
            }
            
            if (updateRuntimePath)
            {
                UpdateRuntimePath();
            }
            
            RegisterForUpdates();
        }

        internal void UpdateRuntimePath()
        {
            if(!Source)
            {
                _runtimeProxyPath = string.Empty;
                return;
            }

            if (string.IsNullOrEmpty(_proxyPath))
            {
                _runtimeProxyPath = string.Empty;
                return;
            }
                
            if(Source.GetType().TryMakeUnityRuntimePath(_proxyPath, out var runtimePath, false, deepSearch: true))
            {
                _runtimeProxyPath = runtimePath.Replace(".Array.data", "");
            }
            else if(_proxyPath.Contains(".Array.data"))
            {
                _runtimeProxyPath = _proxyPath.Replace(".Array.data", "");
            }
            else
            {
                _runtimeProxyPath = string.Empty;
            }
        }
        
        public void Update()
        {
            if (!IsBound)
            {
                return;
            }

            BindProxyPair.UpdateRead();
            BindProxyPair.UpdateWrite();
        }

        public void RegisterForUpdates()
        {
            UnregisterForUpdates();
            _proxyBind = null;

            if (!IsBound)
            {
                return;
            }

            try
            {
                if (_bindData.Flags.HasFlag(BindingSystem.BindData.BitFlags.UpdateInEditor))
                {
                    BindProxyPair.TryGetReadAction(out var readAction);
                    BindProxyPair.TryGetWriteAction(out var writeAction);
                    var update = readAction != null && writeAction != null
                        ? () =>
                        {
                            readAction();
                            writeAction();
                        }
                        : readAction ?? writeAction;
                    BindingEngine.UpdateAtEditTime.Register(Id, BindProxyPair.GetIsAliveFunctor(), update,
                        _proxySource);
                }

                if (_bindData.Flags.HasFlag(BindingSystem.BindData.BitFlags.UpdateOnUpdate))
                {
                    RegisterReadForUpdate(BindingEngine.PreUpdate);
                    RegisterWriteForUpdate(BindingEngine.PostUpdate);
                }

                if (_bindData.Flags.HasFlag(BindingSystem.BindData.BitFlags.UpdateOnLateUpdate))
                {
                    RegisterReadForUpdate(BindingEngine.PreLateUpdate);
                    RegisterWriteForUpdate(BindingEngine.PostLateUpdate);
                }
                
                if (_bindData.Flags.HasFlag(BindingSystem.BindData.BitFlags.UpdateOnPrePostRender))
                {
                    RegisterReadForUpdate(BindingEngine.PreRender);
                    RegisterWriteForUpdate(BindingEngine.PostRender);
                }

                if (_bindData.Flags.HasFlag(BindingSystem.BindData.BitFlags.UpdateOnFixedUpdate))
                {
                    RegisterReadForUpdate(BindingEngine.PreFixedUpdate);
                    RegisterWriteForUpdate(BindingEngine.PostFixedUpdate);
                }

                if (_bindData.Flags.HasFlag(BindingSystem.BindData.BitFlags.UpdateOnChange))
                {
                    BindProxyPair.RegisterToValueChanged();
                }
            }
            catch (InvalidBindProxyException ex)
            {
                // Here perform either the refactoring or the removal of the invalid BindProxy
                if(OnInvalidBindProxy != null)
                {
                    OnInvalidBindProxy(this);
                }
                else
                {
                    Debug.LogError(BindSystem.DebugPrefix +
                                   "Failed to register for updates. Invalid Bind Proxy detected and will be removed.\nException: " + ex.Message);
                    Provider.RemoveProxy(this);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(BindSystem.DebugPrefix + $"Failed to register for updates. Exception: {e}");
            }
        }

        public void UnregisterForUpdates()
        {
            BindingEngine.UnregisterFromAllUpdate(Id);
            _proxyBind?.UnregisterFromValueChanged();
        }

        private void RegisterWriteForUpdate(BindingEngine.DataUpdater updater)
        {
            if (BindProxyPair.TryGetWriteAction(out var action))
            {
                updater.Register(Id, BindProxyPair.GetIsAliveFunctor(), action, _proxySource);
            }
        }
        
        private void RegisterReadForUpdate(BindingEngine.DataUpdater updater)
        {
            if (BindProxyPair.TryGetReadAction(out var action))
            {
                updater.Register(Id, BindProxyPair.GetIsAliveFunctor(), action, _proxySource);
            }
        }

        #region [  STATIC PART  ]
        
        private static Dictionary<Type, Func<BindPair>> _bindPairGenerators = new();

        private static BindPair GenerateBindPair(Type type)
        {
            if (_bindPairGenerators.TryGetValue(type, out var generator))
            {
                return generator();
            }

            var generatedType = typeof(BindPair<>).MakeGenericType(type);
            generator = () => Activator.CreateInstance(generatedType) as BindPair;
            _bindPairGenerators[type] = generator;
            return generator();    
        }
        
        #endregion
        
        private class InvalidBindProxyException : Exception
        {
            public InvalidBindProxyException(string message) : base(message)
            {
            }
        }
        
        public abstract class BindPair
        {
            public abstract void UpdateWrite();
            public abstract void UpdateRead();
            public abstract void RestoreUnboundValue();
            internal abstract void Initialize(BindProxy owner, GetBindDataDelegate getBindData);
            
            internal abstract bool TryGetWriteAction(out Action action);
            internal abstract bool TryGetReadAction(out Action action);
            internal abstract Func<bool> GetIsAliveFunctor();
            internal abstract void RegisterToValueChanged();
            internal abstract void UnregisterFromValueChanged();
        }

        private sealed class BindPair<T> : BindPair, IDataRefresher
        {
            private Object _source;
            private IAccessor<T> _sourceAccessor;
            private Bind<T> _targetBind;
            private T _initialValue;
            private BindProxy _owner;
            private GetBindDataDelegate _getBindData;
            private T _lastValue;
            private bool _isOnChangeRegistered;

            public bool IsAlive() => _source && _targetBind.Source;
            
            public override void UpdateRead()
            {
                if (!_targetBind.CanRead)
                {
                    return;
                }
                UpdateReadInternal();
            }
            
            private void UpdateReadInternal()
            {
                _sourceAccessor.SetValue(_source, _targetBind.Value);
            }

            public override void UpdateWrite()
            {
                if (!_targetBind.CanWrite)
                {
                    return;
                }
                UpdateWriteInternal();
                
                if(!Application.isPlaying || !Application.isEditor || !_targetBind.IsBound)
                {
                    return;
                }
                
                ref var ownerBindData = ref _getBindData();
                
                if (ownerBindData.IsLiveDebug && ownerBindData.Mode.CanWrite() && _targetBind.BindData.HasValue)
                {
                    ownerBindData.DebugValue = _targetBind.BindData.Value.DebugValue;
                }
            }

            private void UpdateWriteInternal()
            {
                _targetBind.Value = _sourceAccessor.GetValue(_source);
            }

            public override void RestoreUnboundValue()
            {
                _sourceAccessor?.SetValue(_source, _initialValue);
            }

            internal override void Initialize(BindProxy owner, GetBindDataDelegate getBindData)
            {
                _owner = owner;
                _source = owner._proxySource;
                _getBindData = getBindData;
                try
                {
                    _sourceAccessor = AccessorsFactory.GetAccessor<T>(_source, owner.RuntimePath);
                    if (Application.isPlaying)
                    {
                        _initialValue = _sourceAccessor.GetValue(_source);
                    }
                }
                catch (Exception e)
                {
                    // Debug.LogWarning(BindSystem.DebugPrefix + $"Failed to initialize BindPair. Exception: {e}");
                    throw new InvalidBindProxyException(e.Message);
                }

                _targetBind = new Bind<T>(owner._bindData);
            }

            internal override bool TryGetWriteAction(out Action action)
            {
                if (_targetBind.CanWrite)
                {
                    action = UpdateWrite;
                    return true;
                }

                action = null;
                return false;
            }

            internal override bool TryGetReadAction(out Action action)
            {
                if (_targetBind.CanRead)
                {
                    action = UpdateRead;
                    return true;
                }

                action = null;
                return false;
            }

            internal override Func<bool> GetIsAliveFunctor() => IsAlive;
            
            internal override void RegisterToValueChanged()
            {
                if(_targetBind == null)
                {
                    return;
                }

                if (_targetBind.CanRead)
                {
                    _targetBind.ValueChanged -= OnValueChanged;
                    _targetBind.ValueChanged += OnValueChanged;
                }

                if (_targetBind.CanWrite)
                {
                    _isOnChangeRegistered = true;
                    _lastValue = _sourceAccessor.GetValue(_source);
                    BindingEngine.RegisterDataRefresher(this);
                }
            }

            internal override void UnregisterFromValueChanged()
            {
                if(_targetBind == null)
                {
                    return;
                }
                _targetBind.ValueChanged -= OnValueChanged;
                if (_isOnChangeRegistered)
                {
                    BindingEngine.UnregisterDataRefresher(this);
                    _isOnChangeRegistered = false;
                }
            }

            private void OnValueChanged(T oldvalue, T newvalue)
            {
                _sourceAccessor.SetValue(_source, newvalue);
            }

            public (Object owner, string path) RefreshId => (_source, _owner.RuntimePath);

            public bool CanRefresh() => _isOnChangeRegistered && _targetBind.BindData?.IsValid == true;

            public void Refresh()
            {
                var currentValue = _sourceAccessor.GetValue(_source);
                if (!Equals(_lastValue, currentValue))
                {
                    _targetBind.Value = currentValue;
                    _lastValue = currentValue;
                }
            }
        }
    }
}
