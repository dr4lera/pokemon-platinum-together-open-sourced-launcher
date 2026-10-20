using System.Diagnostics;
namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    void HelpPage(FlowLayoutPanel page)
    {
        Add(page,"Read setup documentation",()=>{var path=Path.Combine(root,"docs","README.md");if(!File.Exists(path))throw new IOException("Documentation is missing from this package.");Process.Start(new ProcessStartInfo(path){UseShellExecute=true});return Task.CompletedTask;});
        foreach(var link in new[]{("Launcher, mail & documentation","https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher"),("Server & relay source","https://github.com/dr4lera/pokemon-platinum-together-open-sourced-server"),("Map Studio & trainer sprites","https://github.com/dr4lera/pokemon-platinum-together-open-sourced-editors"),("Official Sprite Creator","https://platinumtogether.online/creators/")}){
            var target=link.Item2;Add(page,link.Item1,()=>{Process.Start(new ProcessStartInfo(target){UseShellExecute=true});return Task.CompletedTask;});
        }
    }
}
