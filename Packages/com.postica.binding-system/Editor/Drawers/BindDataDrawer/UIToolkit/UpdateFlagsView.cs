using System;
using Postica.Common;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Postica.BindingSystem
{
    partial class BindDataDrawer
    {
        private class UpdateFlagsView : VisualElement, SmartDropdown.IBuildingBlock
        {
            private BindData.BitFlags _flags;
            private readonly Action<BindData.BitFlags> _onChange;

            public SearchTags SearchTags { get; }
            
            public UpdateFlagsView(BindData.BitFlags flags, Action<BindData.BitFlags> onChange)
            {
                _flags = flags;
                _onChange = onChange;
            }
            
            public VisualElement GetSearchDrawer(string searchValue, Action closeWindow, bool darkMode)
            {
                return null;
            }

            public void OnPathResolved(SmartDropdown.IPathNode node)
            {
                // Nothing for now
            }

            public VisualElement GetDrawer(Action closeWindow, bool darkMode)
            {
                var container = new VisualElement().WithClass("bind-flags__container");
                var updateEditor = CreateFlagCell("EDITOR",BindData.BitFlags.UpdateInEditor);
                var updateOnChange = CreateFlagCell("ON CHANGE",BindData.BitFlags.UpdateOnChange, "C");
                var updateOnUpdate = CreateFlagCell("UPDATE", BindData.BitFlags.UpdateOnUpdate);
                var updateOnLateUpdate = CreateFlagCell("LATE UPDATE",BindData.BitFlags.UpdateOnLateUpdate);
                var updateOnFixedUpdate = CreateFlagCell("FIXED UPDATE",BindData.BitFlags.UpdateOnFixedUpdate);
                var updateOnRenderUpdate = CreateFlagCell("RENDER",BindData.BitFlags.UpdateOnPrePostRender, "R");
                
                // container.Add(new Label("Update On:").WithClass("bind-flags__label"));
                container.Add(updateEditor);
                container.Add(CreateSeparator());
                container.Add(updateOnUpdate);
                container.Add(updateOnLateUpdate);
                container.Add(updateOnRenderUpdate);
                container.Add(updateOnFixedUpdate);
                container.Add(CreateSeparator());
                container.Add(updateOnChange);
                
                return container.AddBSStyle();
            }

            private VisualElement CreateSeparator()
            {
                return new VisualElement().WithClass("bind-flags__separator");
            }

            private Toggle CreateFlagCell(string label, BindData.BitFlags flag, string initial = null)
            {
                var toggle = new Toggle(label)
                {
                    focusable = false,
                    value = _flags.HasFlag(flag),
                    tooltip = flag.GetAttribute<TooltipAttribute>()?.tooltip
                }
                    .WithClass("bind-flags__toggle")
                    .WithChildren(new Label(initial ?? label[0..1]).WithClass("bind-flags__toggle__initial"));

                toggle.RegisterValueChangedCallback(e =>
                {
                    _flags = e.newValue ? _flags | flag : _flags & ~flag;
                    _onChange?.Invoke(_flags);
                });
                return toggle;
            }
        }
    }
}