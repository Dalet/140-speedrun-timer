using System.IO;
using System.Reflection;
using UnityEngine;

namespace SpeedrunTimerMod
{
	class Resources
	{
		public static Resources Instance => _instance;
		static readonly Resources _instance = new Resources();

		public AudioClip CheatBeep { get; private set; }

		private Resources() { }

		public void LoadAllResources()
		{
			Debug.Log("persisten path:" +Application.persistentDataPath);
			using (var stream = GetResourceStream("beep.wav"))
			{
				var bytes = ReadAllBytes(stream);
				CheatBeep = WavUtility.ToAudioClip(bytes);
			}
		}

		static Stream GetResourceStream(string resourceName, string resFolder = "Resources")
		{
			var nameSpace = typeof(Resources).Namespace;
			var manifestResName = $"{nameSpace}.{resFolder}.{resourceName}";
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResName);
		}

		static byte[] ReadAllBytes(Stream stream)
		{
			var reader = new BinaryReader(stream);
			using (var memoryStream = new MemoryStream())
			{
				var buffer = new byte[4096];
				int bytesRead;
				while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) != 0)
					memoryStream.Write(buffer, 0, bytesRead);
				return memoryStream.ToArray();
			}
		}
	}
}
