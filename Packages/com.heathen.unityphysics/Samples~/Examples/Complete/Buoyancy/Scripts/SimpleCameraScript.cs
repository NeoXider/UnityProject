#if HE_SYSCORE

using HeathenEngineering.UnityPhysics;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class SimpleCameraScript : MonoBehaviour
    {
        public Transform playerBoat;
        public Transform mainCamera;

        Vector3 pbOffset;
        Vector3 mcOffset;

        private void Start()
        {
            mcOffset = mainCamera.position - playerBoat.position;
            
        }

        private void LateUpdate()
        {
            var v = playerBoat.position + mcOffset;
            mainCamera.position = new Vector3(v.x, mainCamera.position.y, v.z);
            mainCamera.rotation = Quaternion.Euler(0, playerBoat.eulerAngles.y, 0);
        }
    }
}

#endif