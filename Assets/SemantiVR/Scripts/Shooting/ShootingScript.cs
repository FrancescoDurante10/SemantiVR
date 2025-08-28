using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class ShootingScript : MonoBehaviour
{
    private XRGrabInteractable interactable;
    private Coroutine firingCoroutine;

    [Header("Weapon Settings")]
    [SerializeField] private bool isAutomatic = false; 
    [SerializeField] private float projectileForce = 30f;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firepoint;
    [SerializeField] private bool requiresTwoHands = false;

    [Header("Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private GameObject muzzleLight;

    void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        interactable.activated.AddListener(OnActivated);
        interactable.deactivated.AddListener(OnDeactivated);
    }

    void OnDestroy()
    {
        interactable.activated.RemoveListener(OnActivated);
        interactable.deactivated.RemoveListener(OnDeactivated);
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (requiresTwoHands && interactable.interactorsSelecting.Count < 2)
            return;

        if (isAutomatic)
        {
            if (firingCoroutine == null)
                firingCoroutine = StartCoroutine(AutomaticFire());
        }
        else
        {
            FireWeapon();
        }
    }

    private void OnDeactivated(DeactivateEventArgs args)
    {
        if (isAutomatic && firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
        }
    }

    private IEnumerator AutomaticFire()
    {
        while (true)
        {
            FireWeapon();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void FireWeapon()
    {
        if (projectilePrefab == null || firepoint == null) return;

        Quaternion adjustedRotation = firepoint.rotation * Quaternion.Euler(-90f, 0f, 0f);
        GameObject newProjectile = Instantiate(projectilePrefab, firepoint.position, adjustedRotation);

        Rigidbody rb = newProjectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firepoint.forward * projectileForce, ForceMode.VelocityChange);
        }

        if (audioSource != null) audioSource.PlayOneShot(audioSource.clip);
        if (muzzleFlash != null) muzzleFlash.Play();

        if (muzzleLight != null)
        {
            muzzleLight.SetActive(true);
            StartCoroutine(DisableMuzzleLight());
        }
    }

    private IEnumerator DisableMuzzleLight()
    {
        yield return new WaitForSeconds(0.05f);
        muzzleLight.SetActive(false);
    }
}
