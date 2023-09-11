using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HiddenObject : NetworkBehaviour
{
	public NetworkVariable<bool> isFound = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<ColorName> colorName = new NetworkVariable<ColorName>();

	[SerializeField] private Sprite foundSprite;
	[SerializeField] private Color foundColor;
	[SerializeField] private float colorChangeSpeed;


	private SpriteRenderer spriteRenderer;
	private InputHandler inputHandler;



	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public override void OnNetworkSpawn()
	{
		isFound.OnValueChanged += Found;
		colorName.OnValueChanged += (ColorName color1, ColorName color2) => { Setup(); } ;

	}

	[ServerRpc(RequireOwnership = false)]
	public void FoundServerRPC()
	{
		isFound.Value = true;
	}

	public void Found(bool Previous, bool NewValue)
	{
		print("found");
		if (foundSprite == null)
		{
			ChangeColor();
			return;
		}

		ChangeSprite();
	}

	private void ChangeSprite()
	{

	}

	public void Setup()
	{
		spriteRenderer.color = ColorManager.Instance.GetColor(colorName.Value);
	}

	private async void ChangeColor()
	{

		float t = 0;

		Color currentColor = spriteRenderer.color;

		while (t < 1f)
		{
			t += Time.deltaTime * colorChangeSpeed;

			spriteRenderer.color = Color.Lerp(currentColor, foundColor, t);
			if(transform.localScale.x > 0)
			{
				transform.localScale = new Vector3(transform.localScale.x - t * 0.01f, transform.localScale.y - t * 0.01f, 1);
			}
			await Task.Yield();
		}

		transform.localScale = new Vector3(0, 0, 0);

    }
}
