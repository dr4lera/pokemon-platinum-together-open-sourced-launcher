using System.Diagnostics;
namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    Task OpenTrainerSprites(){var exe=Path.Combine(root,"MapStudio","InvernaMapStudio.exe");if(!File.Exists(exe))throw new IOException("Map Studio is missing from this package.");var info=new ProcessStartInfo(exe){WorkingDirectory=Path.GetDirectoryName(exe),UseShellExecute=true};info.ArgumentList.Add("--sprites");Process.Start(info);return Task.CompletedTask;}
}
