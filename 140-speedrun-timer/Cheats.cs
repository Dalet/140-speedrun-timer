using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class Cheats : MonoBehaviour
	{
		public static bool Enabled { get; set; }
		public static Savepoint[] Savepoints { get; private set; } = new Savepoint[] { };

		List<BeatLayerSwitch> beatSwitches;
		Utils.Label cheatWatermark;

		public void Awake()
		{
			Savepoints = new Savepoint[] { };

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
				Savepoints = levelsFolder.GetComponentsInChildren<Savepoint>();

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
				TeleportToCurrentCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.Home))
			{
				Globals.levelsManager.SetCurrentCheckpoint(1);
				TeleportToCurrentCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.End))
			{
				Globals.levelsManager.SetCurrentCheckpoint(Savepoints.Length - 1);
				TeleportToCurrentCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.PageUp))
			{
				TeleportToNearestCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.PageDown))
			{
				TeleportToNearestCheckpoint(false);
			}
		}

		public static void TeleportToSavepoint(Savepoint savepoint)
		{
			var player = Globals.player.GetComponent<MyCharacterController>();
			player.StopForceMoveTo();
			player.PlacePlayerCharacter(savepoint.transform.position, savepoint.spawnPlayerOnGround);
		}

		public static void TeleportToCurrentCheckpoint()
		{
			if (Savepoints.Length == 0)
				return;

			var savepoint = Globals.levelsManager.GetSavepoint();
			TeleportToSavepoint(savepoint);
		}

		public static void TeleportToNearestCheckpoint(bool forward = true)
		{
			if (Savepoints.Length == 0)
				return;

			var savepoint = GetNearestCheckpoint(forward);
			if (savepoint != null)
				TeleportToSavepoint(savepoint);
		}

		static Savepoint GetNearestCheckpoint(bool forward = true)
		{
			if (Savepoints.Length == 0)
				return null;

			var levelManager = Globals.levelsManager;
			var playerPos = Globals.player.GetComponent<MyCharacterController>().transform.position;
			Savepoint savepoint = null;

			// skip the first savepoint, it is the spawn point
			for (int i = 1; i < Savepoints.Length; i++)
			{
				levelManager.SetCurrentCheckpoint(i);
				savepoint = levelManager.GetSavepoint();

				if (playerPos.x < savepoint.transform.position.x)
				{
					if (!forward)
					{
						if (i <= 1)
							return null;

						savepoint = GetSavepoint(i - 1);
						if (i > 2 && savepoint.transform.position.x == playerPos.x)
							savepoint = GetSavepoint(i - 2);
					}
					break;
				}

				if (!forward && i == Savepoints.Length - 1)
					savepoint = GetSavepoint(i - 1);
			}

			return savepoint;
		}

		static Savepoint GetSavepoint(int id)
		{
			var originalCheckpt = Globals.levelsManager.GetCurrentCheckPoint();
			Globals.levelsManager.SetCurrentCheckpoint(id);
			var s = Globals.levelsManager.GetSavepoint();
			Globals.levelsManager.SetCurrentCheckpoint(originalCheckpt);
			return s;
		}

		public void OnGUI()
		{
			if (Enabled)
				cheatWatermark.OnGUI("CHEATS ENABLED");
		}
	}
}
