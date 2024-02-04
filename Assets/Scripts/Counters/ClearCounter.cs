using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{


    [SerializeField] private KitchenObjectSo kitchenObjectSo;


    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {           
            if (player.HasKitchenObject())
            {
               
                player.GetKitchenObject().SetKitchenObjectParent(this);
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
                else
                {
                   
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSo()))
                        {

                            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                            
                        }
                    }
                }
            }
            else
            {
                
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

}