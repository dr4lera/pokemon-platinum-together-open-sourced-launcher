using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PlatinumGifts;
namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    readonly ListBox giftInbox=new(){Width=790,Height=240,BackColor=Color.FromArgb(12,20,32),ForeColor=Color.White,IntegralHeight=false};
    void GiftPage(FlowLayoutPanel card)
    {
        card.Controls.Add(giftInbox);Add(card,"Refresh my gifts",RefreshGifts);Add(card,"Load selected gift for the Poké Mart deliveryman",ClaimGift);
        card.Controls.Add(new Label{Text="On verified 0.1.5, custom Pokémon, eggs and items can be loaded into the gift queue while your private game is closed, then collected from a Poké Mart deliveryman (up to 8 waiting). This save adapter is not yet verified for 0.1.7. Native MAIL/SUPPLIES gifts use the game's MAIL screen.",Width=790,Height=115,ForeColor=muted});
    }
    async Task RefreshGifts(){await StartServer();var path=Path.Combine(Data,"gift-inbox-cache.json");await Command("inbox","--profile",Profile,"--action","list","--out",path);using var data=JsonDocument.Parse(await File.ReadAllTextAsync(path));giftInbox.Items.Clear();foreach(var g in data.RootElement.GetProperty("gifts").EnumerateArray())giftInbox.Items.Add(new InboxGift(g.GetProperty("id").GetString()!,g.GetProperty("title").GetString()!,g.GetProperty("data").GetString()!));Log($"{giftInbox.Items.Count} custom gift(s) waiting");}
    async Task ClaimGift()
    {
        if(GameRunning())throw new Exception("Save and close the private game before loading a gift.");if(giftInbox.SelectedItem is not InboxGift gift)throw new Exception("Refresh and select a gift first.");
        if(!Guid.TryParseExact(gift.Id,"D",out _))throw new Exception("Invalid gift identifier.");
        var exe=Path.Combine(GameDir,"Platinum.exe");if(!File.Exists(exe)||!LauncherChecks.SaveAdapterSupported(Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(exe)))))throw new Exception("Custom Poké Mart gift delivery has not been verified for this game build. You can still play; no gift or save was changed.");
        using var choose=new OpenFileDialog{InitialDirectory=GameDir,Filter="Private world save|save.bin",Title="Choose the private world that should receive this gift",FileName=Path.Combine(GameDir,"save.bin")};if(choose.ShowDialog(this)!=DialogResult.OK)return;
        var save=Path.GetFullPath(choose.FileName);var prefix=Path.GetFullPath(GameDir)+Path.DirectorySeparatorChar;if(!save.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)||Path.GetFileName(save)!="save.bin")throw new Exception("Choose a save.bin inside this private game's folder.");
        for(var directory=new DirectoryInfo(Path.GetDirectoryName(save)!);directory!=null;directory=directory.Parent){if((directory.Attributes&FileAttributes.ReparsePoint)!=0)throw new Exception("Choose a save in a real folder, outside directory links.");if(directory.FullName.Equals(Path.GetFullPath(GameDir),StringComparison.OrdinalIgnoreCase))break;}
        if((File.GetAttributes(save)&FileAttributes.ReparsePoint)!=0)throw new Exception("Choose the real save file.");
        if(MessageBox.Show(this,$"Load “{gift.Title}” into this world's Poké Mart gift queue?\n{save}\n\nThe original save will be backed up.","Load custom gift",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
        await StartServer();var receipts=Path.Combine(Client,"GiftReceipts");Directory.CreateDirectory(receipts);using var profile=JsonDocument.Parse(await File.ReadAllTextAsync(Profile));var p=profile.RootElement;string binding=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(p.GetProperty("hub_key").GetString()+"\0"+p.GetProperty("player_id").GetString()+"\0"+save.ToUpperInvariant()))).ToLowerInvariant();
        string receipt=Path.Combine(receipts,gift.Id+".json"),response=Path.Combine(receipts,gift.Id+"-response.json");
        await Command("inbox","--profile",Profile,"--action","prepare","--id",gift.Id,"--client",binding,"--out",response);using var prepared=JsonDocument.Parse(await File.ReadAllTextAsync(response));if(prepared.RootElement.TryGetProperty("acknowledged",out var done)&&done.GetBoolean()){Log("This gift was already delivered.");await RefreshGifts();return;}
        var bytes=Convert.FromBase64String(prepared.RootElement.GetProperty("data").GetString()!);MartGift.Validate(bytes);string installedHash;
        if(File.Exists(receipt)){using var journal=JsonDocument.Parse(await File.ReadAllTextAsync(receipt));var j=journal.RootElement;if(j.GetProperty("source").GetString()!=save||j.GetProperty("binding").GetString()!=binding)throw new Exception("This pending gift belongs to another save/account.");var current=Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(save)));installedHash=j.GetProperty("after").GetString()!;if(current!=installedHash){
                if(current!=j.GetProperty("before").GetString())throw new Exception("A pending gift's save changed before acknowledgement. Its backup and journal were kept; do not grant it again.");
                var resumed=MartGift.Build(await File.ReadAllBytesAsync(save),bytes);if(Convert.ToHexString(SHA256.HashData(resumed))!=installedHash)throw new Exception("The pending gift no longer matches its journal.");
                var temporary=save+".gift-"+gift.Id+".tmp";await File.WriteAllBytesAsync(temporary,resumed);if(GameRunning()||Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(save)))!=current)throw new Exception("Game/save changed; pending gift kept.");File.Move(temporary,save,true);
            }}
        else
        {
            var original=await File.ReadAllBytesAsync(save);var edited=MartGift.Build(original,bytes);var before=Convert.ToHexString(SHA256.HashData(original));installedHash=Convert.ToHexString(SHA256.HashData(edited));var backup=Path.Combine(receipts,"Backups",gift.Id);Directory.CreateDirectory(backup);await File.WriteAllBytesAsync(Path.Combine(backup,"save.bin"),original);
            if(GameRunning()||Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(save)))!=before)throw new Exception("The game started or the save changed; delivery stopped.");
            await File.WriteAllTextAsync(receipt,JsonSerializer.Serialize(new{source=save,binding,before,after=installedHash,backup}));var temporary=save+".gift-"+gift.Id+".tmp";await File.WriteAllBytesAsync(temporary,edited);_=MartGift.Layout(await File.ReadAllBytesAsync(temporary));if(GameRunning())throw new Exception("Private game started; pending gift and backup were kept.");File.Move(temporary,save,true);
        }
        await Command("inbox","--profile",Profile,"--action","ack","--id",gift.Id,"--client",binding,"--out",response);File.Move(receipt,receipt+".delivered",true);Log("Gift loaded. Open this world and collect it from the Poké Mart deliveryman.");await RefreshGifts();
    }
    async Task FinishPendingGifts()
    {
        var folder=Path.Combine(Client,"GiftReceipts");if(!Directory.Exists(folder))return;
        foreach(var file in Directory.EnumerateFiles(folder,"*.json")){
            var id=Path.GetFileNameWithoutExtension(file);if(!Guid.TryParseExact(id,"D",out _))continue;
            using var journal=JsonDocument.Parse(await File.ReadAllTextAsync(file));var j=journal.RootElement;var save=j.GetProperty("source").GetString()!;
            var prefix=Path.GetFullPath(GameDir)+Path.DirectorySeparatorChar;if(!Path.GetFullPath(save).StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new Exception("Pending gift points outside this private game; check its delivery journal.");
            var current=Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(save)));
            if(current!=j.GetProperty("after").GetString())throw new Exception("Finish the pending gift in Gift inbox before playing. Its backup and journal are preserved.");
            await StartServer();using var profile=JsonDocument.Parse(await File.ReadAllTextAsync(Profile));var p=profile.RootElement;
            var binding=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(p.GetProperty("hub_key").GetString()+"\0"+p.GetProperty("player_id").GetString()+"\0"+save.ToUpperInvariant()))).ToLowerInvariant();
            if(binding!=j.GetProperty("binding").GetString())throw new Exception("Finish the pending gift using its original account before switching accounts.");
            await Command("inbox","--profile",Profile,"--action","ack","--id",id,"--client",binding,"--out",file+"-ack.json");File.Move(file,file+".delivered",true);Log("Pending gift delivery acknowledged.");
        }
    }
    sealed record InboxGift(string Id,string Title,string Data){public override string ToString()=>Title+" • "+(Convert.FromBase64String(Data)[0] switch{1=>"Pokémon",2=>"Egg",3=>"Item",_=>"Gift"});}
}
