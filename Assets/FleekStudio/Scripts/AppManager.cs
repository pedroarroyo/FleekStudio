using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XZIMG;

namespace FleekStudio
{
    public class AppManager : MonoBehaviour
    {
        [SerializeField] private XmgMagicFaceManager _eyebrowOverlayManager;
        [SerializeField] private string eyebrowTextureFolder;
        
        private Object[] eyebrowTextures;
        private int eyebrowTextureIndex = -1;
        void Start()
        {
            eyebrowTextures = Resources.LoadAll(eyebrowTextureFolder, typeof(Texture2D));
        }

        public void NextEyebrow()
        {
            if (eyebrowTextures.Length == 0)
            {
                return;
            }
            
            // Changes texture on eyebrow overlay. 
            eyebrowTextureIndex++;

            if (eyebrowTextureIndex >= eyebrowTextures.Length)
            {
                eyebrowTextureIndex = 0;
            }
            
            Texture2D texture = (Texture2D)eyebrowTextures[eyebrowTextureIndex];
            Renderer eyebrowOverlayRenderer = _eyebrowOverlayManager.FacePrefabInstanciated.GetComponent<Renderer>();
            eyebrowOverlayRenderer.material.mainTexture = texture;
        }
    }
}
