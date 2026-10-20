using System.Buffers.Binary;
using System.Text;

namespace PlatinumPrivateLauncher;
static class RomChecks
{
    internal static void Run(){
        var bytes=new byte[564];
        void U32(int at,uint value)=>BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(at,4),value);
        U32(64,512);U32(68,24);U32(72,544);U32(76,8);U32(512,8);bytes[518]=1;
        bytes[520]=8;Encoding.ASCII.GetBytes("test.bin").CopyTo(bytes,521);U32(544,560);U32(548,564);bytes[560]=42;
        var entries=RomImport.Read(bytes);if(entries.Count!=2||entries[1]!=new RomImport.Entry("test.bin",560,4))throw new Exception("ROM filesystem roundtrip failed");
        U32(548,600);Reject(bytes);U32(548,564);Encoding.ASCII.GetBytes("../x.bin").CopyTo(bytes,521);Reject(bytes);
        static void Reject(byte[] input){try{RomImport.Read(input);}catch(InvalidDataException){return;}throw new Exception("Unsafe ROM accepted");}
    }
}
