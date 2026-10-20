using System.Text.Json;
using System.Text.RegularExpressions;

namespace PlatinumPrivateLauncher;

static class FriendCodeAddress
{
    public static string Code(string value)
    {
        var code=value.Trim().ToUpperInvariant();
        if(!Regex.IsMatch(code,"^[A-HJKMNP-Z2-9]{6}$"))throw new IOException("Enter the six-character friend code shown by your friend.");
        return code;
    }
    public static string Create(string server,string pin,string code)=>"pt-private://join/?server="+Uri.EscapeDataString(server)+"&key="+Uri.EscapeDataString(pin)+"&code="+Code(code);
    public static string Read(string input,string server,string pin)
    {
        if(!input.Trim().StartsWith("pt-private:",StringComparison.OrdinalIgnoreCase))return Code(input);
        if(input.Length>2048||!Uri.TryCreate(input.Trim(),UriKind.Absolute,out var uri)||uri.Scheme!="pt-private"||uri.Host!="join"||uri.AbsolutePath!="/"||uri.UserInfo!=""||!uri.IsDefaultPort||uri.Fragment!="")throw new IOException("Invalid private friend invite.");
        var fields=new Dictionary<string,string>();
        foreach(var pair in uri.Query.TrimStart('?').Split('&')){var parts=pair.Split('=',2);if(parts.Length!=2||!fields.TryAdd(Uri.UnescapeDataString(parts[0]),Uri.UnescapeDataString(parts[1])))throw new IOException("Invalid private friend invite.");}
        if(fields.Count!=3||!fields.TryGetValue("server",out var origin)||!fields.TryGetValue("key",out var key)||!fields.TryGetValue("code",out var code))throw new IOException("Invalid private friend invite.");
        if(origin!=server)throw new IOException("This invite uses another server. Select its address on the Name page, then join again.");
        if(key!=pin)throw new IOException("This invite has a different server identity. Ask the server owner to check it.");
        return Code(code);
    }
    internal static void SelfTest()
    {
        var link=Create("https://world.example","public+key=","ABCDEF");
        if(Read(link,"https://world.example","public+key=")!="ABCDEF"||Code(" abcdef ")!="ABCDEF")throw new Exception("Friend code round trip failed.");
        foreach(var input in new[]{"AAAAA0",link+"&code=ABCDEF",link.Replace("world.example","other.example"),link.Replace("public%2Bkey%3D","wrong")}){
            try{Read(input,"https://world.example","public+key=");}catch(IOException){continue;}throw new Exception("Unsafe friend invite accepted.");
        }
    }
}

sealed partial class Launcher
{
    readonly TextBox friendCodeInput=new(){Width=790,PlaceholderText="Friend code or pt-private invite"};
    readonly TextBox friendCodeOutput=new(){Width=790,ReadOnly=true};
    readonly TextBox friendInviteOutput=new(){Width=790,ReadOnly=true};
    void FriendCodesPage(FlowLayoutPanel card)
    {
        void ClearInvite(object? sender,EventArgs e){friendCodeOutput.Clear();friendInviteOutput.Clear();}
        remote.CheckedChanged+=ClearInvite;remoteUrl.TextChanged+=ClearInvite;port.ValueChanged+=ClearInvite;
        card.Controls.Add(new Label{Text="Use the same community server as your friend. The server owner must enable invitations.friendCodes in Server/server.json.",Width=790,Height=65,ForeColor=muted});
        Add(card,"Get my friend code",()=>FriendCodeRequest(false));
        card.Controls.Add(friendCodeOutput);card.Controls.Add(friendInviteOutput);
        Add(card,"Copy friend invite",()=>{if(string.IsNullOrWhiteSpace(friendInviteOutput.Text))throw new IOException("Get your friend code first.");Clipboard.SetText(friendInviteOutput.Text);Log("Friend invite copied. Share it with your friend.");return Task.CompletedTask;});
        card.Controls.Add(friendCodeInput);Add(card,"Join friend code",()=>FriendCodeRequest(true));
        card.Controls.Add(new Label{Text="After joining, use the game's party controls to enter the host's world. Codes identify a party on this server; native direct connections remain available. Invites can be pasted here without installing a URL handler.",Width=790,Height=95,ForeColor=muted});
    }
    async Task FriendCodeRequest(bool join)
    {
        await StartServer();var profile=Profile;var origin=Url;
        using var account=JsonDocument.Parse(await File.ReadAllTextAsync(profile));var pin=account.RootElement.GetProperty("hub_key").GetString()!;
        var code=join?FriendCodeAddress.Read(friendCodeInput.Text,origin,pin):null;
        var output=Path.Combine(root,"friend-code-"+Guid.NewGuid().ToString("N")+".json");
        try{
            if(join)await Command("friend-code-join","--profile",profile,"--code",code!,"--out",output);
            else await Command("friend-code-create","--profile",profile,"--out",output);
            using var result=JsonDocument.Parse(await File.ReadAllTextAsync(output));var v=result.RootElement.GetProperty("invite");
            if(profile!=Profile||origin!=Url||v.GetProperty("server").GetString()!=origin||v.GetProperty("hub_key").GetString()!=pin)throw new IOException("The selected server changed. Get the code again.");
            friendCodeOutput.Text=FriendCodeAddress.Code(v.GetProperty("code").GetString()!);
            friendInviteOutput.Text=FriendCodeAddress.Create(origin,pin,friendCodeOutput.Text);
            Log(join?"Friend party joined. Use the game's party controls to enter the host's world.":"Friend code ready for the selected server.");
        }finally{if(File.Exists(output))File.Delete(output);}
    }
}
