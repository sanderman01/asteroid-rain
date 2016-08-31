using UnityEngine;

namespace AsteroidRain
{
	public static class Util
	{
		/// <summary>
		/// Finds a gameobject and component by name and returns it.
		/// Similar to GameObject.Find except that this one includes a check to ensure that the object exists and has the proper component.
		/// In the event of failure, we try to fail hard, by explicitly pausing the editor and throwing an exception.
		/// By using this method, we can be alerted early when the scene is not setup properly.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T SafeFind<T>(string name)
		{
			GameObject obj = GameObject.Find(name);
			if (obj == null)
			{
				Debug.Break();
				throw new System.Exception(string.Format("Could not find gameobject: {0}", name));
			}
			T result = obj.GetComponent<T>();
			if (result == null)
			{
				Debug.Break();
				throw new System.Exception(string.Format("Gameobject: {0} did not have a {1} component", name, typeof(T).Name));
			}
			return result;
		}
	}
}
