using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace SpeedrunTimerModInstaller
{
	class Patcher
	{
		string _gameDllPath;
		string _modDllPath;

		DefaultAssemblyResolver _resolver;
		ReaderParameters _readerParams;

		public Patcher(string assembliesPath, string gameDllPath, string modDllPath)
		{
			_gameDllPath = gameDllPath;
			_modDllPath = modDllPath;

			_resolver = new DefaultAssemblyResolver();
			_resolver.AddSearchDirectory(assembliesPath);
			_readerParams = new ReaderParameters { AssemblyResolver = _resolver };
		}

		public void PatchGameDll(string destination = null)
		{
			if (destination == null)
				destination = _gameDllPath;

			var gameAsmDef = AssemblyDefinition.ReadAssembly(_gameDllPath, _readerParams);
			var gameModule = gameAsmDef.MainModule;
			var modAsmDef = AssemblyDefinition.ReadAssembly(_modDllPath, _readerParams);
			var modModule = modAsmDef.MainModule;

			Insert_Inject(gameModule, modModule);
			Insert_OnResumeAfterDeath(gameModule, modModule);
			Insert_OnLevel3BossEnd(gameModule, modModule);
			Insert_OnMenuKeyUsed(gameModule, modModule);
			Insert_OnPlayerFixedUpdate(gameModule, modModule);

			gameAsmDef.Write(destination);
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
			var gameAsmDef = AssemblyDefinition.ReadAssembly(_gameDllPath, _readerParams);
			var modRef = gameAsmDef.MainModule.AssemblyReferences.FirstOrDefault(a => a.Name.ToLower().Contains("speedrun"));
			if (modRef == null)
				return null;

			var modDllDef = AssemblyDefinition.ReadAssembly(_modDllPath, _readerParams);
			var modDllVer = modDllDef.Name.Version;

			if (modRef.Version == modDllVer)
				return modDllVer;
			else
				return null;
		}

		static void Insert_Inject(ModuleDefinition gameModule, ModuleDefinition modModule)
		{
			// SpeedrunTimerLoader.Inject()
			var injectedMethod = GetMethodDef(modModule, "SpeedrunTimerLoader", "Inject");
			var injectedMethodRef = gameModule.Import(injectedMethod);

			var targetMethod = GetMethodDef(gameModule, "Globals", "Awake");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.Last();
			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		static void Insert_OnPlayerFixedUpdate(ModuleDefinition gameModule, ModuleDefinition modModule)
		{
			// Hooks.OnPlayerFixedUpdate() (calls SpeedrunTimer.EndLoad)
			var injectedMethodRef = GetMethodDef(modModule, "Hooks", "OnPlayerFixedUpdate");
			var targetMethod = GetMethodDef(gameModule, "MyCharacterController", "upDATE_FixedUpdate");

			var ilProc = targetMethod.Body.GetILProcessor();
			var logicPaused = GetFieldDef(gameModule, "MyCharacterController", "logicPaused");
			var controlPaused = GetFieldDef(gameModule, "MyCharacterController", "controlPaused");
			var moveDirection = GetFieldDef(gameModule, "MyCharacterController", "moveDirection");

			var instructionsToInject = new Instruction[]
			{
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, gameModule.Import(logicPaused)),
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, gameModule.Import(controlPaused)),
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, gameModule.Import(moveDirection)),
				ilProc.Create(OpCodes.Call, gameModule.Import(injectedMethodRef))
			};

			foreach (var instruc in instructionsToInject)
				ilProc.InsertBefore(ilProc.Body.Instructions.Last(), instruc);
		}

		static void Insert_OnResumeAfterDeath(ModuleDefinition gameModule, ModuleDefinition modModule)
		{
			var injectedMethodDef = GetMethodDef(modModule, "Hooks", "OnResumeAfterDeath");
			var injectedMethodRef = gameModule.Import(injectedMethodDef);
			var targetMethod = GetMethodDef(gameModule, "MyCharacterController", "ResumeAfterDeath");

			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		static void Insert_OnMenuKeyUsed(ModuleDefinition gameModule, ModuleDefinition modModule)
		{
			var injectedMethodDef = GetMethodDef(modModule, "Hooks", "OnMenuKeyUsed");
			var injectedMethodRef = gameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(gameModule, "MenuSystem", "OnColorSphereOpen");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		static void Insert_OnLevel3BossEnd(ModuleDefinition gameModule, ModuleDefinition modModule)
		{
			var injectedMethodDef = GetMethodDef(modModule, "Hooks", "OnLevel3BossEnd");
			var injectedMethodRef = gameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(gameModule, "BossSphereArena", "OnPart3Done");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
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
