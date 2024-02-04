using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class StoveCounter : BaseCounter , IHasProgress
{

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }
    
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO fryingRecipeSO;
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>();
    private BurningRecipeSO burningRecipeSO;

    

    public override void OnNetworkSpawn()
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {

        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = state.Value
        });

        if(state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            }) ;
        }
    }

    private void Update()
    {

        if (!IsServer)
        {
            return;
        }


        {
            if (HasKitchenObject())
            {
                switch (state.Value)
                {
                    case State.Idle:
                        break;
                    case State.Frying:
                        fryingTimer.Value += Time.deltaTime;

                       
                        if (fryingTimer.Value > fryingRecipeSO.fryingTimerMax)
                        {
                            KitchenObject.DestroyKitchenObject(GetKitchenObject());
                            

                            KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                            state.Value = State.Fried;
                            burningTimer.Value = 0f;
                            SetBurningRecipeSOClientRpc(
                                KitchenGameMultiplayer.Instance.GetKitchenObjectSoIndex(GetKitchenObject().GetKitchenObjectSo())
                                
                                );                                                 
                        }
                        break;
                    case State.Fried:
                        burningTimer.Value += Time.deltaTime;

                       

                        if (burningTimer.Value > burningRecipeSO.burningTimerMax)
                        {
                            KitchenObject.DestroyKitchenObject(GetKitchenObject());
                           

                            KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);


                            state.Value = State.Burned;

                            

                            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                            {
                                progressNormalized = 0f
                            }); ;
                        }
                        break;
                    case State.Burned:
                        break;

                }

            }


        }
    }
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

                    InteractLogicPlaceObjectOnCounterServerRpc(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSoIndex(kitchenObject.GetKitchenObjectSo())
                        );

                   
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


                        SetStateIdleServerRpc();


                    }

                }

            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }



    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSoIndex)
    {
        fryingTimer.Value = 0f;
        state.Value = State.Frying;

        SetClientRecipeSOClientRpc(kitchenObjectSoIndex);
    }

    [ClientRpc]
    private void SetClientRecipeSOClientRpc(int kitchenObjectSoIndex)
    {

        KitchenObjectSo kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSoFromIndex(kitchenObjectSoIndex);
        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSo);

       
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSoIndex)
    {

        KitchenObjectSo kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSoFromIndex(kitchenObjectSoIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSo);


    }
    private bool HasRecipeWithInput(KitchenObjectSo inputKitchenObjectSo)
    {

        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSo);
        return fryingRecipeSO != null;

    }
    private KitchenObjectSo GetOutputForInput(KitchenObjectSo inputKitchenObjectSO)
    {

        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }
        else
        {
            return null;
        }

    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSo inputKitchenObjectSO)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSo inputKitchenObjectSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }

    public bool IsFried()
    {
        return state.Value == State.Fried;
    }
}
