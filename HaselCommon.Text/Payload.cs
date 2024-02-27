using System.Linq;
using System.Reflection;
using Lumina.Extensions;

namespace HaselCommon.Text;

public abstract class Payload
{
    protected const byte START_BYTE = 2;
    protected const byte END_BYTE = 3;

    private static readonly Lazy<Dictionary<MacroCodes, Type>> MacroTypes = new(() =>
    {
        var macroTypes = new Dictionary<MacroCodes, Type>();

        var customPayloadTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Payload)));

        foreach (var type in customPayloadTypes)
        {
            var attr = type.GetCustomAttribute<SeStringPayloadAttribute>();
            if (attr != null)
                macroTypes[attr.Code] = type;
            else if (type != typeof(RawPayload) && type != typeof(TextPayload))
                Service.PluginLog.Warning($"{type.FullName} is missing SeStringPayloadAttribute");
        }

        return macroTypes;
    });

    public static Payload From(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        return From(br);
    }

    public static Payload From(BinaryReader reader)
    {
        Payload payload;

        var initialByte = reader.PeekByte();
        if (initialByte == START_BYTE)
        {
            reader.GetPayloadInfo(out var macroCode, out var dataSizeLength, out var dataSize);
            var payloadLength = 1 + 1 + dataSizeLength + dataSize + 1;

            var startPos = reader.BaseStream.Position;

            using var payloadData = reader.BaseStream.ReadSlice(payloadLength);
            using var dataReader = new BinaryReader(payloadData);

            if (MacroTypes.Value.TryGetValue(macroCode, out var type))
            {
                if (macroCode == MacroCodes.Link)
                {
                    reader.BaseStream.Position += 1 + 1 + dataSizeLength;
                    var linkType = (LinkType)IntegerExpression.Parse(reader.ReadByte(), reader.BaseStream).Value;
                    reader.BaseStream.Position = startPos;
                    type = linkType switch
                    {
                        LinkType.Player => typeof(PlayerLinkPayload), // 0x00

                        LinkType.Item => typeof(ItemLinkPayload), // 0x02
                        LinkType.MapPosition => typeof(MapPositionLinkPayload), // 0x03
                        LinkType.Quest => typeof(QuestLinkPayload), // 0x04
                        LinkType.Achievement => typeof(AchievementLinkPayload), // 0x05
                        LinkType.HowTo => typeof(HowToLinkPayload), // 0x06
                        LinkType.PartyFinderNotification => typeof(PartyFinderNotificationLinkPayload), // 0x07
                        LinkType.Status => typeof(StatusLinkPayload), // 0x08
                        LinkType.PartyFinder => typeof(PartyFinderLinkPayload), // 0x09
                        LinkType.AkatsukiNote => typeof(AkatsukiNoteLinkPayload), // 0x0A

                        LinkType.Dalamud => typeof(DalamudLinkPayload), // 0x0E

                        LinkType.LinkTerminator => typeof(LinkTerminatorPayload), // 0x0E

                        _ => type // fallback: LinkPayload
                    };
                }

                payload = (Payload)Activator.CreateInstance(type)!;
            }
            else
            {
                Service.PluginLog.Warning($"Unhandled MacroCode 0x{(byte)macroCode:X02}");
                payload = new RawPayload();
            }

            payload.Decode(dataReader);

            reader.BaseStream.Position = startPos + payloadLength;
        }
        else
        {
            payload = new TextPayload();
            payload.Decode(reader);
        }

        return payload;
    }

    public void Decode(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        Decode(br);
    }

    public abstract byte[] Encode();
    public abstract void Decode(BinaryReader reader);
    public abstract SeString Resolve(List<Expression>? localParameters = null);

    public static explicit operator Payload(byte[] data) => From(data);
}
