using Postica.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Postica.BindingSystem
{
    // [CustomPropertyDrawer(typeof(object), true)]
    class AllTypesDrawer : StackedPropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // TODO: There is a contextualMenuDelegate in MaterialEditor, use that to add actions to properties
            // TODO: There is a BeginProperty in MaterialEditor, with all needed information, can be used to draw the binding part.
            // TODO: It has a list of data, override that to get all required information
            // MaterialEditor
            return new PropertyField(property).EnsureBind(property);
        }
    }
    
    class BindableMaterialProxy : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnGUI(position, prop, label, editor);
        }
    }
}