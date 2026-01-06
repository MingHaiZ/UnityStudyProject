using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private NetWorkTest _input;


    private void Awake()
    {
        _input = new NetWorkTest();
    }

    private void OnEnable()
    {
        _input.InputsTest.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        var readValue = _input.InputsTest.Player.ReadValue<Vector2>();
        if (readValue != Vector2.zero)
        {
            if (!NetworkManager.Instance.IsHost)
            {
                NetworkManager.Instance.HandlerMessage(readValue);
            }
        }
    }

    private void OnDisable()
    {
        _input.InputsTest.Disable();
    }
}