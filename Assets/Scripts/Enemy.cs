using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public GameObject target;
    public Vector3 targetPosition;
    public float targetRateMin = 0.2f;
    public float targetRateMax = 2.0f;
    public float targetRate = 0.2f;
    private float lastTarget = 0.0f;
    public float targetDistance = 5f;

    private bool spawned = false;
    private bool unearthed = false;
    private float speed = .1f;
    private int scoreValue = 100;

    // Use this for initialization
    void Start ()
    {
        target = GameObject.FindWithTag("Player");
        targetRate = 4;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (target != null)
        {
            if (Time.time > targetRate + lastTarget)
            {
                if (spawned)
                {
                    unearthed = true;
                    SetLayer(10);
                }
                spawned = true;
                lastTarget = Time.time;
                targetRate = Random.Range(targetRateMin, targetRateMax);
                // used to set target lookat position (we could use the target position by itself but that would make the zombies rotate in an undesirable mannor) 
                targetPosition = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
                transform.LookAt(targetPosition);
            }

            if (unearthed && !target.GetComponent<Player>().isDead)
            {
                Vector3 zombiToPlayer = target.transform.position - transform.position;
                float dist = zombiToPlayer.magnitude;

                if (dist <= targetDistance)
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed);
                //else
                //    Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "BG")
        {
            return;
        }
        if (collision.gameObject == target && unearthed)
        {
            target.GetComponent<Player>().isDead = true;
        }
        if (collision.gameObject.tag == "Bullet")
        {
            target.GetComponent<Player>().score += scoreValue;
            Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Color c = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPosition);
        Gizmos.color = c;
    }

    public void SetLayer(int layer, bool includeChildren = true)
    {
        gameObject.layer = layer;
        if (includeChildren)
        {
            foreach (Transform trans in gameObject.transform.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = layer;
            }
        }
    }
}
