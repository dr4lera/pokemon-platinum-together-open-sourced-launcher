using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PlatinumPrivateLauncher;
static class Program
{
    [STAThread] static void Main(string[] args){ApplicationConfiguration.Initialize();if(args.Contains("--selftest")){LauncherChecks.Run();RomChecks.Run();ControlBindings.SelfTest();NativeGameReset.SelfTest();NativeMultiplayerConfig.SelfTest();NativeSignIn.SelfTest();File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"rom-selftest.json"),"{\"passed\":true,\"checks\":[\"FNT/FAT extraction\",\"out of bounds rejection\",\"unsafe path rejection\"]}");return;}using var mutex=new Mutex(true,"Local\\PlatinumPrivateLauncher-"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(AppContext.BaseDirectory)))[..20],out var first);var invite=DiscordLaunch.Secret(args);if(!first){try{DiscordLaunch.Forward(invite).GetAwaiter().GetResult();}catch{MessageBox.Show("The existing launcher did not respond. Bring it to the front and try the invite again.");}return;}using var window=new Launcher();using var stop=new CancellationTokenSource();Task? listener=null;window.Shown+=(_,_)=>{listener=DiscordLaunch.Listen(window,stop.Token);if(invite is not null)window.ReceiveDiscordInvite(invite);};Application.Run(window);stop.Cancel();try{listener?.GetAwaiter().GetResult();}catch(OperationCanceledException){}}
}
sealed partial class Launcher:Form
{
    readonly string root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,".."));
    readonly RichTextBox logs=new(){Dock=DockStyle.Fill,ReadOnly=true,BackColor=Color.FromArgb(14,22,33),ForeColor=Color.Gainsboro,Font=new Font("Consolas",10)};
    readonly Label status=new(){AutoSize=true,Text="Ready • Local private server",ForeColor=Color.LightSteelBlue};
    readonly TextBox source=new(){Width=650,Text=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PlatinumTogether","Game")};
    readonly TextBox name=new(){Width=190,PlaceholderText="Choose a unique username"};
    readonly CheckBox remote=new(){Text="Connect to another private server",AutoSize=true,ForeColor=Color.White};
    readonly TextBox remoteUrl=new(){Width=650,PlaceholderText="Private server URL, such as http://host:8788"};
    readonly Label accountStatus=new(){AutoSize=true,MaximumSize=new(790,0),ForeColor=Color.LightSteelBlue};
    readonly CheckBox lan=new(){Text="Allow connections from my LAN",AutoSize=true,ForeColor=Color.White};
    readonly TextBox advertised=new(){Width=650,PlaceholderText="Address friends use, e.g. http://192.168.1.20:8788"};
    readonly TextBox rom=new(){Width=650,PlaceholderText="Drop your Pokémon Platinum .nds here, or choose a file"};
    readonly NumericUpDown port=new(){Minimum=1,Maximum=65535,Value=8788,Width=100};
    readonly List<Button> actions=[];Process? server,game;bool busy;
    string Data=>configuredData??Path.Combine(root,"Data");string Client=>Path.Combine(root,"Client");string GameDir=>Path.Combine(Client,"Game");
    string Profile=>remote.Checked?Path.Combine(root,"Profiles",Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(remoteUrl.Text.Trim().TrimEnd('/')))).ToLowerInvariant()[..24]+".json"):Path.Combine(Data,"launcher-player.json");string Node=>Path.Combine(root,"Runtime","node.exe");string ServerMain=>Path.Combine(root,"Server","main.mjs");
    public Launcher(){BuildModernView();}
    void Add(Control p,string title,Func<Task> action){var b=new Button{Text=title,AutoSize=true,MinimumSize=new(280,43),BackColor=Color.FromArgb(37,53,73),ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Margin=new Padding(0,6,0,8),Cursor=Cursors.Hand,Padding=new Padding(14,4,14,4)};b.FlatAppearance.BorderSize=0;actions.Add(b);p.Controls.Add(b);b.Click+=async(_,_)=>{if(busy)return;busy=true;Control[] fields=[source,rom,name,port,remote,remoteUrl,lan,advertised];foreach(var x in actions)x.Enabled=false;foreach(var field in fields)field.Enabled=false;try{await action();}catch(Exception e){Log(e.Message);MessageBox.Show(this,e.Message,"Private Worlds",MessageBoxButtons.OK,MessageBoxIcon.Warning);}finally{busy=false;foreach(var x in actions)x.Enabled=true;foreach(var field in fields)field.Enabled=true;remoteUrl.Enabled=remote.Checked;advertised.Enabled=lan.Checked;}};}
    void Log(string s){if(IsDisposed)return;if(InvokeRequired){BeginInvoke(()=>Log(s));return;}logs.AppendText($"[{DateTime.Now:T}] {s}\n");status.Text=s;}
    static void Open(string p)=>Process.Start(new ProcessStartInfo("explorer.exe"){ArgumentList={p},UseShellExecute=true});
    ProcessStartInfo NodeStart(params string[] args){if(!File.Exists(Node)||!File.Exists(ServerMain))throw new Exception("Server/runtime files are missing. Keep the launcher inside its complete folder.");var s=new ProcessStartInfo(Node){WorkingDirectory=root,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};s.ArgumentList.Add(ServerMain);foreach(var a in args)s.ArgumentList.Add(a);return s;}
    async Task Command(params string[] args){using var p=Process.Start(NodeStart(args))!;var stdout=p.StandardOutput.ReadToEndAsync();var stderr=p.StandardError.ReadToEndAsync();await p.WaitForExitAsync();await stdout;var error=await stderr;if(p.ExitCode!=0){var known=new Dictionary<string,string>{{"username_taken","That username is already reserved on this server. Choose another."},{"invalid_username","Use 3–16 letters, numbers or underscores, starting with a letter."},{"server_identity_changed","The server's identity changed. Your saved account was not sent to the replacement."},{"account_not_found","This account is not registered on that server."},{"not_server_owner","Only the local server owner can manage this server."},{"bad_ip","Enter a valid IPv4 or IPv6 address."},{"cannot_ban_server_owner","The server owner account is kept available for server recovery."},{"username_unknown","That username is not registered on this server."},{"user_banned","This user is banned from this server."},{"account_rate_limit","Too many account requests. Wait a minute and try again."}};throw new Exception(known.FirstOrDefault(x=>error.Contains(x.Key)).Value??"The private server request failed. Check the address, account and backend version.");}}
    string Url {
        get {
            if(!remote.Checked)return $"http://127.0.0.1:{port.Value}";
            if(!Uri.TryCreate(remoteUrl.Text.Trim(),UriKind.Absolute,out var uri)||uri.Scheme is not ("http" or "https")||uri.UserInfo!=""||uri.AbsolutePath!="/"||uri.Query!=""||uri.Fragment!="")
                throw new IOException("Enter your friend's server address, or turn off ‘Connect to another private server’ to use this PC.");
            return uri.GetLeftPart(UriPartial.Authority);
        }
    }
    async Task StartServer(){
        if(remote.Checked){using var client=new HttpClient{Timeout=TimeSpan.FromSeconds(8)};using var info=JsonDocument.Parse(await client.GetStringAsync(Url+"/private/server-info"));if(!info.RootElement.TryGetProperty("backend_revision",out var rev)||rev.GetInt32()<5)throw new Exception("That private server needs backend revision 5 for joined worlds and Discord invites. Ask its owner to update.");if(!File.Exists(Profile))await Command("account-register","--url",Url,"--name",name.Text.Trim(),"--out",Profile);RememberServer(Url);RefreshAccount();Log("Private server reachable");return;}
        await EnsureLocalServerConfig();Directory.CreateDirectory(Data);
        using var http=new HttpClient{Timeout=TimeSpan.FromSeconds(2)};
        try{var body=await http.GetStringAsync(Url+"/private/server-info");using var doc=JsonDocument.Parse(body);if(!File.Exists(Path.Combine(Data,"server-key.pem")))throw new Exception("This port belongs to another server. Choose another port.");
            if(File.Exists(Profile)){using var p=JsonDocument.Parse(File.ReadAllText(Profile));if(p.RootElement.GetProperty("hub_key").GetString()!=doc.RootElement.GetProperty("hub_key").GetString())throw new Exception("Another server uses this port. Choose another port.");}
            else throw new Exception("A server is already using this port. Choose another port.");
            AttachOwnedServer();if(!doc.RootElement.TryGetProperty("backend_revision",out var revision)||revision.GetInt32()<5){if(server is null)throw new Exception("An older backend uses this port. Close it before starting the new account server.");await StopServer();await StartServer();return;}if(doc.RootElement.TryGetProperty("bind",out var bind)&&bind.GetString()!=configuredBind)throw new Exception("Restart the server to apply Server/server.json changes.");Log("Private server already running");return;
        }catch(HttpRequestException){}catch(TaskCanceledException){}
        if(!File.Exists(Profile))await Command("enroll","--data",Data,"--name",name.Text.Trim(),"--url",Url,"--out",Profile);
        using(var p=JsonDocument.Parse(File.ReadAllText(Profile)))if(p.RootElement.GetProperty("hub").GetString()!=Url)throw new Exception("This profile uses a different port. Restore its configured port before starting.");
        server=Process.Start(NodeStart("serve","--config",ServerConfig))!;
        // Persist only public process metadata. Never write credentials to logs.
        File.WriteAllText(Path.Combine(Data,"launcher-server.json"),JsonSerializer.Serialize(new{pid=server.Id,started=server.StartTime.ToUniversalTime()}));
        server.OutputDataReceived+=(_,e)=>{if(e.Data?.StartsWith("Private backend:")==true)Log(e.Data);};server.ErrorDataReceived+=(_,e)=>{if(e.Data?.Contains("EADDRINUSE")==true)Log("Port already in use");};server.BeginOutputReadLine();server.BeginErrorReadLine();
        for(int i=0;i<30;i++){if(server.HasExited)throw new Exception("Server stopped before becoming ready. Check the selected port.");try{await http.GetStringAsync(Url+"/health");RememberServer(Url);Log("Private server ready");return;}catch(HttpRequestException){}catch(TaskCanceledException){}await Task.Delay(200);}throw new Exception("Server startup timed out.");
    }
    void AttachOwnedServer(){try{using var d=JsonDocument.Parse(File.ReadAllText(Path.Combine(Data,"launcher-server.json")));var p=Process.GetProcessById(d.RootElement.GetProperty("pid").GetInt32());if(string.Equals(p.MainModule?.FileName,Node,StringComparison.OrdinalIgnoreCase)&&Math.Abs((p.StartTime.ToUniversalTime()-d.RootElement.GetProperty("started").GetDateTime()).TotalSeconds)<1)server=p;else p.Dispose();}catch{}}
    async Task StopServer(){if(remote.Checked)throw new Exception("Switch to local host mode to stop your local server. Remote servers are managed by their owner.");if(GameRunning())throw new Exception("Close the private game first so it can disconnect normally.");if(server is null||server.HasExited)AttachOwnedServer();if(server is null||server.HasExited){Log("No server owned by this launcher is running");return;}using var http=new HttpClient{Timeout=TimeSpan.FromSeconds(3)};using var info=JsonDocument.Parse(await http.GetStringAsync(Url+"/private/server-info"));if(info.RootElement.TryGetProperty("account_protocol",out _))await Command("stop-local","--profile",Profile);else{using var health=JsonDocument.Parse(await http.GetStringAsync(Url+"/health"));if(health.RootElement.GetProperty("players").GetInt32()!=0)throw new Exception("An older server still has connected players. Close those private clients before upgrading it.");Log("Stopping the older local backend for upgrade");server.Kill();}await server.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));server.Dispose();server=null;Log("Server stopped; stored worlds retained");}
    async Task Prepare(){
        if(GameRunning())throw new Exception("Close the private game before repairing its files.");
        var src=Path.GetFullPath(source.Text);if(src.Equals(Path.GetFullPath(GameDir),StringComparison.OrdinalIgnoreCase))throw new Exception("Choose your installed game, not the private copy.");
        var manifest=Path.Combine(src,".platinum-together-install.json");if(!File.Exists(manifest))throw new Exception("The selected folder is missing its installation manifest.");
        using var doc=JsonDocument.Parse(await File.ReadAllTextAsync(manifest));
        foreach(var entry in doc.RootElement.GetProperty("OwnedFiles").EnumerateObject())if(Path.GetFileName(entry.Name).Equals("Platinum.exe",StringComparison.OrdinalIgnoreCase))GameSafety.CheckHash(entry.Value.GetString()!);
        await GameSafety.CheckFile(Path.Combine(src,"Platinum.exe"));var packaged=doc.RootElement.GetProperty("OwnedFiles").EnumerateObject().Select(x=>(x.Name,x.Value.GetString()!)).ToArray();
        var required=doc.RootElement.GetProperty("RequiredFiles").EnumerateObject().Select(x=>(x.Name,x.Value.GetString()!)).ToArray();var files=packaged.Concat(string.IsNullOrWhiteSpace(rom.Text)?required:[]).DistinctBy(x=>x.Name,StringComparer.OrdinalIgnoreCase).ToArray();
        Log("Checking all installed assets before replacing the private copy…");await Task.Run(()=>PreparationPreflight.Validate(src,GameDir,files));
        if(GameRunning())throw new IOException("The private game started during validation. No game assets were replaced.");
        Log("Preparing separate game copy…");await Task.Run(()=>{foreach(var (relative,expected) in files){var from=LauncherChecks.Child(src,relative);var to=LauncherChecks.Child(GameDir,relative);if(!File.Exists(from))throw new IOException("Missing installed asset: "+relative);var bytes=File.ReadAllBytes(from);if(!Convert.ToHexString(SHA256.HashData(bytes)).Equals(expected,StringComparison.OrdinalIgnoreCase))throw new IOException("Installed asset failed validation: "+relative);Directory.CreateDirectory(Path.GetDirectoryName(to)!);File.WriteAllBytes(to,bytes);}});
        if(!string.IsNullOrWhiteSpace(rom.Text))await RomImport.Extract(rom.Text,GameDir,doc.RootElement.GetProperty("RomSha256").GetString()!,packaged.Select(x=>x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase),new Progress<string>(Log));
        foreach(var (relative,expected) in required){var path=LauncherChecks.Child(GameDir,relative);if(!File.Exists(path)||!Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))).Equals(expected,StringComparison.OrdinalIgnoreCase))throw new IOException("Required ROM asset failed validation: "+relative);}
        // The native build needs its local console profile even on a fresh save.
        // Preserve the private copy's profile during every subsequent repair.
        var firmware=Path.Combine(GameDir,"firmware.bin");if(!File.Exists(firmware)){
            var from=Path.Combine(src,"firmware.bin");if(!File.Exists(from))throw new IOException("Start the installed game once to initialize its local console profile, then prepare again.");
            var size=new FileInfo(from).Length;if(size<16||size>262144)throw new IOException("The installed console profile has an unexpected size.");File.Copy(from,firmware,false);
        }
        var preferences=Path.Combine(GameDir,"platinum-pc.ini");if(!File.Exists(preferences))await File.WriteAllTextAsync(preferences,"fullscreen=0\n");
        File.Copy(manifest,Path.Combine(GameDir,".platinum-together-install.json"),true);
        foreach(var extra in new[]{"rom-provenance.json","release.signed.json"})if((extra!="rom-provenance.json"||string.IsNullOrWhiteSpace(rom.Text))&&File.Exists(Path.Combine(src,extra)))File.Copy(Path.Combine(src,extra),Path.Combine(GameDir,extra),true);
        var downloads=Path.Combine(Directory.GetParent(src)!.FullName,"Downloads");if(Directory.Exists(downloads)){Directory.CreateDirectory(Path.Combine(Client,"Downloads"));foreach(var f in Directory.EnumerateFiles(downloads,"*.signed.json"))File.Copy(f,Path.Combine(Client,"Downloads",Path.GetFileName(f)),true);}
        File.WriteAllText(Path.Combine(Client,".private-launcher"),"2\nPrivate game copy; packaged and required ROM files validated. No original saves or credentials imported.");
        Log("Private game copy ready. It uses a separate save and profile.");
    }
    bool GameRunning()=>Process.GetProcessesByName("Platinum").Any(p=>{try{return string.Equals(p.MainModule?.FileName,Path.Combine(GameDir,"Platinum.exe"),StringComparison.OrdinalIgnoreCase);}catch{return false;}finally{p.Dispose();}});
    async Task Play(){
        if(File.Exists(Path.Combine(GameDir,"Platinum.exe")))await GameSafety.CheckFile(Path.Combine(GameDir,"Platinum.exe"));
        await FinishPendingGifts();
        if(GameRunning())throw new Exception("The private game is already running.");if(!File.Exists(Path.Combine(Client,".private-launcher"))||!File.ReadAllText(Path.Combine(Client,".private-launcher")).StartsWith("2\n"))await Prepare();
        await StartServer();await Command("account-login","--profile",Profile);RefreshAccount();
        using var profile=JsonDocument.Parse(await File.ReadAllTextAsync(Profile));var p=profile.RootElement;
        var credentials=JsonSerializer.SerializeToUtf8Bytes(new{schema=1,hub=Url,seed=p.GetProperty("seed").GetString(),token=p.GetProperty("token").GetString(),refresh_token=p.GetProperty("refresh_token").GetString(),name=p.GetProperty("display_name").GetString(),named=true});
        var appdata=Path.Combine(Client,"AppData");NativeSignIn.Write(Client,LocalSeal.Protect(credentials),GameRunning);
        var networkConfig=Path.Combine(GameDir,"multiplayer.ini");
        if(File.Exists(networkConfig)&&new FileInfo(networkConfig).Length>1048576)throw new IOException("Native multiplayer settings are unexpectedly large.");
        var networkBefore=File.Exists(networkConfig)?await File.ReadAllBytesAsync(networkConfig):[];
        var networkUpdated=NativeMultiplayerConfig.Update(networkBefore,Url,p.GetProperty("hub_key").GetString()!);
        var networkStage=networkConfig+".launcher-"+Guid.NewGuid().ToString("N")+".tmp";
        try{
            await File.WriteAllBytesAsync(networkStage,networkUpdated);
            if(GameRunning()||!(File.Exists(networkConfig)?await File.ReadAllBytesAsync(networkConfig):[]).SequenceEqual(networkBefore))throw new IOException("Native game/settings changed while preparing the launch; existing settings were retained.");
            File.Move(networkStage,networkConfig,true);
        }finally{if(File.Exists(networkStage))File.Delete(networkStage);}
        await GameSafety.CheckFile(Path.Combine(GameDir,"Platinum.exe"));
        var info=new ProcessStartInfo(Path.Combine(GameDir,"Platinum.exe")){WorkingDirectory=GameDir,UseShellExecute=false};info.Environment["LOCALAPPDATA"]=appdata;var launched=Process.Start(info);game=launched;Volatile.Write(ref discordState,"Playing a private world");if(launched is not null){launched.EnableRaisingEvents=true;launched.Exited+=(_,_)=>{if(ReferenceEquals(game,launched))Volatile.Write(ref discordState,"In the launcher");};}Log("Private game launched");
    }
    void RefreshAccount(){try{if(File.Exists(Profile)){using var p=JsonDocument.Parse(File.ReadAllText(Profile));var e=p.RootElement;accountStatus.Text=$"Your name: {e.GetProperty("display_name").GetString()}\n{e.GetProperty("hub").GetString()}";}else accountStatus.Text="Pick a unique name to play on this private server.";}catch{accountStatus.Text="Your saved name could not be read.";}}
    async Task AccountSignIn(){if(GameRunning())throw new Exception("Close the private game before switching or changing its account.");await StartServer();await Command("account-login","--profile",Profile);RefreshAccount();Log("Private account signed in");}
    async Task RenameAccount(){if(GameRunning())throw new Exception("Close the private game before renaming its account.");await StartServer();await Command("account-rename","--profile",Profile,"--name",name.Text.Trim());RefreshAccount();Log("Username updated; friends and worlds kept on the same account");}
    async Task ImportAccount(){
        if(GameRunning())throw new Exception("Close the private game before restoring an account.");using var file=new OpenFileDialog{Filter="Private account backup|*.ptaccount.json;*.json"};if(file.ShowDialog()!=DialogResult.OK)return;if(new FileInfo(file.FileName).Length>32768)throw new Exception("That file is too large for an account backup.");
        using var data=JsonDocument.Parse(await File.ReadAllTextAsync(file.FileName));var e=data.RootElement;var hub=e.GetProperty("hub").GetString()!;if(!Uri.TryCreate(hub,UriKind.Absolute,out var uri)||!new[]{"http","https"}.Contains(uri.Scheme)||uri.UserInfo!=""||uri.AbsolutePath!="/"||uri.Query!=""||uri.Fragment!="")throw new Exception("Invalid server address in account backup.");
        foreach(var key in new[]{"seed","key","hub_key"})if(Convert.FromBase64String(e.GetProperty(key).GetString()!).Length!=32)throw new Exception("Invalid account key.");
        var label=e.GetProperty("display_name").GetString();if(MessageBox.Show(this,$"Restore {label} on {uri.GetLeftPart(UriPartial.Authority)}?\n\nThe saved key will be used to sign in to this server. Existing profiles will be backed up.","Restore private account",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
        var folder=Path.Combine(root,"Profiles");Directory.CreateDirectory(folder);var candidate=Path.Combine(folder,"restore-"+Guid.NewGuid().ToString("N")+".json");await File.WriteAllTextAsync(candidate,e.GetRawText());
        try{await Command("account-login","--profile",candidate);remoteUrl.Text=uri.GetLeftPart(UriPartial.Authority);remote.Checked=true;if(File.Exists(Profile))File.Copy(Profile,Profile+".backup-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),false);File.Move(candidate,Profile,true);name.Text=label??"";RefreshAccount();Log("Account restored");}finally{if(File.Exists(candidate))File.Delete(candidate);}
    }
}
static class LauncherChecks
{
    internal static bool Supported(string hash)=>!GameSafety.Blocked(hash)&&PlatinumTrainer.GameBuild.NativeClientSupported(hash);
    internal static bool SaveAdapterSupported(string hash)=>!GameSafety.Blocked(hash)&&PlatinumTrainer.GameBuild.GiftSaveSupported(hash);
    internal static void NativeBuildPolicyChecks()
    {
        foreach(var hash in new[]{PlatinumTrainer.GameBuild.Hash015,PlatinumTrainer.GameBuild.Hash017})
            if(!Supported(hash)||!Supported(hash.ToLowerInvariant())||!SaveAdapterSupported(hash)||!SaveAdapterSupported(hash.ToLowerInvariant()))throw new Exception("Verified native client/gift adapter rejected.");
        if(PlatinumTrainer.GameBuild.Supported(PlatinumTrainer.GameBuild.Hash017))throw new Exception("Gift support enabled the old memory/save profile.");
        if(Supported(new string('0',64))||SaveAdapterSupported(new string('0',64))||Supported(GameSafety.QuarantinedHash)||SaveAdapterSupported(GameSafety.QuarantinedHash))throw new Exception("Unknown or quarantined build accepted.");
    }
    internal const string Hash="27075fceb04b5a3140e707bae1b12d69e04824b86e960121f566ad5418728488";
    internal static string Child(string root,string relative){var full=Path.GetFullPath(Path.Combine(root,relative));if(!full.StartsWith(Path.GetFullPath(root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new IOException("Invalid manifest path");return full;}
    internal static void Run(){NativeBuildPolicyChecks();if(Supported(GameSafety.QuarantinedHash)||Supported(GameSafety.QuarantinedHash.ToUpperInvariant()))throw new Exception("Quarantined build accepted");bool blocked=false;try{GameSafety.CheckHash(GameSafety.QuarantinedHash);}catch(IOException){blocked=true;}if(!blocked)throw new Exception("Quarantine refusal failed");var root=Path.Combine(Path.GetTempPath(),"launcher-test");if(Child(root,"a/file.bin")!=Path.Combine(root,"a","file.bin"))throw new Exception("Path resolution failed");foreach(var bad in new[]{"../escape","C:/Windows/file"}){bool rejected=false;try{Child(root,bad);}catch(IOException){rejected=true;}if(!rejected)throw new Exception("Unsafe path accepted");}var sealedBytes=LocalSeal.Protect(Encoding.UTF8.GetBytes("test-profile"));if(sealedBytes.Length<20)throw new Exception("DPAPI failed");using var ui=new Launcher();_ = ui.Handle;File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"layout-check.json"),JsonSerializer.Serialize(new{ui.DeviceDpi,ui.Width,ui.Height,ui.ClientSize,ui.AutoScaleMode,ui.AutoScaleDimensions,ui.CurrentAutoScaleDimensions,defaultFont=ui.Font.Size,textHeight=TextRenderer.MeasureText("Example",ui.Font).Height}));File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"launcher-selftest.json"),"{\"passed\":true,\"checks\":[\"quarantined build refusal\",\"manifest path isolation\",\"DPAPI profile protection\",\"launcher window construction\"]}");}
}
static class LocalSeal
{
    [StructLayout(LayoutKind.Sequential)]struct Blob{public int Size;public IntPtr Data;}
    [DllImport("crypt32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool CryptProtectData(ref Blob input,string description,ref Blob entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
    [DllImport("kernel32.dll")]static extern IntPtr LocalFree(IntPtr p);
    public static byte[] Protect(byte[] input){var entropy=Encoding.ASCII.GetBytes("PlatinumTogether online sign-in 1");var a=new Blob{Size=input.Length,Data=Marshal.AllocHGlobal(input.Length)};var b=new Blob{Size=entropy.Length,Data=Marshal.AllocHGlobal(entropy.Length)};try{Marshal.Copy(input,0,a.Data,input.Length);Marshal.Copy(entropy,0,b.Data,entropy.Length);if(!CryptProtectData(ref a,"Platinum Together sign-in",ref b,IntPtr.Zero,IntPtr.Zero,1,out var output))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());try{var result=new byte[output.Size];Marshal.Copy(output.Data,result,0,result.Length);return result;}finally{LocalFree(output.Data);}}finally{Marshal.FreeHGlobal(a.Data);Marshal.FreeHGlobal(b.Data);CryptographicOperations.ZeroMemory(input);}}
}
