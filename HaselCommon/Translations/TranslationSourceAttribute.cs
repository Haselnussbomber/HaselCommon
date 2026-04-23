namespace HaselCommon.Translations;

[AttributeUsage(AttributeTargets.Class)]
public class TranslationSourceAttribute(string filename) : Attribute
{
    public string Filename { get; } = filename;
}
