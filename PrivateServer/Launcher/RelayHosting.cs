using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    Process? tunnel;
    readonly TextBox shareAddress=new(){ReadOnly=true,Width=790,PlaceholderText="Start internet relay to get a join address"};
    string TunnelExe=>Path.Combine(root,"Runtime","cloudflared.exe");string TunnelRecord=>Path.Combine(Data,"launcher-relay.json");
    async Task StartRelay()
    {
        if(remote.Checked)throw new Exception("Switch Account to local hosting to share this PC's server.");await StartServer();
        if(!File.Exists(TunnelExe))throw new Exception("The bundled cloudflared runtime is missing.");
        try{using var metadata=JsonDocument.Parse(File.ReadAllText(TunnelRecord));var p=Process.GetProcessById(metadata.RootElement.GetProperty("pid").GetInt32());if(string.Equals(p.MainModule?.FileName,TunnelExe,StringComparison.OrdinalIgnoreCase)&&Math.Abs((p.StartTime.ToUniversalTime()-metadata.RootElement.GetProperty("started").GetDateTime()).TotalSeconds)<1){tunnel=p;shareAddress.Text=metadata.RootElement.GetProperty("url").GetString();await Command("advertise-local","--profile",Path.Combine(Data,"launcher-player.json"),"--url",shareAddress.Text);Log("Internet relay is already running");return;}p.Dispose();}catch(IOException){}catch(ArgumentException){}catch(JsonException){}catch(InvalidOperationException){}
        var config=Path.Combine(Data,"quick-tunnel.yml");await File.WriteAllTextAsync(config,"{}\n");var ready=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var start=new ProcessStartInfo(TunnelExe){WorkingDirectory=Data,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};foreach(var arg in new[]{"tunnel","--config",config,"--no-autoupdate","--protocol","http2","--url",Url})start.ArgumentList.Add(arg);
        tunnel=new Process{StartInfo=start,EnableRaisingEvents=true};var running=tunnel;
        void Line(object sender,DataReceivedEventArgs e){if(e.Data==null)return;var match=Regex.Match(e.Data,@"https://[a-z0-9-]+\.trycloudflare\.com");if(match.Success)ready.TrySetResult(match.Value);}
        running.OutputDataReceived+=Line;running.ErrorDataReceived+=Line;running.Exited+=(_,_)=>{ready.TrySetException(new IOException("Internet relay exited before becoming ready."));Log("Internet relay stopped. Friends need a running relay to connect.");};
        if(!running.Start())throw new Exception("Could not start internet relay.");running.BeginOutputReadLine();running.BeginErrorReadLine();
        try{var address=await ready.Task.WaitAsync(TimeSpan.FromSeconds(45));await Command("advertise-local","--profile",Path.Combine(Data,"launcher-player.json"),"--url",address);shareAddress.Text=address;await File.WriteAllTextAsync(TunnelRecord,JsonSerializer.Serialize(new{pid=running.Id,started=running.StartTime.ToUniversalTime(),url=address}));Log("Internet relay ready. Share the join address; no router port forwarding is needed.");}
        catch{if(!running.HasExited)running.Kill();running.Dispose();tunnel=null;throw;}
    }
    async Task StopRelay()
    {
        if(tunnel==null&&File.Exists(TunnelRecord)){using var d=JsonDocument.Parse(File.ReadAllText(TunnelRecord));try{var p=Process.GetProcessById(d.RootElement.GetProperty("pid").GetInt32());if(string.Equals(p.MainModule?.FileName,TunnelExe,StringComparison.OrdinalIgnoreCase)&&Math.Abs((p.StartTime.ToUniversalTime()-d.RootElement.GetProperty("started").GetDateTime()).TotalSeconds)<1)tunnel=p;else p.Dispose();}catch(ArgumentException){}}
        if(tunnel!=null){if(!tunnel.HasExited)tunnel.Kill();await tunnel.WaitForExitAsync();tunnel.Dispose();tunnel=null;}
        if(File.Exists(TunnelRecord))File.Delete(TunnelRecord);shareAddress.Clear();if(File.Exists(Path.Combine(Data,"launcher-player.json")))await Command("advertise-local","--profile",Path.Combine(Data,"launcher-player.json"),"--url",$"http://127.0.0.1:{port.Value}");Log("Internet relay stopped; local world data retained");
    }
    void RelayCard(FlowLayoutPanel host)
    {
        host.Controls.Add(new Label{Text="INTERNET RELAY • NO PORT FORWARDING",AutoSize=true,Margin=new(0,20,0,8)});host.Controls.Add(shareAddress);Add(host,"Start internet relay",StartRelay);Add(host,"Copy join address",()=>{if(string.IsNullOrWhiteSpace(shareAddress.Text))throw new Exception("Start the relay first.");Clipboard.SetText(shareAddress.Text);return Task.CompletedTask;});Add(host,"Stop internet relay",StopRelay);
        host.Controls.Add(new Label{Text="This temporary address changes when the tunnel restarts. Your PC and server must remain running. A permanent address needs a configured tunnel/domain or public relay host.",Width=790,Height=75,ForeColor=muted});
        Add(host,"Open owner mail sender",()=>{var exe=Path.Combine(root,"MailSender","PlatinumMailSender.exe");if(!File.Exists(exe))throw new Exception("Mail sender is missing.");Process.Start(new ProcessStartInfo(exe){WorkingDirectory=Path.GetDirectoryName(exe),UseShellExecute=true});return Task.CompletedTask;});
    }
}
