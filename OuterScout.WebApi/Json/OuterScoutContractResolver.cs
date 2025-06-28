using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OuterScout.Shared.Extensions;

namespace OuterScout.WebApi.Json;

internal sealed class OuterScoutContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.IsRequiredSpecified is false && member.IsRequired())
        {
            bool isNullable = member switch
            {
                PropertyInfo p => p.IsNullable(),
                FieldInfo f => f.IsNullable(),
                _ => false,
            };

            property.Required = isNullable ? Required.AllowNull : Required.Always;
        }

        return property;
    }
}
