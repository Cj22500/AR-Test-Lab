using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ObjectFinalizer : MonoBehaviour
{
    public void DropObject()
    {
        // 1. Find the XR Grab Interactable component
        var interactable = GetComponent<XRGrabInteractable>();
        
        if (interactable != null)
        {
            // 2. Disable the component so it can't be moved or scaled
            interactable.enabled = false;
            
            // Optional: Change color or play a sound to show it's "locked"
            Debug.Log("Object has been dropped and locked.");
        }
    }
}
