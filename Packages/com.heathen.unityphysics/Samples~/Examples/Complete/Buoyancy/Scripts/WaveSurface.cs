#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UnityPhysics
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class WaveSurface : SurfaceTool
    {
        //Wave height and speed
        public float scale = 0.1f;
        public float speed = 1.0f;
        //The width between the waves
        public float waveDistance = 1f;
        //Noise parameters
        public float noiseStrength = 1f;
        public float noiseWalk = 1f;

        void Update()
        {
            Shader.SetGlobalFloat("_WaterScale", scale);
            Shader.SetGlobalFloat("_WaterSpeed", speed * 10);
            Shader.SetGlobalFloat("_WaterDistance", waveDistance);
            Shader.SetGlobalFloat("_WaterTime", Time.time);
            Shader.SetGlobalFloat("_WaterNoiseStrength", noiseStrength);
            Shader.SetGlobalFloat("_WaterNoiseWalk", noiseWalk);
        }

        private float GetYPos(Vector3 position, float timeSinceStart)
        {
            float waveType = (position.x + position.z) * waveDistance;
            float noiseSample = Mathf.PerlinNoise(position.x, position.z + Mathf.Sin(timeSinceStart) * noiseWalk);
            float y = Mathf.Sin((timeSinceStart * speed * 10 + waveType) / (waveDistance * 20)) * scale;
            //y += noiseSample * noiseStrength;
            return y;
        }

        public override float SurfaceDepth(Vector3 worldPosition)
        {
            float height = GetYPos(worldPosition, Time.time);

            //float distanceToWater = worldPosition.y - height;

            return (SelfTransform.position.y + height) - worldPosition.y;
        }

        public override Vector3 SurfaceNormal(Vector3 worldPoint)
        {
            var a = worldPoint + new Vector3(0, SurfaceDepth(worldPoint), 0);
            var b = a + Vector3.right * 1;
            b = b + new Vector3(0, SurfaceDepth(b), 0);
            var c = b - Vector3.forward * 1;
            c = c + new Vector3(0, SurfaceDepth(c), 0);
            return API.MeshTools.CalculateNormal(a, b, c);
        }
    }
}


#endif