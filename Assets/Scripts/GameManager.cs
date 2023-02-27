using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [Tooltip("Time until game end in seconds.")]
    private float TimeToSurvive = 600;
    
    private float _timePassed;
    private bool _gameWon;
    
    void Update()
    {
        _timePassed += Time.deltaTime;

        if (_timePassed >= TimeToSurvive && !_gameWon)
        {
            _gameWon = true;
            SceneTransition.ToWinScreen();
        }
    }
}
