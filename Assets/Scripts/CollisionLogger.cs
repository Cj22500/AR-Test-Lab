using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
    // This function runs automatically when this object hits another solid object
    private void OnCollisionEnter(Collision collision)
    {
        // Prints the digit 1 to the Unity Console
        Debug.Log(1);
    }
}
