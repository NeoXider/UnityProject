#if HE_SYSCORE
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{

    [CanEditMultipleObjects()]
    [CustomEditor(typeof(PhysicsData))]
    public class PhysicsDataEditor : Editor
    {
        private PhysicsData pData;
        private SerializedProperty debug;
        private SerializedProperty customHullMesh;
        private SerializedProperty ignoredMesh;

        private GUIStyle popupStyle;

        private void OnEnable()
        {
            debug = serializedObject.FindProperty(nameof(PhysicsData.debug));
            customHullMesh = serializedObject.FindProperty(nameof(PhysicsData.customHullMesh));
            ignoredMesh = serializedObject.FindProperty(nameof(PhysicsData.ignoredMesh));
        }

        public override void OnInspectorGUI()
        {
            pData = target as PhysicsData;

            EditorGUILayout.PropertyField(debug);
            EditorGUILayout.HelpBox("Custom Hull Mesh is optional, if not provided the tool will create a hull mesh on start based on the child mesh filters found within this GameObject.", MessageType.Info, true);
            EditorGUILayout.PropertyField(customHullMesh);
            EditorGUILayout.PropertyField(ignoredMesh);
            if (pData.HullGeometry != null && pData.AttachedRigidbody != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Calculated Data (Convex Hull):", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                var rect = EditorGUILayout.GetControlRect();
                var zeroPos = rect.x;
                var fullWidth = rect.width;
                rect.x += fullWidth * 0.05f;
                rect.width = fullWidth * 0.25f;
                EditorGUI.LabelField(rect, "Volume: " + pData.volume.ToString(), EditorStyles.miniLabel);
                rect.x += rect.width;
                rect.width = fullWidth * 0.6f;
                EditorGUI.LabelField(rect, "Density: " + pData.Density.ToString(), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                rect = EditorGUILayout.GetControlRect();
                fullWidth = rect.width;
                rect.x += fullWidth * 0.05f;
                rect.width = fullWidth * 0.25f;
                EditorGUI.LabelField(rect, "Area: " + pData.area.ToString(), EditorStyles.miniLabel);
                rect.x += rect.width;
                rect.width = fullWidth * 0.6f;
                EditorGUI.LabelField(rect, "Avg Cross Section: " + ((pData.xCrossSection + pData.yCrossSection + pData.zCrossSection) / 3f).ToString(), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                rect = EditorGUILayout.GetControlRect();
                fullWidth = rect.width;
                rect.x += fullWidth * 0.05f;
                rect.width = fullWidth * 0.25f;
                EditorGUI.LabelField(rect, "Speed: " + pData.LinearSpeed.ToString(), EditorStyles.miniLabel);
                rect.x += rect.width;
                rect.width = fullWidth * 0.6f;
                EditorGUI.LabelField(rect, "Heading: " + pData.LinearHeading.ToString(), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                rect = EditorGUILayout.GetControlRect();
                fullWidth = rect.width;
                rect.x += fullWidth * 0.05f;
                rect.width = fullWidth * 0.25f;
                EditorGUI.LabelField(rect, "Spin: " + pData.AngularSpeed.ToString(), EditorStyles.miniLabel);
                rect.x += rect.width;
                rect.width = fullWidth * 0.6f;
                EditorGUI.LabelField(rect, "Torque: " + pData.AngularHeading.ToString(), EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}


#endif