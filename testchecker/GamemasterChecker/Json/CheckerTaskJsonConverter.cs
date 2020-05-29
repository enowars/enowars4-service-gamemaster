using EnoCore.Models;
using EnoCore.Models.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnoCore.Json
{
    public class CheckerResultMessageJsonConverter : JsonConverter<CheckerResultMessage>
    {
        private static readonly byte[] InternalErrorBytes = Encoding.ASCII.GetBytes("INTERNAL_ERROR");
        private static readonly byte[] OkBytes = Encoding.ASCII.GetBytes("OK");
        private static readonly byte[] MumbleBytes = Encoding.ASCII.GetBytes("MUMBLE");
        private static readonly byte[] OfflineBytes = Encoding.ASCII.GetBytes("OFFLINE");
        public override CheckerResultMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.ValueSpan.SequenceEqual(InternalErrorBytes))
            {
                return CheckerResultMessage.InternalError;
            }
            else if (reader.ValueSpan.SequenceEqual(OkBytes))
            {
                return CheckerResultMessage.Ok;
            }
            else if (reader.ValueSpan.SequenceEqual(MumbleBytes))
            {
                return CheckerResultMessage.Mumble;
            }
            else if (reader.ValueSpan.SequenceEqual(OfflineBytes))
            {
                return CheckerResultMessage.Offline;
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, CheckerResultMessage value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}