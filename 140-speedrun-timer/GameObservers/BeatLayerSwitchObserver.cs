using System.Linq;
using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class BeatLayerSwitchObserver : MonoBehaviour
	{
		MenuSystem _menuSystem;
		BossGate _bossGate;
		ColorSphere[] _beatSwitchColorSpheres;

		void Awake()
		{
			var levelsFolder = GameObject.Find("Levels");
			if (levelsFolder == null)
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

			_bossGate = levelsFolder.GetComponentInChildren<BossGate>();

			var beatswitches = levelsFolder.GetComponentsInChildren<BeatLayerSwitch>();
			var colorSpheres = beatswitches.Select(b => b.colorSphere);

			if (_menuSystem != null)
				colorSpheres = colorSpheres.Where(c => c != _menuSystem.colorSphere);

			_beatSwitchColorSpheres = colorSpheres.ToArray();
		}

		void OnEnable()
		{
			if (_beatSwitchColorSpheres != null)
			{
				foreach (var colorSphere in _beatSwitchColorSpheres)
					colorSphere.colorSphereExpanding += OnColorSphereExpanding;
			}

			if (_menuSystem != null)
			{
				_menuSystem.colorSphere.colorSphereOpened += OnMenuKeyUsed;
				_menuSystem.colorSphere.colorSphereExpanding += OnMenuColorSphereExpanding;
			}

			if (_bossGate != null)
				_bossGate.colorSphere.colorSphereExpanding += OnBossGateColorSphereExpanding;
		}

		void OnDisable()
		{
			if (_beatSwitchColorSpheres != null)
			{
				foreach (var colorSphere in _beatSwitchColorSpheres)
					colorSphere.colorSphereExpanding -= OnColorSphereExpanding;
			}

			if (_menuSystem != null)
			{
				_menuSystem.colorSphere.colorSphereOpened -= OnMenuKeyUsed;
				_menuSystem.colorSphere.colorSphereExpanding -= OnMenuColorSphereExpanding;
			}

			if (_bossGate != null)
				_bossGate.colorSphere.colorSphereExpanding -= OnBossGateColorSphereExpanding;
		}

		void OnBossGateColorSphereExpanding()
		{
			// colorSphereExpanding is triggered twice for some reason
			_bossGate.colorSphere.colorSphereExpanding -= OnBossGateColorSphereExpanding;
			Debug.Log("BossGate colorsphere expanding: " + DebugBeatListener.DebugStr);
			SpeedrunTimer.Instance?.Split();
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
			SpeedrunTimer.Instance?.StartLoad(660);
		}

		void OnMenuKeyUsed()
		{
			Debug.Log("OnMenuKeyUsed: " + DebugBeatListener.DebugStr);

			if (ModLoader.Settings.ILMode)
			{
				SpeedrunTimer.Instance?.ResetTimer();
				OldSpeedrunTimer.Instance?.ResetTimer();
			}

			SpeedrunTimer.Instance?.Unfreeze();
		}
	}
}
