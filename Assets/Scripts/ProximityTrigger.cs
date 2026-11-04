using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ProximityTrigger : MonoBehaviour
{
    [SerializeField] private ARItem owner;

    private void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.15f;
        owner = GetComponentInParent<ARItem>();
    }

    private void Awake()
    {
        if (owner == null) owner = GetComponentInParent<ARItem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!owner) return;
        var otherItem = other.GetComponentInParent<ARItem>();
        if (!otherItem || otherItem == owner) return;

        ExperimentManager.Instance?.RegisterContact(owner, otherItem);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!owner) return;
        var otherItem = other.GetComponentInParent<ARItem>();
        if (!otherItem || otherItem == owner) return;

        ExperimentManager.Instance?.UnregisterContact(owner, otherItem);
    }
}
