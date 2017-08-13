using System.Collections.Generic;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class Cheats : MonoBehaviour
	{
		public static bool Enabled { get; set; }
		public static bool InvincibilityEnabled { get; private set; }

		List<Savepoint> _savepoints;
		List<BeatLayerSwitch> _beatSwitches;
		Utils.Label _cheatWatermark;
		float _playerHue;

		void Awake()
		{
			_savepoints = new List<Savepoint>();
			_beatSwitches = new List<BeatLayerSwitch>();

			if (Application.loadedLevelName == "Level_Menu")
			{
				InvincibilityEnabled = false;
			}

			_cheatWatermark = new Utils.Label
			{
				positionDelegate = () => new Rect(0, UI.Scale(100), Screen.width, Screen.height - UI.Scale(100)),
				text = "CHEATS ENABLED",
				fontSize = 30,
				style = new GUIStyle
				{
					fontStyle = FontStyle.Bold,
					alignment = (TextAnchor)TextAlignment.Center
				}
			};
			_cheatWatermark.style.normal.textColor = Color.magenta;
		}

		void Start()
		{
			var levelsFolder = GameObject.Find("Levels");
			if (!levelsFolder)
				return;

			_savepoints.AddRange(levelsFolder.GetComponentsInChildren<Savepoint>());
			_savepoints.Sort((s1, s2) => s1.transform.position.x.CompareTo(s2.transform.position.x));

			_beatSwitches.AddRange(levelsFolder.GetComponentsInChildren<BeatLayerSwitch>());
			_beatSwitches.Sort((s1, s2) => s1.globalBeatLayer.CompareTo(s2.globalBeatLayer));

			// last switch of level 1 is out of bounds
			// hub switch will bug out if used with cheats
			if (Application.loadedLevel <= 1)
				_beatSwitches.RemoveAt(_beatSwitches.Count - 1);
		}

		void Update()
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
					if (Input.GetKeyDown(key))
					{
						TeleportToBeatLayerSwitch(keyNum);
					}
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
				Globals.levelsManager.SetCurrentCheckpoint(_savepoints.Count - 1);
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
				TogglePlayerColor();
			}

			if (Input.GetKeyUp(KeyCode.I) && Application.loadedLevelName != "Level_Menu")
			{
				InvincibilityEnabled = !InvincibilityEnabled;
			}

			if (Input.GetKeyDown(KeyCode.Q))
			{
				KillPlayer();
			}

			RainbowPlayerUpdate();
		}

		void RainbowPlayerUpdate()
		{
			if (!InvincibilityEnabled)
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
			if (!ModLoader.IsLegacyVersion)
				level++;

			MirrorModeManager.mirrorModeActive = mirrored;
			MirrorModeManager.respawnFromMirror = false;

			Application.LoadLevel(level);
		}

		void TeleportToBeatLayerSwitch(int id)
		{
			if (id == 4 && Application.loadedLevelName.StartsWith("Level1_"))
			{
				var levelsFolder = GameObject.Find("Levels");
				var bossGate = levelsFolder.GetComponentInChildren<BossGate>();
				if (bossGate)
					TeleportToBeatLayerSwitch(bossGate);
				return;
			}

			if (id >= 0 && id < _beatSwitches.Count)
			{
				TeleportToBeatLayerSwitch(_beatSwitches[id]);
			}
		}

		public static void TeleportToBeatLayerSwitch(BeatLayerSwitch beatSwitch)
		{
			var player = Globals.player.GetComponent<MyCharacterController>();
			player.SetVelocity(Vector2.zero);
			player.PlacePlayerCharacter(beatSwitch.transform.position, true);
			if (beatSwitch.globalBeatLayer >= Globals.beatMaster.GetCurrentBeatLayer())
				beatSwitch.CheatUse();
		}

		public static void TeleportToBeatLayerSwitch(BossGate bossGate)
		{
			var player = Globals.player.GetComponent<MyCharacterController>();
			player.SetVelocity(Vector2.zero);
			player.PlacePlayerCharacter(bossGate.transform.position, true);
			bossGate.CheatUse();
		}

		public static void TeleportToSavepoint(Savepoint savepoint)
		{
			var player = Globals.player.GetComponent<MyCharacterController>();
			player.StopForceMoveTo();
			player.PlacePlayerCharacter(savepoint.transform.position, savepoint.spawnPlayerOnGround);
		}

		public static void TeleportToCurrentCheckpoint()
		{
			var savepoint = Globals.levelsManager.GetSavepoint();
			TeleportToSavepoint(savepoint);
		}

		public void TeleportToNearestCheckpoint(bool searchToRight = true)
		{
			var savepoint = GetNearestCheckpoint(searchToRight);
			if (savepoint != null)
				TeleportToSavepoint(savepoint);
		}

		Savepoint GetNearestCheckpoint(bool searchToRight = true)
		{
			var levelManager = Globals.levelsManager;
			var playerPos = Globals.player.GetComponent<MyCharacterController>().transform.position;
			var currentSavePoint = levelManager.GetSavepoint();

			var i = searchToRight
				? 1 // skip the first savepoint, it is the spawn point
				: _savepoints.Count - 1;
			for (; i >= 0 && i < _savepoints.Count; i += searchToRight ? 1 : -1)
			{
				var savepoint = GetSavepoint(i);
				if (currentSavePoint == savepoint)
					continue;

				var savepointPos = savepoint.transform.position;
				var distance = savepointPos.x - playerPos.x;

				if (searchToRight)
				{
					if (distance > 0)
						return savepoint;
				}
				else
				{
					if (distance < 0)
						return savepoint;
				}
			}

			return null;
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

		public static void KillPlayer()
		{
			var player = Globals.player.GetComponent<MyCharacterController>();
			player.Kill();
		}

		void OnGUI()
		{
			if (!Enabled)
				return;

			_cheatWatermark.OnGUI();
		}
	}
}
