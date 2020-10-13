using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGunControls : MonoBehaviour {
    //Position when firing
    [HideInInspector]
    public Vector3 firePosition;

    //External components
    private TutorialControls player;
    public Sprite[] crosshairs;
    public GameObject projectile;
    public GameObject bomb;
    public GameObject bombTrigger;

    //Global variables across weapons
    public float globalCoolDown = 10f;
    private float globalTimeStamp;
    bool onGlobalCooldown = false;

    //Public variables for the primary weapon
    public string primaryWeapon = "sixgun"; //Player starts with sixgun
    public int startingBullets = 6;
    public int bulletSpeed = 500;
    public float firingRate = 0.5f;
    public float reloadTime = 2f;

    //Private variables for the primary weapon cooldown
    private bool reloading = false;
    [HideInInspector]
    public int projectileCount;
    private float reloadTimeStamp;
    private float projectileTimeStamp;

    //Public variables for the secondary weapon
    public int startingBombs = 2;
    public int bombThrowSpeed = 250;
    public float bombThrowRate = 1f;
    public float bombCooldown = 2.5f;
    [HideInInspector]
    public int bombCount;
    //Private variables for the secondary weapon cooldown
    private bool bombReloading = false;
    private float bombCoodownTimeStamp;
    private float bombThrowTimeStamp;

    //Private variables for aiming
    private Vector3 mousePosition;
    private float rotationSpeed;

    private void Awake() {
        player = GameObject.Find("PlayerTutorialController").GetComponent<TutorialControls>();
    }

    void Start() {
        //Get the rotation speed of the player
        rotationSpeed = player.rotationSpeed;
        projectileCount = startingBullets;
        bombCount = startingBombs;
    }

    void FixedUpdate() {
        //Crosshair follows the mouse in fixed update
        mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = Vector2.Lerp(transform.position, mousePosition, rotationSpeed);
    }

    void Update() {
        //Making sure that the crosshairs stay on top
        gameObject.transform.Translate(Vector3.back);

        //Weapon Select
        if (Input.GetMouseButton(1)) {
            GetComponent<SpriteRenderer>().sprite = crosshairs[1];
        } else {
            GetComponent<SpriteRenderer>().sprite = crosshairs[0];
        }

        //Primary Firing
        if (projectileCount > 0 && projectileTimeStamp <= Time.time && reloadTimeStamp <= Time.time) {

            if (Input.GetMouseButton(0)) {
                ShootProjectile();
                projectileCount--;
                projectileTimeStamp = Time.time + firingRate;
                firePosition = player.transform.position;
            }
        }
        if (projectileCount <= 0 && !reloading) {
            reloading = true;
            reloadTimeStamp = Time.time + reloadTime;
        }
        if (reloadTimeStamp <= Time.time && reloading) {
            reloading = false;
            projectileCount = startingBullets;
        }

        if ((projectileCount < startingBullets && !onGlobalCooldown) || (bombCount < startingBombs && !onGlobalCooldown)) {
            onGlobalCooldown = true;
            globalTimeStamp = Time.time + globalCoolDown;
        }

        if (globalTimeStamp <= Time.time) {
            projectileCount = startingBullets;
            bombCount = startingBombs;
            onGlobalCooldown = false;
        }


        //Secondary Firing        
        if (bombCount > 0 && bombThrowTimeStamp <= Time.time && bombCoodownTimeStamp <= Time.time) {
            if (Input.GetMouseButtonUp(1)) {
                ThrowBomb();
                bombCount--;
                bombThrowTimeStamp = Time.time + bombThrowRate;
            }
        }
        if (bombCount <= 0 && !bombReloading) {
            bombReloading = true;
            bombCoodownTimeStamp = Time.time + reloadTime;
        }
        if (bombCoodownTimeStamp <= Time.time && reloading) {
            reloading = false;
            bombCount = startingBombs;
        }
    }

    void ShootProjectile() {
        Vector3 bulletPos = new Vector3(player.transform.position.x, player.transform.position.y, 0);
        GameObject bullet = Instantiate(projectile, bulletPos, player.transform.rotation) as GameObject;
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.up * bulletSpeed);
    }

    void ThrowBomb() {
        //Create an invisible trigger collider to catch the thrown bomb.
        Vector3 currentPos = transform.position; //Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentPos.z = 0;
        GameObject trigger = Instantiate(bombTrigger, currentPos, player.transform.rotation) as GameObject;

        //Get the correct look rotation beween the crosshair and the player for the bombthrow - it's super accurate
        Quaternion currentRot = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.forward);
        currentRot.x = 0;
        currentRot.y = 0;

        //Create the bomb as the child of the trigger collider.
        Vector3 bombPos = new Vector3(player.transform.position.x, player.transform.position.y, 0);
        GameObject firebomb = Instantiate(bomb, bombPos, currentRot) as GameObject;
        firebomb.GetComponent<Rigidbody>().AddForce(firebomb.transform.up * bombThrowSpeed);
        firebomb.transform.parent = trigger.gameObject.transform;
    }
}