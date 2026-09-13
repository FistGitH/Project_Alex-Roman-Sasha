using UnityEngine;

public class MissionSave : MonoBehaviour
{
    public static MissionSave Instance;


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



    // ===============================
    // ÑÎÕĞÀÍÈÒÜ ÈÃĞÓ
    // ===============================


    public void SaveGame()
    {
        if (MissionManager.Instance == null)
            return;


        PlayerPrefs.Save();
    }




    // ===============================
    // ÇÀÃĞÓÇÈÒÜ ÈÃĞÓ
    // ===============================


    public void LoadGame()
    {
        if (MissionManager.Instance == null)
            return;


        // MissionManager ñàì âîññòàíàâëèâàåò ìèññèş
        // ïğè çàïóñêå ÷åğåç PlayerPrefs
    }





    // ===============================
    // ÓÄÀËÈÒÜ ÑÎÕĞÀÍÅÍÈÅ
    // ===============================


    public void DeleteSave()
    {
        PlayerPrefs.DeleteAll();

        PlayerPrefs.Save();
    }




    // ===============================
    // ÏĞÎÂÅĞÊÀ ÑÎÕĞÀÍÅÍÈß
    // ===============================


    public bool HasSave()
    {
        return PlayerPrefs.HasKey(
            "CurrentMissionID"
        );
    }
}