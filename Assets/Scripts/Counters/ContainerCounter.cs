using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ContainerCounter : BaseCounter
{

    public event EventHandler OnPlayerGrabbedObject;
    
    [SerializeField] private KitchenObjectSo kitchenObjectSo;
  
    

    public  override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSo, player);

            InteractLogicServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }


    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }
   
}
