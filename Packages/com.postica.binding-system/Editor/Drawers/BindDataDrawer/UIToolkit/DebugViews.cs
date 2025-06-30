using Postica.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = Postica.Common.PopupWindow;
namespace Postica.BindingSystem
{
    partial class BindDataDrawer
    {
        internal class DebugViews
        {
            private List<DebugView> _readViews = new List<DebugView>();
            private List<DebugView> _writeViews = new List<DebugView>();
            private List<VisualElement> _containers = new List<VisualElement>();
            private VisualElement _rootContainer;
            private PropertyData _data;

            public PropertyData Data
            {
                get => _data;
                set
                {
                    if(_data == value)
                    {
                        return;
                    }

                    _data = value;
                }
            }

            public void Rebuild(PropertyData data, VisualElement root)
            {
                Clear();
                Data = data;
                _rootContainer = root;
                if (!_data.properties.property.IsAlive())
                {
                    return;
                }
                var bindMode = _data.properties.BindMode;
                // Add the container for pathview
                var pathContainer = CreateContainer(null, "path-debug").WithClass("first");
                _containers.Add(pathContainer);

                var containerInsertIndex = 2; // Right after path and target

                var pathView = root.Q(null, "bind-data__path");
                var pathViewIndex = pathView.parent.IndexOf(pathView);
                pathView.parent.Insert(containerInsertIndex, pathContainer);

                if (bindMode.CanRead())
                {
                    var readPart = AddPart(pathContainer, "input", true);
                    var pathDebugView = new GenericDebugView()
                    {
                        readFunc = v => _data.bindDataDebug?.GetRawData(),
                        container = readPart,
                        isValid = false,
                    };
                    _readViews.Add(pathDebugView);
                }

                var nextWriteContainer = pathContainer;
                VisualElement lastWritePart = null;
                VisualElement lastReadPart = pathContainer;
                VisualElement lastContainer = pathContainer;

                // Add the container for converter
                var converterContainer = CreateContainer(null, "converter-debug");
                containerInsertIndex += 2;

                if (data.readConverter.instance != null && bindMode.CanRead())
                {
                    _containers.Add(converterContainer);

                    root.Insert(containerInsertIndex, converterContainer);

                    var part = AddPart(converterContainer, "converted", true);
                    lastReadPart = part;
                    lastContainer = converterContainer;

                    var converterDebugView = new ConverterDebugView()
                    {
                        converter = _data.readConverter.instance,
                        targetType = data.bindType,
                        container = part,
                        isValid = false,
                    };
                    _readViews.Add(converterDebugView);
                }

                // Repeat for write converter
                if (data.writeConverter.instance != null && bindMode.CanWrite())
                {
                    if (!_containers.Contains(converterContainer))
                    {
                        _containers.Add(converterContainer);
                    }

                    root.Insert(containerInsertIndex, converterContainer);

                    var part = AddPart(nextWriteContainer, "converted", false);
                    nextWriteContainer = converterContainer;
                    lastWritePart = part;
                    lastContainer = converterContainer;
                    Type toType = null;
                    try
                    {
                        toType = AccessorsFactory
                            .GetMemberAtPath(data.sourceTarget.GetType(), data.properties.path.stringValue)
                            ?.GetMemberType();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    var converterDebugView = new ConverterDebugView()
                    {
                        converter = _data.writeConverter.instance,
                        targetType = toType,
                        container = part,
                        isValid = false
                    };

                    _writeViews.Insert(0, converterDebugView);
                }

                // Add the container for modifiers
                var modifiersArray = _data.modifiers.array;
                for (int i = 0; i < modifiersArray.Length; i++)
                {
                    ref var modifier = ref modifiersArray[i];
                    if (!modifier.BindMode.IsCompatibleWith(bindMode))
                    {
                        continue;
                    }

                    var view = modifier.containerView;

                    if(view == null)
                    {
                        continue;
                    }

                    // First remove any containers
                    view.Query(null, "debug-container").ForEach(v => v.RemoveFromHierarchy());

                    var modifierContainer = CreateContainer(null, "modifier-debug-" + i);
                    _containers.Add(modifierContainer);
                    view.hierarchy.Add(modifierContainer);

                    if (modifier.BindMode.CanRead())
                    {
                        var readPart = AddPart(modifierContainer, "mod-" + i, true);
                        lastReadPart = readPart;
                        lastContainer = modifierContainer;
                        var debugView = new GenericDebugView()
                        {
                            readFunc = modifier.readFunc,
                            container = readPart,
                            isValid = false,
                        };
                        _readViews.Add(debugView);
                    }
                    if (modifier.BindMode.CanWrite())
                    {
                        var label = "mod-" + i;
                        if (lastWritePart != null)
                        {
                            lastReadPart.Q<Label>(null, "debug-container__part__label").text = label;
                        }
                        else
                        {
                            label = "output";
                        }
                        var writePart = AddPart(nextWriteContainer, label, false);
                        nextWriteContainer = modifierContainer;
                        lastWritePart = writePart;
                        lastContainer = modifierContainer;

                        var debugView = new GenericDebugView()
                        {
                            readFunc = modifier.writeFunc,
                            container = writePart,
                            isValid = false,
                        };
                        _writeViews.Insert(0, debugView);
                    }
                }

                if (bindMode.CanWrite())
                {
                    var writePart = AddPart(nextWriteContainer, "input", false);
                    var pathDebugView = new GenericDebugView()
                    {
                        readFunc = v => _data.bindDataDebug?.DebugValue,
                        container = writePart,
                        isValid = false,
                    };
                    _writeViews.Insert(0, pathDebugView);
                }

                lastReadPart.Q<Label>(null, "debug-container__part__label").text = "output";

                lastContainer.WithClass("last");

                EditorApplication.update -= Update;
                EditorApplication.update += Update;

                pathContainer.RegisterCallback<DetachFromPanelEvent>(evt =>
                {
                    EditorApplication.update -= Update;
                });
            }

            public void Clear()
            {
                if(_containers.Count == 0)
                {
                    return;
                }

                EditorApplication.update -= Update;

                foreach (var view in _readViews)
                {
                    view?.Clear();
                }

                foreach(var view in _writeViews)
                {
                    view?.Clear();
                }

                foreach (var container in _containers)
                {
                    container?.RemoveFromHierarchy();
                }

                _readViews.Clear();
                _writeViews.Clear();
                _containers.Clear();
            }

            public void Update()
            {
                if (_data == null)
                {
                    return;
                }
                
                if(!_data.properties.mode.IsAlive())
                {
                    return;
                }

                if (_data.properties.BindMode.CanRead())
                {
                    UpdateValues(null, _readViews);
                }

                if (_data.properties.BindMode.CanWrite())
                {
                    var bindData = _data.properties.property.GetValue() as IBindDataDebug;
                    UpdateValues(bindData?.DebugValue, _writeViews);
                }
            }

            private VisualElement CreateContainer(string label, string classname)
            {
                var container = new VisualElement().WithClass("debug-container");
                if (!string.IsNullOrEmpty(classname))
                {
                    container.AddToClassList(classname);
                }
                if (!string.IsNullOrEmpty(label))
                {
                    container.Add(new Label(label).WithClass("debug-container__label"));
                }
                return container;
            }

            private VisualElement AddPart(VisualElement container, string label, bool isRead)
            {
                var classname = isRead ? "read" : "write";
                var subContainer = new VisualElement().WithClass("debug-container__part").WithClass(classname)
                                        .WithChildren(new Image().WithClass("debug-container__part__icon").WithClass(classname));
                container.Add(subContainer);

                if (!string.IsNullOrEmpty(label))
                {
                    subContainer.Insert(0, new Label(label).WithClass("debug-container__part__label"));
                }

                subContainer.RegisterCallback<MouseEnterEvent>(evt => HoverAllViews(true, isRead));
                subContainer.RegisterCallback<MouseLeaveEvent>(evt => HoverAllViews(false, isRead));
                return subContainer;
            }

            private void HoverAllViews(bool hover, bool isRead)
            {
                var list = isRead ? _readViews : _writeViews;
                foreach(var view in list)
                {
                    view.container.EnableInClassList("highlight", hover);
                }
            }

            private void UpdateValues(object startValue, List<DebugView> views)
            {
                if (!_data.properties.mode.IsAlive())
                {
                    return;
                }
                
                // if (!_data.properties.BindMode.CanRead())
                // {
                //     return;
                // }

                object value = startValue;

                for (int i = 0; i < views.Count; i++)
                {
                    var view = views[i];
                    if(!view.TryApplyValue(value, out value))
                    {
                        for (int j = i + 1; j < views.Count; j++)
                        {
                            views[j].isValid = false;
                        }
                    }
                }
            }

            private string BuildSourceInfo(IBindDataDebug bindDebug)
            {
                return $"{bindDebug.Source}.{Accessors.AccessorPath.CleanPath(bindDebug.Path.Replace("Array.data", ""))}";
            }

            private static VisualElement CreateExceptionInfo(Exception ex, float paddingShift = 0)
            {
                var container = new VisualElement();
                const float padding = 8;
                container.style.paddingBottom = padding;
                container.style.paddingTop = padding;
                container.style.paddingLeft = padding + paddingShift;
                container.style.paddingRight = padding;

                const float border = 1;
                container.style.borderBottomWidth = border;
                container.style.borderTopWidth = border;
                container.style.borderLeftWidth = border;
                container.style.borderRightWidth = border;

                var borderColor = Color.red;
                container.style.borderBottomColor = borderColor;
                container.style.borderTopColor = borderColor;
                container.style.borderLeftColor = borderColor;
                container.style.borderRightColor = borderColor;

                var type = new Label(ex.GetType().Name);
                type.style.fontSize = 12;
                type.style.color = Color.red;
                type.style.unityFontStyleAndWeight = FontStyle.Bold;

                if(ex.InnerException != null)
                {
                    container.Add(CreateExceptionInfo(ex.InnerException, 8));
                    return container;
                }

                var message = new Label(ex.Message);
                message.style.fontSize = 11;
                message.style.color = Color.red.Green(0.4f).Blue(0.4f);
                message.style.whiteSpace = WhiteSpace.Normal;

                var source = new Label("Source: " + ex.Source);
                source.style.fontSize = 9;
                source.style.whiteSpace = WhiteSpace.Normal;

                var sb = new StringBuilder();
                var method = ex.TargetSite;
                if(method != null)
                {
                    sb.AppendLine($"{method.ReflectedType.Name}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
                }

                sb.AppendLine().Append(ex.StackTrace);

                var targetSite = new ScrollView();
                targetSite.style.marginTop = 12;

                var stackTrace = new Label("[For Devs]: " + sb);
                stackTrace.style.fontSize = 10;
                stackTrace.style.whiteSpace = WhiteSpace.Normal;
                
                targetSite.Add(stackTrace);

                container.Add(type);
                container.Add(message); 
                container.Add(source);
                container.Add(targetSite);
                return container;
            }

            private sealed class PathDebugView : DebugView
            {
                public IBindDataDebug bindDataDebug;
                public override object ApplyValue(object value) => bindDataDebug.GetRawData();
            }

            private sealed class ConverterDebugView : DebugView
            {
                public IConverter converter;
                public Type targetType;
                
                private Func<object, object> _getValueFromProviderFunc;

                public override object ApplyValue(object value)
                {
                    try
                    {
                        if (_getValueFromProviderFunc != null)
                        {
                            return _getValueFromProviderFunc(value);
                        }
                        return converter.Convert(value);
                    }
                    catch (InvalidCastException) { }

                    if (value is IValueProvider valueProvider)
                    {
                        var providerType = valueProvider.GetType().GetInterface(typeof(IValueProvider<int>).Name);
                        var methodInfo = typeof(ConverterDebugView).GetMethod(nameof(GetGetValueFromProviderFunc), BindingFlags.NonPublic | BindingFlags.Static);
                        var genericMethod = methodInfo.MakeGenericMethod(providerType, targetType);
                        var specificMethod = genericMethod.CreateDelegate(typeof(Func<IConverter, Func<object, object>>)) as Func<IConverter, Func<object, object>>;
                        _getValueFromProviderFunc = specificMethod(converter);
                        return _getValueFromProviderFunc?.Invoke(value);
                    }
                    
                    return converter.Convert(value);
                }
                
                private static Func<object, object> GetGetValueFromProviderFunc<TProvider, T>(IConverter converter)
                {
                    if (converter is not IConverter<TProvider, T> specificConverter)
                    {
                        return null;
                    }
                    return v => v is TProvider valueProvider ? specificConverter.Convert(valueProvider) : converter.Convert(v);
                }
            }

            private sealed class GenericDebugView : DebugView
            {
                public Func<object, object> readFunc;

                public override object ApplyValue(object value) => readFunc(value);
            }

            private abstract class DebugView
            {
                public VisualElement container;
                public VisualElement field;
                public object value;
                public Exception exception;

                public bool isValid
                {
                    get => field?.ClassListContains("invalid") == false;
                    set
                    {
                        if(field == null)
                        {
                            field = new Label("Undefined").WithClass("debug-view").WithClass("undefined");
                            container.Add(field);
                        }
                        container.EnableInClassList("invalid", !value);
                    }
                }

                public abstract object ApplyValue(object value);

                public bool TryApplyValue(object input, out object output)
                {
                    try
                    {
                        output = ApplyValue(input);
                        SetValue(output);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        SetValue(ex);
                        output = null;
                        return false;
                    }
                }

                public void SetValue(object value)
                {
                    if(this.value == value || exception == value)
                    {
                        return;
                    }

                    if(value is Exception ex)
                    {
                        exception = ex;
                    }
                    else
                    {
                        this.value = value;
                    }

                    var newField = GetFieldWithValue(value);
                    isValid = true;

                    if (field != newField || field?.parent != container)
                    {
                        field?.RemoveFromHierarchy();
                        field = newField;
                        container.Add(field);
                    }
                }

                private S GetBaseField<S, T>(T value, Func<S> factory) where S : BaseField<T>
                {
                    if(field is not S)
                    {
                        field?.RemoveFromHierarchy();
                        field = factory().WithClass("debug-view");
                        field.focusable = true;
                    }

                    var sfield = field as S;
                    sfield.value = value;
                    return sfield;
                }

                private S GetBaseField<S, T>(T value) where S : BaseField<T>, new()
                {
                    if (field is not S)
                    {
                        field?.RemoveFromHierarchy();
                        field = new S() { focusable = true }.WithClass("debug-view");
                    }

                    var sfield = field as S;
                    sfield.value = value;
                    return sfield;
                }

                private S GetField<S, T>(T value, Action<S, T> setter) where S : VisualElement, new()
                {
                    if (field is not S)
                    {
                        field?.RemoveFromHierarchy();
                        field = new S() { focusable = true }.WithClass("debug-view");
                    }

                    var sfield = field as S;
                    setter(sfield, value);
                    return sfield;
                }

                public VisualElement GetFieldWithValue(object value)
                {
                    if(value is Exception ex)
                    {
                        if(field is not Button button)
                        {
                            button = new Button().WithClass("exception-field");
                            button.clicked += () => PopupWindow.Show(GUIUtility.GUIToScreenRect(container.worldBound),
                                                 new Vector2(Mathf.Max(300, container.layout.width), 300),
                                                 CreateExceptionInfo(exception));
                        }
                        button.text = ex.GetType().Name;
                        return button;
                    }
                    return value switch
                    {
                        Color v => GetBaseField<ColorField, Color>(v),
                        Vector2Int v => GetBaseField<Vector2IntField, Vector2Int>(v),
                        Vector3Int v => GetBaseField<Vector3IntField, Vector3Int>(v),
                        Vector2 v => GetBaseField<Vector2Field, Vector2>(v),
                        Vector3 v => GetBaseField<Vector3Field, Vector3>(v),
                        Vector4 v => GetBaseField<Vector4Field, Vector4>(v),
                        Quaternion v => GetBaseField<Vector4Field, Vector4>(v.eulerAngles),
                        Rect v => GetBaseField<RectField, Rect>(v),
                        RectInt v => GetBaseField<RectIntField, RectInt>(v),
                        Bounds v => GetBaseField<BoundsField, Bounds>(v),
                        BoundsInt v => GetBaseField<BoundsIntField, BoundsInt>(v),
                        AnimationCurve v => GetBaseField<CurveField, AnimationCurve>(v),
                        Gradient v => GetBaseField<GradientField, Gradient>(v),
                        LayerMask v => GetBaseField<LayerMaskField, int>(v),
                        Enum v => GetBaseField<EnumField, Enum>(v),
                        Object v => GetBaseField<ObjectField, Object>(v),
                        string v => GetBaseField<TextField, string>(v),
                        bool v => GetBaseField<Toggle, bool>(v),
                        int v => GetBaseField<IntegerField, int>(v),
                        float v => GetBaseField<FloatField, float>(v),
                        double v => GetBaseField<DoubleField, double>(v),
                        long v => GetBaseField<LongField, long>(v),
                        ulong v => GetBaseField<LongField, long>((long)v),
                        byte v => GetBaseField<IntegerField, int>(v),
                        sbyte v => GetBaseField<IntegerField, int>(v),
                        short v => GetBaseField<IntegerField, int>(v),
                        ushort v => GetBaseField<IntegerField, int>(v),
                        uint v => GetBaseField<IntegerField, int>((int)v),
                        IntPtr v => GetBaseField<IntegerField, int>((int)v),
                        UIntPtr v => GetBaseField<IntegerField, int>((int)v),
                        char v => GetBaseField<TextField, string>(new string(v, 1)),
                        Color32 v => GetBaseField<ColorField, Color>(v),
                        _ => GetBaseField<TextField, string>(value?.ToString()),
                    };
                }

                internal void Clear()
                {
                    field?.Clear();
                }
            }
        }
    }
}