using UnityEngine;

namespace Postica.BindingSystem.Samples
{
    public class TotalTargetScore : MonoBehaviour
    {
        public float Points { get; set; }
        public bool IsZero { get => Points == 0; set => Points = value ? 0 : Points; }
    } 
}
