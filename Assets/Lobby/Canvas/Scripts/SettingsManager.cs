using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;


    [Header("Volume")]
    [Range(0f, 1f)]
    public float volume = 1f;



    [Header("Graphics")]
    public bool highQuality = true;



    [Header("Brightness URP")]
    [Range(-2f, 2f)]
    public float brightness = 0f;

    public VolumeProfile volumeProfile;

    private ColorAdjustments colorAdjustments;



    [Header("Resolution")]
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;



    [Header("Mouse")]
    [Range(0.1f, 5f)]
    public float mouseSensitivity = 1f;



    [Header("Default Settings")]
    private float defaultVolume;
    private int defaultQuality;
    private float defaultBrightness;
    private int defaultResolutionWidth;
    private int defaultResolutionHeight;
    private float defaultMouseSensitivity;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);


            SetupBrightness();


            SaveDefaultSettings();


            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }



    // ================= BRIGHTNESS SETUP =================

    void SetupBrightness()
    {
        if (volumeProfile != null)
        {
            volumeProfile.TryGet(out colorAdjustments);


            if (colorAdjustments == null)
            {
                colorAdjustments =
                    volumeProfile.Add<ColorAdjustments>(true);
            }
        }
    }



    // ================= SAVE FIRST SETTINGS =================

    void SaveDefaultSettings()
    {
        if (!PlayerPrefs.HasKey("DefaultSaved"))
        {
            PlayerPrefs.SetFloat(
                "DefaultVolume",
                volume
            );


            PlayerPrefs.SetInt(
                "DefaultQuality",
                highQuality ? 1 : 0
            );


            PlayerPrefs.SetFloat(
                "DefaultBrightness",
                brightness
            );


            PlayerPrefs.SetInt(
                "DefaultResolutionWidth",
                resolutionWidth
            );


            PlayerPrefs.SetInt(
                "DefaultResolutionHeight",
                resolutionHeight
            );


            PlayerPrefs.SetFloat(
                "DefaultMouseSensitivity",
                mouseSensitivity
            );


            PlayerPrefs.SetInt(
                "DefaultSaved",
                1
            );


            PlayerPrefs.Save();
        }
    }



    // ================= VOLUME =================

    public void SetVolume(float value)
    {
        volume = value;

        AudioListener.volume = volume;


        PlayerPrefs.SetFloat(
            "Volume",
            volume
        );

        PlayerPrefs.Save();
    }



    // ================= QUALITY =================

    public void SetQuality(int quality)
    {
        highQuality = quality == 1;


        QualitySettings.SetQualityLevel(
            quality
        );


        PlayerPrefs.SetInt(
            "Quality",
            quality
        );

        PlayerPrefs.Save();
    }



    // ================= BRIGHTNESS =================

    public void SetBrightness(float value)
    {
        brightness = value;


        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value =
                brightness;
        }


        PlayerPrefs.SetFloat(
            "Brightness",
            brightness
        );

        PlayerPrefs.Save();
    }



    // ================= RESOLUTION =================

    public void SetResolution(int width, int height)
    {
        resolutionWidth = width;
        resolutionHeight = height;


        Screen.SetResolution(
            width,
            height,
            true
        );


        PlayerPrefs.SetInt(
            "ResolutionWidth",
            width
        );


        PlayerPrefs.SetInt(
            "ResolutionHeight",
            height
        );


        PlayerPrefs.Save();
    }



    // ================= MOUSE =================

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;


        PlayerPrefs.SetFloat(
            "MouseSensitivity",
            mouseSensitivity
        );


        PlayerPrefs.Save();
    }



    // ================= RESET SETTINGS =================

    public void ResetSettings()
    {
        volume = PlayerPrefs.GetFloat(
            "DefaultVolume",
            0.5f
        );


        AudioListener.volume = volume;



        int quality = PlayerPrefs.GetInt(
            "DefaultQuality",
            1
        );


        highQuality = quality == 1;


        QualitySettings.SetQualityLevel(
            quality
        );



        brightness = PlayerPrefs.GetFloat(
            "DefaultBrightness",
            -2f
        );


        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value =
                brightness;
        }



        resolutionWidth = PlayerPrefs.GetInt(
            "DefaultResolutionWidth",
            1920
        );


        resolutionHeight = PlayerPrefs.GetInt(
            "DefaultResolutionHeight",
            1080
        );


        Screen.SetResolution(
            resolutionWidth,
            resolutionHeight,
            true
        );



        mouseSensitivity = PlayerPrefs.GetFloat(
            "DefaultMouseSensitivity",
            1f
        );



        // сохраняем сброс

        PlayerPrefs.SetFloat(
            "Volume",
            volume
        );


        PlayerPrefs.SetInt(
            "Quality",
            quality
        );


        PlayerPrefs.SetFloat(
            "Brightness",
            brightness
        );


        PlayerPrefs.SetInt(
            "ResolutionWidth",
            resolutionWidth
        );


        PlayerPrefs.SetInt(
            "ResolutionHeight",
            resolutionHeight
        );


        PlayerPrefs.SetFloat(
            "MouseSensitivity",
            mouseSensitivity
        );


        PlayerPrefs.Save();

        FindFirstObjectByType<SettingsUI>()?.LoadUI();
    }



    // ================= LOAD =================

    void LoadSettings()
    {
        // Volume

        volume = PlayerPrefs.GetFloat(
            "Volume",
            1f
        );

        AudioListener.volume = volume;



        // Quality

        int quality = PlayerPrefs.GetInt(
            "Quality",
            1
        );


        highQuality = quality == 1;


        QualitySettings.SetQualityLevel(
            quality
        );



        // Brightness

        brightness = PlayerPrefs.GetFloat(
            "Brightness",
            0f
        );


        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value =
                brightness;
        }



        // Resolution

        resolutionWidth = PlayerPrefs.GetInt(
            "ResolutionWidth",
            Screen.currentResolution.width
        );


        resolutionHeight = PlayerPrefs.GetInt(
            "ResolutionHeight",
            Screen.currentResolution.height
        );


        Screen.SetResolution(
            resolutionWidth,
            resolutionHeight,
            true
        );



        // Mouse

        mouseSensitivity = PlayerPrefs.GetFloat(
            "MouseSensitivity",
            1f
        );
    }
}