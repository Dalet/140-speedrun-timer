using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class Cheats : MonoBehaviour
	{
		public static bool Enabled { get; set; }
		public static Savepoint[] Savepoints { get; private set; } = new Savepoint[] { };
		public static bool RainbowPlayerEnabled { get; set; }

		List<BeatLayerSwitch> _beatSwitches;
		Utils.Label _cheatWatermark;
		float _playerHue;

		public void Awake()
		{
			Savepoints = new Savepoint[] { };

			_cheatWatermark = new Utils.Label
			{
				position = new Rect(UI.Scale(500), UI.Scale(100), Screen.width, Screen.height),
				text = "CHEATS ENABLED",
				style = new GUIStyle
				{
					fontSize = UI.Scale(30),
					fontStyle = FontStyle.Bold
				}
			};
			_cheatWatermark.style.normal.textColor = Color.magenta;
		}

		public void Start()
		{
			_beatSwitches = new List<BeatLayerSwitch>();

			var levelsFolder = GameObject.Find("Levels");
			if (levelsFolder)
			{
				Savepoints = levelsFolder.GetComponentsInChildren<Savepoint>();

				var beatLayerSwitches = levelsFolder.GetComponentsInChildren<BeatLayerSwitch>();
				foreach (var layerSwitch in beatLayerSwitches)
				{
					_beatSwitches.Add(layerSwitch);
				}

				_beatSwitches.Sort((s1, s2) => s1.globalBeatLayer < s2.globalBeatLayer ? -1 : 1);

				// last switch of level 1 is out of bounds
				// hub switch will bug out if used with cheats
				if (Application.loadedLevel <= 1)
					_beatSwitches.RemoveAt(_beatSwitches.Count - 1);
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
			var player = Globals.player.GetComponent<MyCharacterController>();

			var rightAlt = Input.GetKey(KeyCode.RightAlt);
			if (rightAlt || Input.GetKey(KeyCode.LeftAlt))
			{
				for (var key = KeyCode.Alpha1; key <= KeyCode.Alpha4; key++)
				{
					if (!Input.GetKeyDown(key))
						continue;

					LoadLevel(key - KeyCode.Alpha0, rightAlt);
					break;
				}
			}
			else if (!player.IsForceMoveActive() && !player.IsLogicPause())
			{
				for (var key = KeyCode.Alpha1; key <= KeyCode.Alpha9; key++)
				{
					int keyNum = key - KeyCode.Alpha1;
					if (keyNum >= _beatSwitches.Count || !Input.GetKeyDown(key))
						continue;

					TeleportToBeatLayerSwitch(_beatSwitches[keyNum]);
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

			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
				{
					RainbowPlayerEnabled = !RainbowPlayerEnabled;
				}
				else
				{
					if (RainbowPlayerEnabled)
						RainbowPlayerEnabled = false;
					else
						TogglePlayerColor();
				}
			}

			RainbowPlayerUpdate();
		}

		void RainbowPlayerUpdate()
		{
			if (!RainbowPlayerEnabled)
				return;

			var player = Globals.player.GetComponent<MyCharacterController>();
			var c = Utils.HslToRgba(_playerHue, 1, 0.5f, 1);
			player.visualPlayer.SetColor(c, 1f);
			_playerHue += 0.5f * Time.deltaTime;
			if (_playerHue > 1)
				_playerHue = 0;
		}

		public static void LoadLevel(int level, bool mirrored)
		{
			// levels are offset by 1 in the 2017 update
			if (!SpeedrunTimerLoader.IsLegacyVersion)
				level++;

			MirrorModeManager.mirrorModeActive = mirrored;
			MirrorModeManager.respawnFromMirror = false;

			Application.LoadLevel(level);
		}

		public static void TeleportToBeatLayerSwitch(BeatLayerSwitch beatSwitch)
		{
			var player = Globals.player.GetComponent<MyCharacterController>();
			player.SetVelocity(Vector2.zero);
			player.PlacePlayerCharacter(beatSwitch.transform.position, true);
			if (beatSwitch.globalBeatLayer >= Globals.beatMaster.GetCurrentBeatLayer())
				beatSwitch.CheatUse();
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

		public static void TogglePlayerColor()
		{
			var player = Globals.player.GetComponent<MyCharacterController>();
			var color = player.visualPlayer.GetColor() == Color.black
				? Color.white
				: Color.black;
			player.visualPlayer.SetColor(color, 1);
		}

		public void OnGUI()
		{
			if (Enabled)
				_cheatWatermark.OnGUI();
		}
	}
}
