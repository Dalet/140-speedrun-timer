using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class BeatLayerSwitchObserver : MonoBehaviour
	{
		GameObject _levelsFolder;
		MenuSystem _menuSystem;

		void Awake()
		{
			_levelsFolder = GameObject.Find("Levels");
			if (_levelsFolder == null)
			{
				Debug.Log($"{nameof(BeatLayerSwitchObserver)}: Couldn't find Levels object");
				Destroy(this);
				return;
			}

			var menuSystemObj = GameObject.Find("_MenuSystem");
			if (menuSystemObj != null)
				_menuSystem = menuSystemObj.GetComponent<MenuSystem>();
			else
				Debug.Log($"{nameof(BeatLayerSwitchObserver)}: Couldn't find _MenuSystem object");
		}

		void OnEnable()
		{
			var colorSpheres = _levelsFolder.GetComponentsInChildren<ColorSphere>();
			foreach (var colorSphere in colorSpheres)
			{
				if (_menuSystem != null && colorSphere == _menuSystem.colorSphere)
				{
					colorSphere.colorSphereOpened += OnMenuKeyUsed;
					colorSphere.colorSphereExpanding += OnMenuColorSphereExpanding;
				}
				else
				{
					colorSphere.colorSphereExpanding += OnColorSphereExpanding;
				}
			}
		}

		void OnDisable()
		{
			if (_levelsFolder == null)
				return;

			var colorSpheres = _levelsFolder.GetComponentsInChildren<ColorSphere>();
			foreach (var colorSphere in colorSpheres)
			{
				if (_menuSystem != null && colorSphere == _menuSystem.colorSphere)
				{
					colorSphere.colorSphereOpened -= OnMenuKeyUsed;
					colorSphere.colorSphereExpanding -= OnMenuColorSphereExpanding;
				}
				else
				{
					colorSphere.colorSphereExpanding -= OnColorSphereExpanding;
				}
			}
		}

		void OnColorSphereExpanding()
		{
			Debug.Log("BeatLayerSwitch colorsphere expanding: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.Split();
		}

		void OnMenuColorSphereExpanding()
		{
			Debug.Log("OnMenuColorSphereExpanding: " + DebugBeatListener.DebugStr);

			// colorsphere expands 32 quarterbeats after opening, then starts the load after 0.66s
			SpeedrunTimer.Instance?.Split();
			SpeedrunTimer.Instance?.StartLoad(660);
		}

		void OnMenuKeyUsed()
		{
			Debug.Log("OnMenuKeyUsed: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.Unfreeze();
		}
	}
}
