using ovidVR.UIManagement;
using UnityEngine;

public class GenericUIResetTimer : MonoBehaviour
{
    [SerializeField, Range(0, 16), Tooltip("If value is set to zero the UI will remain forever or till the user closes it")]
    private float resetToDefaultUI = 1;
    //[SerializeField, Tooltip("If every UI spawned by UIManagement must be destroyed. Use with CAUTION")]
    //private bool resetAllUIManagement = false;

    private float deltaTimer = 0;

    void Start()
    {
        if (resetToDefaultUI == 0)
            Destroy(this);
    }

    void Update()
    {
        deltaTimer += Time.deltaTime;

        if (deltaTimer >= resetToDefaultUI)
        {
            //if (resetAllUIManagement)
            //    UIManagement.ResetUIManagement();

            Destroy(this.gameObject, 0.2f);
            deltaTimer = 0f;
        }
    }

    public void ResetTimer()
    {
        deltaTimer = 0;
    }
}
