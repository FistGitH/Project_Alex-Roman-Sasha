using UnityEngine;

public enum MissionType { Collect, Interact, Reach, Kill, Custom }

[CreateAssetMenu(fileName = "Mission", menuName = "Game/Mission")]
public class MissionData : ScriptableObject
{
    public string missionID;
    public string title;
    [TextArea] public string description;
    public MissionType type;
    public string targetID;
    public int requiredAmount = 1;
    public MissionData nextMission;
}