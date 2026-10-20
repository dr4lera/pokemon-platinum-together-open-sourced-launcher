namespace PlatinumTrainer;
internal static class GameBuild
{
    public const string Hash015="27075FCEB04B5A3140E707BAE1B12D69E04824B86E960121F566AD5418728488";
    public const string Hash016="5520B7D176A3598B67F3C126359DEADA0F4099A6F961143453D7D790D69AB6C2";
    public static bool Supported(string hash)=>hash==Hash015||hash==Hash016;
    public static int Rva016(int old)=>old switch
    {
        0x54947 => 0x54947,
        0xd8d200 => 0xdaf200,
        0xd92d20 => 0xdb4d20,
        0xd99800 => 0xdbb800,
        0xdb5200 => 0xdd7200,
        0xdb52c0 => 0xdd72c0,
        0x11fbbe0 => 0x122fbe0,
        0x11fbbf0 => 0x122fbf0,
        0x11fbbf4 => 0x122fbf4,
        0x11fbbf8 => 0x122fbf8,
        0x11fbc00 => 0x122fc00,
        0x12083e0 => 0x123c3e0,
        0x1217160 => 0x124b160,
        0x1448000 => 0x147c000,
        0x155fdc0 => 0x1594920,
        _=>throw new System.IO.IOException("Unverified 0.1.6 memory reference.")
    };
}
