using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class GenerateCheckerboardWithBlur : MonoBehaviour
    {
        public bool generateMipmaps;

        public TextureAdjustments_Checkerboard checkerBoard = new TextureAdjustments_Checkerboard();
        public TextureAdjustments_BlurGaussian blur = new TextureAdjustments_BlurGaussian();

        Texture2D adjustedTexture;



        void Start()
        {
            //Create empty source texture
            Texture2D sourceTexture = new Texture2D(512, 512);

            //Declare rendertexture
            RenderTexture renderTexture = null;



            //Render all adjustments usng one 'Render' method
            TextureAdjustments.Render(sourceTexture, ref renderTexture, true, false, checkerBoard, blur);



            //Convert renderTexture to 2D (with mipmaps)
            adjustedTexture = renderTexture.ToTexture2D(generateMipmaps);
            adjustedTexture.wrapMode = TextureWrapMode.Clamp;



            //Release used resources
            TextureAdjustments.ReleaseResources(checkerBoard, blur);

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