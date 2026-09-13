using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Menu")]

    [SerializeField] private GameObject MenuPanel;


    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool Ispaused = false;

            if (Ispaused = false)
            {
                OpenMenu();
                Ispaused = true;
            }

            if (Ispaused)
            {
                CloseMenu();
                Ispaused = false;
            }
        }*/
    }

    public void OpenMenu()
    {
        MenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void CloseMenu()
    {
        MenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    

}