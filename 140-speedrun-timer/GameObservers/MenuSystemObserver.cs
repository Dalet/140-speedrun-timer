using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class MenuSystemObserver : MonoBehaviour
	{
		MenuSystem _menuSystem;

		void Awake()
		{
			var menuSystemObj = GameObject.Find("_MenuSystem");
			if (menuSystemObj == null)
			{
				Debug.Log($"{nameof(MenuSystemObserver)}: Couldn't find _MenuSystem object");
				Destroy(this);
				return;
			}

			_menuSystem = menuSystemObj.GetComponent<MenuSystem>();
		}

		void OnEnable()
		{
			_menuSystem.colorSphere.colorSphereOpened += OnMenuKeyUsed;
			_menuSystem.colorSphere.colorSphereExpanding += OnColorSphereExpanding;
		}

		void OnDisable()
		{
			if (_menuSystem == null)
				return;

			_menuSystem.colorSphere.colorSphereOpened -= OnMenuKeyUsed;
			_menuSystem.colorSphere.colorSphereExpanding -= OnColorSphereExpanding;
		}

		void OnColorSphereExpanding()
		{
			Debug.Log("Menu colorsphere expanding: " + DebugBeatListener.DebugStr);

			// colorsphere expands 32 quarterbeats after opening, then starts the load after 0.66s
			SpeedrunTimer.Instance?.StartLoad();
		}

		void OnMenuKeyUsed() // triggered slightly before OnKeyUsed
		{
			Debug.Log("OnMenuKeyUsed: " + DebugBeatListener.DebugStr);

			SpeedrunTimer.Instance?.Unfreeze();
			//SpeedrunTimer.Instance?.StartLoad(150, -1);
		}
	}
}
