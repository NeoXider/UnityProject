using UnityEngine;

using AmazingAssets.TextureAdjustments;


namespace AmazingAssets.TextureAdjustments.Examples
{
    public class AnimateTextureBombingWithDynamicMask : MonoBehaviour
    {
        public bool generateMipmaps;

        public Texture2D sourceTexture;
        public Texture2D maskTexture;

        public TextureAdjustments_TextureBombing textureBombing = new TextureAdjustments_TextureBombing();
        public TextureAdjustments_Rotator rotator = new TextureAdjustments_Rotator();


        RenderTexture textureBombingRT;
        RenderTexture rotatorRT;

        Material material;


        
        void Start()
        {
            //No need to initialize rendertextures, Render method below will take care of both of them
            textureBombingRT = null;
            rotatorRT = null;


            //Instantiate material
            material = GetComponent<Renderer>().material;
        }

        void Update()
        {
            //Animate rotation angle
            rotator.angle = Time.time * 60;

            //Make sure wrpamode is Clamp
            rotator.wrapMode = TextureWrapMode.Clamp;

            //Render rotator adjustment
            rotator.Render(maskTexture, ref rotatorRT, true, generateMipmaps);



            //Animate count
            textureBombing.count = (int)Mathf.PingPong(Time.time * 10, 50);

            //Change random seed
            if (textureBombing.count == 0)
                textureBombing.randomSeed = Random.Range(1, 10000);

            //Make sure mask is enabled and it uses texture rendered by 'rotator'
            textureBombing.mask.isEnabled = true;
            textureBombing.mask.texture = rotatorRT;

            //Render texture bombing
            textureBombing.Render(sourceTexture, ref textureBombingRT, true, generateMipmaps);



            //Use rendered texture inside material
            material.SetTexture("_MainTex", textureBombingRT);
        }

        void OnDestroy()
        {
            //Release render texture (temporary)
            RenderTexture.ReleaseTemporary(textureBombingRT);
            RenderTexture.ReleaseTemporary(rotatorRT);

            //Release used resources
            TextureAdjustments.ReleaseResources(textureBombing, rotator);

            //Destroy instantiated material
            if (material != null)
                DestroyImmediate(material);
        }
    }
}