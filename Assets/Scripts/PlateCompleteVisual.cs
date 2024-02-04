using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectSo_GameObject
    {
        public KitchenObjectSo kitchenObjectSo;
        public GameObject gameObject;
    }



    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSo_GameObject> kitchenObjectSoGameObjectList;


    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;

        foreach (KitchenObjectSo_GameObject kitchenObjectSoGameObject in kitchenObjectSoGameObjectList)
        {
           
                kitchenObjectSoGameObject.gameObject.SetActive(false);
            
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach(KitchenObjectSo_GameObject kitchenObjectSoGameObject in kitchenObjectSoGameObjectList)
        {
            if (kitchenObjectSoGameObject.kitchenObjectSo == e.kitchenObjectSo)
            {
                kitchenObjectSoGameObject.gameObject.SetActive(true);
            }
        }
  
  

    }
}
