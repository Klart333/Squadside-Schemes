using UnityEngine.InputSystem;

public class InputHandler : Singleton<InputHandler>
{
    public InputAction Fire
    {
        get
        {
            return fire;
        }
    }

    public InputAction Look
    {
        get
        {
            return look;
        }
    }

    private InputAction fire;
    private InputAction look;
    private PlayerControls inputs;

    protected override void Awake()
    {
        base.Awake();

        inputs = new PlayerControls();

        ApplyInputs();
    }

    private void ApplyInputs()
    {
        fire = inputs.Player.Fire;
        fire.Enable();


        look = inputs.Player.Look;
        look.Enable();
    }

    private void OnDisable()
    {
        fire.Disable();
        look.Disable();
    }


}
