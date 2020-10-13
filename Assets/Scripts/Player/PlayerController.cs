using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    ReportGenerator reportGenerator;
    public GameObject dashReloadAnimation;

    public float movementSpeed;
    private float originalMovementSpeed;
    public float rotationSpeed = 0.1f;

    public float dashMultiplier = 3f;
    public float dashCoolDown = 1f;
    public float dashDuration = 0.2f;
    private float dashTimeStamp;
    [HideInInspector]
    public bool dashOnCD = false;
    [HideInInspector]
    public bool dashing = false;
    private Renderer renderMaterial;
    Color originalColor;

    private Rigidbody rigidBody;

    //Direction of movement
    private Vector2 direction;
    private float inputX;
    private float inputY;

    private bool animateDash = false;

    private void Start()
    {
        //reportGenerator = GameObject.Find("ReportGenerator").GetComponent<ReportGenerator>();
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
        inputY = Input.GetAxisRaw("Vertical");// * movementSpeed;
        inputX = Input.GetAxisRaw("Horizontal");// * movementSpeed;
        
        direction = new Vector2(inputX, inputY);

        //rigidBody.AddForce(direction);
        //transform.position = new Vector3(transform.position.x, transform.position.y, 1);
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x + inputX, transform.position.y + inputY, 1), movementSpeed * Time.deltaTime);

        //Dashing controls
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Space)) {
            if (dashTimeStamp <= Time.time && !dashing) {
                //reportGenerator.currentPlaySession.numberOfDashes++;
                renderMaterial.material.color = new Color32(35, 130, 140, 255);
                dashing = true;
                dashTimeStamp = Time.time + dashCoolDown + dashDuration;
           
                movementSpeed = originalMovementSpeed * dashMultiplier;
                dashOnCD = true;
                animateDash = true;
            }
        }

        if (dashTimeStamp <= Time.time && !dashing) {
            if (animateDash) {
                animateDash = false;
                GameObject dashReload = Instantiate(dashReloadAnimation, new Vector3(transform.position.x, transform.position.y, -1), transform.rotation);
                dashReload.transform.parent = transform;
            }
            renderMaterial.material.color = originalColor;
            dashOnCD = false;
        }

        if (dashTimeStamp - dashCoolDown<= Time.time) {
            dashing = false;
            movementSpeed = originalMovementSpeed;
        }
    }
}