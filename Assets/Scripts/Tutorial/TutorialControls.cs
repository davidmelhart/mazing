using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialControls : MonoBehaviour {
    public float movementSpeed;
    private float originalMovementSpeed;
    public float rotationSpeed = 0.1f;
    private Rigidbody rigidBody;
    public GameObject destroyEffect;

    public float dashMultiplier = 5f;
    public float dashCoolDown = 2f;
    public float dashDuration = 0.2f;
    private float dashTimeStamp;
    private bool dashing = false;
    private Renderer renderMaterial;
    Color originalColor;

    private bool isMoving = false;
    //Direction of movement
    private Vector2 direction;
    private float inputX;
    private float inputY;

    private void Start() {
        rigidBody = GetComponent<Rigidbody>();
        originalMovementSpeed = movementSpeed;
        renderMaterial = GetComponent<Renderer>();
        originalColor = renderMaterial.material.color;
    }

    void FixedUpdate() {
        //Facing mouse position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Quaternion rot = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);

        transform.rotation = Quaternion.Lerp(transform.rotation, rot, rotationSpeed);
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
        rigidBody.angularVelocity = Vector3.zero;

        //Button controls
        inputY = Input.GetAxis("Vertical") * movementSpeed;
        inputX = Input.GetAxis("Horizontal") * movementSpeed;
        direction = new Vector2(inputX, inputY);


        if(Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) {
            isMoving = true;
        } else {
            isMoving = false;
        }
        
        rigidBody.AddForce(direction);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -8, 8), Mathf.Clamp(transform.position.y, -5, 5), 0);

        if (!isMoving) {
            transform.position = Vector3.Lerp(transform.position, Vector3.zero, movementSpeed*Time.deltaTime*0.2f);
        }

        //Dashing controls
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Space)) {
            if (dashTimeStamp <= Time.time && !dashing) {
                renderMaterial.material.color = new Color32(35, 130, 140, 255);
                dashing = true;
                dashTimeStamp = Time.time + dashCoolDown + dashDuration;

                movementSpeed = originalMovementSpeed * dashMultiplier;               
            }
        }

        if (dashTimeStamp <= Time.time && !dashing) {
            renderMaterial.material.color = originalColor;
        }

        if (dashTimeStamp - dashCoolDown <= Time.time) {
            dashing = false;
            movementSpeed = originalMovementSpeed;
        }

    }
}
