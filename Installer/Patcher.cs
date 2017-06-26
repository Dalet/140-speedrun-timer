using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace SpeedrunTimerModInstaller
{
	class Patcher
	{
		public bool IsLegacyVersion => _isLegacyVersion.Value;
		Lazy<bool> _isLegacyVersion;

		string _gameDllPath;
		string _modDllPath;

		DefaultAssemblyResolver _resolver;
		ReaderParameters _readerParams;

		AssemblyDefinition GameAsmDef => _gameAsmDef.Value;
		AssemblyDefinition ModAsmDef => _modAsmDef.Value;
		Lazy<AssemblyDefinition> _gameAsmDef;
		Lazy<AssemblyDefinition> _modAsmDef;

		ModuleDefinition GameModule => GameAsmDef?.MainModule;
		ModuleDefinition ModModule => ModAsmDef?.MainModule;

		public Patcher(string assembliesPath, string gameDllPath, string modDllPath)
		{
			_gameDllPath = gameDllPath;
			_modDllPath = modDllPath;

			_resolver = new DefaultAssemblyResolver();
			_resolver.AddSearchDirectory(assembliesPath);
			_readerParams = new ReaderParameters { AssemblyResolver = _resolver };

			_gameAsmDef = new Lazy<AssemblyDefinition>(() =>
			{
				try
				{
					return AssemblyDefinition.ReadAssembly(_gameDllPath, _readerParams);
				}
				catch (UnauthorizedAccessException)
				{
					throw;
				}
				catch
				{
					return null;
				}
			});
			_modAsmDef = new Lazy<AssemblyDefinition>(() =>
			{
				try
				{
					return AssemblyDefinition.ReadAssembly(_modDllPath, _readerParams);
				}
				catch (UnauthorizedAccessException)
				{
					throw;
				}
				catch
				{
					return null;
				}
			});

			_isLegacyVersion = new Lazy<bool>(CheckLegacyVersion);
		}

		bool CheckLegacyVersion()
		{
			return !GameModule.GetTypes().Any(n => n.Name == "GravityBoss");
		}

		public void PatchGameDll(string destination = null)
		{
			if (destination == null)
				destination = _gameDllPath;

			Insert_Inject();
			Insert_OnResumeAfterDeath();
			//Insert_OnLevel3BossEnd();
			//Insert_OnMenuKeyUsed();
			Insert_OnKeyUsed();
			Insert_OnPlayerFixedUpdate();
			Patch_NoCheatAchievements();
			Patch_InvincibilityCheat();

			if (!IsLegacyVersion)
				Insert_OnLevel4BossEnd();

			GameAsmDef.Write(destination);
		}

		public bool IsGameDllPatched(string path = null)
		{
			if (path == null)
				path = _gameDllPath;

			var gameAsmDef = AssemblyDefinition.ReadAssembly(path, _readerParams);
			return gameAsmDef.MainModule.AssemblyReferences.Any(a => a.Name.ToLower().Contains("speedrun"));
		}

		public Version GetModDllVersion()
		{
			var modRef = GameModule?.AssemblyReferences.FirstOrDefault(a => a.Name.ToLower().Contains("speedrun"));
			if (modRef == null)
				return null;

			var modDllVer = ModAsmDef?.Name.Version;

			if (modRef.Version == modDllVer)
				return modDllVer;
			else
				return null;
		}

		void Insert_Inject()
		{
			// SpeedrunTimerLoader.Inject()
			var injectedMethod = GetMethodDef(ModModule, "SpeedrunTimerLoader", "Inject");
			var injectedMethodRef = GameModule.Import(injectedMethod);

			var targetMethod = GetMethodDef(GameModule, "Globals", "Awake");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.Last();
			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnPlayerFixedUpdate()
		{
			// Hooks.OnPlayerFixedUpdate() (calls SpeedrunTimer.EndLoad)
			var injectedMethodRef = GetMethodDef(ModModule, "Hooks", "OnPlayerFixedUpdate");
			var targetMethodName = IsLegacyVersion ? "FixedUpdate" : "upDATE_FixedUpdate";
			var targetMethod = GetMethodDef(GameModule, "MyCharacterController", targetMethodName);

			var ilProc = targetMethod.Body.GetILProcessor();
			var logicPaused = GetFieldDef(GameModule, "MyCharacterController", "logicPaused");
			var controlPaused = GetFieldDef(GameModule, "MyCharacterController", "controlPaused");
			var moveDirection = GetFieldDef(GameModule, "MyCharacterController", "moveDirection");

			var instructionsToInject = new Instruction[]
			{
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, GameModule.Import(logicPaused)),
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, GameModule.Import(controlPaused)),
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, GameModule.Import(moveDirection)),
				ilProc.Create(OpCodes.Call, GameModule.Import(injectedMethodRef))
			};

			foreach (var instruc in instructionsToInject)
				ilProc.InsertBefore(ilProc.Body.Instructions.Last(), instruc);
		}

		void Insert_OnResumeAfterDeath()
		{
			var injectedMethodDef = GetMethodDef(ModModule, "Hooks", "OnResumeAfterDeath");
			var injectedMethodRef = GameModule.Import(injectedMethodDef);
			var targetMethod = GetMethodDef(GameModule, "MyCharacterController", "ResumeAfterDeath");

			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnMenuKeyUsed()
		{
			var injectedMethodDef = GetMethodDef(ModModule, "Hooks", "OnMenuKeyUsed");
			var injectedMethodRef = GameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(GameModule, "MenuSystem", "OnColorSphereOpen");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnKeyUsed()
		{
			var injectedMethodDef = GetMethodDef(ModModule, "Hooks", "OnKeyUsed");
			var injectedMethodRef = GameModule.Import(injectedMethodDef);

			var targetClass = IsLegacyVersion ? "Key" : "GateKey";
			var targetMethod = GetMethodDef(GameModule, targetClass, "ColorSphereOpened");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnLevel3BossEnd()
		{
			var injectedMethodDef = GetMethodDef(ModModule, "Hooks", "OnLevel3BossEnd");
			var injectedMethodRef = GameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(GameModule, "BossSphereArena", "OnPart3Done");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnLevel4BossEnd()
		{
			var injectedMethodDef = GetMethodDef(ModModule, "Hooks", "OnLevel4BossEnd");
			var injectedMethodRef = GameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(GameModule, "GravityBossEnding", "StartEnding");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Patch_NoCheatAchievements()
		{
			var injectedMethodDef = GetMethodDef(ModModule, "Cheats", "get_Enabled");
			var injectedMethodRef = GameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(GameModule, "ProgressAndAchivements", "GrantAchievement");
			var ilProc = targetMethod.Body.GetILProcessor();

			ilProc.InsertBefore(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Call, injectedMethodRef));
			ilProc.InsertAfter(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Brtrue, ilProc.Body.Instructions.Last()));
		}

		void Patch_InvincibilityCheat()
		{
			var injectedMethodDef = GetMethodDef(ModModule, "Cheats", "get_InvincibilityEnabled");
			var injectedMethodRef = GameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(GameModule, "MyCharacterController", "Kill");
			var ilProc = targetMethod.Body.GetILProcessor();

			ilProc.InsertBefore(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Call, injectedMethodRef));
			ilProc.InsertAfter(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Brtrue, ilProc.Body.Instructions.Last()));
		}

		static MethodDefinition GetMethodDef(ModuleDefinition module, string typeName, string methodName)
		{
			var type = module.GetTypes().Single(n => n.Name == typeName);
			return type.Methods.Single(m => m.Name == methodName);
		}

		static FieldDefinition GetFieldDef(ModuleDefinition module, string typeName, string fieldName)
		{
			var type = module.GetTypes().Single(n => n.Name == typeName);
			return type.Fields.Single(m => m.Name == fieldName);
		}
	}
}
