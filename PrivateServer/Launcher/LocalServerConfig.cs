using System.Text.Json;
namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    string? configuredData;
    string configuredBind="127.0.0.1";
    string ServerConfig=>Path.Combine(root,"Server","server.json");
    void LoadLocalServerConfig()
    {
        if(!File.Exists(ServerConfig))return;
        try
        {
            using var doc=JsonDocument.Parse(File.ReadAllText(ServerConfig));var c=doc.RootElement;
            if(c.GetProperty("schema").GetInt32()!=1)throw new IOException("Unsupported server config schema.");
            var n=c.GetProperty("network");int configuredPort=n.GetProperty("port").GetInt32();
            if(configuredPort<1||configuredPort>65535)throw new IOException("Invalid server port.");
            port.Value=configuredPort;configuredBind=n.GetProperty("bind").GetString()??"127.0.0.1";
            configuredData=Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ServerConfig)!,c.TryGetProperty("data",out var data)?data.GetString()??"../Data":"../Data"));
        }
        catch(Exception e){throw new IOException("Read Server/server.json: "+e.Message,e);}
    }
    async Task EnsureLocalServerConfig()
    {
        if(!File.Exists(ServerConfig))
        {
            // First-use migration preserves previous local host preferences.
            var settings=new{schema=1,name="Platinum Private Worlds",role="combined",data="../Data",network=new{bind=lan.Checked?"0.0.0.0":"127.0.0.1",port=(int)port.Value,publicUrl=lan.Checked&&!string.IsNullOrWhiteSpace(advertised.Text)?advertised.Text.Trim():null,trustedLoopbackProxy=false},logging=new{enabled=true},plugins=new{enabled=true,directory="../Data/Plugins"}};
            using var file=new FileStream(ServerConfig,FileMode.CreateNew,FileAccess.Write,FileShare.None);
            JsonSerializer.Serialize(file,settings,new JsonSerializerOptions{WriteIndented=true});
        }
        await Command("check-config","--config",ServerConfig);LoadLocalServerConfig();
    }
}
