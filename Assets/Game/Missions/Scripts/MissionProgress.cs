using System;

[Serializable]
public class MissionProgress
{
    public MissionData mission;
    public int currentAmount;
    public bool completed;

    public MissionProgress(MissionData mission)
    {
        this.mission = mission;
        currentAmount = 0;
        completed = false;
    }
}