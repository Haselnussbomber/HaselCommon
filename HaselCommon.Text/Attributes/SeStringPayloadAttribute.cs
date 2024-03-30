namespace HaselCommon.Text.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SeStringPayloadAttribute(MacroCodes Code) : Attribute
{
    public MacroCodes Code { get; } = Code;
}
