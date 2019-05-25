using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class Cheats : MonoBehaviour
	{
		public static bool Enabled { get; set; }
		public static bool InvincibilityEnabled { get; private set; }
		public static bool FlashWatermarkOnStart { get; set; }

		public static bool LevelLoadedByCheat { get; private set; }
		static bool _loadingLevel;

		static FieldInfo _previousSphereField;
		static FieldInfo _activeColorSpheresField;
		static FieldInfo _allColorSpheresField;

		List<Savepoint> _savepoints;
		List<BeatLayerSwitch> _beatSwitches;
		Utils.Label _cheatWatermark;
		float _playerHue;
		AudioSource _cheatBeep;

		public void FlashWatermarkAcrossLoad()
		{
			FlashWatermarkOnStart = true;
			FlashWatermark();
		}

		public void FlashWatermark(bool playSound = true)
		{
			if (!ModLoader.Settings.FlashCheatWatermark)
				return;

			_cheatWatermark.ResetTimer();
			_cheatWatermark.Enabled = true;
			if (playSound)
			{
				_cheatBeep.Play();
			}
		}

		void Awake()
		{
			if (!_loadingLevel)
				LevelLoadedByCheat = false;
			else
				_loadingLevel = false;

			_cheatBeep = gameObject.AddComponent<AudioSource>();
			_cheatBeep.clip = Resources.Instance.CheatBeep;

			_cheatWatermark = new Utils.Label
			{
				positionDelegate = () => new Rect(0, UI.ScaleVertical(100), Screen.width, Screen.height - UI.ScaleVertical(100)),
				text = "CHEATS ENABLED",
				fontSize = 40,
				enableOutline = true,
				outlineColor = Color.black,
				style = new GUIStyle
				{
					fontStyle = FontStyle.Bold,
					alignment = TextAnchor.UpperCenter
				}
			};

			if (ModLoader.Settings.FlashCheatWatermark)
			{
				_cheatWatermark.fontSize = 60;
				_cheatWatermark.positionDelegate = () => new Rect(0, 0, Screen.width, Screen.height);
				_cheatWatermark.displayTime = 2f;
				_cheatWatermark.text = "CHEAT USED";
				_cheatWatermark.style.alignment = TextAnchor.MiddleCenter;
			}

			_cheatWatermark.style.normal.textColor = Color.magenta;
		}

		void Start()
		{
			if (Application.loadedLevelName == "Level_Menu")
			{
				InvincibilityEnabled = false;
			}

			if (FlashWatermarkOnStart)
			{
				FlashWatermarkOnStart = false;
				FlashWatermark();
			}

			LoadTeleportPoints();
		}

		void LoadTeleportPoints()
		{
			_savepoints = new List<Savepoint>();
			_beatSwitches = new List<BeatLayerSwitch>();

			var levelsFolder = GameObject.Find("Levels");
			if (!levelsFolder)
				return;

			foreach (var savepoint in levelsFolder.GetComponentsInChildren<Savepoint>())
			{
				if (!(savepoint is SpawnPoint))
					_savepoints.Add(savepoint);
			}
			_savepoints.Sort((s1, s2) => s1.GetGlobalSavepointID().CompareTo(s2.GetGlobalSavepointID()));

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
				{
					Enabled = true;
					FlashWatermark();
				}
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

					FlashWatermarkAcrossLoad();
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
						FlashWatermark();
						TeleportToBeatLayerSwitch(keyNum);
					}
				}
			}

			if (Input.GetKeyDown(KeyCode.Delete))
			{
				FlashWatermark();
				TeleportToCurrentCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.Home))
			{
				FlashWatermark();
				var savepoint = _savepoints[0];
				TeleportToSavepoint(savepoint);
			}
			else if (Input.GetKeyDown(KeyCode.End))
			{
				FlashWatermark();
				var savepoint = _savepoints[_savepoints.Count - 1];
				TeleportToSavepoint(savepoint);
			}
			else if (Input.GetKeyDown(KeyCode.PageUp))
			{
				FlashWatermark();
				TeleportToNearestCheckpoint();
			}
			else if (Input.GetKeyDown(KeyCode.PageDown))
			{
				FlashWatermark();
				TeleportToNearestCheckpoint(false);
			}

			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				FlashWatermark();
				TogglePlayerColor();
			}

			if (Input.GetKeyUp(KeyCode.I) && Application.loadedLevelName != "Level_Menu")
			{
				FlashWatermark();
				InvincibilityEnabled = !InvincibilityEnabled;
			}

			if (Input.GetKeyDown(KeyCode.Q))
			{
				FlashWatermark();
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
			_loadingLevel = true;
			LevelLoadedByCheat = true;

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

			if (id < 0 || id >= _beatSwitches.Count)
			{
				return;
			}
			var beatSwitch = _beatSwitches[id];
			if (beatSwitch.globalBeatLayer > Globals.beatMaster.GetCurrentBeatLayer())
			{
				// change the colors of the level and player according to the gate
				var player = Globals.player.GetComponent<MyCharacterController>();
				player.visualPlayer.SetColor((id % 2 == 0) ? Color.black : Color.white, 1f);
				CheatColorSphere(id + 1);
			}
			TeleportToBeatLayerSwitch(beatSwitch);
		}

		void CheatColorSphere(int beatIndex)
		{
			if (beatIndex < 1)
			{
				return;
			}

			if (_previousSphereField == null)
			{
				_previousSphereField = typeof(ColorChangeSystem)
					.GetField("previousSphere", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			if (_activeColorSpheresField == null)
			{
				_activeColorSpheresField = typeof(ColorChangeSystem)
					.GetField("activeColorSpheres", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			if (_allColorSpheresField == null)
			{
				_allColorSpheresField = typeof(ColorChangeSystem)
					.GetField("allColorSpheres", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			var previousSphere = (ColorSphere)_previousSphereField.GetValue(Globals.colorChangeSystem);
			var activeColorSpheres = (List<ColorSphere>)_activeColorSpheresField.GetValue(Globals.colorChangeSystem);
			var allColorSpheres = (List<ColorSphere>)_allColorSpheresField.GetValue(Globals.colorChangeSystem);

			activeColorSpheres.Clear();

			if (beatIndex == 1)
			{
				_previousSphereField.SetValue(Globals.colorChangeSystem, null);
				activeColorSpheres.Add(allColorSpheres.Find(colorSphere => colorSphere.beatLayer == -1));
				return;
			}

			if (beatIndex == 2)
			{
				var prev = allColorSpheres.Find(colorSphere => colorSphere.beatLayer == -1);
				_previousSphereField.SetValue(Globals.colorChangeSystem, prev);
				activeColorSpheres.Add(allColorSpheres.Find(colorSphere => colorSphere.beatLayer == 1));
				return;
			}

			var previous = allColorSpheres.Find(colorSphere => colorSphere.beatLayer == beatIndex - 2);
			_previousSphereField.SetValue(Globals.colorChangeSystem, previous);
			activeColorSpheres.Add(allColorSpheres.Find(colorSphere => colorSphere.beatLayer == beatIndex - 1));
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
			var playerPos = Globals.player.transform.position;
			var currentSavePoint = levelManager.GetSavepoint();

			var i = searchToRight ? 0 : _savepoints.Count - 1;
			for (; i >= 0 && i < _savepoints.Count; i += searchToRight ? 1 : -1)
			{
				var savepoint = _savepoints[i];
				var savepointPos = savepoint.transform.position;
				var distance = savepointPos.x - playerPos.x;

				if (searchToRight)
				{
					if (distance > 0.1)
						return savepoint;
				}
				else
				{
					if (distance < -0.1)
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
