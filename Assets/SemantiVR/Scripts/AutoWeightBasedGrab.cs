using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class AutoWeightBasedGrab : MonoBehaviour
{
    [Header("Massa (auto-calcolo)")]
    public float density = 300f; // Densità virtuale del materiale
    public float minMass = 0.5f;
    public float maxMass = 30f;

    [Header("Reattività al grab")]
    public float minAttachTime = 0.05f;
    public float maxAttachTime = 0.8f;

    void Start()
    {
        XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
        Rigidbody rb = GetComponent<Rigidbody>();

        float volume = EstimateObjectVolume();
        float mass = Mathf.Clamp(volume * density, minMass, maxMass);
        rb.mass = mass;

        float t = (mass - minMass) / (maxMass - minMass);

        grab.attachEaseInTime = Mathf.Lerp(minAttachTime, maxAttachTime, t);
        grab.movementType = XRGrabInteractable.MovementType.VelocityTracking;

        // Damping proporzionale alla massa, con minimi garantiti
        rb.linearDamping = Mathf.Max(1f, mass);                // Linear damping minimo 1
        rb.angularDamping = Mathf.Max(0.05f, mass * 0.05f);    // Angular damping minimo 0.05
    }

    float EstimateObjectVolume()
    {
        Collider col = GetComponent<Collider>();

        if (col is BoxCollider box)
        {
            Vector3 size = box.size;
            return size.x * size.y * size.z * VolumeScaleMultiplier();
        }
        else if (col is SphereCollider sphere)
        {
            float r = sphere.radius * transform.lossyScale.x;
            return (4f / 3f) * Mathf.PI * Mathf.Pow(r, 3);
        }
        else if (col is CapsuleCollider cap)
        {
            float r = cap.radius * transform.lossyScale.x;
            float h = cap.height * transform.lossyScale.y;
            return Mathf.PI * Mathf.Pow(r, 2) * h;
        }
        else
        {
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                Bounds b = rend.bounds;
                return b.size.x * b.size.y * b.size.z;
            }
        }

        return 1f;
    }

    float VolumeScaleMultiplier()
    {
        Vector3 s = transform.lossyScale;
        return s.x * s.y * s.z;
    }
}

