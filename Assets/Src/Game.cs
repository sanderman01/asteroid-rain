using UnityEngine;
using UnityEngine.UI;

namespace AsteroidRain
{
	/// <summary>
	/// Asteroid Rain: A game where asteroids fall from the top of the screen and the player needs to tap to destroy them.
	/// Any asteroid hit will be removed and adds one point to the player's score. Large asteroids split into smaller asteroids.
	/// Any asteroid that passes the bottom of the screen will remove one life from the player.
	/// The game is over when the player is out of lives or the timer reaches 0.
	/// </summary>
	public class Game : MonoBehaviour
	{
		// Values
		[SerializeField]
		private float GameDuration = 60; // Duration of time from game start in seconds until stopwatch reaches 0.

		[SerializeField]
		public const float AsteroidRemovePosition = -4; // Y value below which the asteroid will be removed
		
		[SerializeField]
		private AudioClip sfxExplosion; // explosion sound to be played when the player hits an asteroid

		// UI elements
		Text txtTitle, txtLives, txtScore, txtMessage;
		Button btnPlay, btnExit;
		Transform panelMenu;
		ColorOverlay overlayDamage;
		Transform panelClock;
		Image imgClockHand;


		// Game State
		public enum GameState { Menu, Playing }
		public GameState State { get; private set; }
		private float gamestart;
		private int score;
		private int lives;
		private AsteroidSpawner asteroids;

		public static Game Instance { get; private set; }

		void Awake()
		{
			Instance = this;

			asteroids = Util.SafeFind<AsteroidSpawner>("AsteroidSpawner");

			// Initialise references to various UI elements.
			txtTitle = Util.SafeFind<Text>("txtTitle");
			txtLives = Util.SafeFind<Text>("txtLives");
			txtScore = Util.SafeFind<Text>("txtScore");
			txtMessage = Util.SafeFind<Text>("txtMessage");
			btnPlay = Util.SafeFind<Button>("btnPlay");
			btnExit = Util.SafeFind<Button>("btnExit");
			panelMenu = Util.SafeFind<Transform>("panelMenu");
			overlayDamage = Util.SafeFind<ColorOverlay>("overlay");
			panelClock = Util.SafeFind<Transform>("panelClock");
			imgClockHand = Util.SafeFind<Image>("imgClockHand");
			
			btnPlay.onClick.AddListener(OnBtnPlay);
			btnExit.onClick.AddListener(OnBtnExit);

			asteroids.enabled = false;
		}

		void Start()
		{
			State = GameState.Menu;
			txtLives.gameObject.SetActive(false);
			txtScore.gameObject.SetActive(false);
			panelClock.gameObject.SetActive(false);
		}

		void Update()
		{
			txtLives.text = "Lives: " + new string('I', lives);
			txtScore.text = "Score: " + score;

			switch (State)
			{
				case GameState.Menu:
					UpdateMenu();
					break;
				case GameState.Playing:
					UpdatePlaying();
					break;
			}
		}

		private void OnBtnExit()
		{
			Application.Quit();
		}

		private void OnBtnPlay()
		{
			StartGame();
		}

		private void StartGame()
		{
			State = GameState.Playing;
			gamestart = Time.time;
			lives = 5;
			score = 0;

			txtLives.gameObject.SetActive(true);
			txtScore.gameObject.SetActive(true);
			panelMenu.gameObject.SetActive(false);
			panelClock.gameObject.SetActive(true);

			asteroids.SetAsteroidObjectsEnabled(true);
			asteroids.ClearAsteroids();
			asteroids.enabled = true;
		}

		void UpdateMenu() { }

		void UpdatePlaying()
		{
			float gameTime = Time.time - gamestart;
			float clockHandAngle = 360f * (gameTime / GameDuration);
			imgClockHand.rectTransform.localRotation = Quaternion.Euler(0, 0, clockHandAngle);

			if (gameTime > GameDuration)
				GameOver();
		}

		private void GameOver()
		{
			asteroids.SetAsteroidObjectsEnabled(false);
			asteroids.enabled = false;
			txtMessage.text = "Game Over";
			State = GameState.Menu;
			panelMenu.gameObject.SetActive(true);
		}

		public void IncrementScore()
		{
			score += 1;
		}

		public void DecrementLives()
		{
			lives--;
			TriggerDamageEffect();

			if (lives <= 0)
				GameOver();
		}

		public void TriggerDamageEffect()
		{
			overlayDamage.TriggerEffect();
		}
	}
}