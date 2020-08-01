using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace PassManager
{
	class VaultConverter : JsonConverter<Credential>
	{
		public override Credential Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

 
		public override void Write(Utf8JsonWriter w, Credential v, JsonSerializerOptions options)
		{
			w.WriteStartObject();
			w.WritePropertyName("Id");
			w.WriteStringValue(v.Id.ToString());
			w.WriteEndObject();
		}
 
	}
}
