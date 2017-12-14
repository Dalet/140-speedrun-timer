using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod.GameObservers
{
	[GameObserver]
	class TitleScreenObserver : MonoBehaviour
	{
		static FieldInfo _titleScreenStateField;

		TitleScreen _titlescreen;
		object _titleScreenStateStart;
		object _prevTitleScreenState;

		void Awake()
		{
			if (Application.loadedLevelName != "Level_Menu")
			{
				Destroy(this);
				return;
			}

			if (_titleScreenStateField == null)
				_titleScreenStateField = typeof(TitleScreen)
					.GetField("myState", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		void Start()
		{
			var titlescreenObj = GameObject.Find("_TitleScreen");
			if (titlescreenObj == null)
			{
				Debug.Log($"{nameof(TitleScreenObserver)}: Couldn't find _TitleScreen object");
				Destroy(this);
				return;
			}

			_titlescreen = titlescreenObj.GetComponent<TitleScreen>();
		}

		void Update()
		{
			if (_titleScreenStateStart == null)
				_titleScreenStateStart = _titleScreenStateField.GetValue(_titlescreen);
		}

		void LateUpdate()
		{
			if (_titleScreenStateStart == null || SpeedrunTimer.Instance.IsRunning)
				return;

			var titleScreenState = _titleScreenStateField.GetValue(_titlescreen);
			if (titleScreenState != _prevTitleScreenState && _titleScreenStateStart == _prevTitleScreenState)
			{
				OnTitleScreenStart();
			}
			_prevTitleScreenState = titleScreenState;
		}

		void OnTitleScreenStart()
		{
			SpeedrunTimer.Instance.StartTimer();
			Debug.Log("SpeedrunTimer started: " + DebugBeatListener.DebugStr);
		}
	}
}
