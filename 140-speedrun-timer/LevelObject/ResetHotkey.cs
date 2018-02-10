using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class ResetHotkey : MonoBehaviour
	{
		static FieldInfo isInChargeStateField;

		float _resetTimer;
		float _resetHits;

		void OnLevelWasLoaded(int level)
		{
			if (!ModLoader.IsLegacyVersion)
			{
				if (isInChargeStateField == null)
					isInChargeStateField = typeof(ColorSphere).GetField("public_isInChargeState");
				isInChargeStateField.SetValue(null, false);
			}
		}

		void Update()
		{
			if (Application.isLoadingLevel)
				return;

			if (_resetHits == 1)
				_resetTimer += Time.deltaTime;

			if (_resetTimer > 0.5f)
				_resetHits = 0;

			if (Input.GetKeyDown(KeyCode.R))
			{
				if (_resetHits == 0)
					_resetTimer = 0;

				_resetHits++;
				if (_resetHits > 1 && _resetTimer < 0.5f)
				{
					_resetHits = 0;
					SpeedrunTimer.Instance.ResetTimer();
					OldSpeedrunTimer.Instance.ResetTimer();
					MirrorModeManager.mirrorModeActive = false;
					MirrorModeManager.respawnFromMirror = false;
					if (Cheats.Enabled)
					{
						var cheatComponent = ModLoader.LevelObject.GetComponent<Cheats>();
						cheatComponent.FlashWatermarkAcrossLoad();
					}
					Application.LoadLevel("Level_Menu");
				}
			}
		}
	}
}
