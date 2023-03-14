using UnityEngine;
using UnityEngine.InputSystem;

public class EditorController : MonoBehaviour
{
    [SerializeField]
    private float moveByKeySpeed = 4f;
    
    [SerializeField]
    private float lookSpeedH = 2f;

    [SerializeField]
    private float lookSpeedV = 2f;

    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float dragSpeed = 3f;

    [SerializeField]
    private float shiftkeySpeedMultiplier = 5f;

    private float yaw = 0f;
    private float pitch = 0f;

    private bool buttonCameraForward;
    private bool buttonCameraBack;
    private bool buttonCameraLeft;
    private bool buttonCameraRight;
    private bool buttonCameraUp;
    private bool buttonCameraDown;
    private bool buttonCameraMultiplySpeed;
    private bool buttonCameraDrag;
    private bool buttonCameraZoomActive;
    private Vector2 look;
    private float zoom;

    public float Yaw { get => yaw; set => yaw = value; }
    public float Pitch { get => pitch; set => pitch = value; }

    private void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    private void OnZoom(InputValue value)
    {
        zoom = value.Get<float>();
    }

    private void OnCameraForward(InputValue value)
    {
        buttonCameraForward = value.isPressed;
    }

    private void OnCameraBack(InputValue value)
    {
        buttonCameraBack = value.isPressed;
    }

    private void OnCameraLeft(InputValue value)
    {
        buttonCameraLeft = value.isPressed;
    }

    private void OnCameraRight(InputValue value)
    {
        buttonCameraRight = value.isPressed;
    }

    private void OnCameraUp(InputValue value)
    {
        buttonCameraUp = value.isPressed;
    }

    private void OnCameraDown(InputValue value)
    {
        buttonCameraDown = value.isPressed;
    }

    private void OnCameraMultiplySpeed(InputValue value)
    {
        buttonCameraMultiplySpeed = value.isPressed;
    }

    private void OnCameraDrag(InputValue value)
    {
        buttonCameraDrag = value.isPressed;
    }

    private void OnCameraZoomActive(InputValue value)
    {
        buttonCameraZoomActive = value.isPressed;
    }

    private void Start()
    {
        // Initialize the correct initial rotation
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    private void Update()
    {
        float moveSpeed = moveByKeySpeed;
        if (buttonCameraMultiplySpeed)
        {
            moveSpeed *= shiftkeySpeedMultiplier;
        }

        if (buttonCameraForward)
        {
            transform.position += transform.forward * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraBack)
        {
            transform.position -= transform.forward * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraRight)
        {
            transform.position += transform.right * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraLeft)
        {
            transform.position -= transform.right * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraUp)
        {
            transform.position += transform.up * Time.deltaTime * moveSpeed;
        }
        if (buttonCameraDown)
        {
            transform.position -= transform.up * Time.deltaTime * moveSpeed;
        }

        // Look around when Mouse is not pressed
        if (!buttonCameraDrag && !buttonCameraZoomActive)
        {
            yaw += lookSpeedH * look.x;
            pitch -= lookSpeedV * look.y;

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        // Drag camera around with Middle Mouse
        if (buttonCameraDrag)
        {
            transform.Translate(-look.x * Time.deltaTime * dragSpeed, -look.y * Time.deltaTime * dragSpeed, 0);
        }

        if (buttonCameraZoomActive)
        {
            //Zoom in and out with Right Mouse
            transform.Translate(0, 0, look.x * zoomSpeed * .07f, Space.Self);
        }

        // Zoom in and out with Mouse Wheel
        transform.Translate(0, 0, zoom * zoomSpeed, Space.Self);
    }
}