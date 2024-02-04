using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSo kitchenObjectSo;
    

    public IKitchenObjectParent kitchenObjectParent;
    private FollowTransform followTransfom;


    protected virtual void Awake()
    {
        followTransfom = GetComponent<FollowTransform>();
    }

    public KitchenObjectSo GetKitchenObjectSo()
    {
        return kitchenObjectSo;
    }


    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectClientRpc(kitchenObjectParentNetworkObjectReference);
    }

    [ClientRpc]
    private void SetKitchenObjectClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();




        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError(" IKitchenObjectParent already has a kitchen Object");
        }

        kitchenObjectParent.SetKitchenObject(this);


        followTransfom.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }
    
    

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
      
        Destroy(gameObject);
    }

    public void ClearKitchenObjectOnParent()
    {
        kitchenObjectParent.ClearKitchenObject();

    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if(this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }


    public static void SpawnKitchenObject(KitchenObjectSo kitchenObjectSo, IKitchenObjectParent kitchenObjectParent)
    {

        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSo, kitchenObjectParent);
       
    }

    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }
}
