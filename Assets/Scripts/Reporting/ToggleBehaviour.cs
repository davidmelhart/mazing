using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBehaviour : MonoBehaviour {
    Toggle toggle;
    ToggleGroup toggleGroup;

    Color idle;
    Color pressed;

    private void Start(){
        toggle = GetComponent<Toggle>();
        toggleGroup = GetComponentInParent<ToggleGroup>();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);

        ColorBlock cb = toggle.colors;
        idle = cb.normalColor;
        pressed = cb.pressedColor;
    }
 
    private void OnToggleValueChanged(bool isOn) {
        Toggle[] allToggles = toggleGroup.GetComponentsInChildren<Toggle>();
        ColorBlock cb = toggle.colors;
        if (isOn) {
            cb.normalColor = pressed;
            cb.highlightedColor = pressed;
            for(int i = 0; i < allToggles.Length; i++) {
                if (allToggles[i] != toggle) {
                    allToggles[i].isOn = false;
                }
            }
        } else {
            cb.normalColor = idle;
            cb.highlightedColor = idle;
        }
        toggle.colors = cb;
    }
}
