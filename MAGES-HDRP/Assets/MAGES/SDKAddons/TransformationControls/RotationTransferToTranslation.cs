using System;
using UnityEngine;

public class RotationTransferToTranslation : MonoBehaviour
{
    
    private Transform _thisTransform;
    private Quaternion _rotationStart;
    private Vector3 _startingPosition;
    public float speed;
    private float diffAngle;
    private float prevResult;

    [Range(0.0f, 1.0f)]
    public float RotateXTransfer = 0.0f;
    [Range(0.0f, 1.0f)]
    public float RotateYTransfer = 0.0f;
    [Range(0.0f, 1.0f)]
    public float RotateZTransfer = 0.0f;


    private void Start()
    {
        _thisTransform = transform;
        _rotationStart = _thisTransform.localRotation;
        _startingPosition = _thisTransform.localPosition;
    }

    private void Update()
    {

        var result = Quaternion.Angle(_rotationStart,_thisTransform.localRotation);
        if (Math.Abs(result - prevResult) > 0)
            diffAngle += result;
        else
        {
            diffAngle = 0;
        }

        Debug.Log(result);

        var localPosition = _thisTransform.localPosition;
        var resistance = speed / 1000;

        localPosition = _startingPosition +  new Vector3(
                                                        (resistance*diffAngle) * RotateXTransfer,
                                                        (resistance * diffAngle) * RotateYTransfer,
                                                        (resistance * diffAngle) * RotateZTransfer
                                            );
        _thisTransform.localPosition = localPosition;
        _rotationStart = _thisTransform.localRotation;
        prevResult = result;
    }
}
