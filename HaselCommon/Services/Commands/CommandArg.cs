namespace HaselCommon.Services.Commands;

public record struct CommandArg(string Name, Type Type, bool Required, bool ConsumeRest);
