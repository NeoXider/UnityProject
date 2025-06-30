#if HE_SYSCORE

using UnityEngine;
using System;
using UnityEditor;

namespace HeathenEngineering.UnityPhysics
{
    [Serializable]
    public class ScaledAnimationCurve
    {
        public float scale;
        public AnimationCurve curve;
        public float Evaluate(float weight) => curve.Evaluate(weight) * scale;

        public static ScaledAnimationCurve Linear(float scale, float startTime, float startValue, float endTime, float endValue)
        {
            return new ScaledAnimationCurve
            {
                scale = scale,
                curve = AnimationCurve.Linear(startTime, startValue, endTime, endValue)
            };
        }

        public static ScaledAnimationCurve EaseInOut(float scale, float startTime, float startValue, float endTime, float endValue)
        {
            return new ScaledAnimationCurve
            {
                scale = scale,
                curve = AnimationCurve.EaseInOut(startTime, startValue, endTime, endValue)
            };
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScaledAnimationCurve))]
    public class ScaledAnimationCurveEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var scale = property.FindPropertyRelative("scale");
            var curve = property.FindPropertyRelative("curve");
            Rect pos = EditorGUI.PrefixLabel(position, label);
            pos.width = pos.width * 0.2f;
            EditorGUI.PropertyField(pos, scale, GUIContent.none);
            pos.x += pos.width;
            pos.width *= 4f;
            EditorGUI.PropertyField(pos, curve, GUIContent.none);
        }
    }
#endif
}

#endif