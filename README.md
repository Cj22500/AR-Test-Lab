# AR Test Lab - 3D Spatial Computing & Interactive AR Experiment Simulator

A 3D Mobile Augmented Reality (AR) interactive simulation built with **Unity 3D (C#)**, **AR Foundation (ARCore / ARKit)**, **XR Interaction Toolkit**, and **Firebase**.

## Core 3D & Spatial Computing Architecture (`Assets/Scripts/`)

### 1. Real-World Point Cloud / Plane Raycasting & 3D Spawning
- **[`ARPlacementSpawner.cs`](Assets/Scripts/ARPlacementSpawner.cs)**: Interfaces with `ARRaycastManager` (`TrackableType.PlaneWithinPolygon`, powered by ARCore/ARKit real-world feature point clouds and tracked 3D plane polygons) to project 2D screen touch coordinates into 3D world space (`Pose`, `Vector3`, `Quaternion`) and instantiate physics-enabled 3D prefabs with vertical settling offsets.
- **[`ARPlacementManager.cs`](Assets/Scripts/ARPlacementManager.cs)**: Subscribes to `ObjectSpawner` lifecycle events and manages 3D spatial manipulation locks via `XRGrabInteractable`.

### 2. 3D Rigid-Body Physics, Vector Interpolation & Proximity Triggers
- **[`MergeEntity.cs`](Assets/Scripts/MergeEntity.cs)**: Handles 3D rigid-body collision events (`OnCollisionEnter`), computes 3D midpoint vector interpolation (`(transform.position + other.transform.position) / 2`), and spawns upgraded 3D mesh prefabs (`Quaternion.identity`).
- **[`ProximityTrigger.cs`](Assets/Scripts/ProximityTrigger.cs)** & **[`ARItem.cs`](Assets/Scripts/ARItem.cs)**: Uses 3D `SphereCollider` trigger volumes (`radius = 0.15f`) to detect spatial proximity between 3D laboratory equipment and chemical reagents in real time.

### 3. Multi-Stage 3D Experiment State Machine & Cloud Persistence
- **[`ExperimentManager.cs`](Assets/Scripts/ExperimentManager.cs)**, **[`TaskSequencer.cs`](Assets/Scripts/TaskSequencer.cs)** & **[`ObjectFinalizer.cs`](Assets/Scripts/ObjectFinalizer.cs)**: Orchestrates multi-step interactive 3D laboratory experiments (`Experiment1` through `Experiment5`), validating 3D object interactions and reagent combinations.
- **[`FirebaseManager.cs`](Assets/Scripts/FirebaseManager.cs)**: Integrates asynchronous **Firebase Auth** and **Cloud Firestore** (`ContinueWithOnMainThread`, `CancellationTokenSource`) for user authentication and experiment progress tracking.
- **[`MissionSuccessOverlay_AutoUI.cs`](Assets/Scripts/MissionSuccessOverlay_AutoUI.cs)**: Programmatically constructs responsive Unity Canvas (`RectTransform`, `VerticalLayoutGroup`, `TextMeshProUGUI`) overlays at runtime.

## Tech Stack
- **3D Engine & Spatial SDKs**: Unity 3D, AR Foundation (`UnityEngine.XR.ARFoundation`, `UnityEngine.XR.ARSubsystems`), ARCore, ARKit, XR Interaction Toolkit (`3.3.0`)
- **Language**: C# (`.NET` / MonoBehaviour lifecycle, async/await task orchestration)
- **Backend / Cloud**: Firebase Authentication & Cloud Firestore
