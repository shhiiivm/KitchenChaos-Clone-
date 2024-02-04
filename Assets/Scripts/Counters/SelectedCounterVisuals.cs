using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisuals : MonoBehaviour
{

    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    private void Start()
    {
        if(Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
        
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
    }

   

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if(e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
       foreach(GameObject visuaGameObject in visualGameObjectArray)
        {
            visuaGameObject.SetActive(true);
        }
        
    }
    private void Hide()
    {

        foreach (GameObject visuaGameObject in visualGameObjectArray)
        {
            visuaGameObject.SetActive(false);
        }
            
    }
}
