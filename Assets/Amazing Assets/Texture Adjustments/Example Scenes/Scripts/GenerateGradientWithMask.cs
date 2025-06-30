using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class GenerateGradientWithMask : MonoBehaviour
    {
        public bool generateMipmaps;

        public Texture2D sourceTexture;

        public TextureAdjustments_Gradient gradient = new TextureAdjustments_Gradient();


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
            //Animate phase
            gradient.phase = Time.time * -0.5f;

            //Render adjustment
            gradient.Render(sourceTexture, ref renderTexture, true, generateMipmaps);



            //Use renderTexture inside material
            material.SetTexture("_MainTex", renderTexture);
        }

        void OnDestroy()
        {
            //Release render texture (temporary)
            RenderTexture.ReleaseTemporary(renderTexture);

            //Release used resources
            gradient.ReleaseResources();

            //Destroy instantiated material
            if (material != null)
                DestroyImmediate(material);
        }
    }
}