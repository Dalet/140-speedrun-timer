using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpeedrunTimerModInstaller
{
	class Patcher
	{
		public bool IsLegacyVersion => _isLegacyVersion.Value;
		Lazy<bool> _isLegacyVersion;

		string _gameDllPath;
		string _modDllPath;
		string _unityDllPath;

		DefaultAssemblyResolver _resolver;
		ReaderParameters _readerParams;

		AssemblyDefinition GameAsmDef => _gameAsmDef.Value;
		AssemblyDefinition ModAsmDef => _modAsmDef.Value;
		AssemblyDefinition UnityAsmDef => _unityAsmDef.Value;
		Lazy<AssemblyDefinition> _gameAsmDef;
		Lazy<AssemblyDefinition> _modAsmDef;
		Lazy<AssemblyDefinition> _unityAsmDef;

		ModuleDefinition GameModule => GameAsmDef?.MainModule;
		ModuleDefinition ModModule => ModAsmDef?.MainModule;
		ModuleDefinition UnityModule => UnityAsmDef.MainModule;

		public Patcher(string assembliesPath, string gameDllPath, string modDllPath)
		{
			_gameDllPath = gameDllPath;
			_modDllPath = modDllPath;
			_unityDllPath = Path.Combine(assembliesPath, "UnityEngine.dll");

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
			_unityAsmDef = new Lazy<AssemblyDefinition>(() =>
			{
				try
				{
					return AssemblyDefinition.ReadAssembly(_unityDllPath, _readerParams);
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
			Patch_NoCheatAchievements();
			Patch_InvincibilityCheat();

			if (!IsLegacyVersion)
			{
				Patch_TrailFix();
				Patch_GlobalBeatMaster_Deltatime();
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

		void Patch_GlobalBeatMaster_Deltatime()
		{
			var get_fixedDeltaTimeRef = GameModule.Import(GetMethodDef(UnityModule, "Time", "get_fixedDeltaTime"));
			var get_deltaTimeRef = GameModule.Import(GetMethodDef(UnityModule, "Time", "get_deltaTime"));

			var targetMethod = GetMethodDef(GameModule, "GlobalBeatMaster", "Update");
			var ilProc = targetMethod.Body.GetILProcessor();

			foreach (var inst in targetMethod.Body.Instructions)
			{
				if (inst.OpCode == OpCodes.Call && ((MethodReference)inst.Operand).FullName == get_fixedDeltaTimeRef.FullName)
				{
					inst.Operand = get_deltaTimeRef;
					break;
				}
			}
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
