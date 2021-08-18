using System;
using UnityEngine;

public class KeyboardMovement : MonoBehaviour
{
    private Transform head;
    private CharacterController controller;
    [Range(0.5f, 2f), Tooltip("Set Translation Speed")]
    public float translationSpeed = 1.2f;

    [Range(1, 10)]
    private int quartileSteps = 4;
    private Vector3 moveDirection = Vector3.zero;

    private void Awake()
    {
        head = transform.Find("head");

        InitializeMovement();
    }

    public void InitializeMovement()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("No CharacterController found");
            this.enabled = false;
        }

        controller.detectCollisions = true;
    }

    void Update()
    {
        CameraTranslation();
    }

    private void CameraTranslation()
    {
        // Body Movement
        Vector3 front = head.forward;
        Vector3 right = Vector3.Cross(front, Vector3.up);

        //Maintain movement on a single plane
        front.y = 0;
        right.y = 0;

        //Up-Down
        Vector3 upvector = new Vector3();

        if (Input.GetKey(KeyCode.Q))
            upvector.y = -0.01f * translationSpeed;
        if (Input.GetKey(KeyCode.E))
            upvector.y = 0.01f * translationSpeed;


        moveDirection = front * (translationSpeed * GetHorizontalAxis() * Time.deltaTime) +
                        right * (translationSpeed * GetVerticalAxis() * Time.deltaTime) +
                        upvector;

        if (Input.GetKeyDown(KeyCode.R))
            transform.Rotate(new Vector3(0, 1, 0), -(90f / (float)quartileSteps), Space.Self);

        if (Input.GetKeyDown(KeyCode.T))
            transform.Rotate(new Vector3(0, 1, 0), (90f / (float)quartileSteps), Space.Self);

        controller.Move(moveDirection);


    }

    private float GetHorizontalAxis()
    {
        return Convert.ToInt32(Input.GetKey(KeyCode.W)) - Convert.ToInt32(Input.GetKey(KeyCode.S));
    }

    private float GetVerticalAxis()
    {
        return Convert.ToInt32(Input.GetKey(KeyCode.A)) - Convert.ToInt32(Input.GetKey(KeyCode.D));
    }

}
