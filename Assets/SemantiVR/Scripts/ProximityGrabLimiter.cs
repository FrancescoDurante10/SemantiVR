using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ProximityGrabLimiter : MonoBehaviour
{
    public float maxGrabDistance = 2f;

    XRGrabInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();

        interactable.selectEntered.AddListener(CheckDistance);
    }

    void CheckDistance(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject;
        float distance = Vector3.Distance(interactor.transform.position, transform.position);

        if (distance > maxGrabDistance)
        {
            interactable.interactionManager.SelectExit(interactor, interactable);
            Debug.Log($"❌ Grab troppo lontano bloccato: {gameObject.name}");
        }
    }

    void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(CheckDistance);
    }
}
