using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour
{
    public enum AIR_COLLISION_TYPE
    {
        NONE = 0,
        FLOOR,
        WALL,
    }

    private Animator anim;
    private Rigidbody rbody;
    private CapsuleCollider collider;

    public float moveForce = 365f;
    public float maxSpeed = 5f;
    public Transform body;

    public bool jump = false;
    public float jumpForce = 1000f;
    float distToGround;
    public Transform groundCheck;
    public AIR_COLLISION_TYPE grounded = AIR_COLLISION_TYPE.NONE;

    public GameObject bulletPrefab;
    public float bulletSpeed = 20;
    public float fireRate = 0.2f;
    private float lastShot = 0.0f;

    public int score = 0;
    public bool isDead = false;

    void Awake()
    {
        // Get reference to required player components
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
    }

    // Use this for initialization
    void Start ()
    {
        // Get base distance for collision check
        distToGround = collider.bounds.extents.y;
    }

    // Update is called once per frame
    void Update ()
    {
        // Check to see if we are on the ground or touching a wall
        grounded = IsGrounded();

        // Set jump if we can
        if (Input.GetButtonDown("Jump") && grounded != AIR_COLLISION_TYPE.NONE)
        {
            jump = true;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (Time.time > fireRate + lastShot)
            {
                Fire();
                lastShot = Time.time;
            }
        }
    }

    // We use FixedUpdate over Update that way we are checking input along with the physics. If you use update it will check every frame.
    void FixedUpdate()
    {
        // Gets horizontal input
        float h = Input.GetAxis("Horizontal");

        // Tells animator what are speed is
        anim.SetFloat("Speed", Mathf.Abs(h));

        // Spedds us up
        if (h * rbody.velocity.x < maxSpeed)
            rbody.AddForce(Vector2.right * h * moveForce);

        // Caps are speed
        if (Mathf.Abs(rbody.velocity.x) > maxSpeed)
            rbody.velocity = new Vector2(Mathf.Sign(rbody.velocity.x) * maxSpeed, rbody.velocity.y);

        // Sets what direction we are looking
        if (h > .01)
        {
            // Looks right
            body.transform.eulerAngles = new Vector3(0, 90, 0);
        }
        else if (h < -.01)
        {
            // Looks left
            body.transform.eulerAngles = new Vector3(0, -90, 0);
        }
        //else
        //{
        //    // Looks at camera
        //    body.transform.eulerAngles = new Vector3(0, 180, 0);
        //}

        // If we can jump lets do it
        if (jump)
        {
            // Tells animator we are jumping
            anim.SetTrigger("Jump");

            // Adds vertical jump force
            if (grounded == AIR_COLLISION_TYPE.WALL)
                rbody.AddForce(new Vector2(0f, jumpForce*2));
            else
                rbody.AddForce(new Vector2(0f, jumpForce));

            // Resets jump trigger
            jump = false;
        }
    }

    AIR_COLLISION_TYPE IsGrounded()
    {
        // Ray we are using for collision check
        RaycastHit hitInfo;

        // Set default no collision
        AIR_COLLISION_TYPE act = AIR_COLLISION_TYPE.NONE;

        // Check if hit a wall to the left
        Physics.Raycast(groundCheck.position, Vector3.left, out hitInfo, distToGround / 2);
        if (hitInfo.collider != null)
        {
            // Hit a wall to the left
            act = AIR_COLLISION_TYPE.WALL;
        }
        else
        {
            // Check if hit a wall to the right
            Physics.Raycast(groundCheck.position, Vector3.right, out hitInfo, distToGround / 2);
            if (hitInfo.collider != null)
            {
                // Hit a wall to the right
                act = AIR_COLLISION_TYPE.WALL;
            }
            else
            {
                // Check if we hit the floor
                Physics.Raycast(groundCheck.position, -Vector3.up, out hitInfo, distToGround / 2);
                if (hitInfo.collider != null)
                {
                    // Hit the floor
                    act = AIR_COLLISION_TYPE.FLOOR;
                }
            }
        }

        // Slow us down if we hit something
        if (hitInfo.collider != null)
            rbody.drag = 5f;
        else
            rbody.drag = 0f;

        return act;
    }

    // draw lines to show collision check distances
    void OnDrawGizmos()
    {
        Color c = Gizmos.color;
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(groundCheck.position, -Vector3.up * (distToGround / 2));
        Gizmos.DrawRay(groundCheck.position, Vector3.left * (distToGround / 2));
        Gizmos.DrawRay(groundCheck.position, Vector3.right * (distToGround / 2));
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(groundCheck.position, body.forward * (4));
        Gizmos.color = c;
    }

    void Fire()
    {
        // create the bullet object from the bullet prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            transform.position - transform.forward,
            Quaternion.identity);

        // make the bullet move away in front of the player
        bullet.GetComponent<Rigidbody>().velocity = body.forward * bulletSpeed;

        // Tells animator we are firing
        anim.SetTrigger("Fire");
        
        // make bullet disappear after 2 seconds
        Destroy(bullet, 2.0f);
    }
}

