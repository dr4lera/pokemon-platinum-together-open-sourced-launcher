using System.Text.Json;
namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    async Task ExportNameBackup()
    {
        // A typed name is not a recoverable identity until its key is registered.
        // Reuse the same name flow as Play; never generate a second backup-only key.
        if(!File.Exists(Profile))await UseChosenName();
        var bytes=await File.ReadAllBytesAsync(Profile);
        using var profile=JsonDocument.Parse(bytes);
        var handle=profile.RootElement.GetProperty("display_name").GetString()??"my-name";
        foreach(var field in new[]{"seed","key","hub_key"})
            if(Convert.FromBase64String(profile.RootElement.GetProperty(field).GetString()!).Length!=32)
                throw new IOException("Your saved name needs recovery before it can be backed up.");
        var safeName=string.Concat(handle.Select(c=>char.IsAsciiLetterOrDigit(c)||c=='_'?c:'_'));
        using var dialog=new SaveFileDialog{Filter="Name backup|*.ptaccount.json",FileName=safeName+".ptaccount.json",DefaultExt="ptaccount.json",AddExtension=true,OverwritePrompt=true};
        if(dialog.ShowDialog(this)!=DialogResult.OK)return;
        if(string.Equals(Path.GetFullPath(dialog.FileName),Path.GetFullPath(Profile),StringComparison.OrdinalIgnoreCase))
            throw new IOException("Choose a separate backup file; your active name file must stay in place.");
        var temporary=dialog.FileName+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{
            await File.WriteAllBytesAsync(temporary,bytes);
            if(!(await File.ReadAllBytesAsync(temporary)).SequenceEqual(bytes))throw new IOException("Backup verification failed.");
            File.Move(temporary,dialog.FileName,true);
        }finally{if(File.Exists(temporary))File.Delete(temporary);}
        Log("Name backup saved for "+handle+". Keep it private; it contains your sign-in key.");
    }
}
