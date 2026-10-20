using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
namespace PlatinumTrainer;

public record SaveLocation(int Map,int Warp,int X,int Z,int Facing);
public record SaveLabel(int Id,string Name)
{
    public override string ToString()=>$"{Id} · {Name}";
}
public record SaveFooter(int Sector,int Block,uint Global,uint Counter,bool Valid);
public record SaveEditPlan(Dictionary<int,bool> Flags,Dictionary<int,ushort> Variables,SaveLocation? Location);

public sealed class PortSaveEditor
{
    public const int FileSize=0x80000,NormalSize=0xcf40,BoxSize=0x121ec;
    public const int VariablesOffset=3512,FlagsOffset=3512+0x240,LocationOffset=4748,TransitionFlag=0x966;
    public byte[] Original {get;}
    public SaveFooter[] Footers {get;}
    public int NormalBase {get;}
    public int BoxBase {get;}
    public string OriginalHash=>Hash(Original);
    public static string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes));
    static int I32(byte[] b,int offset)=>BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(offset,4));
    static uint U32(byte[] b,int offset)=>BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(offset,4));
    static ushort U16(byte[] b,int offset)=>BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(offset,2));
    static void Put32(byte[] b,int offset,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(offset,4),value);
    static void Put16(byte[] b,int offset,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(offset,2),value);
    public static ushort Crc(ReadOnlySpan<byte> data)
    {
        ushort crc=0xffff;
        foreach(byte value in data){crc^=(ushort)(value<<8);for(int bit=0;bit<8;bit++)crc=(ushort)((crc&0x8000)!=0?(crc<<1)^0x1021:crc<<1);}
        return crc;
    }
    static int Compare(uint a,uint b)=>a==uint.MaxValue&&b==0?-1:a==0&&b==uint.MaxValue?1:a.CompareTo(b);
    static SaveFooter Newer(SaveFooter a,SaveFooter b)=>Compare(a.Global,b.Global) is var d&&d!=0?(d>0?a:b):Compare(a.Counter,b.Counter)>=0?a:b;
    public PortSaveEditor(byte[] data)
    {
        if(data.Length!=FileSize)throw new InvalidDataException("Expected this port's 524,288-byte save.bin.");
        Original=(byte[])data.Clone();var checks=new List<SaveFooter>();
        for(int sector=0;sector<2;sector++)for(int block=0;block<2;block++)
        {
            int length=block==0?NormalSize:BoxSize,start=sector*0x40000+(block==0?0:NormalSize),footer=start+length-20;
            bool valid=U32(data,footer+8)==length&&U32(data,footer+12)==0x20060623&&data[footer+16]==block&&U16(data,footer+18)==Crc(data.AsSpan(start,length-20));
            checks.Add(new(sector,block,U32(data,footer),U32(data,footer+4),valid));
        }
        Footers=checks.ToArray();
        if(Footers.Any(c=>!c.Valid))throw new InvalidDataException("Save layout or CRC differs, or a backup block needs recovery. No edits are allowed by this profile.");
        var normal=Newer(Footers[0],Footers[2]);var box=Newer(Footers[1],Footers[3]);
        if(normal.Global!=box.Global)throw new InvalidDataException("Normal/box save generations do not match. Recover the save in-game first.");
        NormalBase=normal.Sector*0x40000;
        BoxBase=box.Sector*0x40000+NormalSize;
        var location=Location;
        if(location.Map is <0 or >=593||location.Facing is <0 or >3||location.X is <0 or >32767||location.Z is <0 or >32767)throw new InvalidDataException("Saved location doesn't match this verified profile.");
    }
    public bool Flag(int id)
    {
        if(id is <1 or >2911)throw new ArgumentOutOfRangeException(nameof(id));
        return (Original[NormalBase+FlagsOffset+id/8]&(1<<(id%8)))!=0;
    }
    public ushort Variable(int id)
    {
        if(id is <0x4000 or >=0x4120)throw new ArgumentOutOfRangeException(nameof(id));
        return U16(Original,NormalBase+VariablesOffset+2*(id-0x4000));
    }
    SaveLocation ReadLocation(int relative)
    {
        int p=NormalBase+LocationOffset+relative;return new(I32(Original,p),I32(Original,p+4),I32(Original,p+8),I32(Original,p+12),I32(Original,p+16));
    }
    public SaveLocation Location=>ReadLocation(0);
    public SaveLocation Entrance=>ReadLocation(20);
    public SaveLocation Previous=>ReadLocation(40);
    public byte[] Build(SaveEditPlan plan)
    {
        var result=(byte[])Original.Clone();
        void SetFlag(int id,bool value)
        {
            _=Flag(id);int at=NormalBase+FlagsOffset+id/8,mask=1<<(id%8);
            result[at]=(byte)(value?result[at]|mask:result[at]&~mask);
        }
        foreach(var (id,value) in plan.Flags)SetFlag(id,value);
        foreach(var (id,value) in plan.Variables){_=Variable(id);Put16(result,NormalBase+VariablesOffset+2*(id-0x4000),value);}
        if(plan.Location is {} location&&location!=Location)
        {
            if(plan.Flags.ContainsKey(TransitionFlag))throw new InvalidOperationException("Remove the manual edit to flag 0x0966 before staging a location; the location change manages this temporary flag.");
            if(location.Map is <3 or >=593||location.X is <0 or >32767||location.Z is <0 or >32767||location.Facing is <0 or >3||location.Warp!=-1)throw new ArgumentException("Use a real map ID 3–592, tile coordinates 0–32767, a facing direction and warp -1.");
            if(Flag(TransitionFlag))throw new InvalidOperationException("This save is already in a special return transition. Finish it in-game before editing location.");
            // Continue normally restores the old NPC/player object array. Use the
            // engine's existing one-shot return transition instead: it clears
            // this flag and rebuilds the player/NPC objects for specialLocation.
            foreach(int relative in new[]{0,60})
            {
                int p=NormalBase+LocationOffset+relative;Put32(result,p,location.Map);Put32(result,p+4,-1);Put32(result,p+8,location.X);Put32(result,p+12,location.Z);Put32(result,p+16,location.Facing);
            }
            SetFlag(TransitionFlag,true);
        }
        if(result.SequenceEqual(Original))return result;
        int crcAt=NormalBase+NormalSize-2;
        Put16(result,crcAt,Crc(result.AsSpan(NormalBase,NormalSize-20)));
        _=new PortSaveEditor(result);
        return result;
    }
    public string Install(string source,SaveEditPlan plan,Func<bool>? running=null,string? backupRoot=null)
        =>InstallPrepared(source,Build(plan),plan,running,backupRoot);
    public string InstallPrepared(string source,byte[] edited,object review,Func<bool>? running=null,string? backupRoot=null)
    {
        running??=()=>Process.GetProcessesByName("Platinum").Length!=0;
        if(running())throw new InvalidOperationException("Save normally and close Platinum Together before Apply. Your running world has not been changed.");
        string gameRoot=Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PlatinumTogether","Game"))+Path.DirectorySeparatorChar;
        if(Path.GetFullPath(source).StartsWith(gameRoot,StringComparison.OrdinalIgnoreCase)&&!GameBuild.Supported(Hash(File.ReadAllBytes(Path.Combine(gameRoot,"Platinum.exe")))))throw new InvalidOperationException("The installed game changed from the verified 0.1.5/0.1.6 builds. Recheck the save profile before applying.");
        byte[] current=File.ReadAllBytes(source);
        if(Hash(current)!=OriginalHash)throw new InvalidOperationException("The save changed since it was loaded. Reload it and review the edits again.");
        _=new PortSaveEditor(edited);if(edited.SequenceEqual(Original))throw new InvalidOperationException("No changes are staged.");
        var parent=Path.GetDirectoryName(Path.GetFullPath(source))!;
        string backup=Path.Combine(backupRoot??Path.Combine(AppContext.BaseDirectory,"Backups","SaveEditor"),DateTime.Now.ToString("yyyyMMdd-HHmmss-fffffff"));
        Directory.CreateDirectory(backup);
        foreach(string path in Directory.GetFiles(parent,"save*.bin").Append(source).Distinct(StringComparer.OrdinalIgnoreCase))File.Copy(path,Path.Combine(backup,Path.GetFileName(path)));
        File.WriteAllText(Path.Combine(backup,"edit-plan.json"),JsonSerializer.Serialize(new{source=Path.GetFullPath(source),originalHash=OriginalHash,editedHash=Hash(edited),review},new JsonSerializerOptions{WriteIndented=true}));
        if(running()||Hash(File.ReadAllBytes(source))!=OriginalHash)throw new IOException("Game started or save changed during backup; nothing installed.");
        string temporary=Path.Combine(parent,".studio-save-"+Guid.NewGuid().ToString("N")+".tmp");
        try{File.WriteAllBytes(temporary,edited);if(!File.ReadAllBytes(temporary).SequenceEqual(edited))throw new IOException("Staged write verification failed");File.Move(temporary,source,true);}
        finally{if(File.Exists(temporary))File.Delete(temporary);}
        return backup;
    }
}
