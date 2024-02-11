using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.STD;

namespace HaselCommon.Structs;

// If you're looking at gnum68, for example, you need to subtract 1 because this list/the StdDeque is 0-based.

// |-------|---------------|----------------------------------------------------------|
// | Index | Type          | Label                                                    |
// |-------|---------------|----------------------------------------------------------|
// |     0 | Utf8StringPtr | Player Name                                              |
// |     1 | BytePtr       | Player Name                                              |
// |     3 | Integer       | Player Sex                                               |
// |    10 | Integer       | Eorzea Time Hours                                        |
// |    11 | Integer       | Eorzea Time Minutes                                      |
// |    12 | Integer       | Log Text Colors - Chat 1 - Say                           |
// |    13 | Integer       | Log Text Colors - Chat 1 - Shout                         |
// |    14 | Integer       | Log Text Colors - Chat 1 - Tell                          |
// |    15 | Integer       | Log Text Colors - Chat 1 - Party                         |
// |    16 | Integer       | Log Text Colors - Chat 1 - Alliance                      |
// | 17-24 | Integer       | Log Text Colors - Chat 2 - LS1-8                         |
// |    25 | Integer       | Log Text Colors - Chat 2 - Free Company                  |
// |    26 | Integer       | Log Text Colors - Chat 2 - PvP Team                      |
// | 29,30 | Integer       | Log Text Colors - Chat 1 - Emotes                        |
// |    31 | Integer       | Log Text Colors - Chat 1 - Yell                          |
// |    34 | Integer       | Log Text Colors - Chat 2 - CWLS1                         |
// |    27 | Integer       | Log Text Colors - General - PvP Team Announcements       |
// |    28 | Integer       | Log Text Colors - Chat 2 - Novice Network                |
// |    32 | Integer       | Log Text Colors - General - Free Company Announcements   |
// |    33 | Integer       | Log Text Colors - General - Novice Network Announcements |
// |    35 | Integer       | Log Text Colors - Battle - Damage Dealt                  |
// |    36 | Integer       | Log Text Colors - Battle - Missed Attacks                |
// |    37 | Integer       | Log Text Colors - Battle - Actions                       |
// |    38 | Integer       | Log Text Colors - Battle - Items                         |
// |    39 | Integer       | Log Text Colors - Battle - Healing                       |
// |    40 | Integer       | Log Text Colors - Battle - Enchanting Effects            |
// |    41 | Integer       | Log Text Colors - Battle - Enfeebing Effects             |
// |    42 | Integer       | Log Text Colors - General - Echo                         |
// |    43 | Integer       | Log Text Colors - General - System Messages              |
// |    54 | Utf8StringPtr | Companion Name                                           |
// |    56 | Integer       | Log Text Colors - General - Battle System Messages       |
// |    57 | Integer       | Log Text Colors - General - Gathering System Messages    |
// |    58 | Integer       | Log Text Colors - General - Error Messages               |
// |    59 | Integer       | Log Text Colors - General - NPC Dialogue                 |
// |    60 | Integer       | Log Text Colors - General - Item Drops                   |
// |    61 | Integer       | Log Text Colors - General - Level Up                     |
// |    62 | Integer       | Log Text Colors - General - Loot                         |
// |    63 | Integer       | Log Text Colors - General - Synthesis                    |
// |    64 | Integer       | Log Text Colors - General - Gathering                    |
// |    67 | Integer       | Player ClassJobId                                        |
// |    68 | Integer       | Player Level                                             |
// |    70 | Integer       | Player Race                                              |
// |    77 | Integer       | Client/Plattform?                                        |
// |-------|---------------|----------------------------------------------------------|

[StructLayout(LayoutKind.Explicit, Size = 0x20)]
public unsafe struct TextParameter
{
    [FieldOffset(0)] public int IntValue;
    [FieldOffset(0)] public byte* BytePtrValue;
    [FieldOffset(0)] public Utf8String* Utf8StringValue;
    [FieldOffset(0x8)] public void* ValuePtr; // a pointer to the value
    [FieldOffset(0x10)] public TextParameterType Type;

    public static TextParameter Get(ulong index)
    {
        var globalDeque = (StdDeque<TextParameter>*)((nint)RaptureTextModule.Instance() + 0x40);
        return globalDeque->Get(index);
    }
}

public enum TextParameterType : sbyte
{
    Uninitialized = -1,
    Integer = 0,
    Utf8StringPtr = 1,
    BytePtr = 2
}

