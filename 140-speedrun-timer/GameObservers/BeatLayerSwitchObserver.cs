using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class BeatLayerSwitchObserver : MonoBehaviour
	{
		static Type _keyType;
		static EventInfo _keyUsedEvent;
		static MethodInfo _onKeyUsed;

		readonly Delegate _onKeyUsedDelegate;

		MenuSystem _menuSystem;
		BossGate _bossGate;
		ColorSphere[] _beatSwitchColorSpheres;
		object[] _keys;
		bool _openGateOnNextBeat;

		static BeatLayerSwitchObserver()
		{
			var typeName = ModLoader.IsLegacyVersion ? "Key" : "GateKey";
			_keyType = typeof(MenuSystem).Assembly.GetType(typeName);
			_keyUsedEvent = _keyType.GetEvent("keyUsedEvent");
			_onKeyUsed = typeof(BeatLayerSwitchObserver)
				.GetMethod(nameof(OnKeyUsed), BindingFlags.NonPublic | BindingFlags.Instance);
		}

		BeatLayerSwitchObserver()
		{
			var handlerType = _keyUsedEvent.EventHandlerType;
			_onKeyUsedDelegate = Delegate.CreateDelegate(handlerType, this, _onKeyUsed);
		}

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

			_keys = levelsFolder.GetComponentsInChildren(_keyType);
		}

		void OnEnable()
		{
			SubscribeGlobalBeatMaster();

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

			if (_keys != null)
			{
				foreach (var key in _keys)
					_keyUsedEvent.AddEventHandler(key, _onKeyUsedDelegate);
			}
		}

		void OnDisable()
		{
			UnsubGlobalBeatMaster();

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

			if (_keys != null)
			{
				foreach (var key in _keys)
					_keyUsedEvent.RemoveEventHandler(key, _onKeyUsedDelegate);
			}
		}

		void SubscribeGlobalBeatMaster()
		{
			if (Globals.beatMaster == null)
				return;

			UnsubGlobalBeatMaster();
			Globals.beatMaster.onBeat += BeatMaster_onBeat;
		}

		void UnsubGlobalBeatMaster()
		{
			if (Globals.beatMaster == null)
				return;

			Globals.beatMaster.onBeat -= BeatMaster_onBeat;
		}

		void BeatMaster_onBeat(int index)
		{
			if (_openGateOnNextBeat && (index + 1) % 4 == 0)
			{
				_openGateOnNextBeat = false;
				SpeedrunTimer.Instance.Split(660, 32);
			}
		}

		void OnKeyUsed()
		{
			_openGateOnNextBeat = true;
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
				SpeedrunTimer.Instance?.ResetTimer();

			SpeedrunTimer.Instance?.Unfreeze();
		}
	}
}
