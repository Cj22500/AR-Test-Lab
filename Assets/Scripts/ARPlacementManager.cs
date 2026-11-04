using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets; // Needed to talk to ObjectSpawner
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Needed for interaction components

public class ARPlacementManager : MonoBehaviour
{
    [SerializeField] ObjectSpawner spawner;
    private GameObject lastSpawnedObject;

    void OnEnable()
    {
        // Listen to the event from your ObjectSpawner script
        if (spawner != null)
            spawner.objectSpawned += OnObjectSpawned;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent errors
        if (spawner != null)
            spawner.objectSpawned -= OnObjectSpawned;
    }

    private void OnObjectSpawned(GameObject obj)
    {
        lastSpawnedObject = obj;
    }

    // This is the function your UI Button will call
    public void DropCurrentObject()
    {
        if (lastSpawnedObject != null)
        {
            // Disable the Grab Interactable so it can't be moved
            var interactable = lastSpawnedObject.GetComponent<XRGrabInteractable>();
            if (interactable != null)
                interactable.enabled = false;

            // Clear the reference so you don't drop the same thing twice
            lastSpawnedObject = null;
            Debug.Log("Object Locked in Place!");
        }
    }
}
