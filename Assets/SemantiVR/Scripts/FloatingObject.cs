using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FloatingObject : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatAmplitude = 0.25f;
    public float rotationSpeed = 30f;

    private Rigidbody rb;
    private XRGrabInteractable grab;
    private float startY;
    private bool isFloating = true;

    void Start()
    {
        startY = transform.position.y;
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();

        if (grab != null && rb != null)
        {
            grab.selectEntered.AddListener(OnGrabbed);
            grab.selectExited.AddListener(OnReleased);

            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!isFloating) return;

        float newY = startY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isFloating = false;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        this.enabled = false;
    }
}
