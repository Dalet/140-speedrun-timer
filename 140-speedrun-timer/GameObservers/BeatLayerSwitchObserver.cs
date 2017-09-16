using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class BeatLayerSwitchObserver : MonoBehaviour
	{
		GameObject _levelsFolder;

		void Awake()
		{
			_levelsFolder = GameObject.Find("Levels");
			if (_levelsFolder == null)
			{
				Destroy(this);
				return;
			}
		}

		void OnEnable()
		{
			var colorSpheres = _levelsFolder.GetComponentsInChildren<ColorSphere>();
			foreach (var colorSphere in colorSpheres)
				colorSphere.colorSphereOpened += OnKeyUsed;
		}

		void OnDisable()
		{
			if (_levelsFolder == null)
				return;

			var colorSpheres = _levelsFolder.GetComponentsInChildren<ColorSphere>();
			foreach (var colorSphere in colorSpheres)
				colorSphere.colorSphereOpened -= OnKeyUsed;
		}

		void OnKeyUsed()
		{
			Debug.Log("OnKeyUsed: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.Split(150, -1);
		}
	}
}
