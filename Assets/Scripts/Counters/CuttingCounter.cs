using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CuttingCounter : BaseCounter , IHasProgress
{

    public static event EventHandler OnAnyCut;

    new  public static void ResetStaticData()
    {
        OnAnyCut = null;
    }
    
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;



    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {

                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSo()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRpc();
                    
                }               
            }
            else
            {

            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {

                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSo()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                       
                    }

                }
            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;


        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f

        });
    }

    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSo()))
        {

            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();


        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {

        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSo()))
        {

            CutObjectClientRpc();
           
        }
        
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        cuttingProgress++;


        OnCut?.Invoke(this, EventArgs.Empty);

        OnAnyCut?.Invoke(this, EventArgs.Empty);



        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSo());

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });

       
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSo()))
        {
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSo());

            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
            {
                KitchenObjectSo outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSo());

                KitchenObject.DestroyKitchenObject(GetKitchenObject());

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);

            }
        }
                
    }

    private bool HasRecipeWithInput(KitchenObjectSo inputKitchenObjectSo)
    {

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSo);
        return cuttingRecipeSO != null;
       
    }
    private KitchenObjectSo GetOutputForInput(KitchenObjectSo inputKitchenObjectSO)
    {

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if(cuttingRecipeSO != null)
        {
          return  cuttingRecipeSO.output;
        }
        else
        {
            return null;
        }
       
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSo inputKitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
