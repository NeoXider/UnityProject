using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class AnimateTextureBombing : MonoBehaviour
    {
        public bool generateMipmaps;

        public Texture2D sourceTexture;

        public TextureAdjustments_TextureBombing textureBombing = new TextureAdjustments_TextureBombing();


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
            //Animate count
            textureBombing.count = (int)Mathf.PingPong(Time.time * 10, 50);

            //Change random seed
            if (textureBombing.count == 0)
                textureBombing.randomSeed = Random.Range(1, 10000);

            //Render texture bombing
            textureBombing.Render(sourceTexture, ref renderTexture, true, generateMipmaps);



            //Use renderTexture inside material
            material.SetTexture("_MainTex", renderTexture);
        }

        void OnDestroy()
        {
            //Release render texture (temporary)
            RenderTexture.ReleaseTemporary(renderTexture);

            //Release used resources
            textureBombing.ReleaseResources();

            //Destroy instantiated material
            if (material != null)
                DestroyImmediate(material);
        }
    }
}