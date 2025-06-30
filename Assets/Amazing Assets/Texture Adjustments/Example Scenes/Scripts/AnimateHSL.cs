using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class AnimateHSL : MonoBehaviour
    {
        public bool generateMipmaps;

        public Texture2D sourceTexture;

        public TextureAdjustments_HueSaturationLightness hsl = new TextureAdjustments_HueSaturationLightness();


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
            hsl.hue = -180 + Mathf.PingPong(Time.time * 100, 360);

            //Render adjustment
            hsl.Render(sourceTexture, ref renderTexture, true, generateMipmaps);



            //Use renderTexture inside material
            material.SetTexture("_MainTex", renderTexture);
        }

        void OnDestroy()
        {
            //Release render texture (temporary)
            RenderTexture.ReleaseTemporary(renderTexture);

            //Release used resources
            hsl.ReleaseResources();

            //Destroy instantiated material
            if (material != null)
                DestroyImmediate(material);
        }
    }
}