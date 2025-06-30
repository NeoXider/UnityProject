using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class AnimateThreshold : MonoBehaviour
    {
        public bool generateMipmaps;

        public Texture2D sourceTexture;

        public TextureAdjustments_TilingAndOffset tilingAndOffset = new TextureAdjustments_TilingAndOffset();
        public TextureAdjustments_Threshold threshold = new TextureAdjustments_Threshold();


        RenderTexture renderTexture;
        Material material;


        
        void Start()
        {
            //No need to initialize 'renderTexture', Render method below will take care of it.
            renderTexture = null;


            //Instantiate material
            material = GetComponent<Renderer>().material;
        }

        void Update()
        {
            //Animate hue
            threshold.level = 1 + Mathf.PingPong(Time.time * 100, 254);

            //Render adjustment
            TextureAdjustments.Render(sourceTexture, ref renderTexture, true, generateMipmaps, tilingAndOffset, threshold);



            //Use renderTexture inside material
            material.SetTexture("_MainTex", renderTexture);
        }

        void OnDestroy()
        {
            //Release render texture (temporary)
            RenderTexture.ReleaseTemporary(renderTexture);

            //Release used resources
            threshold.ReleaseResources();
            tilingAndOffset.ReleaseResources();

            //Destroy instantiated material
            if (material != null)
                DestroyImmediate(material);
        }
    }
}