using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCursor : MonoBehaviour {
    Vector3 mousePosition;

    private void Awake() {
        Cursor.visible = false;
    }

    void FixedUpdate() {
        mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //transform.position = Vector2.Lerp(transform.position, mousePosition, 0.1f);
        //transform.position = new Vector3(transform.position.x, transform.position.y, -5);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, -5);
    }
}
