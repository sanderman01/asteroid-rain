using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AsteroidRain
{
	/// <summary>
	/// Manages asteroids and spawns asteroids at random intervals.
	/// Inactive asteroids will be kept in an object pool for re-use.
	/// </summary>
	public class AsteroidSpawner : MonoBehaviour
	{
		// Settings

		[SerializeField]
		private float MinSpawnDuration = 0.2f;
		[SerializeField]
		private float MaxSpawnDuration = 0.8f;

		[SerializeField]
		private float LargeAsteroidChance = 0.2f; // Chance to spawn a large asteroid instead of a regular.
		[SerializeField]
		private Rect SpawnArea = new Rect(new Vector2(-2, 4), new Vector2(4, 0));

		[SerializeField]
		private float MinVelocity = 1;
		[SerializeField]
		private float MaxVelocity = 3;
		[SerializeField]
		private float MaxRotVelocity = 3;

		[SerializeField]
		private int ExplosionParticlesPerAsteroid = 20; // Number of particles to spawn every time an asteroid gets destroyed by player.

		// Asset References
		// We could also use Resources.Load instead of setting these through the inspector.

		[SerializeField]
		private Asteroid prefabAsteroid;
		[SerializeField]
		private Sprite[] sprites;
		[SerializeField]
		private AudioClip sfxExplosion;

		// Scene References
		[SerializeField]
		private ParticleSystem fxExplosion;

		// Private fields

		private LinkedList<Asteroid> inactiveAsteroids = new LinkedList<Asteroid>();
		private HashSet<Asteroid> activeAsteroids = new HashSet<Asteroid>();

		private float scheduledSpawn;

		void Awake()
		{
			fxExplosion = Util.SafeFind<ParticleSystem>("fxExplode");

			// Fill object pool with inactive asteroid objects.
			const int AsteroidObjectPoolSize = 30;
			for(int i = 0; i < AsteroidObjectPoolSize; ++i)
			{
				Asteroid roid = Instantiate<Asteroid>(prefabAsteroid);
				roid.transform.SetParent(transform);
				roid.gameObject.SetActive(false);
				inactiveAsteroids.AddFirst(roid);
			}
		}

		void OnEnable()
		{
			ScheduleRandomSpawn(MinSpawnDuration, MaxSpawnDuration);
		}

		void Update()
		{
			if(0 < scheduledSpawn && scheduledSpawn < Time.time)
			{
				SpawnRandomAsteroid();
				ScheduleRandomSpawn(MinSpawnDuration, MaxSpawnDuration);
			}
		}

		private void ScheduleRandomSpawn(float minDuration, float maxDuration)
		{
			scheduledSpawn = Time.time + Random.Range(minDuration, maxDuration);
		}

		public Asteroid SpawnRandomAsteroid()
		{
			// Determine random location, velocity, size and spawn the roid
			Vector2 pos = new Vector2(Random.Range(SpawnArea.xMin, SpawnArea.xMax), SpawnArea.center.y);
			Vector2 vel = new Vector2(0, -Random.Range(MinVelocity, MaxVelocity));
			float rotVel = Random.Range(-MaxRotVelocity, MaxRotVelocity);
			int size = Random.value < LargeAsteroidChance ? 2 : 1;
			Sprite sprite = sprites[Random.Range(0, sprites.Length)];
			return SpawnAsteroid(size, pos, vel, rotVel, sprite);
		}

		public Asteroid SpawnAsteroid(int size, Vector2 pos, Vector2 vel, float rotationVelocity, Sprite sprite)
		{
			// Take an asteroid from the inactive pool and activate it.
			Asteroid newRoid = inactiveAsteroids.First.Value;
			inactiveAsteroids.RemoveFirst();
			newRoid.Init(this, size, pos, vel, rotationVelocity, sprite);
			activeAsteroids.Add(newRoid);
			return newRoid;
		}

		public void RemoveAsteroid(Asteroid roid)
		{
			// disable and put the asteroid back into the inactive pool
			roid.gameObject.SetActive(false);
			activeAsteroids.Remove(roid);
			inactiveAsteroids.AddFirst(roid);
		}

		public void SetAsteroidObjectsEnabled(bool enabled)
		{
			foreach (Asteroid roid in activeAsteroids) roid.enabled = enabled;
		}

		public void ClearAsteroids()
		{
			// removing from a collection while iterating over it tends to be bad, so make a copy first
			List<Asteroid> roids = new List<Asteroid>(activeAsteroids);
			foreach (Asteroid roid in roids) RemoveAsteroid(roid);
		}

		public void OnAsteroidHitBottom(Asteroid roid)
		{
			RemoveAsteroid(roid);
			Game.Instance.DecrementLives();
		}

		public void OnAsteroidTouched(Asteroid roid)
		{
			// play explosion sound
			AudioSource.PlayClipAtPoint(sfxExplosion, Camera.main.transform.position);

			// spawn explosion particles at the same position as the asteroid and in front of the asteroids
			ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
			for (int i = 0; i < ExplosionParticlesPerAsteroid; ++i)
			{
				emitParams.position = roid.transform.position + new Vector3(0, 0, -1);
				emitParams.velocity = (Vector3)UnityEngine.Random.insideUnitCircle + roid.Velocity;
				fxExplosion.Emit(emitParams, 1);
			}

			// check if we should split the asteroid
			if (roid.Size > 1)
			{
				// roid was a large one, so create 2 smaller ones before removing the old
				Vector3 offset = new Vector3(0.5f, 0);
				SpawnAsteroid(roid.Size - 1, roid.transform.position + offset, roid.Velocity, roid.RotationVelocity, roid.Image);
				SpawnAsteroid(roid.Size - 1, roid.transform.position - offset, roid.Velocity, roid.RotationVelocity, roid.Image);
			}
			RemoveAsteroid(roid);
			Game.Instance.IncrementScore();
		}
	}
}

