using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }

    [Header("Scene Config")]
    [SerializeField] private SceneExperimentConfig sceneConfig;

    [Header("Overlay")]
    [SerializeField] private MissionSuccessOverlay successOverlay;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip electricSfx;

    [Header("VFX Prefabs (optional)")]
    [SerializeField] private GameObject electricityVfxPrefab; // banana+water (optional also coin reaction)
    [SerializeField] private GameObject cloudVfxPrefab;       // alcohol evaporation
    [SerializeField] private GameObject chalkPrefab;          // egg+water chalk appear

    [Header("Materials / Visuals (optional)")]
    [SerializeField] private Material paperWetBlueMaterial;
    [SerializeField] private Material paperLightGrayMaterial;
    [SerializeField] private Material spoonRustLight;
    [SerializeField] private Material spoonRustMedium;
    [SerializeField] private Material spoonRustDark;

    // Contact graph (touching)
    private readonly Dictionary<ARItem, HashSet<ARItem>> _touch = new();

    private bool _experimentRunning = false;
    private readonly HashSet<string> _completedExperiments = new();

    private ExperimentId _activeExperiment = ExperimentId.Experiment1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (sceneConfig == null)
            sceneConfig = FindAnyObjectByType<SceneExperimentConfig>();

        if (sceneConfig != null)
        {
            _activeExperiment = sceneConfig.activeExperiment;

            // Auto-map next scene if not set
            string next = sceneConfig.nextSceneName;
            if (string.IsNullOrWhiteSpace(next))
                next = GetAutoNextSceneName(SceneManager.GetActiveScene().name);

            successOverlay?.Configure(sceneConfig.mainMenuSceneName, next);
        }
        else
        {
            // fallback if no config exists in scene
            successOverlay?.Configure("MainMenu", GetAutoNextSceneName(SceneManager.GetActiveScene().name));
        }
    }

    // Called by ProximityTrigger
public void RegisterContact(ARItem a, ARItem b)
{
    Debug.Log("Touch detected: " + a.itemType + " <-> " + b.itemType);

    AddEdge(a, b);
    AddEdge(b, a);
    TryResolveExperiments();
}

    public void UnregisterContact(ARItem a, ARItem b)
    {
        RemoveEdge(a, b);
        RemoveEdge(b, a);
    }

    public void UnregisterItem(ARItem item)
    {
        if (_touch.TryGetValue(item, out var set))
        {
            foreach (var other in set)
            {
                if (_touch.TryGetValue(other, out var otherSet))
                {
                    otherSet.Remove(item);
                    if (otherSet.Count == 0) _touch.Remove(other);
                }
            }
            _touch.Remove(item);
        }
    }

    private void AddEdge(ARItem from, ARItem to)
    {
        if (!_touch.TryGetValue(from, out var set))
        {
            set = new HashSet<ARItem>();
            _touch[from] = set;
        }
        set.Add(to);
    }

    private void RemoveEdge(ARItem from, ARItem to)
    {
        if (_touch.TryGetValue(from, out var set))
        {
            set.Remove(to);
            if (set.Count == 0) _touch.Remove(from);
        }
    }

    private void TryResolveExperiments()
    {
        if (_experimentRunning) return;

        foreach (var cluster in GetClusters())
        {
            var types = cluster.Select(i => i.itemType).ToHashSet();

            switch (_activeExperiment)
            {
                case ExperimentId.Experiment1:
                    if (HasAll(types, ItemType.CoinCopper, ItemType.SaltSodiumChloride, ItemType.Water))
                        TriggerOnce("EXP1", () => StartCoroutine(Exp1_ElectrolyteAndCopper(cluster)));
                    break;

                case ExperimentId.Experiment2:
                    if (HasAll(types, ItemType.PaperCellulose, ItemType.Water, ItemType.TimeMachineCube))
                        TriggerOnce("EXP2_WATER", () => StartCoroutine(Exp2_PaperWater(cluster)));
                    else if (HasAll(types, ItemType.PaperCellulose, ItemType.RubbingAlcoholIsopropyl, ItemType.TimeMachineCube))
                        TriggerOnce("EXP2_ALCOHOL", () => StartCoroutine(Exp2_PaperAlcohol(cluster)));
                    break;

                case ExperimentId.Experiment3:
                    if (HasAll(types, ItemType.SpoonIron, ItemType.Water, ItemType.TimeMachineCube))
                        TriggerOnce("EXP3", () => StartCoroutine(Exp3_Rust(cluster)));
                    break;

                case ExperimentId.Experiment4:
                    if (HasAll(types, ItemType.BananaPotassium, ItemType.Water))
                        TriggerOnce("EXP4", () => StartCoroutine(Exp4_BananaSpark(cluster)));
                    break;

                case ExperimentId.Experiment5:
                    if (HasAll(types, ItemType.EggCalciumCarbonate, ItemType.Water))
                        TriggerOnce("EXP5", () => StartCoroutine(Exp5_Chalk(cluster)));
                    break;
            }

            if (_experimentRunning) return; // stop checking if triggered
        }
    }

    private void TriggerOnce(string key, System.Action trigger)
    {
        if (_completedExperiments.Contains(key)) return;
        _completedExperiments.Add(key);
        _experimentRunning = true;
        trigger?.Invoke();
    }

    private static bool HasAll(HashSet<ItemType> set, params ItemType[] needed)
    {
        for (int i = 0; i < needed.Length; i++)
            if (!set.Contains(needed[i])) return false;
        return true;
    }

    private List<List<ARItem>> GetClusters()
    {
        var nodes = new HashSet<ARItem>();
        foreach (var kv in _touch)
        {
            nodes.Add(kv.Key);
            foreach (var n in kv.Value) nodes.Add(n);
        }

        var visited = new HashSet<ARItem>();
        var clusters = new List<List<ARItem>>();

        foreach (var start in nodes)
        {
            if (visited.Contains(start)) continue;

            var cluster = new List<ARItem>();
            var q = new Queue<ARItem>();
            q.Enqueue(start);
            visited.Add(start);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                cluster.Add(cur);

                if (_touch.TryGetValue(cur, out var neigh))
                {
                    foreach (var nxt in neigh)
                    {
                        if (!visited.Contains(nxt))
                        {
                            visited.Add(nxt);
                            q.Enqueue(nxt);
                        }
                    }
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    private string GetAutoNextSceneName(string current)
    {
        if (current == "ExperimentScene") return "ExperimentScene2";
        if (current == "ExperimentScene2") return "ExperimentScene3";
        if (current == "ExperimentScene3") return "ExperimentScene4";
        if (current == "ExperimentScene4") return "ExperimentScene5";
        return ""; // last scene has no next
    }

    // -------------------------
    // EXPERIMENT IMPLEMENTATIONS
    // -------------------------

    private IEnumerator Exp1_ElectrolyteAndCopper(List<ARItem> cluster)
    {
        // coin + salt + water -> electric sound (optional VFX)
        PlayOneShot(electricSfx);

        // optional: spawn electricity near coin if you want
        // var coin = FindItem(cluster, ItemType.CoinCopper);
        // if (electricityVfxPrefab && coin) SpawnPrefab(electricityVfxPrefab, coin.transform.position, coin.transform.rotation, 2.5f);

        yield return new WaitForSeconds(0.5f);

        successOverlay?.Show(
            "Mission Success!\n\nElectrolyte Dissolution & Copper Oxidation\n" +
            "Ionic compounds dissolve into ions (Na⁺, Cl⁻) in water.\n\n"
            
        );

        _experimentRunning = false;
    }

    private IEnumerator Exp2_PaperWater(List<ARItem> cluster)
    {
        var paper = FindItem(cluster, ItemType.PaperCellulose);
        if (paper && paper.targetRenderer && paperWetBlueMaterial)
            paper.targetRenderer.sharedMaterial = paperWetBlueMaterial;

        yield return new WaitForSeconds(0.4f);

        successOverlay?.Show(
            "Mission Success!\n\nEvaporation Rate Comparison (Water)\n" +
            "Water evaporates slower due to stronger intermolecular forces.\n\n"
            
        );

        _experimentRunning = false;
    }

    private IEnumerator Exp2_PaperAlcohol(List<ARItem> cluster)
    {
        var paper = FindItem(cluster, ItemType.PaperCellulose);

        Material original = null;
        if (paper && paper.targetRenderer)
        {
            original = paper.targetRenderer.sharedMaterial;
            if (paperLightGrayMaterial) paper.targetRenderer.sharedMaterial = paperLightGrayMaterial;
        }

        if (cloudVfxPrefab && paper)
            SpawnPrefab(cloudVfxPrefab, paper.transform.position, paper.transform.rotation, 5.5f);

        yield return new WaitForSeconds(5f);

        if (paper && paper.targetRenderer && original != null)
            paper.targetRenderer.sharedMaterial = original;

        successOverlay?.Show(
            "Mission Success!\n\nEvaporation Rate Comparison (Alcohol)\n" +
            "Alcohol evaporates faster (more volatile), so it dries quicker.\n\n"
        );

        _experimentRunning = false;
    }

    private IEnumerator Exp3_Rust(List<ARItem> cluster)
    {
        var spoon = FindItem(cluster, ItemType.SpoonIron);
        if (spoon == null || spoon.targetRenderer == null)
        {
            yield return EndWithOverlay(
                "Mission Success!\n\nMetal Reactivity & Corrosion\n" +
                "(Assign targetRenderer on the Spoon to show rust stages.)\n\n"
            );
            yield break;
        }

        yield return new WaitForSeconds(3f);
        if (spoonRustLight) spoon.targetRenderer.sharedMaterial = spoonRustLight;

        yield return new WaitForSeconds(3f);
        if (spoonRustMedium) spoon.targetRenderer.sharedMaterial = spoonRustMedium;

        yield return new WaitForSeconds(3f);
        if (spoonRustDark) spoon.targetRenderer.sharedMaterial = spoonRustDark;

        successOverlay?.Show(
            "Mission Success!\n\nMetal Reactivity & Corrosion\n" +
            "Oxidation causes iron to form rust over time.\n\n"
        );

        _experimentRunning = false;
    }

    private IEnumerator Exp4_BananaSpark(List<ARItem> cluster)
    {
        var banana = FindItem(cluster, ItemType.BananaPotassium);

        PlayOneShot(electricSfx);

        if (electricityVfxPrefab && banana)
            SpawnPrefab(electricityVfxPrefab, banana.transform.position, banana.transform.rotation, 2.5f);

        yield return new WaitForSeconds(0.6f);

        successOverlay?.Show(
            "Mission Success!\n\nSpark Reactivity\n" +
            "Potassium reacts strongly with water, releasing energy.\n\n"
        );

        _experimentRunning = false;
    }

    private IEnumerator Exp5_Chalk(List<ARItem> cluster)
    {
        var egg = FindItem(cluster, ItemType.EggCalciumCarbonate);

        if (chalkPrefab && egg)
        {
            var pos = egg.transform.position + egg.transform.right * 0.08f;
            Instantiate(chalkPrefab, pos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.4f);

        successOverlay?.Show(
            "Mission Success!\n\nChalk Creation\n" +
            "Eggshells contain calcium carbonate, which can be formed into chalk.\n\n"
        );

        _experimentRunning = false;
    }

    // -------------------------
    // Helpers
    // -------------------------

    private ARItem FindItem(List<ARItem> cluster, ItemType type)
        => cluster.FirstOrDefault(i => i && i.itemType == type);

    private void PlayOneShot(AudioClip clip)
    {
        if (!clip || !sfxSource) return;
        sfxSource.PlayOneShot(clip);
    }

    private void SpawnPrefab(GameObject prefab, Vector3 pos, Quaternion rot, float destroyAfterSeconds)
    {
        var go = Instantiate(prefab, pos, rot);
        if (destroyAfterSeconds > 0f) Destroy(go, destroyAfterSeconds);
    }

    private IEnumerator EndWithOverlay(string msg)
    {
        yield return new WaitForSeconds(0.2f);
        successOverlay?.Show(msg);
        _experimentRunning = false;
    }
}
