#if HE_SYSCORE
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [CanEditMultipleObjects()]
    [CustomEditor(typeof(ForceEffectField))]
    public class ForceEffectFieldEditor : Editor
    {
        private SerializedProperty tags;
        private SerializedProperty global;
        private SerializedProperty strength;
        private SerializedProperty radius;
        private SerializedProperty curve;
        private SerializedProperty effects;

        private void OnEnable()
        {
            tags = serializedObject.FindProperty(nameof(ForceEffectField.tags));
            global = serializedObject.FindProperty("_isGlobal");
            strength = serializedObject.FindProperty(nameof(ForceEffectField.strength));
            radius = serializedObject.FindProperty(nameof(ForceEffectField.radius));
            curve = serializedObject.FindProperty(nameof(ForceEffectField.curve));
            effects = serializedObject.FindProperty(nameof(ForceEffectField.forceEffects));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Heathen Behaviour", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(tags);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(global);
            EditorGUILayout.PropertyField(strength);
            EditorGUI.indentLevel--;
            if (!global.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Falloff", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(radius);
                EditorGUILayout.PropertyField(curve, new GUIContent("Strength Curve"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(effects);
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CanEditMultipleObjects()]
    [CustomEditor(typeof(ForceEffectDirection))]
    public class ForceEffectDirectionEditor : Editor
    {
        private SerializedProperty tags;
        private SerializedProperty global;
        private SerializedProperty strength;
        private SerializedProperty reach;
        private SerializedProperty curve;
        private SerializedProperty effects;

        private void OnEnable()
        {
            tags = serializedObject.FindProperty(nameof(ForceEffectDirection.tags));
            global = serializedObject.FindProperty("_isGlobal");
            strength = serializedObject.FindProperty(nameof(ForceEffectDirection.strength));
            reach = serializedObject.FindProperty(nameof(ForceEffectDirection.reach));
            curve = serializedObject.FindProperty(nameof(ForceEffectDirection.curve));
            effects = serializedObject.FindProperty(nameof(ForceEffectDirection.forceEffects));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Heathen Behaviour", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(tags);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(global);
            EditorGUILayout.PropertyField(strength);
            EditorGUI.indentLevel--;
            if (!global.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Falloff", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(reach);
                EditorGUILayout.PropertyField(curve, new GUIContent("Strength Curve"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(effects);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
