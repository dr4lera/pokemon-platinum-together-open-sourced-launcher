using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PlatinumPrivateLauncher;
internal static class RomImport
{
    internal record Entry(string Path,int Start,int Length);
    internal static List<Entry> Read(byte[] rom){
        if(rom.Length<512||rom.Length>536870912)throw new InvalidDataException("Choose a valid .nds ROM (512 bytes–512 MB).");
        int U32(int offset){if(offset<0||offset>rom.Length-4)throw new InvalidDataException("Truncated ROM table.");return checked((int)BinaryPrimitives.ReadUInt32LittleEndian(rom.AsSpan(offset,4)));}
        int U16(int offset){if(offset<0||offset>rom.Length-2)throw new InvalidDataException("Truncated ROM directory.");return BinaryPrimitives.ReadUInt16LittleEndian(rom.AsSpan(offset,2));}
        void Range(int start,int size){if(start<0||size<0||(long)start+size>rom.Length)throw new InvalidDataException("ROM filesystem is outside the file.");}
        int fnt=U32(64),fntSize=U32(68),fat=U32(72),fatSize=U32(76);Range(fnt,fntSize);Range(fat,fatSize);
        if(fntSize<8||fatSize%8!=0)throw new InvalidDataException("Invalid ROM filesystem.");int count=U16(fnt+6);if(count is <1 or >4096||count*8>fntSize)throw new InvalidDataException("Invalid ROM directories.");
        var entries=new List<Entry>{new("header.bin",0,512)};var visited=new HashSet<int>();var paths=new HashSet<string>(StringComparer.OrdinalIgnoreCase);long total=0;
        void Dir(int id,string prefix,int depth){
            if(depth>64||(id&0xf000)!=0xf000||(id&0xfff)>=count||!visited.Add(id))throw new InvalidDataException("Invalid or cyclic ROM directories.");
            int table=fnt+(id&0xfff)*8,offset=U32(table),file=U16(table+4);if(offset<count*8||offset>=fntSize)throw new InvalidDataException("Invalid filename table offset.");int pos=fnt+offset;
            while(true){if(pos>=fnt+fntSize)throw new InvalidDataException("Unterminated filename table.");int code=rom[pos++];if(code==0)break;int len=code&127;if(len==0||pos+len>fnt+fntSize)throw new InvalidDataException("Invalid ROM filename.");var name=Encoding.ASCII.GetString(rom,pos,len);pos+=len;if(name is "." or ".."||name.IndexOfAny(Path.GetInvalidFileNameChars())>=0||name.Any(c=>c<32||c>126))throw new InvalidDataException("Unsafe ROM filename.");var relative=prefix+name;if(!paths.Add(relative))throw new InvalidDataException("Duplicate ROM path.");
                if((code&128)!=0){if(pos+2>fnt+fntSize)throw new InvalidDataException("Truncated ROM directory.");int child=U16(pos);pos+=2;Dir(child,relative+"/",depth+1);}else{if(file>=fatSize/8)throw new InvalidDataException("ROM file ID outside FAT.");int start=U32(fat+file*8),end=U32(fat+file*8+4);file++;if(end<start)throw new InvalidDataException("Invalid ROM file extent.");Range(start,end-start);total+=end-start;if(total>2147483648L||entries.Count>=100000)throw new InvalidDataException("ROM filesystem too large.");entries.Add(new(relative,start,end-start));}
            }
        }
        Dir(0xf000,"",0);if(visited.Count!=count)throw new InvalidDataException("Unreachable ROM directories.");return entries;
    }
    internal static async Task Extract(string path,string game,string expectedHash,HashSet<string> packaged,IProgress<string>? progress=null){
        var info=new FileInfo(path);if(!info.Exists||info.Length<512||info.Length>536870912)throw new InvalidDataException("Choose your Pokémon Platinum .nds ROM.");
        var bytes=await File.ReadAllBytesAsync(path);var hash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();if(!hash.Equals(expectedHash,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("This ROM does not match the installed game's validated Pokémon Platinum ROM. The source ROM was left unchanged.");
        var entries=Read(bytes);await Task.Run(()=>{int done=0;foreach(var e in entries){if(packaged.Contains(e.Path))continue;var output=LauncherChecks.Child(game,e.Path);Directory.CreateDirectory(Path.GetDirectoryName(output)!);File.WriteAllBytes(output,bytes.AsSpan(e.Start,e.Length).ToArray());if(++done%100==0)progress?.Report($"Extracting ROM data • {done} / {entries.Count}");}});
        await File.WriteAllTextAsync(Path.Combine(game,"rom-provenance.json"),JsonSerializer.Serialize(new{Source=Path.GetFileName(path),Sha256=hash,Bytes=bytes.Length,Files=entries.Count-1,ExtractedBytes=entries.Skip(1).Sum(x=>(long)x.Length),SourceModified=false,Method="Nintendo DS FNT/FAT filesystem extraction only; no CPU code executed or emulator used."}));
    }
}
