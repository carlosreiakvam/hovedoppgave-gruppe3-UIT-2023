using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MaterialManager : NetworkBehaviour
{

    [SerializeField] private Material caveMaterial;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SetCaveMaterials();
    }

    private void SetCaveMaterials()
    {
        var spawnedObjects = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList;
        foreach (var obj in spawnedObjects)
        {
            bool isInCave = obj.transform.position.x > 50f;


            if (isInCave)
            {

                SpriteRenderer[] spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in spriteRenderers) { sr.material = caveMaterial; }

                Image[] images = obj.GetComponentsInChildren<Image>();
                foreach (Image image in images) { image.material = caveMaterial; }

            }

        }

    }

}
