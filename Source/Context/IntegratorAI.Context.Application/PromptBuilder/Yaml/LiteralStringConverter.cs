using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace IntegratorAI.Context.Application.PromptBuilder.Yaml;

internal sealed class LiteralStringConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(LiteralString);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) =>
        throw new NotSupportedException();

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var literal = (LiteralString)value!;
        emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, literal.Value, ScalarStyle.Literal, true, false));
    }
}
