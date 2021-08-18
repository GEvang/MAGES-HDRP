using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RayFollowCamera : MonoBehaviour
{
    public float strength= 100f;
    public float cameraZ;
    private Camera _camera;
    void Start()
    {
        //copy a reference to main camera, for both convenience and performance:
        _camera = Camera.main;
    }
    public void Update()
    {
        var distance_to_screen = Camera.main.nearClipPlane;
        var pos_move = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
        transform.position = new Vector3(pos_move.x, pos_move.y, pos_move.z);

        transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, 1);

        
    }
}
