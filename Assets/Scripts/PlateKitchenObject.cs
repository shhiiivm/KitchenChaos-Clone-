using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlateKitchenObject : KitchenObject
{


    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSo kitchenObjectSo;
    }


    [SerializeField] private List<KitchenObjectSo> validKitchenObjectSoList;


    private List<KitchenObjectSo> kitchenObjectSoList;


    protected override void Awake()
    {
        base.Awake();
        kitchenObjectSoList = new List<KitchenObjectSo>();
    }

    public bool TryAddIngredient(KitchenObjectSo kitchenObjectSo)
    {
        if (!validKitchenObjectSoList.Contains(kitchenObjectSo))
        {
            // Not a valid ingredient
            return false;
        }
        if (kitchenObjectSoList.Contains(kitchenObjectSo))
        {
            // Already has this type
            return false;
        }
        else
        {
            AddIngredientServerRpc(
                KitchenGameMultiplayer.Instance.GetKitchenObjectSoIndex(kitchenObjectSo)
            );

            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSoIndex)
    {
        AddIngredientClientRpc(kitchenObjectSoIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSoIndex)
    {
        KitchenObjectSo kitchenObjectSo =  KitchenGameMultiplayer.Instance.GetKitchenObjectSoFromIndex(kitchenObjectSoIndex);

        kitchenObjectSoList.Add(kitchenObjectSo);

        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            kitchenObjectSo = kitchenObjectSo
        });
    }


    public List<KitchenObjectSo> GetKitchenObjectSoList()
    {
        return kitchenObjectSoList;
    }

}