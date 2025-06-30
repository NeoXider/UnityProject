using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    [CreateAssetMenu(menuName = "BindingSystem/Data Object Example")]
    public class DataObject : ScriptableObject
    {
        public TargetPoints targetPoints = new TargetPoints()
        {
            greenPoints = 1,
            redPoints = 3,
            blackPoints = 7,
        };

        [Serializable]
        public struct TargetPoints 
        {
            public float redPoints;
            public float greenPoints;
            public float blackPoints;
        }
    }

}