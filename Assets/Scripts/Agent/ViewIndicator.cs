using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewIndicator : MonoBehaviour {

    FieldOfView fov;
    Movement status;
    Renderer renderMaterial;
    [HideInInspector]
    public bool detected = false;
    public bool detectedByFire = false;

    // Use this for initialization
    void Awake() {
        fov = GameObject.Find("Monster").GetComponent<FieldOfView>();
        status = GameObject.Find("Monster").GetComponent<Movement>();
        renderMaterial = GetComponent<Renderer>();
        renderMaterial.material.color = new Color(1, 1, 1, 0.2f);
    }

    private void Update() {
        /*if (fov.targetDetected || status.targetLastSeen || status.hitRegistered) {
            renderMaterial.material.color = new Color(1, 0, 0, 0.2f);
        } else {
            renderMaterial.material.color = new Color(1, 1, 1, 0.2f);
        }*/

        if (detected || detectedByFire) {
            renderMaterial.material.color = new Color(renderMaterial.material.color.r, renderMaterial.material.color.g, renderMaterial.material.color.b, 0.2f);
        } else {
            renderMaterial.material.color = new Color(renderMaterial.material.color.r, renderMaterial.material.color.g, renderMaterial.material.color.b, 0.08f);
        }
    }
}
