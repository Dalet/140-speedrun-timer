using UnityEngine;

namespace SpeedrunTimerMod
{
	sealed class SpeedrunTimerLoader : MonoBehaviour
	{
		static GameObject modObject;

		public void Awake()
		{
			var modLevelObject = new GameObject();
			modLevelObject.AddComponent<Misc>();
			modLevelObject.AddComponent<Cheats>();

			if (modObject != null)
				return;
			modObject = new GameObject();
			modObject.AddComponent<SpeedrunTimer>();
			DontDestroyOnLoad(modObject);
			Destroy(this);
		}
	}
}
