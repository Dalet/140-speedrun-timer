using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class SpeedrunTimerLoader : MonoBehaviour
	{
		static GameObject _modObject;

		public void Awake()
		{
			var modLevelObject = new GameObject();
			modLevelObject.AddComponent<Misc>();
			modLevelObject.AddComponent<Cheats>();

			if (_modObject != null)
				return;
			_modObject = new GameObject();
			_modObject.AddComponent<SpeedrunTimer>();
			DontDestroyOnLoad(_modObject);
			Destroy(this);
		}

		public static void Inject()
		{
			Globals.Instance().gameObject.AddComponent<SpeedrunTimerLoader>();
		}
	}
}
