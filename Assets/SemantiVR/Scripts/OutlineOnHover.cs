using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))] 
[RequireComponent(typeof(Outline))]
public class OutlineOnHover : MonoBehaviour
{
    private Outline outline;
    private XRBaseInteractable interactable;

    void Awake()
    {
        outline = GetComponent<Outline>();
        interactable = GetComponent<XRBaseInteractable>();

        outline.enabled = false;

        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEnter);
            interactable.hoverExited.RemoveListener(OnHoverExit);
        }
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        outline.enabled = true;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        outline.enabled = false;
    }
}
