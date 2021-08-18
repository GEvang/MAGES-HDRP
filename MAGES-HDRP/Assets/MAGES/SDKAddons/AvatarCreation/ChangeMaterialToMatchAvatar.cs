using UnityEngine;

public class ChangeMaterialToMatchAvatar : MonoBehaviour
{
    private Material handMaterial;
    void Start()
    {
        if (AvatarManager.Instance == null)
        {
            return;
        }

        handMaterial = Resources.Load("MAGESres/AvatarCustomization/Selections/Materials/Hands/Hand" + AvatarManager.Instance.currentCustomizationData.skinIdx, typeof(Material)) as Material;

        if (handMaterial)
        {
            gameObject.GetComponent<Renderer>().material = handMaterial;

        }
    }

}
