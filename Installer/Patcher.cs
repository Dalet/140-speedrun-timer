using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace SpeedrunTimerModInstaller
{
	class Patcher
	{
		public bool IsLegacyVersion { get; private set; }

		string _gameDllPath;
		string _modDllPath;

		DefaultAssemblyResolver _resolver;
		ReaderParameters _readerParams;

		AssemblyDefinition _gameAsmDef;
		AssemblyDefinition _modAsmDef;
		ModuleDefinition _gameModule;
		ModuleDefinition _modModule;

		public Patcher(string assembliesPath, string gameDllPath, string modDllPath)
		{
			_gameDllPath = gameDllPath;
			_modDllPath = modDllPath;

			_resolver = new DefaultAssemblyResolver();
			_resolver.AddSearchDirectory(assembliesPath);
			_readerParams = new ReaderParameters { AssemblyResolver = _resolver };

			_gameAsmDef = AssemblyDefinition.ReadAssembly(_gameDllPath, _readerParams);
			_gameModule = _gameAsmDef.MainModule;
			_modAsmDef = AssemblyDefinition.ReadAssembly(_modDllPath, _readerParams);
			_modModule = _modAsmDef.MainModule;

			IsLegacyVersion = isLegacyVersion();
		}

		bool isLegacyVersion()
		{
			return !_gameModule.GetTypes().Any(n => n.Name == "GravityBoss");
		}

		public void PatchGameDll(string destination = null)
		{
			if (destination == null)
				destination = _gameDllPath;

			Insert_Inject();
			Insert_OnResumeAfterDeath();
			Insert_OnLevel3BossEnd();
			Insert_OnMenuKeyUsed();
			Insert_OnPlayerFixedUpdate();

			if (!IsLegacyVersion)
				Insert_OnLevel4BossEnd();

			_gameAsmDef.Write(destination);
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
			var modRef = _gameModule.AssemblyReferences.FirstOrDefault(a => a.Name.ToLower().Contains("speedrun"));
			if (modRef == null)
				return null;

			var modDllVer = _modAsmDef.Name.Version;

			if (modRef.Version == modDllVer)
				return modDllVer;
			else
				return null;
		}

		void Insert_Inject()
		{
			// SpeedrunTimerLoader.Inject()
			var injectedMethod = GetMethodDef(_modModule, "SpeedrunTimerLoader", "Inject");
			var injectedMethodRef = _gameModule.Import(injectedMethod);

			var targetMethod = GetMethodDef(_gameModule, "Globals", "Awake");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.Last();
			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnPlayerFixedUpdate()
		{
			// Hooks.OnPlayerFixedUpdate() (calls SpeedrunTimer.EndLoad)
			var injectedMethodRef = GetMethodDef(_modModule, "Hooks", "OnPlayerFixedUpdate");
			var targetMethodName = IsLegacyVersion ? "FixedUpdate" : "upDATE_FixedUpdate";
			var targetMethod = GetMethodDef(_gameModule, "MyCharacterController", targetMethodName);

			var ilProc = targetMethod.Body.GetILProcessor();
			var logicPaused = GetFieldDef(_gameModule, "MyCharacterController", "logicPaused");
			var controlPaused = GetFieldDef(_gameModule, "MyCharacterController", "controlPaused");
			var moveDirection = GetFieldDef(_gameModule, "MyCharacterController", "moveDirection");

			var instructionsToInject = new Instruction[]
			{
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, _gameModule.Import(logicPaused)),
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, _gameModule.Import(controlPaused)),
				ilProc.Create(OpCodes.Ldarg_0),
				ilProc.Create(OpCodes.Ldfld, _gameModule.Import(moveDirection)),
				ilProc.Create(OpCodes.Call, _gameModule.Import(injectedMethodRef))
			};

			foreach (var instruc in instructionsToInject)
				ilProc.InsertBefore(ilProc.Body.Instructions.Last(), instruc);
		}

		void Insert_OnResumeAfterDeath()
		{
			var injectedMethodDef = GetMethodDef(_modModule, "Hooks", "OnResumeAfterDeath");
			var injectedMethodRef = _gameModule.Import(injectedMethodDef);
			var targetMethod = GetMethodDef(_gameModule, "MyCharacterController", "ResumeAfterDeath");

			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnMenuKeyUsed()
		{
			var injectedMethodDef = GetMethodDef(_modModule, "Hooks", "OnMenuKeyUsed");
			var injectedMethodRef = _gameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(_gameModule, "MenuSystem", "OnColorSphereOpen");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnLevel3BossEnd()
		{
			var injectedMethodDef = GetMethodDef(_modModule, "Hooks", "OnLevel3BossEnd");
			var injectedMethodRef = _gameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(_gameModule, "BossSphereArena", "OnPart3Done");
			var ilProc = targetMethod.Body.GetILProcessor();
			var targetInstruction = ilProc.Body.Instructions.First();

			ilProc.InsertBefore(targetInstruction, ilProc.Create(OpCodes.Call, injectedMethodRef));
		}

		void Insert_OnLevel4BossEnd()
		{
			var injectedMethodDef = GetMethodDef(_modModule, "Hooks", "OnLevel4BossEnd");
			var injectedMethodRef = _gameModule.Import(injectedMethodDef);

			var targetMethod = GetMethodDef(_gameModule, "GravityBossEnding", "StartEnding");
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
