using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircleLine : MonoBehaviour
{
    public int NumSegments { get; internal set; } = 50;
    public float Radius { get; internal set; } = 0.05f;
    internal LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = NumSegments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        CreatePositions();
    }

    internal void CreatePositions()
    {
        float angle = 0.0f;
        for (int i = 0; i < lineRenderer.positionCount; ++i)
        {
            lineRenderer.SetPosition(i, new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle), 0) * Radius);
            angle += 360.0f / NumSegments;
        }

        lineRenderer.SetPosition(lineRenderer.positionCount - 1, lineRenderer.GetPosition(0));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
