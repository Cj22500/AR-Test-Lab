using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;

public class TaskSequencer : MonoBehaviour
{
    [Header("Firestore Task Info")]
    [SerializeField] private string challengeDocumentName = "CHEMTASK01";
    [SerializeField] private string fieldName = "test";
    [SerializeField] private string fieldValue = "Done";

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    // Call this from the Next Level button
    public void CompleteTaskAndGoNext()
    {
        FirebaseUser currentUser = auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError("TaskSequencer: No user is signed in. Cannot update Firestore.");
            return;
        }

        string uid = currentUser.UserId;

        DocumentReference docRef = db.Collection("user_progress")
                                     .Document(uid)
                                     .Collection("challenges")
                                     .Document(challengeDocumentName);

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { fieldName, fieldValue }
        };

        Debug.Log($"Updating Firestore for user: {uid}, challenge: {challengeDocumentName}");

        docRef.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Firestore update failed: " + task.Exception);
                return;
            }

            Debug.Log("Firestore updated successfully.");

            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentIndex + 1);
        });
    }
}
