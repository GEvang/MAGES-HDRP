using System.Collections;
using ovidVR.Networking;
using UnityEngine;

[RequireComponent(typeof(NetworkMultiTransformSync))]
public class CreateDeformMeshNetworked : CreateDeformMesh
{
    private OvidVrNetworkingId _netId;
    private void Start()
    {
        _netId = GetComponent<OvidVrNetworkingId>();
    }
    
    public override void FixedUpdate()
    {
        if (_netId.HasAuthority)
        {
            base.FixedUpdate();
        }
    }

    IEnumerator MultiplayerSetUp()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponent<NetworkMultiTransformSync>().Initialise();

        if (_netId.HasAuthority)
        {
            foreach (ParticleHelper ph in particlesList)
            {
                ph.Rb.constraints =
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            }
        }
        else
        {
            center.GetComponent<Rigidbody>().isKinematic = true;
            foreach (ParticleHelper ph in particlesList)
            {
                ph.Rb.isKinematic = true;
                ph.Rb.constraints =
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }
    public override void AfterDeformMeshInitFunction()
    {
        StartCoroutine(MultiplayerSetUp());
        base.AfterDeformMeshInitFunction();
    }
}
