using UnityEngine;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class GrabWeightResistor : MonoBehaviour
{
    public float resistanceMultiplier = 2f;
    public float maxGrabbableMass = 50f; 

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private float defaultDrag;
    private float defaultAngularDrag;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        defaultDrag = rb.linearDamping;
        defaultAngularDrag = rb.angularDamping;

        grab.movementType = XRGrabInteractable.MovementType.VelocityTracking;

        grab.selectExited.AddListener(_ => ResetDrag());
        grab.selectEntered.AddListener(CheckIfTooHeavy); 
    }

    void FixedUpdate()
    {
        if (!grab.isSelected) return;

        var interactor = grab.interactorsSelecting.FirstOrDefault();
        if (interactor == null) return;

        Vector3 targetPos = interactor.GetAttachTransform(grab).position;
        Vector3 direction = targetPos - transform.position;

        float resistance = Mathf.Clamp01(1f / (rb.mass * resistanceMultiplier));
        rb.linearVelocity = direction / Time.fixedDeltaTime * resistance;
    }

    void ResetDrag()
    {
        rb.linearDamping = defaultDrag;
        rb.angularDamping = defaultAngularDrag;
    }

    void CheckIfTooHeavy(SelectEnterEventArgs args)
    {
        if (rb.mass > maxGrabbableMass)
        {
        
            grab.interactionManager.SelectExit(args.interactorObject, grab);
        }
    }

    void OnDestroy()
    {
        grab.selectExited.RemoveListener(_ => ResetDrag());
        grab.selectEntered.RemoveListener(CheckIfTooHeavy);
    }
}
