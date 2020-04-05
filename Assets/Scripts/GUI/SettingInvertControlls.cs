using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Accessibility;

public class SettingInvertControlls : MonoBehaviour
{

    private KeyController keyController;

    public Dropdown invertControllsDropdown;

    Resolution[] resolutions;

    private void Start()
    {
        keyController = FindObjectOfType<KeyController>();
        invertControllsDropdown.value = (keyController.CheckInvertedControlls()) ? 1 : 0;
        invertControllsDropdown.RefreshShownValue();
    }

    public void SetInvertedControlls(bool value)
    {
        invertControllsDropdown.value = (value) ? 1 : 0;
        invertControllsDropdown.RefreshShownValue();
    }

    public void InvertControlls()
    {
        bool value = (invertControllsDropdown.value == 0) ? false : true;
        keyController.InvertedControlls(value);
    }
}
