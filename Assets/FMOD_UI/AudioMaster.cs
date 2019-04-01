using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioMaster : MonoBehaviour
{
    public static AudioMaster instance;

    public bool useThisUI;
    private GameObject audioSettingsUI;
    
    [SerializeField, EventRef] private string testSFX = "";
    [SerializeField] private EventInstance SFXVolumeTestEvent;

    [Range(0,1)] public float masterVolume = 1f;
    [Range(0,1)] public float musicVolume = 1f;
    [Range(0,1)] public float sfxVolume = 1f;
    
    private float oldSfxVolume = 1f; // for testing SFX

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;
    private const string masterVolumePrefString = "Master_Volume";
    private const string musicVolumePrefString = "Music_Volume";
    private const string sfxVolumePrefString = "SFX_Volume";
    
    private void Awake()
    {
        Singleton();
        
        // Create an instance of the sound to be played
        SFXVolumeTestEvent = RuntimeManager.CreateInstance(testSFX);
        
        // Are we asked to use the built in UI?
        if (!useThisUI) return;
        
        // We are, let's make sure it's here
        audioSettingsUI = transform.GetChild(0).gameObject;
        
        // Let's warn and disable the bool if it's not
        if (audioSettingsUI == null)
        {
            useThisUI = false;
            Debug.LogWarning("There is no UI in the hierarchy.");
        };

        // Establish the buses to be loaded and managed
        masterBus = RuntimeManager.GetBus("bus:/Main");
        musicBus = RuntimeManager.GetBus("bus:/Main/Music");
        sfxBus = RuntimeManager.GetBus("bus:/Main/SFX");  
        
        // Check player prefs, if there is a value for each take it, if not make it
        if (PlayerPrefs.HasKey(masterVolumePrefString)) 
            masterVolume = PlayerPrefs.GetFloat(masterVolumePrefString);
        
        if (PlayerPrefs.HasKey(musicVolumePrefString))
            musicVolume = PlayerPrefs.GetFloat(musicVolumePrefString);
        
        if (PlayerPrefs.HasKey(sfxVolumePrefString))
            sfxVolume = PlayerPrefs.GetFloat(sfxVolumePrefString);

        // reset the oldValue before testing anything
        oldSfxVolume = sfxVolume;
    }

    private void Update()
    {
        // Listen for changes in the audio menu
        masterBus.setVolume(masterVolume);
        PlayerPrefs.SetFloat(masterVolumePrefString, masterVolume);
        
        musicBus.setVolume(musicVolume);
        PlayerPrefs.SetFloat(musicVolumePrefString, musicVolume);
        
        SetSfxVolume(sfxVolume);
        
        if (useThisUI)
        {
            UsingThisUI();
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
    }
    
    public void SetSfxVolume(float value)
    {
        sfxVolume = value;
        
        sfxBus.setVolume(sfxVolume);
        PlayerPrefs.SetFloat(sfxVolumePrefString, sfxVolume);
        
        SFXVolumeLevel();
    } 

    private void SFXVolumeLevel()
    {
        if (oldSfxVolume == sfxVolume) return;
        
        // Play a sound when we test the SFX Volume
        PLAYBACK_STATE playbackState;
        SFXVolumeTestEvent.getPlaybackState(out playbackState);
        if (playbackState != PLAYBACK_STATE.PLAYING)
        {
            SFXVolumeTestEvent.start();
        }

        oldSfxVolume = sfxVolume;
    }

    private void UsingThisUI()
    {
        if (!useThisUI) return;
        
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleActive();
        }
    }

    public void ToggleActive()
    {
        audioSettingsUI.SetActive(!audioSettingsUI.activeSelf);
    }
    
    private void Singleton()
    {
        if (instance == null)
        {
            instance = this;
            //@TODO do we want to not destory on load?
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
        {
            Debug.Log("not the instance " + name);
            Destroy(gameObject);
        }
    }
}