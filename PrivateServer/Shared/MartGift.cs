using System.Buffers.Binary;
using PKHeX.Core;
using PlatinumTrainer;
namespace PlatinumGifts;
public static class MartGift
{
    public const int Offset=46292,BlockSize=4912,DataSize=4904;
    public static byte[] Pokemon(byte[] file)
    {
        if(file.Length is not (136 or 236))throw new IOException("Choose a Gen IV .pk4 Pokémon or egg file (136 or 236 bytes).");var mon=new PK4((byte[])file.Clone());ValidatePokemon(mon);var gift=new byte[260];BinaryPrimitives.WriteUInt16LittleEndian(gift,(ushort)(mon.IsEgg?2:1));BinaryPrimitives.WriteInt32LittleEndian(gift.AsSpan(4),1);mon.WriteEncryptedDataParty(gift.AsSpan(8,236));return gift;
    }
    public static byte[] Item(int id){if(id is <1 or >467)throw new IOException("This build supports item IDs 1–467.");var gift=new byte[260];BinaryPrimitives.WriteUInt16LittleEndian(gift,3);BinaryPrimitives.WriteInt32LittleEndian(gift.AsSpan(4),id);BinaryPrimitives.WriteInt32LittleEndian(gift.AsSpan(8),1);return gift;}
    static void ValidatePokemon(PK4 mon){if(!mon.ChecksumValid||!((mon.Species>=1&&mon.Species<=493)||(mon.Species>=512&&mon.Species<=576)))throw new IOException("Pokémon checksum/species is unsupported by the verified gift format.");if(new[]{mon.Move1,mon.Move2,mon.Move3,mon.Move4}.Any(m=>m>467))throw new IOException("Pokémon contains an unsupported move.");}
    public static void Validate(byte[] gift)
    {
        if(gift.Length!=260)throw new IOException("Invalid Poké Mart gift size.");int type=BinaryPrimitives.ReadUInt16LittleEndian(gift);if(type==3){_=Item(BinaryPrimitives.ReadInt32LittleEndian(gift.AsSpan(4)));return;}if(type is not (1 or 2))throw new IOException("This delivery adapter accepts Pokémon, eggs, and items.");var mon=new PK4(gift.AsSpan(8,236).ToArray());ValidatePokemon(mon);if(mon.IsEgg!=(type==2))throw new IOException("Egg gift type and Pokémon data disagree.");
    }
    public static (int NormalBase,int BoxBase) Layout(byte[] save)
    {
        if(save.Length!=PortSaveEditor.FileSize)throw new IOException("Expected the verified game's 524,288-byte save.");
        var valid=new List<(int Sector,int Block,uint Generation,uint Counter)>();
        for(int sector=0;sector<2;sector++)for(int block=0;block<2;block++){int length=block==0?PortSaveEditor.NormalSize:PortSaveEditor.BoxSize,start=sector*0x40000+(block==0?0:PortSaveEditor.NormalSize),footer=start+length-20;var f=save.AsSpan(footer);if(BinaryPrimitives.ReadUInt32LittleEndian(f[8..])==length&&BinaryPrimitives.ReadUInt32LittleEndian(f[12..])==0x20060623&&f[16]==block&&BinaryPrimitives.ReadUInt16LittleEndian(f[18..])==PortSaveEditor.Crc(save.AsSpan(start,length-20)))valid.Add((sector,block,BinaryPrimitives.ReadUInt32LittleEndian(f),BinaryPrimitives.ReadUInt32LittleEndian(f[4..])));}
        if(!valid.Any(v=>v.Block==0)||!valid.Any(v=>v.Block==1))throw new IOException("No complete valid save blocks; recover the save in-game first.");
        static bool Newer(uint a,uint b)=>a==0&&b==uint.MaxValue||a>b&&!(a==uint.MaxValue&&b==0);
        var n=valid.First(v=>v.Block==0);var b=valid.First(v=>v.Block==1);foreach(var v in valid){var old=v.Block==0?n:b;if(Newer(v.Generation,old.Generation)||(v.Generation==old.Generation&&Newer(v.Counter,old.Counter))){if(v.Block==0)n=v;else b=v;}}
        if(n.Generation!=b.Generation)throw new IOException("Normal and box save generations differ. Recover in-game before claiming gifts.");return (n.Sector*0x40000,b.Sector*0x40000+PortSaveEditor.NormalSize);
    }
    public static byte[] Build(byte[] save,byte[] gift)
    {
        Validate(gift);var document=Layout(save);int at=document.NormalBase+Offset;
        if(BinaryPrimitives.ReadUInt16LittleEndian(save.AsSpan(at+BlockSize-4))!=PortSaveEditor.Crc(save.AsSpan(at,BlockSize-4)))throw new IOException("Mystery Gift block checksum differs from the verified layout.");
        int free=-1;for(int i=0;i<8;i++){int type=BinaryPrimitives.ReadUInt16LittleEndian(save.AsSpan(at+256+i*260));if(type>13)throw new IOException("Existing gift data is invalid; save preserved.");if(type==0&&free<0)free=i;}
        if(free<0)throw new IOException("The deliveryman already holds 8 gifts. Collect them in-game before adding more.");
        var result=(byte[])save.Clone();gift.CopyTo(result,at+256+free*260);result[at+256+free*260+2]=(byte)((result[at+256+free*260+2]&252)|3);result[at+255]|=128;
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(at+BlockSize-4),PortSaveEditor.Crc(result.AsSpan(at,BlockSize-4)));
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(document.NormalBase+PortSaveEditor.NormalSize-2),PortSaveEditor.Crc(result.AsSpan(document.NormalBase,PortSaveEditor.NormalSize-20)));_=Layout(result);return result;
    }
}
