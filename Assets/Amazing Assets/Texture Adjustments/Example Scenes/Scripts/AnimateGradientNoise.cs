using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class AnimateGradientNoise : MonoBehaviour
    {
        public bool generateMipmaps;

        public TextureAdjustments_NoiseGradient noise = new TextureAdjustments_NoiseGradient();

        Texture2D adjustedTexture;



        void Start()
        {
            //Create empty source texture
            Texture2D sourceTexture = new Texture2D(512, 512);

            //Declare rendertexture
            RenderTexture renderTexture = null;



            //Render adjustment
            noise.Render(sourceTexture, ref renderTexture, true, false);



            //Convert renderTexture to 2D (with mipmaps)
            adjustedTexture = renderTexture.ToTexture2D(generateMipmaps);
            adjustedTexture.wrapMode = TextureWrapMode.Clamp;



            //Release used resources
            noise.ReleaseResources();

            //Release renderTexture (temporary)
            RenderTexture.ReleaseTemporary(renderTexture);

            //Destroy sourceTexture
            if (sourceTexture != null)
                DestroyImmediate(sourceTexture);



            //Instantiate new material and use generated adjustedTexture
            GetComponent<Renderer>().material.SetTexture("_MainTex", adjustedTexture);
        }
        void OnDestroy()
        {
            DestroyImmediate(adjustedTexture);
        }
    }
}