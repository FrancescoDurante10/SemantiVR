using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class RagdollActivator : MonoBehaviour
{
    [Header("Impostazioni")]
    public Animator animator;
    public float ragdollForceMultiplier = 10f;
    public float impactThreshold = 2f; // 💥 forza minima per attivare ragdoll

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private bool isRagdollActive = false;

    void Start()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        foreach (Rigidbody rb in ragdollBodies)
            rb.isKinematic = true;

        isRagdollActive = false;
    }

    public void SetRagdoll(bool active, Vector3 impactPoint = default, Vector3 impactForce = default)
    {
        if (isRagdollActive) return;
        isRagdollActive = active;

        if (animator != null)
            animator.enabled = !active;

        foreach (Rigidbody rb in ragdollBodies)
            rb.isKinematic = !active;

        if (active && impactForce != Vector3.zero)
        {
            Rigidbody hitBody = FindNearestRagdollBody(impactPoint);
            if (hitBody != null)
                hitBody.AddForceAtPosition(impactForce * ragdollForceMultiplier, impactPoint, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isRagdollActive) return;

        float force = collision.relativeVelocity.magnitude;

        if (force >= impactThreshold)
        {
            Vector3 impactPoint = collision.contacts[0].point;
            Vector3 impactDir = collision.relativeVelocity.normalized;

            SetRagdoll(true, impactPoint, impactDir);
        }
    }

    private Rigidbody FindNearestRagdollBody(Vector3 point)
    {
        Rigidbody nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Rigidbody rb in ragdollBodies)
        {
            float dist = Vector3.Distance(rb.worldCenterOfMass, point);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = rb;
            }
        }

        return nearest;
    }
}
