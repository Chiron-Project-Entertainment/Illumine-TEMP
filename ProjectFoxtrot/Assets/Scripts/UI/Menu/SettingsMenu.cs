///////////////////////////////////////////////////////////////
///                                                         ///
///             Script coded by Hakohn (Robert).            ///
///                                                         ///
///////////////////////////////////////////////////////////////

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// The script attached to the settings menu gameObject.
/// Its role is to deal with the button presses and to
/// save and apply any changes done at the settings menus.
/// </summary>
public class SettingsMenu : MonoBehaviour 
{
    #region Gameplay settings variables
    [Header("Gameplay settings")]
    /// <summary> The toggle button used for enabling or disabling subtitles. </summary>
    [SerializeField] private Toggle subtitlesToggle = null;
    private bool subtitlesEnabled = false;
    /// <summary> The drop-down holding all the languages available. </summary>
    [SerializeField] private TMP_Dropdown languageDropdown = null;
    #endregion

    #region Video settings variables
    [Header("Video settings")]
    /// <summary> The drop-down holding all the resolution options. </summary>
    [SerializeField] private TMP_Dropdown resolutionDropdown = null;
    /// <summary> The drop-down holding all the quality types. </summary>
    [SerializeField] private TMP_Dropdown qualityDropdown = null;
    /// <summary> The toggle button used for going to and out of fullscreen. </summary>
    [SerializeField] private Toggle fullscreenToggle = null;
    /// <summary> The toggle button used for going in and out of the colorblind mode. </summary>
    [SerializeField] private Toggle colorblindToggle = null;
    #endregion

    #region Audio settings
    [Header("Audio settings")]
    /// <summary>
    /// The audio mixer which holds all the pitch, volume 
    /// and effects applied to the different sound types 
    /// (master, environment, HUD, announcer etc.).
    /// </summary>
    [SerializeField] private AudioMixer audioMixer = null;
    #endregion

    private void Awake()
	{
        #region Gameplay settings

        #endregion

        #region Video settings
        #region Setting up the resolution and resolution drop-down.
        // Adding the existing resolutions to the drop-down settings.
        resolutionDropdown.ClearOptions();
		List<string> resolutionOptions = new List<string>();
		int maxScreenRes = 0;
		// The iteration required for string conversion (not possible with LINQ, sadly).
		for(int i = 0; i < Screen.resolutions.Length; i++)
		{
			resolutionOptions.Add(Screen.resolutions[i].width + "x" + Screen.resolutions[i].height + ", " + Screen.resolutions[i].refreshRate + "Hz");
            Vector3 tempResVals = new Vector3(Screen.resolutions[i].width, Screen.resolutions[i].height, Screen.resolutions[i].refreshRate);
            Vector3 maxResVals = new Vector3(Screen.width, Screen.height, Screen.currentResolution.refreshRate);

            if (tempResVals == maxResVals) maxScreenRes = i;
		}
		// Making sure that we have selected the right resolution.
		resolutionDropdown.AddOptions(resolutionOptions);
        SetResolution(maxScreenRes);
		resolutionDropdown.value = maxScreenRes;
		resolutionDropdown.RefreshShownValue();
        #endregion

        #region Setting up the quality and quality drop-down.
        // Adding the existing qualities to the drop-down settings.
        qualityDropdown.ClearOptions();
        List<string> qualityOptions = new List<string>();
        int currentQualitySettings = 0;
        for(int i = 0; i < QualitySettings.names.Length; i++)
        {
            qualityOptions.Add(QualitySettings.names[i]);
            if (i == QualitySettings.GetQualityLevel()) currentQualitySettings = i;
        }
        // Making sure that we have selected the right quality.
        qualityDropdown.AddOptions(qualityOptions);
        SetQuality(currentQualitySettings);
        qualityDropdown.value = currentQualitySettings;
        qualityDropdown.RefreshShownValue();
        #endregion

        #region Setting up the toggles
        // Making sure that we have the right fullscreen toggle.
        fullscreenToggle.isOn = Screen.fullScreen;
        // Making sure that we have the right colorblind toggle.
        colorblindToggle.isOn = false;
        #endregion
        #endregion

        #region Controls settings
        
        #endregion
    }


    #region Methods used dynamically by buttons, drop-downs, sliders, toggles and input fields.
    public void SetSubtitles(bool areOn) { subtitlesEnabled = areOn; }
    public void SetLanguage(int languageIndex) { /*pass*/ }
	public void SetQuality(int qualityIndex) { QualitySettings.SetQualityLevel(qualityIndex); }
	public void SetFullscreen(bool isFullscreen) { Screen.fullScreen = isFullscreen; }
	public void SetResolution(int resolutionIndex) { var res = Screen.resolutions[resolutionIndex]; Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate); }
    public void SetMasterVolume(float volume) { audioMixer.SetFloat("MasterVolume", volume); }
	public void SetEnvironmentVolume(float volume) { audioMixer.SetFloat("EnvironmentVolume", volume); }
	public void SetHUDVolume(float volume) { audioMixer.SetFloat("HUDVolume", volume); }
	public void SetAnnouncerVolume(float volume) { audioMixer.SetFloat("AnnouncerVolume", volume); }
    #endregion
}
