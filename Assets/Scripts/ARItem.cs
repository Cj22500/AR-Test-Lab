using UnityEngine;

public class ARItem : MonoBehaviour
{
    [Header("Identity")]
    public ItemType itemType;

    [Header("Visual Target (optional)")]
    public Renderer targetRenderer;

    public string DebugName => $"{name} ({itemType})";

    private void Awake()
    {
        Debug.Log($"ARItem Awake -> name: {name}, itemType: {itemType}");
    }

    private void OnDestroy()
    {
        if (ExperimentManager.Instance != null)
        {
            ExperimentManager.Instance.UnregisterItem(this);
        }
    }
    }
