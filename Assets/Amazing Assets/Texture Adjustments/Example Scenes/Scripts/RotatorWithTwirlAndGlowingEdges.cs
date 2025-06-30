using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class RotatorWithTwirlAndGlowingEdges : MonoBehaviour
    {
        public bool generateMipmaps;

        public Texture2D sourceTexture;

        public TextureAdjustments_Rotator rotator = new TextureAdjustments_Rotator();
        public TextureAdjustments_Twirl twirl = new TextureAdjustments_Twirl();
        public TextureAdjustments_GlowingEdges glowingEdges = new TextureAdjustments_GlowingEdges();


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
            //Animate rotation angle
            rotator.angle = Time.time * 30;

            //Render all adjustments usng one 'Render' method
            TextureAdjustments.Render(sourceTexture, ref renderTexture, true, generateMipmaps, rotator, twirl, glowingEdges);



            //Use renderTexture inside material
            material.SetTexture("_MainTex", renderTexture);
        }

        void OnDestroy()
        {
            //Release render texture (temporary)
            RenderTexture.ReleaseTemporary(renderTexture);

            //Release used resources
            TextureAdjustments.ReleaseResources(rotator, twirl, glowingEdges);

            //Destroy instantiated material
            if (material != null)
                DestroyImmediate(material);
        }
    }
}