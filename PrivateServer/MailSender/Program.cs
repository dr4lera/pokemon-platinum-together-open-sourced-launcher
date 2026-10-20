using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PlatinumGifts;
namespace PlatinumMailSender;
static class Program
{
    [STAThread] static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if(args.Contains("--selftest")){var test=RandomNumberGenerator.GetBytes(32);if(!test.SequenceEqual(Seal.Apply(Seal.Apply(test,false),true)))throw new Exception("Key protection failed");using var f=new Sender();_ = f.Handle;File.WriteAllText(Path.Combine(AppContext.BaseDirectory,"mail-sender-selftest.json"),"{\"passed\":true,\"checks\":[\"Windows key protection round trip\",\"sender form construction\"]}");return;}
        using var mutex=new Mutex(true,"Local\\PlatinumMailSender-"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(AppContext.BaseDirectory)))[..20],out var first);if(first)Application.Run(new Sender());
    }
}
sealed class Sender:Form
{
    readonly string root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,".."));
    readonly TextBox url=new(){Text="http://127.0.0.1:8788",Dock=DockStyle.Fill};
    readonly TextBox title=new(){MaxLength=40,Dock=DockStyle.Fill,PlaceholderText="Letter title"},body=new(){MaxLength=240,Multiline=true,Dock=DockStyle.Fill,PlaceholderText="Letter message (240 characters)"};
    readonly CheckedListBox recipients=new(){Dock=DockStyle.Fill,CheckOnClick=true};
    readonly ComboBox gift=new(){DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill};
    readonly NumericUpDown count=new(){Minimum=1,Maximum=999,Value=1,Dock=DockStyle.Fill};
    readonly ListBox attachments=new(){Dock=DockStyle.Fill};
    readonly Label status=new(){Text="Connect to your local private server to select recipients.",Dock=DockStyle.Fill,AutoSize=false};
    readonly Button connect=new(){Text="Connect / load accounts",Dock=DockStyle.Fill},send=new(){Text="Send letter and gifts",Dock=DockStyle.Fill},add=new(){Text="Add gift",Dock=DockStyle.Fill},remove=new(){Text="Remove selected gift",Dock=DockStyle.Fill};
    readonly Button importPokemon=new(){Text="PK4 / egg…",Dock=DockStyle.Fill};
    readonly HttpClient http=new(){Timeout=TimeSpan.FromSeconds(12)};
    static readonly string[] Items=["ultra-ball","great-ball","poke-ball","net-ball","nest-ball","luxury-ball","premier-ball","dusk-ball","quick-ball","potion","antidote","parlyz-heal","super-potion","full-heal"];
    string Vault=>Path.Combine(root,"OwnerKeys","mail-sender.dpapi");string Pending=>Path.Combine(root,"OwnerKeys","pending-mail.json");
    public Sender()
    {
        Text="Platinum • Owner Mail Sender";Size=new(940,730);MinimumSize=new(820,660);BackColor=Color.FromArgb(15,23,35);ForeColor=Color.White;Font=new("Segoe UI",11);StartPosition=FormStartPosition.CenterScreen;
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new(24),ColumnCount=2,RowCount=9};layout.ColumnStyles.Add(new(SizeType.Percent,40));layout.ColumnStyles.Add(new(SizeType.Percent,60));foreach(var h in new[]{58,42,42,45,110,45,-1,45,65})layout.RowStyles.Add(h<0?new(SizeType.Percent,100):new(SizeType.Absolute,h));Controls.Add(layout);
        void Both(Control c,int row){layout.Controls.Add(c,0,row);layout.SetColumnSpan(c,2);}
        Both(new Label{Text="Owner mail & gifts",Font=new("Segoe UI",23,FontStyle.Bold),Dock=DockStyle.Fill},0);Both(new Label{Text="Private server only • Sender key locked to this Windows user and PC",Dock=DockStyle.Fill},1);layout.Controls.Add(url,0,2);layout.Controls.Add(connect,1,2);layout.Controls.Add(recipients,0,3);layout.SetRowSpan(recipients,4);layout.Controls.Add(title,1,3);layout.Controls.Add(body,1,4);
        var line=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=4};foreach(var width in new[]{46,14,18,22})line.ColumnStyles.Add(new(SizeType.Percent,width));line.Controls.Add(gift,0,0);line.Controls.Add(count,1,0);line.Controls.Add(add,2,0);line.Controls.Add(importPokemon,3,0);layout.Controls.Add(line,1,5);layout.Controls.Add(attachments,1,6);layout.Controls.Add(remove,0,7);layout.Controls.Add(send,1,7);Both(status,8);
        gift.Items.Add("Letter only");gift.Items.AddRange(Items.Cast<object>().ToArray());gift.Items.AddRange(new object[]{"money","boots","lure","egg"});
        // Use the dependency's licensed localized catalog, rather than shipping
        // a table extracted from one user's installed game.
        var itemNames=PKHeX.Core.GameInfo.GetStrings("en").itemlist;
        for(int id=1;id<=467&&id<itemNames.Length;id++)gift.Items.Add(new ItemChoice(id,itemNames[id]));gift.SelectedIndex=0;
        foreach(var b in new[]{connect,send,add,remove}){b.FlatStyle=FlatStyle.Flat;b.BackColor=Color.FromArgb(28,67,82);b.ForeColor=Color.White;b.Margin=new(4);}
        connect.Click+=async(_,_)=>await Work(async()=>{EnsureKey();var data=await Request(new{action="recipients"});recipients.Items.Clear();foreach(var p in data.GetProperty("players").EnumerateArray())recipients.Items.Add(p.GetString()!);status.Text=$"{recipients.Items.Count} private accounts loaded. Check recipients to send a letter.";});
        gift.SelectedIndexChanged+=(_,_)=>count.Maximum=gift.Text switch{"money"=>999999,"boots" or "egg"=>1,"lure"=>99,_=>999};
        add.Click+=(_,_)=>{try{if(gift.SelectedIndex<=0)return;int copies=gift.SelectedItem is ItemChoice?(int)count.Value:1;if(attachments.Items.Count+copies>8)throw new IOException("A letter holds 8 gifts. Poké Mart items occupy one gift slot each; use native supplies for bulk items.");if(gift.SelectedItem is ItemChoice choice){for(int i=0;i<copies;i++)attachments.Items.Add(new Gift(choice.Name,1,Convert.ToBase64String(MartGift.Item(choice.Id))));status.Text="Custom item → launcher Gift inbox → Poké Mart deliveryman.";}else attachments.Items.Add(new Gift(gift.Text,(int)count.Value));}catch(Exception e){status.Text=e.Message;}};
        importPokemon.Click+=(_,_)=>{try{if(attachments.Items.Count>=8)throw new IOException("A letter holds 8 gifts.");using var file=new OpenFileDialog{Filter="Gen IV Pokémon or egg|*.pk4"};if(file.ShowDialog(this)!=DialogResult.OK)return;var pgt=MartGift.Pokemon(File.ReadAllBytes(file.FileName));attachments.Items.Add(new Gift(Path.GetFileName(file.FileName),1,Convert.ToBase64String(pgt)));status.Text="Exact Pokémon/egg queued for Poké Mart delivery. Recipient loads it from the launcher's Gift inbox before playing.";}catch(Exception e){status.Text=e.Message;}};remove.Click+=(_,_)=>{if(attachments.SelectedIndex>=0)attachments.Items.RemoveAt(attachments.SelectedIndex);};
        send.Click+=async(_,_)=>await Work(Send);
        FormClosed+=(_,_)=>http.Dispose();
    }
    Uri Endpoint(string path){var u=new Uri(url.Text.Trim());if(u.Scheme!="http"||u.Host!="127.0.0.1"||u.AbsolutePath!="/"||u.Query!=""||u.UserInfo!="")throw new Exception("Use this PC's server: http://127.0.0.1:8788. Remote sending is disabled.");return new(u,path);}
    void EnsureKey()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Vault)!);Directory.CreateDirectory(Path.Combine(root,"Data"));
        var trust=Path.Combine(root,"Data","mail-sender-public.pem");
        using var rsa=RSA.Create(3072);
        if(File.Exists(Vault)){var raw=Seal.Apply(File.ReadAllBytes(Vault),true);try{rsa.ImportPkcs8PrivateKey(raw,out _);}finally{CryptographicOperations.ZeroMemory(raw);}}
        else {if(File.Exists(trust))throw new Exception("This server already trusts a different sender. Keep its original OwnerKeys folder.");var raw=rsa.ExportPkcs8PrivateKey();try{var encrypted=Seal.Apply(raw,false);using var f=new FileStream(Vault,FileMode.CreateNew);f.Write(encrypted);}finally{CryptographicOperations.ZeroMemory(raw);}}
        var pem=rsa.ExportSubjectPublicKeyInfoPem();if(File.Exists(trust)){if(File.ReadAllText(trust)!=pem)throw new Exception("Sender key does not match this server's trusted sender.");}else{using var f=new StreamWriter(new FileStream(trust,FileMode.CreateNew));f.Write(pem);}
    }
    async Task<JsonElement> Request(object data)
    {
        EnsureKey();var challenge=await http.GetFromJsonAsync<JsonElement>(Endpoint("/private/mail/challenge"));var nonce=challenge.GetProperty("nonce").GetString()!;var at=challenge.GetProperty("server_ms").GetInt64();var payload=JsonSerializer.SerializeToUtf8Bytes(data);var proof=Encoding.ASCII.GetBytes("PT-PRIVATE-MAIL\0").Concat(Convert.FromBase64String(nonce)).Concat(Encoding.ASCII.GetBytes(at+"\0")).Concat(SHA256.HashData(payload)).ToArray();
        var raw=Seal.Apply(File.ReadAllBytes(Vault),true);using var rsa=RSA.Create();try{rsa.ImportPkcs8PrivateKey(raw,out _);}finally{CryptographicOperations.ZeroMemory(raw);}
        using var result=await http.PostAsJsonAsync(Endpoint("/private/mail"),new{nonce,server_ms=at,data=Convert.ToBase64String(payload),sig=Convert.ToBase64String(rsa.SignData(proof,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1))});var reply=await result.Content.ReadFromJsonAsync<JsonElement>();if(!result.IsSuccessStatusCode)throw new Exception(reply.TryGetProperty("error",out var error)?error.GetString():"Mail request failed");return reply;
    }
    async Task Send()
    {
        object draft;
        if(File.Exists(Pending))
        {
            if(MessageBox.Show(this,"A previous delivery has an unknown result. Retry that exact delivery? The server prevents duplicates.","Recover pending delivery",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;draft=JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(Pending));
        }
        else
        {
            var to=recipients.CheckedItems.Cast<string>().ToArray();if(to.Length==0||string.IsNullOrWhiteSpace(title.Text))throw new Exception("Check recipients and enter a letter title.");
            var gifts=attachments.Items.Cast<Gift>().Select(g=>g.Pgt!=null?new Dictionary<string,object>{{"kind","pgt"},{"data",g.Pgt}}:new Dictionary<string,object>{{"kind",Items.Contains(g.Name)?"item":g.Name},{"count",g.Count}}.Concat(Items.Contains(g.Name)?new[]{new KeyValuePair<string,object>("item",g.Name)}:[]).ToDictionary(x=>x.Key,x=>x.Value)).ToArray();
            draft=new{action="send",op=Guid.NewGuid().ToString(),to,title=title.Text,body=body.Text,attachments=gifts};
            if(MessageBox.Show(this,$"Send “{title.Text}” to {to.Length} selected account(s) with {gifts.Length} gift(s)?","Send private mail",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            EnsureKey();using var f=new StreamWriter(new FileStream(Pending,FileMode.CreateNew));f.Write(JsonSerializer.Serialize(draft));
        }
        var result=await Request(draft);var done=Path.Combine(root,"OwnerKeys","Delivered");Directory.CreateDirectory(done);File.Move(Pending,Path.Combine(done,result.GetProperty("op").GetString()+".json"),true);status.Text=$"Delivered to {result.GetProperty("delivered").GetInt32()} account(s). Use MAIL for supplies, or Launcher → Gift inbox for custom Pokémon, eggs and items.";
    }
    async Task Work(Func<Task> action){foreach(var c in new Control[]{connect,send,add,remove,importPokemon,url,title,body,recipients,gift,count,attachments})c.Enabled=false;try{await action();}catch(Exception e){status.Text=e.Message;}finally{if(!IsDisposed)foreach(var c in new Control[]{connect,send,add,remove,importPokemon,url,title,body,recipients,gift,count,attachments})c.Enabled=true;}}
    protected override void Dispose(bool disposing){if(disposing)http.Dispose();base.Dispose(disposing);}
    sealed record Gift(string Name,int Count,string? Pgt=null){public override string ToString()=>$"{Name} ×{Count}"+(Pgt!=null?" • Poké Mart":" • MAIL/SUPPLIES");}
    sealed record ItemChoice(int Id,string Name){public override string ToString()=>$"{Id} · {Name} (Poké Mart)";}
}
static class Seal
{
    [StructLayout(LayoutKind.Sequential)]struct Blob{public int Size;public IntPtr Data;}
    [DllImport("crypt32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool CryptProtectData(ref Blob input,string description,ref Blob entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
    [DllImport("crypt32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool CryptUnprotectData(ref Blob input,IntPtr description,ref Blob entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
    [DllImport("kernel32.dll")]static extern IntPtr LocalFree(IntPtr data);
    internal static byte[] Apply(byte[] input,bool decrypt)
    {
        var entropy=Encoding.ASCII.GetBytes("Platinum Private Mail Sender v1");var a=new Blob{Size=input.Length,Data=Marshal.AllocHGlobal(input.Length)};var b=new Blob{Size=entropy.Length,Data=Marshal.AllocHGlobal(entropy.Length)};
        try{Marshal.Copy(input,0,a.Data,input.Length);Marshal.Copy(entropy,0,b.Data,entropy.Length);Blob output;var ok=decrypt?CryptUnprotectData(ref a,IntPtr.Zero,ref b,IntPtr.Zero,IntPtr.Zero,1,out output):CryptProtectData(ref a,"Private mail sender",ref b,IntPtr.Zero,IntPtr.Zero,1,out output);if(!ok)throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());try{var result=new byte[output.Size];Marshal.Copy(output.Data,result,0,result.Length);return result;}finally{LocalFree(output.Data);}}finally{Marshal.FreeHGlobal(a.Data);Marshal.FreeHGlobal(b.Data);}
    }
}
