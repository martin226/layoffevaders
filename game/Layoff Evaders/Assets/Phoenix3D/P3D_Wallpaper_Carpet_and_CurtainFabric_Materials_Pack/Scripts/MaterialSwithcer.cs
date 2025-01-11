using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Phoenix3D
{
    public class MaterialSwithcer : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        public int materialIndex = 0;
        public int startIndex = 0;
        public Text materialName;
        public List<Material> materials = new List<Material>();

        int currentIndex = 0;

        private void Start()
        {
            currentIndex = startIndex;
            materialName.text = meshRenderer.sharedMaterials[materialIndex].name;

        }

        public void Go2NextMaterial()
        {
            currentIndex = ++currentIndex % materials.Count;

            SwitchMaterial();
        }

        public void Go2PrevMaterial()
        {
            currentIndex = --currentIndex;
            if (currentIndex < 0)
                currentIndex = materials.Count - 1;

            SwitchMaterial();
        }

        Material[] mats;
        private void SwitchMaterial()
        {
            //mats = meshRenderer.sharedMaterial;
            //Resources.UnloadAsset(meshRenderer.sharedMaterial);
            //mats = materials[currentIndex];

            ////if(currentIndex != 0)
            ////    Resources.UnloadAsset(mats[currentIndex - 1].mainTexture);

            //meshRenderer.sharedMaterial = mats;

            //materialName.text = materials[currentIndex].name;
            //Resources.UnloadUnusedAssets();

            mats = meshRenderer.sharedMaterials;
            Resources.UnloadAsset(mats[materialIndex]);
            mats[materialIndex] = materials[currentIndex];

            meshRenderer.sharedMaterials = mats;

            materialName.text = materials[currentIndex].name;
            Resources.UnloadUnusedAssets();


        }
    }

}
