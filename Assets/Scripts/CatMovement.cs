﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class CatMovement : MonoBehaviour
{
    /**** general movement variables ****/
    private BoxCollider2D boxcollider2D;
    private Rigidbody2D rigidbody2D;

    /**** instance variables for horizontal movement ***/
    private float maxSpeed = 5f;
    private bool facingLeft = true;
    private Animator anim;
    
    /**** for restarting game ****/
    [SerializeField] private Animator gameOverAnim;
    [SerializeField] private Animator restartAnim;
    [SerializeField] private string nextScene;
    private bool onSide = false;
    private bool raycastEnabled = true;
    private bool canStart = false;

    
    /**** for rotating the cat ****/
    private Vector3 currentEuler;
    private Quaternion newRotation;
    private float groundSlopeAngle = 0f;
    
    /**** ground detection for vertical motion ****/
    private bool grounded;
    [SerializeField] private LayerMask Ground;
    
    /**** max jump force ****/
    private float jumpForce = 8f;

    /**** cat sound effects ****/
    private AudioSource meow;
    [SerializeField] private GameObject BGMusicObject;
    private AudioSource BGMusic;
    [SerializeField] private GameObject cheeseSpawn;
    private AudioSource cheeseChomp;
    [SerializeField] private GameObject plus100;
    [SerializeField] private GameObject plus1000;
    public GameObject BurgerBar;

    /**** for pausing game ****/
    public static bool canPause;

    /**** skins ****/
    [SerializeField] private GameObject blackCat;
    [SerializeField] private GameObject calicoCat;
    [SerializeField] private GameObject brownCat;
    [SerializeField] private GameObject orangeCat;

    void Start()
    {
        string skin = PlayerPrefs.GetString("skin");
        switch (skin)
        {
            case "calicoCat":
                anim = calicoCat.GetComponent<Animator>();
                calicoCat.SetActive(true);
                break;
            case "brownCat":
                anim = brownCat.GetComponent<Animator>();
                brownCat.SetActive(true);
                break;
            case "orangeCat":
                anim = orangeCat.GetComponent<Animator>();
                orangeCat.SetActive(true);
                break;
            default:
                anim = blackCat.GetComponent<Animator>();
                blackCat.SetActive(true);
                break;
        }

        boxcollider2D = transform.GetComponent<BoxCollider2D>();
        BGMusic = BGMusicObject.GetComponent<AudioSource>();
        meow = GetComponent<AudioSource>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        GetComponent<CircleCollider2D>().enabled = false;
        cheeseChomp = cheeseSpawn.GetComponent<AudioSource>();
        canPause = true;
        restartAnim.enabled = true;
        StartCoroutine(StartGameAnimation());
    }

    void FixedUpdate()
    {
        //vertical motion
        anim.SetFloat("vSpeed", rigidbody2D.velocity.y);
        grounded = isGrounded();
        anim.SetBool("Ground", grounded);
        
        float move = 0f;
        //horizontal motion
        if (raycastEnabled && canStart) { //if cat is on the side of the building for a long time, disable movement
            move = Input.GetAxis("Horizontal");
        }
        if (grounded)
        {
            CheckGround(new Vector3(transform.position.x, transform.position.y - (boxcollider2D.size.x / 2) + 0.2f, transform.position.z));
        }
        else
        {
            groundSlopeAngle = 0f;
        }
        Quaternion newAngle = Quaternion.Euler(0, 0, groundSlopeAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, newAngle, 0.5f);

        rigidbody2D.velocity = new Vector3
                    (move * maxSpeed, rigidbody2D.velocity.y);
        anim.SetFloat("Speed", Mathf.Abs(move));

        if (move < 0 && facingLeft || move > 0 && !facingLeft)
        {
            flip();
        }

        
    }

    void Update()
    {
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetBool("Ground", false);
            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0f);
            rigidbody2D.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void flip()
    {
        facingLeft = !facingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Building")
        {
            //Debug.Log("contact points: " + other.contactCount);
            //Debug.Log("min x: " + other.collider.bounds.min.x);
            //Debug.Log("max y: " + other.collider.bounds.max.y);
            rigidbody2D.velocity = Vector3.zero;
            rigidbody2D.angularVelocity = 0f;
            
            for (int i = 0; i < other.contactCount; i++)
            {
                Vector2 contactPoint = other.GetContact(i).point;
                //Debug.Log("contactPoint: " + contactPoint);
                if ((Mathf.Abs(contactPoint.x - other.collider.bounds.min.x) < 0.2f || Mathf.Abs(contactPoint.x - other.collider.bounds.max.x) < 0.2f) 
                    && (contactPoint.y < (other.collider.bounds.max.y * 0.8f)) && !grounded)
                {
                    //float collisionTime = Time.time;
                    //while (Time.time - collisionTime < 1){};
                    onSide = true;
                    StartCoroutine(catFall());
                    
                    //rigidbody2D.AddForce(new Vector2(0, -9.8f));
                    //other.collider.enabled = false;
                    //rigidbody2D.Sleep();
                    

                }
            }   
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.tag == "Building")
        {
            onSide = false;
        }
    }

    IEnumerator catFall()
    {
        yield return new WaitForSeconds(0.75f);
        if (onSide)
        {
            raycastEnabled = false;
            //boxcollider2D.size = Vector3.zero;
            boxcollider2D.enabled = false;
            GetComponent<CircleCollider2D>().enabled = true;
        }
    }

    bool isGrounded()
    {
        if (raycastEnabled == false) {
            return false;
        }

        float extraHeight = 0.2f;
        
        Vector2 backFeetOrigin = new Vector2(boxcollider2D.bounds.min.x + .5f, boxcollider2D.bounds.min.y);
        Vector2 frontFeetOrigin = new Vector2(boxcollider2D.bounds.max.x - .5f, boxcollider2D.bounds.min.y);

        RaycastHit2D backFeet = Physics2D.Raycast(backFeetOrigin, Vector2.down, extraHeight, Ground);
        RaycastHit2D frontFeet = Physics2D.Raycast(frontFeetOrigin, Vector2.down, extraHeight, Ground);

        
        //RaycastHit2D raycastHit = Physics2D.BoxCast(boxcollider2D.bounds.center, boxcollider2D.bounds.size, 0f, Vector2.down, extraHeight, Ground);
        Color rayColor;
        if (backFeet.collider != null && frontFeet.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(backFeetOrigin, Vector2.down * (boxcollider2D.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(frontFeetOrigin, Vector2.down * (boxcollider2D.bounds.extents.y + extraHeight), rayColor);


        //Debug.DrawRay(boxcollider2D.bounds.center + new Vector3(boxcollider2D.bounds.extents.x, 0), Vector2.down * (boxcollider2D.bounds.extents.y + extraHeight), rayColor);
        //Debug.DrawRay(boxcollider2D.bounds.center - new Vector3(boxcollider2D.bounds.extents.x, 0), Vector2.down * (boxcollider2D.bounds.extents.y + extraHeight), rayColor);
        //Debug.DrawRay(boxcollider2D.bounds.center - new Vector3(0, boxcollider2D.bounds.extents.y), Vector2.right * (boxcollider2D.bounds.extents.y + extraHeight), rayColor);

        return backFeet.collider != null && frontFeet.collider != null;
    }

    public void CheckGround(Vector3 origin)
    {
        Vector3 groundSlopeDir;
        float startDistanceFromBottom = 0.2f;   // Should probably be higher than skin width
        float sphereCastRadius = 0.25f;
        float sphereCastDistance = 0.75f;
        Vector3 rayOriginOffset1 = new Vector3(-0.2f, 0f, 0.16f);
        Vector3 rayOriginOffset2 = new Vector3(0.2f, 0f, -0.16f);
        float raycastLength = 0.75f;
        bool showDebug = true;
        
        // Out hit point from our cast(s)
        RaycastHit hit;

        // SPHERECAST
        // "Casts a sphere along a ray and returns detailed information on what was hit."
        if (Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, Ground))
        {
            // Angle of our slope (between these two vectors). 
            // A hit normal is at a 90 degree angle from the surface that is collided with (at the point of collision).
            // e.g. On a flat surface, both vectors are facing straight up, so the angle is 0.
            groundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        }

        // Now that's all fine and dandy, but on edges, corners, etc, we get angle values that we don't want.
        // To correct for this, let's do some raycasts. You could do more raycasts, and check for more
        // edge cases here. There are lots of situations that could pop up, so test and see what gives you trouble.
        RaycastHit slopeHit1;
        RaycastHit slopeHit2;

        // FIRST RAYCAST
        if (Physics.Raycast(origin + rayOriginOffset1, Vector3.down, out slopeHit1, raycastLength))
        {
            // Debug line to first hit point
            if (showDebug) { Debug.DrawLine(origin + rayOriginOffset1, slopeHit1.point, Color.red); }
            // Get angle of slope on hit normal
            float angleOne = Vector3.Angle(slopeHit1.normal, Vector3.up);

            // 2ND RAYCAST
            if (Physics.Raycast(origin + rayOriginOffset2, Vector3.down, out slopeHit2, raycastLength))
            {
                // Debug line to second hit point
                if (showDebug) { Debug.DrawLine(origin + rayOriginOffset2, slopeHit2.point, Color.red); }
                // Get angle of slope of these two hit points.
                float angleTwo = Vector3.Angle(slopeHit2.normal, Vector3.up);
                // 3 collision points: Take the MEDIAN by sorting array and grabbing middle.
                float[] tempArray = new float[] { groundSlopeAngle, angleOne, angleTwo };
                Array.Sort(tempArray);
                groundSlopeAngle = tempArray[1];
            }
            else
            {
                // 2 collision points (sphere and first raycast): AVERAGE the two
                float average = (groundSlopeAngle + angleOne) / 2;
		        groundSlopeAngle = average;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Street") || other.CompareTag("Pigeon"))
        {
            canPause = false;
            rigidbody2D.velocity = Vector3.zero;
            rigidbody2D.angularVelocity = 0f;
            BGMusic.Stop();
            meow.Play();
            StartCoroutine(LoadScene());
        }
        else if (other.gameObject.layer == 10)
        {
            Vector3 spawnBonus = new Vector3(other.gameObject.transform.position.x, other.gameObject.transform.position.y + 0.5f, other.gameObject.transform.position.z);
            if (other.gameObject.tag == "Burger")
            {
                PlayerPrefs.SetInt("burgerTotal", PlayerPrefs.GetInt("burgerTotal")+1);
                Camera.main.GetComponent<pancam>().playerScore += 100;
                Instantiate(plus1000, spawnBonus, Quaternion.identity);
            }
            else
            {
                BurgerBar.GetComponent<BurgerBar>().ingredientGathered(other.tag);
                
                Camera.main.GetComponent<pancam>().playerScore += 10;
                Instantiate(plus100, spawnBonus, Quaternion.identity);
            }
            cheeseChomp.Play();
            Destroy(other.gameObject);


        }
    }

    IEnumerator LoadScene()
    {
        gameOverAnim.SetTrigger("CircleS2B");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextScene);
    
    }

    IEnumerator StartGameAnimation()
    {
        meow.Play();
        BGMusic.Play();
        yield return new WaitForSeconds(1f);
        canStart = true;
    }
}
