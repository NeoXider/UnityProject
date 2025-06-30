using Postica.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Postica.BindingSystem
{
    partial class BindDataDrawer
    {
        internal partial struct ModifierData
        {
            public const string ussModifierViewHeader = "modifier-view__header";
            public const string ussModifierViewRestOfHeader = "modifier-view__header-rest";
            public const string ussModifierViewFixedLabel = "modifier-view__fixed-label";
            public const string ussModifierViewFoldout = "modifier-view__foldout";
            public const string ussModifierViewProperty = "modifier-view__property";

            public VisualElement GetOrCreateView(Contents contents, Action onBindModeClicked, Action onRemove)
            {
                if (view != null && (view.name == "modifier-header") == (properties.Length == 0))
                {
                    return view;
                }

                var (root, header, restOfHeader) = PrepareViews();

                restOfHeader.Add(new Image()
                {
                    tooltip = "This modifier is not compatible with current bind mode"
                }.WithClass("modifier-view__incompatible-icon"));

                bindModeButton = new Button(onBindModeClicked) { focusable = false }
                                    .WithClass("modifier-view__bind-mode")
                                    .WithChildren(new Image().WithClass("modifier-view__bind-mode__icon"));
                
                restOfHeader.Add(bindModeButton);

                var removeButton = new Button(onRemove)
                {
                    focusable = false,
                    text = contents.modifierRemove.text,
                    tooltip = contents.modifierRemove.tooltip
                }
                .WithClass("modifier-view__remove");

                restOfHeader.Add(removeButton);

                view = root;

                return view;
            }

            private (VisualElement view, VisualElement header, VisualElement restOfHeader) PrepareViews()
            {
                var header = new VisualElement() { name = "modifier-header" }.WithClass(ussModifierViewHeader);
                var restOfHeader = new VisualElement().WithClass(ussModifierViewRestOfHeader);

                if (properties.Length == 0)
                {
                    header.WithChildren(new Label(collapsedContent.text).WithClass(ussModifierViewFixedLabel), restOfHeader);
                    return (header, header, restOfHeader);
                }

                if (properties.Length == 1 && isOneLine)
                {
                    header.WithChildren(properties.properties[0].view.WithClass(ussModifierViewProperty), restOfHeader);
                    return (header, header, restOfHeader);
                }

                var foldout = new Foldout()
                {
                    tooltip = collapsedContent.tooltip,
                    value = false,
                }.WithClass(ussModifierViewFoldout);

                var modifierSnapshot = modifier;
                var mainProperty = properties.mainProperty;
                var canUpdateText = !isOneLine;

                if (canUpdateText)
                {
                    foldout.OnAttachToPanel(evt =>
                    {
                        foldout.text = foldout.value
                            ? modifierSnapshot.Id
                            : modifierSnapshot.Id + "  " + modifierSnapshot.ShortDataDescription;
                    }, 0);
                }

                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (evt.target != foldout) return;

                    if (mainProperty.IsAlive())
                    {
                        mainProperty.isExpanded = evt.newValue;
                    }

                    if (canUpdateText)
                    {
                        foldout.text = evt.newValue
                            ? modifierSnapshot.Id
                            : modifierSnapshot.Id + "  " + modifierSnapshot.ShortDataDescription;
                    }
                });

                var foldoutToggle = foldout.hierarchy.ElementAt(0) as Toggle;
                foldoutToggle?.RemoveFromHierarchy();
                
                if (isOneLine)
                {
                    foldout.text = null;
                    restOfHeader.Add(properties.properties[0].view.WithClass(ussModifierViewProperty));
                }
                else
                {
                    foldout.text = foldout.value ? expandedContent.text : collapsedContent.text;
                    foldout.Add(properties.properties[0].view.WithClass(ussModifierViewProperty));
                }
                
                for (var index = 1; index < properties.properties.Length; index++)
                {
                    var property = properties.properties[index];
                    foldout.Add(property.view.WithClass(ussModifierViewProperty));
                }

                foldout.value = properties.mainProperty.isExpanded;

                foldout.hierarchy.Insert(0, header.WithChildren(foldoutToggle, restOfHeader));

                return (foldout, header, restOfHeader);
            }
        }

        internal readonly partial struct Modifiers
        {

            public class ModifiersView : VisualElement
            {
                private readonly Contents _contents;
                private ListView _list;
                private ScrollView _scrollView;
                private Action<VisualElement, VisualElement, Vector2, Vector2> _addConnectingLinesCallback;
                private bool _connectingLinesUpdateRequested;
                private PropertyData _data;
                private Action<PropertyData> _updateData;

                public ModifiersView(PropertyData data, Contents contents)
                {
                    AddToClassList("bind-modifiers");
                    _contents = contents;
                    ref var modifiers = ref data.modifiers;

                    _list = new ListView()
                    {
                        reorderable = true,
                        reorderMode = ListViewReorderMode.Animated,
                        showAddRemoveFooter = false,
                        showFoldoutHeader = false,
                        showBorder = false,
                        showBoundCollectionSize = false,
                        selectionType = SelectionType.None,
                        virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                        horizontalScrollingEnabled = false,

                        destroyItem = DestroyModifierView,
                        makeItem = MakeModifierView,
                        bindItem = BindModifierView,
                        unbindItem = UnbindModifierView,
                        
                        itemsSource = modifiers.array,
                        
                        viewDataKey = data.properties.modifiers.GetViewDataKey()
                    }
                    .WithClass("bind-modifiers__list");

                    Add(_list);
                    _list.itemIndexChanged += ModifierIndexChanged;
                }

                private void ModifierIndexChanged(int from, int to)
                {
                    if(_data == null)
                    {
                        return;
                    }

                    // Exchange expand states of their respective properties
                    ref var modifiers = ref _data.modifiers;
                    var fromExpandState = modifiers.array[from].properties.mainProperty.isExpanded;

                    if (from > to)
                    {
                        for (int i = from; i > to; i--)
                        {
                            modifiers.array[i].properties.mainProperty.isExpanded =
                                modifiers.array[i - 1].properties.mainProperty.isExpanded;
                        }
                        modifiers.array[to].properties.mainProperty.isExpanded = fromExpandState;
                    }
                    else
                    {
                        for (int i = to; i < from; i++)
                        {
                            modifiers.array[i].properties.mainProperty.isExpanded =
                                modifiers.array[i + 1].properties.mainProperty.isExpanded;
                        }
                        modifiers.array[to].properties.mainProperty.isExpanded = fromExpandState;
                    }
                    
                    _data.properties.modifiers.MoveArrayElement(from, to);
                    _data.modifiers = default;
                    _updateData?.Invoke(_data);
                }

                public void AddConnectingLines(Action<VisualElement, VisualElement, Vector2, Vector2> addConnectingLineCallback)
                {
                    _addConnectingLinesCallback = addConnectingLineCallback;

                    UpdateConnectingLines();
                }

                private void RequestConnectingLinesUpdate(long delayMs = 0)
                {
                    if (!_connectingLinesUpdateRequested || delayMs > 0)
                    {
                        _connectingLinesUpdateRequested = true;
                        schedule.Execute(UpdateConnectingLines).ExecuteLater(delayMs);
                    }
                }

                private void UpdateConnectingLines()
                {
                    _connectingLinesUpdateRequested = false;

                    var items = _list.Query(className: ListView.itemUssClassName).ToList();

                    if (_scrollView == null)
                    {
                        _scrollView = this.Q<ScrollView>();
                        if (_scrollView != null)
                        {
                            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                        }
                    }

                    foreach (var item in items)
                    {
                        var connectingLine = item.Q<ConnectingLine>();
                        if (connectingLine != null)
                        {
                            connectingLine.RemoveFromHierarchy();
                        }

                        var handle = item.Q(null, ListView.reorderableItemHandleBarUssClassName);
                        var modifierView = item.Q(null, "modifier-view");
                        _addConnectingLinesCallback?.Invoke(handle, modifierView, new Vector2(-3, 2), default);
                    };
                }

                private void UnbindModifierView(VisualElement element, int index)
                {
                    // Do nothing for now
                }

                private void BindModifierView(VisualElement element, int index)
                {
                    ref var modifiers = ref _data.modifiers;

                    ref var modifier = ref modifiers.array[index];

                    var mode = _data.properties.BindMode;

                    modifier.containerView = element;
                    
                    element.EnableInClassList("modifier-view--one-line", modifier.isOneLine);
                    element.EnableInClassList("modifier-view--more-properties", modifier.properties.properties.Length > 1);

                    void RefreshView()
                    {
                        ref var modifier = ref _data.modifiers.array[index];

                        var isIncompatible = !modifier.BindMode.IsCompatibleWith(mode);

                        element.EnableInClassList("incompatible", isIncompatible);
                        //element.parent.parent.EnableInClassList("hidden", isIncompatible && !showIncompatibleModifiers);

                        var bindMode = modifier.BindMode;
                        modifier.bindModeButton.SetEnabled(modifier.canChangeMode);
                        modifier.bindModeButton.EnableInClassList("hidden", !modifier.canChangeMode && modifier.isOneLine);
                        modifier.bindModeButton.EnableInClassList("read", bindMode.CanRead());
                        modifier.bindModeButton.EnableInClassList("write", bindMode.CanWrite());
                        modifier.bindModeButton.tooltip = _contents.bindModesForModifiers[(int)bindMode].tooltip;
                    }


                    var view = modifier.GetOrCreateView(_contents, 
                        () =>
                        {
                            ref var modifierData = ref _data.modifiers.array[index];
                            Undo.RecordObjects(_data.serializedObject.targetObjects, "Change Modifier Bind Mode");
                            modifierData.SetBindMode(modifierData.BindMode.NextMode());
                            RefreshView();
                        },
                        () =>
                        {
                            _data.properties.modifiers.DeleteArrayElementAtIndex(index);
                            _data.modifiers = default;
                            _updateData(_data);
                        }).WithClass("modifier-view__item");
                    
                    RefreshView();

                    var prevView = element.Q(null, "modifier-view__item");
                    prevView?.RemoveFromHierarchy();
                    element.Add(view);

                    RequestConnectingLinesUpdate();
                }

                private VisualElement MakeModifierView()
                {
                    return new VisualElement().WithClass("modifier-view");
                }

                private void DestroyModifierView(VisualElement element)
                {
                    // Nothing for now
                }

                public void Refresh(PropertyData data, Action<PropertyData> updateData)
                {
                    _data = data;
                    _updateData = updateData;

                    EnableInClassList("minimal", true);

                    if (_list == null)
                    {
                        EnableInClassList("hidden", true);
                        return;
                    }

                    ref var modifiers = ref data.modifiers;

                    EnableInClassList("hidden", modifiers.array == null || modifiers.array.Length == 0);

                    if (modifiers.array == null)
                    {
                        _list.itemsSource = modifiers.array;
                        _list.Rebuild();
                        return;
                    }
                    
                    _list.itemsSource = modifiers.array;
                    _list.Rebuild();
                    
                    for (int i = 0; i < modifiers.array.Length; i++)
                    {
                        ref var modifier = ref modifiers.array[i];
                        modifier.properties.Refresh();
                    }

                    RequestConnectingLinesUpdate();
                }

            }
        }
    }
}