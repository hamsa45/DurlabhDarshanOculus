using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TwoStateButton : MonoBehaviour
{
    public enum State
    {
        StateOne,
        StateTwo
    }
    private Button button;
    [SerializeField] public GameObject StateOne;
    [SerializeField] public GameObject StateTwo;
    
    private State currentState = State.StateOne;
    public State CurrentState => currentState;

    void Start()
    {
        button = GetComponent<Button>();
        // Initialize visual state
        SetState(currentState);
    }
    
    public void SetState(State state)
    {
        currentState = state;
        StateOne.SetActive(state == State.StateOne);
        StateTwo.SetActive(state == State.StateTwo);
    }
    
    public void ToggleState()
    {
        SetState(currentState == State.StateOne ? State.StateTwo : State.StateOne);
    }
}
