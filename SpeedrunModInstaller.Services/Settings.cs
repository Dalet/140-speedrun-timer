namespace SpeedrunModInstaller.Services
{
	public class Settings
	{
		public string Path { get; set; }

		public override string ToString()
		{
			return $"{nameof(Path)}: {Path}";
		}
	}
}
