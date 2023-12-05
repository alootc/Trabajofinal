using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField]private float force_jump;
    

    private Rigidbody rb;
    private PlayerInput playerInput;

    private Vector2 inputs;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    
    void Update()
    {
        inputs = playerInput.actions["Movimiento"].ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector2 movement = inputs * speed;
         
        rb.velocity = new Vector3(inputs.x, rb.velocity.y, inputs.y) * speed;

    }

    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            rb.AddForce(Vector3.up * force_jump);
        }
    }
}
