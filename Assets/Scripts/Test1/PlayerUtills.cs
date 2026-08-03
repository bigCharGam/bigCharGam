using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUtills : MonoBehaviour
{
    private PlayerInput playerInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnPotion()
    {
        playerInput.SwitchCurrentActionMap("OnPotion");
        Debug.Log("Potion");
    }
}
