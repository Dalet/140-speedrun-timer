using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
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
			Insert_PlayerResumeControl();
			Patch_NoCheatAchievements();
			Insert_OnResumeAfterDeath();
			Patch_InvincibilityCheat();

			if (!IsLegacyVersion)
			{
				Patch_TrailFix();
			}

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
			var injectedMethod = GetMethodDef(ModModule, "ModLoader", "Inject");
			var injectedMethodRef = GameModule.Import(injectedMethod);

			var targetMethod = GetMethodDef(GameModule, "Globals", "Awake");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.Last();
			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_PlayerResumeControl()
		{
			var injectedMethodRef = GetMethodDef(ModModule, "Hooks", "PlayerResumeControl");
			var targetMethod = GetMethodDef(GameModule, "MyCharacterController", "ResumeControl");
			var ilProc = targetMethod.Body.GetILProcessor();

			var instruction = ilProc.Create(OpCodes.Call, GameModule.Import(injectedMethodRef));
			ilProc.InsertBefore(ilProc.Body.Instructions.Last(), instruction);
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

		void Patch_TrailFix()
		{
			var methods = new List<string>
			{
				"AddToPos",
				"GetPos",
				"GetRot",
				"GetShape",
				"GetSpeed",
				"GetDeltaTime"
			};

			foreach (var method in methods)
				PatchTrailInstruction(method);
		}

		void PatchTrailInstruction(string methodName)
		{
			var historySizeDef = GetFieldDef(GameModule, "PlayerHistory", "historySize");
			var targetMethod = GetMethodDef(GameModule, "PlayerHistory", methodName);
			var ilProc = targetMethod.Body.GetILProcessor();
			var instructions = ilProc.Body.Instructions;

			for (var i = 0; i < instructions.Count - 1; i++)
			{
				var instruction = instructions[i];
				var nextInstruction = instructions[i + 1];

				if (instruction.OpCode == OpCodes.Ldsfld && instruction.Operand == historySizeDef
					&& nextInstruction.OpCode == OpCodes.Sub)
				{
					nextInstruction.OpCode = OpCodes.Rem;
					break;
				}
			}
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
