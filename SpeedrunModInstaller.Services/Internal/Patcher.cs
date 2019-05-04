using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SpeedrunModInstaller.Services.Internal
{
	internal class Patcher
	{
		private readonly string _gameDllPath;
		private readonly string _modDllPath;
		private readonly ReaderParameters _readerParams;

		private readonly DefaultAssemblyResolver _resolver;
		private readonly string _unityDllPath;

		public Patcher(string assembliesPath, string gameDllPath, string modDllPath)
		{
			_gameDllPath = gameDllPath;
			_modDllPath = modDllPath;
			_unityDllPath = Path.Combine(assembliesPath, "UnityEngine.dll");

			_resolver = new DefaultAssemblyResolver();
			_resolver.AddSearchDirectory(assembliesPath);
			_readerParams = new ReaderParameters { AssemblyResolver = _resolver };
		}

		public void PatchGameDll(string destination = null)
		{
			if (destination == null)
				destination = _gameDllPath;

			using (var gameDef = GameAsmDef())
			using (var gameModule = gameDef.MainModule)
			using (var modDef = ModAsmDef())
			using (var unityDef = UnityAsmDef())
			using (var unityModule = unityDef.MainModule)
			{
				Insert_Inject(gameDef, modDef);
				Patch_NoCheatAchievements(gameDef, modDef);
				Patch_InvincibilityCheat(gameDef, modDef);

				if (!IsLegacyVersion(gameDef))
				{
					Patch_TrailFix(gameDef);
					Patch_GlobalBeatMaster_Deltatime(gameModule, unityModule);
				}

				gameDef.Write(destination);
			}
		}

		public bool IsGameDllPatched(string path = null)
		{
			if (path == null)
				path = _gameDllPath;

			using (var gameAsmDef = AssemblyDefinition.ReadAssembly(path, _readerParams))
			{
				return gameAsmDef.MainModule.AssemblyReferences.Any(a => a.Name.ToLower().Contains("speedrun"));
			}
		}

		public Version GetModDllVersion()
		{
			using (var gameDef = GameAsmDef())
			using (var modDef = ModAsmDef())
			{
				var modRef = gameDef.MainModule?.AssemblyReferences.FirstOrDefault(a => a.Name.ToLower().Contains("speedrun"));
				if (modRef == null)
					return null;

				var modDllVer = modDef?.Name.Version;

				if (modRef.Version == modDllVer)
					return modDllVer;
				return null;
			}
		}

		private void Insert_Inject(AssemblyDefinition gameDef, AssemblyDefinition modDef)
		{
			// SpeedrunTimerLoader.Inject()
			var injectedMethod = GetMethodDef(modDef.MainModule, "ModLoader", "Inject");
			var injectedMethodRef = gameDef.MainModule.Import(injectedMethod);

			var targetMethod = GetMethodDef(gameDef.MainModule, "Globals", "Awake");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.Last();
			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		private void Patch_NoCheatAchievements(AssemblyDefinition gameDef, AssemblyDefinition modDef)
		{
			var injectedMethodDef = GetMethodDef(modDef.MainModule, "Cheats", "get_Enabled");
			var injectedMethodRef = gameDef.MainModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(gameDef.MainModule, "ProgressAndAchivements", "GrantAchievement");
			var ilProc = targetMethod.Body.GetILProcessor();

			ilProc.InsertBefore(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Call, injectedMethodRef));
			ilProc.InsertAfter(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Brtrue, ilProc.Body.Instructions.Last()));
		}

		private void Patch_InvincibilityCheat(AssemblyDefinition gameDef, AssemblyDefinition modDef)
		{
			var injectedMethodDef = GetMethodDef(modDef.MainModule, "Cheats", "get_InvincibilityEnabled");
			var injectedMethodRef = gameDef.MainModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(gameDef.MainModule, "MyCharacterController", "Kill");
			var ilProc = targetMethod.Body.GetILProcessor();

			ilProc.InsertBefore(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Call, injectedMethodRef));
			ilProc.InsertAfter(ilProc.Body.Instructions.First(), ilProc.Create(OpCodes.Brtrue, ilProc.Body.Instructions.Last()));
		}

		private void Patch_TrailFix(AssemblyDefinition gameDef)
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
				PatchTrailInstruction(gameDef, method);
		}

		private void Patch_GlobalBeatMaster_Deltatime(ModuleDefinition gameModule, ModuleDefinition unityModule)
		{
			var get_fixedDeltaTimeRef = gameModule.Import(GetMethodDef(unityModule, "Time", "get_fixedDeltaTime"));
			var get_deltaTimeRef = gameModule.Import(GetMethodDef(unityModule, "Time", "get_deltaTime"));

			var targetMethod = GetMethodDef(gameModule, "GlobalBeatMaster", "Update");
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

		private void PatchTrailInstruction(AssemblyDefinition gameDef, string methodName)
		{
			var historySizeDef = GetFieldDef(gameDef.MainModule, "PlayerHistory", "historySize");
			var targetMethod = GetMethodDef(gameDef.MainModule, "PlayerHistory", methodName);
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

		private static MethodDefinition GetMethodDef(ModuleDefinition module, string typeName, string methodName)
		{
			var type = module.GetTypes().Single(n => n.Name == typeName);
			return type.Methods.Single(m => m.Name == methodName);
		}

		private static FieldDefinition GetFieldDef(ModuleDefinition module, string typeName, string fieldName)
		{
			var type = module.GetTypes().Single(n => n.Name == typeName);
			return type.Fields.Single(m => m.Name == fieldName);
		}

		private bool IsLegacyVersion(AssemblyDefinition gameDef)
		{
			return !gameDef.MainModule.GetTypes().Any(n => n.Name == "GravityBoss");
		}

		private AssemblyDefinition GameAsmDef()
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
		}

		private AssemblyDefinition ModAsmDef()
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
		}

		private AssemblyDefinition UnityAsmDef()
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
		}
	}
}
