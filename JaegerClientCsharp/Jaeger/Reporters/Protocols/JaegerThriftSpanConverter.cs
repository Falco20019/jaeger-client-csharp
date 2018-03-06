using System;
using System.Collections.Generic;
using System.Linq;
using Jaeger.Thrift;
using OpenTracing;

namespace Jaeger.Reporters.Protocols
{
    public class JaegerThriftSpanConverter
    {
        private JaegerThriftSpanConverter()
        {
        }

        public static Thrift.Span convertSpan(Span span)
        {
            SpanContext context = span.Context;

            bool oneChildOfParent = span.References.FirstOrDefault()?.ReferenceType == References.ChildOf;

            return new Thrift.Span(
                context.TraceId,
                0, // TraceIdHigh is currently not supported
                context.SpanId,
                oneChildOfParent ? context.ParentId : 0,
                span.OperationName,
                (int)context.Flags,
                span.StartTimestamp.ToUnixTimeMilliseconds() * 1000,
                (long)(span.Duration.TotalMilliseconds * 1000))
            {
                References = oneChildOfParent ? new List<SpanRef>() : BuildReferences(span.References),
                Tags = BuildTags(span.Tags),
                Logs = BuildLogs(span.LogEntries)
            };
        }

        static List<SpanRef> BuildReferences(IList<Span.Reference> references)
        {
            return references.Select(reference =>
            {
                SpanRefType thriftRefType = References.ChildOf.Equals(reference.ReferenceType) ? SpanRefType.CHILD_OF : SpanRefType.FOLLOWS_FROM;
                return new SpanRef(thriftRefType, reference.Context.TraceId, 0, reference.Context.SpanId);
            }).ToList();
        }

        static List<Log> BuildLogs(IList<Span.LogEntry> logs)
        {
            return logs.Select(logData => new Log
            {
                Timestamp = logData.Timestamp.ToUnixTimeMilliseconds() * 1000,
                Fields = BuildTags(logData.Fields)
            }).ToList();
        }

        public static List<Tag> BuildTags(IReadOnlyDictionary<string, object> tags)
        {
            return tags.Select(entry => BuildTag(entry.Key, entry.Value)).ToList();
        }

        private static Tag BuildTag(String tagKey, Object tagValue)
        {
            Tag tag = new Tag
            {
                Key = tagKey
            };
            if (tagValue is int || tagValue is short || tagValue is long)
            {
                tag.VType = TagType.LONG;
                tag.VLong = Convert.ToInt64(tagValue);
            }
            else if (tagValue is double || tagValue is float)
            {
                tag.VType = TagType.DOUBLE;
                tag.VDouble = Convert.ToDouble(tagValue);
            }
            else if (tagValue is bool) {
                tag.VType = TagType.BOOL;
                tag.VBool = Convert.ToBoolean(tagValue);
            } else
            {
                tag.VType = TagType.STRING;
                tag.VStr = tagValue.ToString();
            }
            return tag;
        }
    }
}
