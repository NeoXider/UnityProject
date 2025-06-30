using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Scripting;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

namespace Postica.BindingSystem
{
    internal class BindingEngine
    {
        public interface IDataRefresherHandler
        {
            void Register(IDataRefresher refresher);
            void Unregister(IDataRefresher refresher);
        }

        internal static Action<Action> _registerToEditorUpdate;
        internal static Action<Action> _unregisterFromEditorUpdate;
        
        private static readonly Dictionary<(Type updateType, bool isPre), StageUpdater> _customStageUpdaters = new();

        public readonly static EditTimeUpdater UpdateAtEditTime = new();
        public readonly static PreStageUpdater<Update> PreUpdate = new();
        public readonly static PostStageUpdater<Update> PostUpdate = new();
        public readonly static PreStageUpdater<PreLateUpdate> PreLateUpdate = new();
        public readonly static PostStageUpdater<PreLateUpdate> PostLateUpdate = new();
        public readonly static PreStageUpdater<PostLateUpdate> PreRender = new();
        public readonly static PreStageUpdater<PreUpdate> PostRender = new();
        public readonly static PreStageUpdater<FixedUpdate> PreFixedUpdate = new();
        public readonly static PostStageUpdater<FixedUpdate> PostFixedUpdate = new();
        
        public static IDataRefresherHandler DataRefresherHandler { get; private set; } = new BindDataRefresher()
        {
            AutoRegister = false
        };


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            ((BindDataRefresher)DataRefresherHandler).RegisterToPlayerLoop();
        }
        
        ~BindingEngine()
        {
            ((BindDataRefresher)DataRefresherHandler).ForcedUnregisterFromPlayerLoop();
        }
        
        public static StageUpdater GetStageUpdater<T>(bool isPreStage)
        {
            var type = typeof(T);
            if (!_customStageUpdaters.TryGetValue((type, isPreStage), out var updater))
            {
                updater = isPreStage ? new PreStageUpdater<T>() : new PostStageUpdater<T>();
                _customStageUpdaters.Add((type, isPreStage), updater);
            }

            return updater;
        }
        
        public static StageUpdater GetStageUpdater(Type updateType, bool isPreStage)
        {
            if (!_customStageUpdaters.TryGetValue((updateType, isPreStage), out var updater))
            {
                updater = isPreStage ? new PreStageUpdater(updateType) : new PostStageUpdater(updateType);
                _customStageUpdaters.Add((updateType, isPreStage), updater);
            }

            return updater;
        }

        public static void RegisterDataRefresher(IDataRefresher refresher) => DataRefresherHandler.Register(refresher);
        public static void UnregisterDataRefresher(IDataRefresher refresher) => DataRefresherHandler.Unregister(refresher);
        
        public static void UnregisterFromAllUpdate(int id)
        {
            UpdateAtEditTime.Unregister(id);
            PreUpdate.Unregister(id);
            PreLateUpdate.Unregister(id);
            PostUpdate.Unregister(id);
            PostLateUpdate.Unregister(id);
            PreFixedUpdate.Unregister(id);
            PostFixedUpdate.Unregister(id);
        }
        
        // TODO: Implement on Changes updater

        class PlayerLoopHandler
        {

            public static bool Register<TSystem>(PlayerLoopSystem.UpdateFunction callback, params Type[] entryPoints)
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                var originalLoop = loop;
                // PrintPlayerLoop(loop);

                try
                {
                    bool shouldUpdateLoop = false;
                    foreach(var entryPoint in entryPoints)
                    {
                        shouldUpdateLoop |= RegisterSystem(entryPoint, typeof(TSystem), ref loop, 0, callback);
                    }

                    if (shouldUpdateLoop)
                    {
                        PlayerLoop.SetPlayerLoop(loop);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    PlayerLoop.SetPlayerLoop(originalLoop);
                }
                return false;
            }
            
            public static bool RegisterPost<TSystem>(PlayerLoopSystem.UpdateFunction callback, params Type[] entryPoints)
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                var originalLoop = loop;
                // PrintPlayerLoop(loop);

                try
                {
                    bool shouldUpdateLoop = false;
                    foreach(var entryPoint in entryPoints)
                    {
                        shouldUpdateLoop |= RegisterSystem(entryPoint, typeof(TSystem), ref loop, -1, callback);
                    }

                    if (shouldUpdateLoop)
                    {
                        PlayerLoop.SetPlayerLoop(loop);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    PlayerLoop.SetPlayerLoop(originalLoop);
                }
                return false;
            }

            public static bool Unregister<TSystem>() => Unregister(typeof(TSystem));

            public static bool Unregister(Type systemType)
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                var originalLoop = loop;

                try
                {
                    var shouldUpdateLoop = RemoveAllSystems(systemType, ref loop);

                    if (shouldUpdateLoop)
                    {
                        PlayerLoop.SetPlayerLoop(loop);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    PlayerLoop.SetPlayerLoop(originalLoop);
                }
                return false;
            }

            private static bool RegisterSystem(Type entryPointType, Type systemType, ref PlayerLoopSystem loop, int index, PlayerLoopSystem.UpdateFunction callback)
            {
                var refreshSystem = new PlayerLoopSystem()
                {
                    subSystemList = null,
                    type = systemType,
                    updateDelegate = callback,
                };

                return InsertSystem(entryPointType, ref loop, index, refreshSystem);
            }

            public static void PrintPlayerLoop()
            {
                PrintPlayerLoop(PlayerLoop.GetCurrentPlayerLoop());
            }
            
            public static void PrintPlayerLoop(PlayerLoopSystem loop)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("UNITY PLAYER LOOP");
                foreach (var subSystem in loop.subSystemList)
                {
                    PrintSubsystem(subSystem, sb, 0);
                }

                Debug.Log(sb.ToString());
            }

            private static bool InsertSystem(Type entryPointType, ref PlayerLoopSystem loop, int index, in PlayerLoopSystem systemToInsert)
            {
                if (loop.type == entryPointType)
                {
                    var list = new List<PlayerLoopSystem>();
                    if (loop.subSystemList != null)
                    {
                        list.AddRange(loop.subSystemList);
                    }

                    if (index < 0 || index >= list.Count)
                    {
                        list.Add(systemToInsert);
                    }
                    else
                    {
                        list.Insert(index, systemToInsert);
                    }

                    loop.subSystemList = list.ToArray();
                    return true;
                }

                var subSystems = loop.subSystemList;
                if (subSystems != null)
                {
                    for (int i = 0; i < subSystems.Length; i++)
                    {
                        if (InsertSystem(entryPointType, ref subSystems[i], index, systemToInsert))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private static bool RemoveSystem(Type systemType, ref PlayerLoopSystem loop)
            {
                var subSystems = loop.subSystemList;
                if (subSystems != null)
                {
                    for (int i = 0; i < subSystems.Length; i++)
                    {
                        if (subSystems[i].type == systemType)
                        {
                            var list = new List<PlayerLoopSystem>(subSystems);
                            list.RemoveAt(i);
                            loop.subSystemList = list.ToArray();
                            return true;
                        }
                        if (RemoveSystem(systemType, ref subSystems[i]))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private static bool RemoveAllSystems(Type systemType, ref PlayerLoopSystem loop)
            {
                bool success = false;
                var subSystems = loop.subSystemList;
                if (subSystems != null)
                {
                    var list = subSystems.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (subSystems[i].type == systemType)
                        {
                            list.RemoveAt(i--);
                            success = true;
                        }
                        else
                        {
                            success |= RemoveSystem(systemType, ref subSystems[i]);
                        }
                    }
                    loop.subSystemList = list.ToArray();
                }

                return success;
            }

            // TODO: Hook this method into some update
            private static void PrintSubsystem(PlayerLoopSystem system, StringBuilder sb, int level)
            {
                sb.Append(' ', level * 4).AppendLine(system.type.ToString());
                if (system.subSystemList?.Length > 0)
                {
                    foreach (var subSystem in system.subSystemList)
                    {
                        PrintSubsystem(subSystem, sb, level + 1);
                    }
                }
            }
        }

        private static void AddBindingsSyncSubsystem(ref PlayerLoopSystem system)
        {
            var subSystems = system.subSystemList.ToList();
            subSystems.Add(new PlayerLoopSystem()
            {
                type = typeof(SynchronizeBindings),
                updateDelegate = SyncBindings,
            });
            system.subSystemList = subSystems.ToArray();
        }

        private class SynchronizeBindings { }

        private static void SyncBindings()
        {
            throw new NotImplementedException();
        }
        
        internal class BindDataRefresher : IDataRefresherHandler, IDisposable
        {
            private readonly List<IDataRefresher> _dataRefreshers = new List<IDataRefresher>(128);
            private bool _active;
            
            public bool AutoRegister { get; set; } = true;

            public void Register(IDataRefresher refresher)
            {
                if (AutoRegister && !_active)
                {
                    RegisterToPlayerLoop();
                }

                _dataRefreshers.RemoveAll(r => r.RefreshId == refresher.RefreshId && r != refresher);
                _dataRefreshers.Add(refresher);
            }

            public void Unregister(IDataRefresher refresher)
            {
                if (_dataRefreshers.Remove(refresher))
                {
                    TryUnregisterFromPlayerLoop();
                    return;
                }

                var removedItems = _dataRefreshers.RemoveAll(r => r.RefreshId == refresher.RefreshId);
                if(removedItems > 0 && AutoRegister && _dataRefreshers.Count == 0)
                {
                    TryUnregisterFromPlayerLoop();
                }
            }

            public void RegisterToPlayerLoop()
            {
                if (_active)
                {
                    return;
                }
                _active = PlayerLoopHandler.Register<BindDataRefresher>(RefreshData, typeof(Update), typeof(PreLateUpdate));
            }

            public void TryUnregisterFromPlayerLoop()
            {
                if (AutoRegister && _dataRefreshers.Count == 0 && _active && PlayerLoopHandler.Unregister<BindDataRefresher>())
                {
                    _active = false;
                }
            }

            public void ForcedUnregisterFromPlayerLoop()
            {
                PlayerLoopHandler.Unregister<BindDataRefresher>();
            }

            internal void RefreshData()
            {
                if (_dataRefreshers.Count == 0)
                {
                    return;
                }

                if (!Application.isPlaying)
                {
                    return;
                }

                for (int i = 0; i < _dataRefreshers.Count; i++)
                {
                    var dataRefresher = _dataRefreshers[i];
                    if (dataRefresher == null)
                    {
                        _dataRefreshers.RemoveAt(i--);
                        continue;
                    }
                    var (owner, path) = dataRefresher.RefreshId;
                    if (string.IsNullOrEmpty(path) || IsObjectDead(owner) || !dataRefresher.CanRefresh())
                    {
                        _dataRefreshers.RemoveAt(i--);
                        continue;
                    }

                    try
                    {
                        dataRefresher.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, owner);
                    }
                }
            }

            private static bool IsObjectDead(Object obj) => !ReferenceEquals(obj, null) && !obj;

            public void Dispose()
            {
                ForcedUnregisterFromPlayerLoop();
            }
        }
        
        internal abstract class DataUpdater
        {
            protected readonly List<StageData> _dataUpdaters = new(256);
            
            protected readonly struct StageData
            {
                public readonly int id;
                public readonly Func<bool> isAlive;
                public readonly Action update;
                public readonly Object context;
                
                public StageData(int id, Func<bool> isAlive, Action update, Object context)
                {
                    this.id = id;
                    this.isAlive = isAlive;
                    this.update = update;
                    this.context = context;
                }
            }

            public virtual void Register(int id, Func<bool> isAlive, Action update, Object context)
            {
                _dataUpdaters.RemoveAll(r => r.id == id);
                _dataUpdaters.Add(new StageData(id, isAlive, update, context));
            }

            public virtual void Unregister(int id)
            {
                _dataUpdaters.RemoveAll(r => r.id == id);
            }
        }

        internal abstract class StageUpdater<T> : StageUpdater
        {
            protected StageUpdater(bool isPreStage) : base(typeof(T), isPreStage) { }
        }
        
        internal abstract class StageUpdater : DataUpdater, IDisposable
        {
            private bool _active;
            private readonly bool _isPreStage;
            private readonly Type _stageType;

            public StageUpdater(Type stageType, bool isPreStage)
            {
                _isPreStage = isPreStage;
                _stageType = stageType;
            }
            
            public override void Register(int id, Func<bool> isAlive, Action update, Object context)
            {
                base.Register(id, isAlive, update, context);
                RegisterToPlayerLoop();
            }

            public override void Unregister(int id)
            {
                base.Unregister(id);
                TryUnregisterFromPlayerLoop();
            }

            public void RegisterToPlayerLoop()
            {
                if (_active)
                {
                    return;
                }

                if (!Application.isPlaying)
                {
                    return;
                }
                
                _active = _isPreStage 
                        ? PlayerLoopHandler.Register<BindDataRefresher>(RefreshData, _stageType)
                        : PlayerLoopHandler.RegisterPost<BindDataRefresher>(RefreshData, _stageType);
            }

            private void TryUnregisterFromPlayerLoop()
            {
                if (_dataUpdaters.Count == 0 && _active && PlayerLoopHandler.Unregister(GetType()))
                {
                    _active = false;
                }
            }

            private void RefreshData()
            {
                if (_dataUpdaters.Count == 0)
                {
                    return;
                }

                if (!Application.isPlaying)
                {
                    return;
                }

                for (int i = 0; i < _dataUpdaters.Count; i++)
                {
                    var dataUpdater = _dataUpdaters[i];
                    if (!dataUpdater.isAlive())
                    {
                        _dataUpdaters.RemoveAt(i--);
                        continue;
                    }

                    try
                    {
                        dataUpdater.update();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, dataUpdater.context);
                    }
                }
            }
            
            public void Dispose()
            {
                PlayerLoopHandler.Unregister(GetType());
            }
        }
        
        internal sealed class PreStageUpdater<T> : StageUpdater<T>
        {
            public PreStageUpdater() : base(true) { }
        }
        
        internal sealed class PostStageUpdater<T> : StageUpdater<T>
        {
            public PostStageUpdater() : base(false) { }
        }
        
        private sealed class PreStageUpdater : StageUpdater
        {
            public PreStageUpdater(Type stageType) : base(stageType, true) { }
        }
        
        private sealed class PostStageUpdater : StageUpdater
        {
            public PostStageUpdater(Type stageType) : base(stageType, false) { }
        }

        internal sealed class EditTimeUpdater : DataUpdater, IDisposable
        {
            private bool _active;
            
            public override void Register(int id, Func<bool> isAlive, Action update, Object context)
            {
                base.Register(id, isAlive, update, context);
                if (!_active && _registerToEditorUpdate != null)
                {
                    _registerToEditorUpdate(RefreshData);
                    _active = true;
                }
            }

            public override void Unregister(int id)
            {
                base.Unregister(id);
                if (_dataUpdaters.Count == 0 && _active && _unregisterFromEditorUpdate != null)
                {
                    _unregisterFromEditorUpdate(RefreshData);
                    _active = false;
                }
            }
            
            private void RefreshData()
            {
                if (_dataUpdaters.Count == 0)
                {
                    return;
                }

                if (Application.isPlaying)
                {
                    return;
                }

                for (int i = 0; i < _dataUpdaters.Count; i++)
                {
                    var dataUpdater = _dataUpdaters[i];
                    if (!dataUpdater.isAlive())
                    {
                        _dataUpdaters.RemoveAt(i--);
                        continue;
                    }

                    try
                    {
                        dataUpdater.update();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, dataUpdater.context);
                    }
                }
            }

            public void Dispose()
            {
                _unregisterFromEditorUpdate?.Invoke(RefreshData);
            }
        }
    }
}
