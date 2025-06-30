#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    public class TargetScore : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [TabGroup("Odin Tab")]
#endif
        [WriteOnlyBind]
        public Bind<float> points;

        public ReadOnlyBind<float> pointsPerHit = 1f.Bind();

        private float _totalScore;
        public float TotalPoints
        {
            get => _totalScore;
            set => points.Value = _totalScore = value;
        }

        private void OnCollisionEnter(Collision collision)
        {
            TotalPoints += pointsPerHit;   
        }
    } 
}
