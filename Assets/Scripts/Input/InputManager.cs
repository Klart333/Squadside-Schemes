using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private PlayerControls inputs;

    private InputAction fire;
    private InputAction roll;
    private InputAction buyXP;
    private InputAction inspect;

    public InputAction Fire => fire;
    public InputAction Roll => roll;
    public InputAction BuyXP => buyXP;
    public InputAction Inspect => inspect;

    protected override void Awake()
    {
        base.Awake();

        inputs = new PlayerControls();

        fire = inputs.Player.Fire;
        fire.Enable();

        roll = inputs.Player.Roll;
        roll.Enable();

        buyXP = inputs.Player.BuyXP;
        buyXP.Enable();

        inspect = inputs.Player.Inspect;
        inspect.Enable();
    }

    private void OnDisable()
    {
        fire.Disable();
        roll.Disable();
        buyXP.Disable();
        inspect.Disable();
    }
}
