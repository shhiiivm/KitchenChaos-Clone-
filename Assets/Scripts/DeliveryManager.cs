using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;


    public static DeliveryManager Instance { get; private set; }


    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> WaitingRecipeSOList;

    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;

    private int successfullRecipesAmount;
    private void Awake()
    {
        Instance = this;



        WaitingRecipeSOList = new List<RecipeSO>();
    }
    private void Update()
    {

        if (!IsServer)
        {
            return;
        }
        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if( KitchenGameManager.Instance.IsGamePlaying() && WaitingRecipeSOList.Count < waitingRecipeMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                

                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);


               
            }
             
        } 
        
    }



    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {

        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];

        WaitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for(int i = 0; i < WaitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = WaitingRecipeSOList[i];


            if(waitingRecipeSO.kitchenObjectSoList.Count == plateKitchenObject.GetKitchenObjectSoList().Count)
            {
                bool plateContentsMatchesRecipe = true;

                foreach (KitchenObjectSo recipeKitchenObjectSo in waitingRecipeSO.kitchenObjectSoList) 
                {
                    bool ingredientFound = false;
                    
                    foreach(KitchenObjectSo plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSoList())
                    {
                        if(plateKitchenObjectSO == recipeKitchenObjectSo)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }

                    if (!ingredientFound)
                    {
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }

           
        }


        DeliveryInCorrectRecipeServerRpc();


    }
    [ServerRpc(RequireOwnership = false)]
    private void DeliveryInCorrectRecipeServerRpc()
    {
        DeliveryInCorrectRecipeClientRpc();
    }
    [ClientRpc]
    private void DeliveryInCorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
        DeliveryCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }
    [ClientRpc]
    private void DeliveryCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        successfullRecipesAmount++;
        WaitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return WaitingRecipeSOList;
    }

    public int GetSuccesfullRecipesAmount()
    {
        return successfullRecipesAmount;
    }
}
