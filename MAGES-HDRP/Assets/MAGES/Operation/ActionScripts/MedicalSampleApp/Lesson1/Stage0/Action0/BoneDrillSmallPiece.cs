using UnityEngine;
using ovidVR.Utilities;

public class BoneDrillSmallPiece : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Drill")
        {
            ToggleRenderer(false);
        }
    }

    public void ToggleRenderer(bool value)
    {
        gameObject.GetComponent<Renderer>().enabled = value;
        transform.GetChild(0).GetComponent<Renderer>().enabled = value;
        transform.GetChild(1).GetComponent<Renderer>().enabled = value;
        //Disable Drill Hologram
        if (!value)
            PrefabImporter.DisableCurrentHologram();

    }
}
