using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;   // Needed for ContinueWithOnMainThread
using TMPro;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    [Header("Firebase References")]
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseUser user;

    [Header("Login UI")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;
    public GameObject loadingOverlay;
    public Button loginButton;

    private bool isFirebaseReady = false;
    private bool isBusy = false;

    private CancellationTokenSource cts;

    private void Awake()
    {
        cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    private async void Start()
    {
        SetReady(false, "Connecting: Checking Google Play Services...");
        SetLoading(false);

        await InitializeFirebaseAsync(cts.Token);
    }

    // ---------- UI helpers ----------
    private void SetLoading(bool show)
    {
        isBusy = show;
        if (loadingOverlay != null) loadingOverlay.SetActive(show);
        UpdateLoginButtonState();
    }

    private void SetReady(bool ready, string message = null)
    {
        isFirebaseReady = ready;
        if (!string.IsNullOrEmpty(message) && statusText != null)
            statusText.text = message;

        UpdateLoginButtonState();
    }

    private void UpdateLoginButtonState()
    {
        if (loginButton == null) return;
        loginButton.interactable = isFirebaseReady && !isBusy;
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    // ---------- Firebase init ----------
    private async Task InitializeFirebaseAsync(CancellationToken token)
    {
        try
        {
            await Task.Yield();

            SetStatus("Connecting: Verifying App Identity (SHA-1)...");
            var depTask = FirebaseApp.CheckAndFixDependenciesAsync();

            var status = await WithTimeout(depTask, 12000, token);

            if (status != DependencyStatus.Available)
            {
                SetReady(false, "Firebase Error: " + status);
                Debug.LogError("Firebase dependency error: " + status);
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;

            SetReady(true, "Ready to Login");
        }
        catch (TimeoutException)
        {
            SetReady(false, "Init timed out. Check Internet / Play Services.");
            Debug.LogError("Firebase init timed out.");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Firebase init canceled.");
        }
        catch (Exception e)
        {
            SetReady(false, "Initialization Error. Check Internet.");
            Debug.LogError("Firebase init exception: " + e);
        }
    }

    // ---------- Login button ----------
    public async void OnLoginClick()
    {
        if (!isFirebaseReady || auth == null)
        {
            SetStatus("Still connecting... please wait a moment.");
            return;
        }

        string email = emailInput != null ? emailInput.text.Trim() : "";
        string pass = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            SetStatus("Please enter email and password.");
            return;
        }

        await LoginFlowAsync(email, pass, cts.Token);
    }

    private async Task LoginFlowAsync(string email, string password, CancellationToken token)
    {
        SetLoading(true);
        SetStatus("Authenticating with Firebase...");

        try
        {
            bool ok = await SignInAsync(email, password, token);
            if (!ok)
            {
                SetLoading(false);
                return;
            }

            SetStatus("Login Success! Loading Profile...");
            await FetchUserDataAndGoAsync(token);
        }
        catch (TimeoutException)
        {
            SetStatus("Request timed out. Check your internet.");
            SetLoading(false);
        }
        catch (OperationCanceledException)
        {
            SetLoading(false);
        }
        catch (Exception e)
        {
            SetStatus("Unexpected error: " + e.Message);
            Debug.LogError(e);
            SetLoading(false);
        }
    }

    private async Task<bool> SignInAsync(string email, string password, CancellationToken token)
    {
        try
        {
            var tcs = new TaskCompletionSource<AuthResult>();

            auth.SignInWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled) tcs.TrySetCanceled();
                    else if (task.IsFaulted) tcs.TrySetException(task.Exception);
                    else tcs.TrySetResult(task.Result);
                });

            var result = await WithTimeout(tcs.Task, 12000, token);
            user = result.User;

            return user != null;
        }
        catch (Exception e)
        {
            // Flatten aggregate exceptions (common with Firebase)
            if (e is AggregateException ae) e = ae.Flatten().InnerException ?? e;

            // Use FirebaseException (works across Firebase Unity SDK versions)
            string msg = "Login failed. Check credentials or internet.";

            if (e is FirebaseException fe)
            {
                // FirebaseAuth uses AuthError enum codes
                try
                {
                    var authError = (AuthError)fe.ErrorCode;
                    msg = AuthErrorToMessage(authError);
                }
                catch
                {
                    msg = "Login failed: " + fe.Message;
                }
            }

            SetStatus(msg);
            Debug.LogWarning("SignIn exception: " + e);
            return false;
        }
    }

    // ---------- Firestore fetch ----------
    private async Task FetchUserDataAndGoAsync(CancellationToken token)
    {
        if (db == null || user == null)
        {
            SetStatus("Internal error: Firebase not ready.");
            SetLoading(false);
            return;
        }

        DocumentReference docRef = db.Collection("user_account").Document(user.UserId);

        try
        {
            // Safe write even if doc doesn't exist
            await WithTimeout(
                docRef.SetAsync(
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "last_login", FieldValue.ServerTimestamp }
                    },
                    SetOptions.MergeAll),
                12000,
                token
            );

            var tcs = new TaskCompletionSource<DocumentSnapshot>();
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled) tcs.TrySetCanceled();
                else if (task.IsFaulted) tcs.TrySetException(task.Exception);
                else tcs.TrySetResult(task.Result);
            });

            DocumentSnapshot snapshot = await WithTimeout(tcs.Task, 12000, token);

            if (!snapshot.Exists)
            {
                SetStatus("User document not found in Firestore.");
                SetLoading(false);
                return;
            }

            string firstName = snapshot.ContainsField("first_name")
                ? snapshot.GetValue<string>("first_name")
                : "Player";

            SetStatus($"Welcome, {firstName}!");
            await Task.Delay(900, token);

            SceneManager.LoadScene("MainMenu");
        }
        catch (Exception e)
        {
            if (e is AggregateException ae) e = ae.Flatten().InnerException ?? e;

            SetStatus("Firestore Error: " + e.Message);
            Debug.LogError("Firestore exception: " + e);
            SetLoading(false);
        }
    }

    // ---------- Helpers ----------
    private static string AuthErrorToMessage(AuthError code)
    {
        switch (code)
        {
            case AuthError.InvalidEmail: return "Invalid email format.";
            case AuthError.WrongPassword: return "Wrong password.";
            case AuthError.UserNotFound: return "Account not found.";
            case AuthError.NetworkRequestFailed: return "Network error. Check internet.";
            case AuthError.TooManyRequests: return "Too many attempts. Try again later.";
            default: return "Login failed: " + code;
        }
    }

    private static async Task<T> WithTimeout<T>(Task<T> task, int ms, CancellationToken token)
    {
        var timeoutTask = Task.Delay(ms, token);
        var finished = await Task.WhenAny(task, timeoutTask);

        if (finished == timeoutTask) throw new TimeoutException();
        token.ThrowIfCancellationRequested();
        return await task;
    }

    private static async Task WithTimeout(Task task, int ms, CancellationToken token)
    {
        var timeoutTask = Task.Delay(ms, token);
        var finished = await Task.WhenAny(task, timeoutTask);

        if (finished == timeoutTask) throw new TimeoutException();
        token.ThrowIfCancellationRequested();
        await task;
    }
}
