using UnityEngine;
using UnityEngine.EventSystems;

namespace AsteroidRain
{
	public class Asteroid : MonoBehaviour, IPointerDownHandler
	{
		private AsteroidSpawner spawner;
		private int size;
		private Vector3 velocity;
		private float rotationVelocity;

		private SpriteRenderer spriteRenderer;

		public int Size { get { return size; } }
		public Vector3 Velocity { get { return velocity; } }
		public float RotationVelocity { get { return rotationVelocity; } }

		public Sprite Image
		{
			get { return spriteRenderer.sprite; }
			private set { spriteRenderer.sprite = value; }
		}

		void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		public void Init(AsteroidSpawner spawner, int size, Vector2 position, Vector2 velocity, float rotationVelocity, Sprite sprite)
		{
			// set basic values
			this.spawner = spawner;
			this.size = size;
			this.velocity = velocity;
			this.rotationVelocity = rotationVelocity;
			Image = sprite;

			transform.position = position;
			transform.localScale = Vector3.one * this.size;

			gameObject.SetActive(true);
		}

		void FixedUpdate()
		{
			transform.Rotate(0, 0, rotationVelocity);

			// move and rotate this asteroid by its velocity, remove it when it crosses the bottom boundary
			transform.position += velocity * Time.fixedDeltaTime;
			if (transform.position.y < Game.AsteroidRemovePosition)
			{
				spawner.OnAsteroidHitBottom(this);
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			// tell manager to remove this asteroid from the scene
			spawner.OnAsteroidTouched(this);
		}
	}
}