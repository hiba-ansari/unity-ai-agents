using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DrawGUI : MonoBehaviour
{
	public Sprite HeartSprite;
	public Sprite[] FlySprites;

	private int _iconSize = 20;
	private int _iconSeparation = 10;

	private Texture2D _heartTex;
	private Texture2D _flyTex;

	private Frog _frogScript;
	private Snake _snakeScript;
	private Fly[] _fliesScript;

	public GameObject gameLostPanel;
	public GameObject gameWonPanel;
	private bool gameOver = false;

	void Start()
	{
		_heartTex = SpriteToTexture(HeartSprite);
		_flyTex = SpriteToTexture(FlySprites[0]);

		_frogScript = GameObject.Find("Frog").GetComponent<Frog>();
		_snakeScript = GameObject.FindWithTag("Snake").GetComponent<Snake>();
		_fliesScript = FindObjectsByType<Fly>(FindObjectsSortMode.None);

		// Disable panel at start
		gameLostPanel.SetActive(false);
		gameWonPanel.SetActive(false);
	}

	void OnGUI()
	{
		int _maxFiles = 9;

		GUI.Box(new Rect(10, 10, 30 * _maxFiles + 10, 60), "");

		// At the moment, the GUI is hardcoded to 3 health.
		// The counts are wrong, and don't change as the frog takes damage.
		for (int i = 0; i < _frogScript.Health; i++)
		{
			GUI.DrawTexture(new Rect(20 + (_iconSize + _iconSeparation) * i, 20, _iconSize, _iconSize), _heartTex, ScaleMode.ScaleToFit, true, 0.0f);
		}

		for (int i = 0; i < _frogScript.FliesCaught; i++)
		{
			GUI.DrawTexture(new Rect(20 + (_iconSize + _iconSeparation) * i, 45, _iconSize, _iconSize), _flyTex, ScaleMode.ScaleToFit, true, 0.0f);
		}
	}

	// Helper function to convert sprites to textures.
	// Follows the code from http://answers.unity3d.com/questions/651984/convert-sprite-image-to-texture.html
	private Texture2D SpriteToTexture(Sprite sprite)
	{
		if (sprite.rect.width != sprite.texture.width)
		{
			Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
			Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
			texture.SetPixels(pixels);
			texture.Apply();

			return texture;
		}
		else
		{
			return sprite.texture;
		}
	}

	// Handle game over
	void Update()
	{
		// Game lost if health is 0
		if (_frogScript.Health == 0 && !gameOver)
		{
			PauseGame();
			gameLostPanel.SetActive(true);
		}

		// Game won if flies are 10
		if (_frogScript.FliesCaught == 10 && !gameOver)
		{
			PauseGame();
			gameWonPanel.SetActive(true);
		}

		// Press R to restart when game is over
		if (gameOver)
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
		}
	}

	// Pause the game when the game is lost or won
	void PauseGame()
	{
		gameOver = true;

		// Disable all movements
		_frogScript.enabled = false;
		_frogScript.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

		_snakeScript.enabled = false;
		_snakeScript.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

		foreach (var fly in _fliesScript)
		{
			fly.enabled = false;
			fly.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
		}
	}
}
