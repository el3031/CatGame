﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestartGame : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private Animator restartGameAnim;
    [SerializeField] private Text scoreGUI;
    [SerializeField] private RectTransform scoreCard;
    [SerializeField] private Transform mainCameraPosition;
    [SerializeField] private Button restart;
    [SerializeField] private Button mainMenu;
    [SerializeField] private GameObject player;
    [SerializeField] private Text highScoreGUI;
    private Vector3 scoreCardHidden;
    private Vector3 scoreCardShown;

    private int score;
    private int highScore;
    
    void Start()
    {
        //change later!
        score = PlayerPrefs.GetInt("Score");

        if (score > PlayerPrefs.GetInt("highScore"))
        {
            PlayerPrefs.SetInt("highScore", score);
        }
        highScore = PlayerPrefs.GetInt("highScore");
        scoreGUI.horizontalOverflow = HorizontalWrapMode.Overflow;
        highScoreGUI.horizontalOverflow = HorizontalWrapMode.Overflow;
        scoreCardHidden = new Vector3(0, 1300, 0);
        scoreCardShown = new Vector3(0, 70, 0);
        restart.onClick.AddListener(RestartOnClick);
        mainMenu.onClick.AddListener(toMainMenu);
    }
    
    void FixedUpdate()
    {
        scoreCard.anchoredPosition = Vector3.Lerp(scoreCard.anchoredPosition, scoreCardShown, 0.125f);
    }
    
    void OnGUI()
    {    
        scoreGUI.text = score.ToString();
        highScoreGUI.text = highScore.ToString();
    }

    void RestartOnClick()
    {
        StartCoroutine(LoadScene());
    }

    void toMainMenu()
    {
        nextScene = "MainMenu";
        StartCoroutine(LoadScene());
    }
    
    IEnumerator LoadScene()
    {
        //Debug.Log(scoreCardHidden);
        //Debug.Log("transform.position: " + scoreCard.transform.position);
        //restartGameAnim.SetTrigger("restart");
        
        float moveDurationTimer = 0.0f;
        float moveDuration = 0.8f;

        while (moveDurationTimer < moveDuration) 
        {
            moveDurationTimer += Time.deltaTime;
            // Lerp using initial value!
            scoreCard.anchoredPosition = Vector2.Lerp(scoreCard.anchoredPosition, scoreCardHidden, moveDurationTimer / moveDuration);
            yield return null;
        }
        PlayerPrefs.SetString("lastScene", "gameOver");

        SceneManager.LoadScene(nextScene);
    }
    void OnApplicationQuit()
    {
        PlayerPrefs.SetString("lastScene", "quit");
    }
}
