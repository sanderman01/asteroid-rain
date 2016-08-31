using UnityEngine;
using UnityEngine.UI;

namespace AsteroidRain
{
	/// <summary>
	/// Overlay that slowly fades out by decaying the image alpha.
	/// </summary>
	public class ColorOverlay : MonoBehaviour
	{
		private const float startAlpha = 0.5f;
		private const float decay = 0.95f;

		private Image image;

		void Awake()
		{
			image = GetComponent<Image>();
		}

		public void TriggerEffect()
		{
			Color c = image.color;
			c.a = startAlpha;
			image.color = c;
		}

		void Update()
		{
			
			Color c = image.color;
			c.a *= decay;
			image.color = c;
		}
	}
}