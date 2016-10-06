using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class Cheats : MonoBehaviour
	{
		public static bool Enabled { get; set; }

		List<BeatLayerSwitch> beatSwitches;
		BossGate bossGate;
		Savepoint[] savepoints;
		Utils.Label cheatWatermark;

		public void Awake()
		{
			cheatWatermark = new Utils.Label
			{
				position = new Rect(UI.Scale(500), UI.Scale(100), Screen.width, Screen.height),
				text = "CHEATS ENABLED",
				style = new GUIStyle
				{
					fontSize = UI.Scale(30),
					fontStyle = FontStyle.Bold
				}
			};
			cheatWatermark.style.normal.textColor = Color.magenta;
		}

		public void Start()
		{
			beatSwitches = new List<BeatLayerSwitch>();

			var levelsFolder = GameObject.Find("Levels");
			if (levelsFolder)
			{
				bossGate = levelsFolder.GetComponent<BossGate>();
				savepoints = levelsFolder.GetComponentsInChildren<Savepoint>();

				var beatLayerSwitches = levelsFolder.GetComponentsInChildren<BeatLayerSwitch>();
				foreach (var layerSwitch in beatLayerSwitches)
				{
					Debug.Log($"[{beatSwitches.Count}] globalBeatLayer={layerSwitch.globalBeatLayer},"
						+ $"activateAllPreviousLayers={layerSwitch.activateAllPreviousLayers},"
						+ $"deActivateAllPreivousLayers={layerSwitch.deActivateAllPreviousLayers}");
					beatSwitches.Add(layerSwitch);
				}

				beatSwitches.Sort((s1, s2) => s1.globalBeatLayer < s2.globalBeatLayer ? -1 : 1);

				// last switch of level 1 is out of bounds
				// hub switch will bug out if used with cheats
				if (Application.loadedLevel <= 1)
					beatSwitches.RemoveAt(beatSwitches.Count - 1);
			}
		}

		public void Update()
		{
			if (Application.isLoadingLevel)
				return;

			if (!Enabled)
			{
				if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
					&& Input.GetKeyDown(KeyCode.F12))
					Enabled = true;
				else
					return;
			}

			var rigthAlt = Input.GetKey(KeyCode.RightAlt);
			if (rigthAlt || Input.GetKey(KeyCode.LeftAlt))
			{
				// check 1 to 3 alpha keys
				for (var key = KeyCode.Alpha1; key <= KeyCode.Alpha3; key++)
				{
					if (!Input.GetKeyDown(key))
						continue;

					MirrorModeManager.mirrorModeActive = rigthAlt;
					MirrorModeManager.respawnFromMirror = false;
					Application.LoadLevel(key - KeyCode.Alpha1 + 1);
					break;
				}
			}
			else
			{
				// check 1 to 9 alpha keys
				for (var key = KeyCode.Alpha1; key <= KeyCode.Alpha9; key++)
				{
					int keyNum = key - KeyCode.Alpha1;
					if (keyNum >= beatSwitches.Count || !Input.GetKeyDown(key))
						continue;

					var player = Globals.player.GetComponent<MyCharacterController>();
					if (player.IsForceMoveActive() || player.IsLogicPause())
						break;

					var beatSwitch = beatSwitches[keyNum];
					player.SetVelocity(Vector2.zero);
					player.PlacePlayerCharacter(beatSwitch.transform.position, true);
					if (beatSwitch.globalBeatLayer >= Globals.beatMaster.GetCurrentBeatLayer())
						beatSwitch.CheatUse();
					break;
				}
			}

			if (Input.GetKeyDown(KeyCode.Delete))
			{
				TeleportToCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.Home))
			{
				Globals.levelsManager.SetCurrentCheckpoint(0);
				TeleportToCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.End))
			{
				Globals.levelsManager.SetCurrentCheckpoint(savepoints.Length - 1);
				TeleportToCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.PageUp))
			{
				if (Globals.levelsManager.GetCurrentCheckPoint() < savepoints.Length - 1)
					Globals.levelsManager.IncreaseCheckPoint();
				TeleportToCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.PageDown))
			{
				var currentCheckpoint = Globals.levelsManager.GetCurrentCheckPoint();
				if (currentCheckpoint > 0)
					Globals.levelsManager.SetCurrentCheckpoint(currentCheckpoint - 1);
				TeleportToCheckpoint();
			}
		}

		void TeleportToCheckpoint()
		{
			var savePoint = Globals.levelsManager.GetSavepoint();
			var player = Globals.player.GetComponent<MyCharacterController>();
			player.PlacePlayerCharacter(savePoint.transform.position, savePoint.spawnPlayerOnGround);
		}

		public void OnGUI()
		{
			if (Enabled)
				cheatWatermark.OnGUI();
		}
	}
}
