using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("Mission Chain")]
    [SerializeField] private MissionData firstMission;
    [SerializeField] private MissionData lastMission;

    private readonly List<MissionProgress> missions = new List<MissionProgress>();
    private MissionProgress currentMission;

    public event System.Action<MissionData> OnMissionChanged;
    public event Action<int, int> OnProgressChanged;
    public event System.Action<MissionData> OnMissionCompleted;

    private const string CurrentMissionIDKey = "CurrentMissionID";
    private const string CurrentMissionAmountKey = "CurrentMissionAmount";
    private const string CurrentMissionCompletedKey = "CurrentMissionCompleted";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadProgress();

        if (currentMission == null)
        {
            StartMission(firstMission);
        }
    }

    public void StartMission(MissionData mission)
    {
        if (mission == null)
        {
            Debug.LogError("[Mission] Cannot start mission: MissionData is null.");
            return;
        }

        if (currentMission != null && currentMission.mission == mission)
            return;

        MissionProgress progress = new MissionProgress(mission);

        missions.Add(progress);
        currentMission = progress;

        Debug.Log("[Mission] Started: " + mission.title + " | ID: " + mission.missionID);

        OnMissionChanged?.Invoke(mission);
        OnProgressChanged?.Invoke(progress.currentAmount, mission.requiredAmount);

        SaveProgress();
    }

    public MissionData GetCurrentMission()
    {
        return currentMission != null ? currentMission.mission : null;
    }

    public MissionProgress GetCurrentProgress()
    {
        return currentMission;
    }

    public bool HasMission()
    {
        return currentMission != null;
    }

    public bool CheckTarget(string id)
    {
        if (currentMission == null || currentMission.mission == null)
            return false;

        return currentMission.mission.targetID == id;
    }

    public void AddProgress(string targetID, int amount = 1)
    {
        if (currentMission == null || currentMission.mission == null)
            return;

        if (currentMission.completed)
            return;

        if (currentMission.mission.targetID != targetID)
        {
            Debug.LogWarning(
                "[Mission] Wrong target. Expected '" +
                currentMission.mission.targetID +
                "', received '" + targetID + "'."
            );
            return;
        }

        currentMission.currentAmount += amount;

        currentMission.currentAmount = Mathf.Clamp(
            currentMission.currentAmount,
            0,
            Mathf.Max(1, currentMission.mission.requiredAmount)
        );

        Debug.Log(
            "[Mission] Progress: " +
            currentMission.currentAmount +
            "/" +
            currentMission.mission.requiredAmount
        );

        OnProgressChanged?.Invoke(
            currentMission.currentAmount,
            currentMission.mission.requiredAmount
        );

        if (currentMission.currentAmount >= currentMission.mission.requiredAmount)
        {
            CompleteMission();
            return;
        }

        SaveProgress();
    }

    public void CompleteMission()
    {
        if (currentMission == null || currentMission.mission == null)
            return;

        if (currentMission.completed)
            return;

        currentMission.completed = true;

        MissionData completedMission = currentMission.mission;

        Debug.Log("[Mission] COMPLETED: " + completedMission.title);

        OnMissionCompleted?.Invoke(completedMission);

        SaveProgress();

        if (completedMission == lastMission)
        {
            Debug.Log("[Mission] ALL MISSIONS COMPLETED!");

            ClearSave();

            currentMission = null;
            return;
        }

        MissionData nextMission = completedMission.nextMission;

        if (nextMission == null)
        {
            Debug.LogWarning(
                "[Mission] Mission '" +
                completedMission.missionID +
                "' has no Next Mission."
            );

            ClearSave();
            currentMission = null;
            return;
        }

        StartMission(nextMission);
    }

    public int GetCurrentAmount()
    {
        return currentMission != null ? currentMission.currentAmount : 0;
    }

    public int GetRequiredAmount()
    {
        return currentMission != null && currentMission.mission != null
            ? currentMission.mission.requiredAmount
            : 0;
    }

    public float GetProgressPercent()
    {
        if (currentMission == null || currentMission.mission == null)
            return 0f;

        int required = currentMission.mission.requiredAmount;

        if (required <= 0)
            return 1f;

        return Mathf.Clamp01(
            (float)currentMission.currentAmount / required
        );
    }

    public bool IsCompleted()
    {
        return currentMission != null && currentMission.completed;
    }

    private void SaveProgress()
    {
        if (currentMission == null || currentMission.mission == null)
            return;

        PlayerPrefs.SetString(
            CurrentMissionIDKey,
            currentMission.mission.missionID
        );

        PlayerPrefs.SetInt(
            CurrentMissionAmountKey,
            currentMission.currentAmount
        );

        PlayerPrefs.SetInt(
            CurrentMissionCompletedKey,
            currentMission.completed ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (!PlayerPrefs.HasKey(CurrentMissionIDKey))
            return;

        string id = PlayerPrefs.GetString(CurrentMissionIDKey);

        MissionData mission = FindMissionByID(id);

        if (mission == null)
        {
            Debug.LogWarning(
                "[Mission] Saved mission ID '" +
                id +
                "' was not found. Old save will be cleared."
            );

            ClearSave();
            return;
        }

        currentMission = new MissionProgress(mission);

        currentMission.currentAmount = PlayerPrefs.GetInt(
            CurrentMissionAmountKey,
            0
        );

        currentMission.currentAmount = Mathf.Clamp(
            currentMission.currentAmount,
            0,
            Mathf.Max(1, mission.requiredAmount)
        );

        currentMission.completed = PlayerPrefs.GetInt(
            CurrentMissionCompletedKey,
            0
        ) == 1;

        missions.Add(currentMission);

        Debug.Log(
            "[Mission] Loaded: " +
            mission.title +
            " | " +
            currentMission.currentAmount +
            "/" +
            mission.requiredAmount
        );

        OnMissionChanged?.Invoke(mission);

        OnProgressChanged?.Invoke(
            currentMission.currentAmount,
            mission.requiredAmount
        );
    }

    private MissionData FindMissionByID(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        // First search the mission chain starting from firstMission.
        MissionData current = firstMission;
        HashSet<MissionData> visited = new HashSet<MissionData>();

        while (current != null && !visited.Contains(current))
        {
            if (current.missionID == id)
                return current;

            visited.Add(current);
            current = current.nextMission;
        }

        // Fallback: also search Resources.
        MissionData[] allMissions = Resources.LoadAll<MissionData>("");

        foreach (MissionData mission in allMissions)
        {
            if (mission != null && mission.missionID == id)
                return mission;
        }

        return null;
    }

    public void ResetProgress()
    {
        ClearSave();

        missions.Clear();
        currentMission = null;

        StartMission(firstMission);
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(CurrentMissionIDKey);
        PlayerPrefs.DeleteKey(CurrentMissionAmountKey);
        PlayerPrefs.DeleteKey(CurrentMissionCompletedKey);
        PlayerPrefs.Save();

        Debug.Log("[Mission] Save cleared.");
    }

    public List<MissionProgress> GetAllMissions()
    {
        return missions;
    }

    public MissionData GetFirstMission()
    {
        return firstMission;
    }

    public MissionData GetLastMission()
    {
        return lastMission;
    }
}