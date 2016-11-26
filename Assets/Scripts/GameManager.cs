using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//using System;

public class GameManager : MonoBehaviour
{
    public enum GAMESTATE
    {
        MAINMENU = 0,
        HUD,
        GAMEOVER,
    }
    public AudioSource heartAudioSource;
    public AudioClip deathClip;                                 // The audio clip to play when the player dies.
    bool isDead;                                                // Whether the player is dead.
    public GameObject[] spawnPoints;
    public Player knight;

    public GameObject enemyPrefab;
    public float enemyRateMin = 0.2f;
    public float enemyRateMax = 2.0f;
    public float enemyRate = 0.2f;
    private float lastEnemy = 0.0f;
    public List<GameObject> enemies = new List<GameObject>();
    public Transform enemyHolder;
    Vector3 startPosition;
    Quaternion startRotation;

    int score;

    public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    GAMESTATE gameState = GAMESTATE.MAINMENU;

    public Text hudScore;
    public Text gameoverScore;

    // Use this for initialization
    void Start()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        startRotation = knight.transform.rotation;
        startPosition = knight.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GAMESTATE.HUD)
        {
            if (!knight.isDead)
            {
                if (Time.time > enemyRate + lastEnemy)
                {
                    lastEnemy = Time.time;
                    enemyRate = Random.Range(enemyRateMin, enemyRateMax);
                    int spawnpoint = Random.Range(0, spawnPoints.Length);
                    var enemy = (GameObject)Instantiate(
                    enemyPrefab,
                    spawnPoints[spawnpoint].transform.position,
                    Quaternion.identity);
                    enemy.transform.parent = enemyHolder;
                    enemies.Add(enemy);
                }
                gameoverScore.text = hudScore.text = knight.score.ToString();

                // Get enemies in range
                int cnt = GetNearEnemyCount();

                if (cnt == 0)
                {
                    heartAudioSource.pitch = 0;
                }
                else if (cnt == 1)
                {
                    heartAudioSource.pitch = 1;
                }
                else if (cnt <= 3)
                {
                    heartAudioSource.pitch = 2;
                }
                else
                {
                    heartAudioSource.pitch = 3;
                }
            }
            else
            {
                GameOver();
            }
        }
        else if (gameState == GAMESTATE.MAINMENU)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
        }
        else if (gameState == GAMESTATE.GAMEOVER)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Reset();
            }
        }
    }

    private int GetNearEnemyCount()
    {
        int cnt = 0;
        foreach (GameObject go in enemies)
        {
            Vector3 zombiToPlayer = knight.transform.position - go.transform.position;
            float dist = zombiToPlayer.magnitude;
            if (dist <= go.GetComponent<Enemy>().targetDistance * 2)
            {
                cnt++;
            }
        }

        return cnt;
    }

    public void Reset()
    {
        foreach (GameObject go in enemies)
            Destroy(go);
        enemies = new List<GameObject>();
        knight.transform.rotation = startRotation;
        knight.transform.position = startPosition;
        knight.isDead = true;
        knight.score = 0;
        ShowPanel(mainMenuPanel);
        gameState = GAMESTATE.MAINMENU;
    }

    public void StartGame()
    {
        Reset();
        knight.isDead = false;
        lastEnemy = Time.time;
        ShowPanel(hudPanel);
        knight.gameObject.SetActive(true);
        gameState = GAMESTATE.HUD;
        knight.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void GameOver()
    {
        GetComponent<AudioSource>().PlayOneShot(deathClip);
        ShowPanel(gameOverPanel);
        knight.gameObject.SetActive(false);
        gameState = GAMESTATE.GAMEOVER;
        knight.GetComponent<Rigidbody>().isKinematic = true;
    }

    public void ShowPanel(GameObject pan)
    {
        mainMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        pan.SetActive(true);
    }
}