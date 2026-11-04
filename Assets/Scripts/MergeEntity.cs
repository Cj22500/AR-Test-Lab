using UnityEngine;

public class MergeEntity : MonoBehaviour
{
    [Header("Settings")]
    public int level = 1; // Used to track which level the object is
    public GameObject nextLevelPrefab; // What does this turn into?
    
    private bool isMerging = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we hit another mergeable object
        MergeEntity other = collision.gameObject.GetComponent<MergeEntity>();

        if (other != null && !isMerging && !other.isMerging)
        {
            // Only merge if they are the same level
            if (other.level == this.level)
            {
                isMerging = true;
                other.isMerging = true;
                PerformMerge(other);
            }
        }
    }

    void PerformMerge(MergeEntity other)
    {
        // Calculate middle point
        Vector3 spawnPos = (transform.position + other.transform.position) / 2;

        if (nextLevelPrefab != null)
        {
            Instantiate(nextLevelPrefab, spawnPos, Quaternion.identity);
        }

        // Cleanup the two old objects
        Destroy(other.gameObject);
        Destroy(this.gameObject);
    }
}