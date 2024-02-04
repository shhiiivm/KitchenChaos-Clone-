using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeDeliveredText;
    [SerializeField] private Button playAgainButton;


    private void Awake()
    {
        playAgainButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });

    }


    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;

        Hide();
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {

        
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();


            recipeDeliveredText.text = DeliveryManager.Instance.GetSuccesfullRecipesAmount().ToString();

        }
        else
        {
            Hide();
        }
    }

    

    private void Show()
    {
        gameObject.SetActive(true);


    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
