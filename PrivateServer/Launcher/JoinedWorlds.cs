using System.Text.Json;
namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    readonly ComboBox savedServerList=new(){Width=650,DropDownStyle=ComboBoxStyle.DropDownList};
    string SavedServerFile=>Path.Combine(root,"servers.json");
    List<string> ReadSavedServers()
    {
        try{return (JsonSerializer.Deserialize<List<string>>(File.ReadAllText(SavedServerFile))??[]).Where(ValidServer).Distinct(StringComparer.OrdinalIgnoreCase).Take(100).ToList();}
        catch(IOException){return [];}catch(JsonException){return [];}
    }
    static bool ValidServer(string address)=>Uri.TryCreate(address,UriKind.Absolute,out var uri)&&uri.Scheme is "http" or "https"&&uri.UserInfo==""&&uri.AbsolutePath=="/"&&uri.Query==""&&uri.Fragment=="";
    void ReloadSavedServers(){savedServerList.Items.Clear();foreach(var address in ReadSavedServers())savedServerList.Items.Add(address);}
    void RememberServer(string address)
    {
        if(!ValidServer(address))return;
        var saved=ReadSavedServers();saved.RemoveAll(x=>string.Equals(x,address,StringComparison.OrdinalIgnoreCase));saved.Insert(0,address);
        try {var temporary=SavedServerFile+".tmp";File.WriteAllText(temporary,JsonSerializer.Serialize(saved.Take(100)));File.Move(temporary,SavedServerFile,true);ReloadSavedServers();}
        catch(IOException){Log("Connected, but the saved server list could not be updated.");}
        catch(UnauthorizedAccessException){Log("Connected, but the saved server list is read-only.");}
    }
    void SavedServers(FlowLayoutPanel card)
    {
        card.Controls.Add(savedServerList);
        Add(card,"Use selected Inverna server",()=>{
            if(GameRunning())throw new IOException("Close the private game before switching servers.");
            if(savedServerList.SelectedItem is not string selected)throw new IOException("Select a saved server first, or enter a new address above.");
            remoteUrl.Text=selected;remote.Checked=!string.Equals(selected,$"http://127.0.0.1:{port.Value}",StringComparison.OrdinalIgnoreCase);RefreshAccount();Log("Server selected. Use your name to sign in on this server.");return Task.CompletedTask;
        });
        Shown+=(_,_)=>ReloadSavedServers();
    }
    void JoinedWorlds(FlowLayoutPanel card)
    {
        var worlds=new DataGridView{Width=790,Height=300,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=surface,ForeColor=Color.Black};
        worlds.Columns.Add("name","World");worlds.Columns.Add("owner","Owner");worlds.Columns.Add("type","Type");worlds.Columns.Add("visited","Last connected");worlds.Columns.Add("id","World ID");card.Controls.Add(worlds);
        Add(card,"Refresh joined worlds",async()=>{
            await StartServer();var output=Path.Combine(Path.GetDirectoryName(Profile)!,"world-list-"+Guid.NewGuid().ToString("N")+".json");
            try{
                await Command("worlds","--profile",Profile,"--out",output);using var doc=JsonDocument.Parse(await File.ReadAllTextAsync(output));worlds.Rows.Clear();
                foreach(var w in doc.RootElement.GetProperty("worlds").EnumerateArray())worlds.Rows.Add(w.GetProperty("name").GetString()??"",w.GetProperty("owner_name").GetString()??"",w.GetProperty("kind").GetString()??"",DateTimeOffset.FromUnixTimeMilliseconds(w.GetProperty("joined_at_ms").GetInt64()).LocalDateTime.ToString("g"),w.GetProperty("world").GetString()??"");
                Log(worlds.Rows.Count==0?"No recorded world connections on this server yet.":$"{worlds.Rows.Count} world connection(s) on this server.");
            }finally{if(File.Exists(output))File.Delete(output);}
        });
        card.Controls.Add(new Label{Text="Choose another Inverna/community server in Name. This list comes from that server and survives backend restarts. Hosting configuration, bans and plugins are managed in the server files and CLI. Native world entry is still under development.",Width=790,Height=105,ForeColor=muted});
    }
}
