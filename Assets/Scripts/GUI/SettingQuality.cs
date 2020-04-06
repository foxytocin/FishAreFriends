using UnityEngine;
using UnityEngine.UI;

public class SettingQuality : MonoBehaviour
{

    public Dropdown qualityDropdown;


    void Start()
    {

        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    public void setQuality()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
    }
}
