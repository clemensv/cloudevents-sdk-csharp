﻿// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

namespace CloudNative.CloudEvents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Mime;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Formatter that implements the JSON Event Format
    /// </summary>
    public class JsonEventFormatter : ICloudEventFormatter
    {

        public const string MediaTypeSuffix = "+json";

        public CloudEvent DecodeStructuredEvent(Stream data, params ICloudEventExtension[] extensions)
        {
            return DecodeStructuredEvent(data, (IEnumerable<ICloudEventExtension>)extensions);
        }

        public CloudEvent DecodeStructuredEvent(Stream data, IEnumerable<ICloudEventExtension> extensions = null)
        {
            var jsonReader = new JsonTextReader(new StreamReader(data, Encoding.UTF8, true, 8192, true));
            var jObject = JObject.Load(jsonReader);
            return DecodeJObject(jObject, extensions);
        }

        public CloudEvent DecodeStructuredEvent(byte[] data, params ICloudEventExtension[] extensions)
        {
            return DecodeStructuredEvent(data, (IEnumerable<ICloudEventExtension>)extensions);
        }

        public CloudEvent DecodeStructuredEvent(byte[] data, IEnumerable<ICloudEventExtension> extensions = null)
        {
            var jsonText = Encoding.UTF8.GetString(data);
            var jObject = JObject.Parse(jsonText);
            return DecodeJObject(jObject, extensions);
        }

        public CloudEvent DecodeJObject(JObject jObject, IEnumerable<ICloudEventExtension> extensions = null)
        {
            CloudEventsSpecVersion specVersion = CloudEventsSpecVersion.Default;
            if (jObject.ContainsKey(CloudEventAttributes.SpecVersionAttributeName(CloudEventsSpecVersion.V0_1)) ||
                jObject.ContainsKey(CloudEventAttributes.SpecVersionAttributeName(CloudEventsSpecVersion.V0_1).ToLowerInvariant()))
            {
                specVersion = CloudEventsSpecVersion.V0_1;
            }
            if (jObject.ContainsKey(CloudEventAttributes.SpecVersionAttributeName(CloudEventsSpecVersion.V0_2)) ||
                jObject.ContainsKey(CloudEventAttributes.SpecVersionAttributeName(CloudEventsSpecVersion.V0_2).ToLowerInvariant()))
            {
                specVersion =
                    ((string)jObject[CloudEventAttributes.SpecVersionAttributeName(CloudEventsSpecVersion.V0_2)] ==
                     "0.2")
                        ? CloudEventsSpecVersion.V0_2
                        : CloudEventsSpecVersion.Default;
            }
            var cloudEvent = new CloudEvent(specVersion, extensions);
            var attributes = cloudEvent.GetAttributes();
            foreach (var keyValuePair in jObject)
            {
                if (keyValuePair.Key.Equals(
                        CloudEventAttributes.SpecVersionAttributeName(CloudEventsSpecVersion.V0_1), StringComparison.InvariantCultureIgnoreCase) ||
                    keyValuePair.Key.Equals(CloudEventAttributes.SpecVersionAttributeName(CloudEventsSpecVersion.V0_2), StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                switch (keyValuePair.Value.Type)
                {
                    case JTokenType.String:
                        attributes[keyValuePair.Key] = keyValuePair.Value.ToObject<string>();
                        break;
                    case JTokenType.Date:
                        attributes[keyValuePair.Key] = keyValuePair.Value.ToObject<DateTime>();
                        break;
                    case JTokenType.Uri:
                        attributes[keyValuePair.Key] = keyValuePair.Value.ToObject<Uri>();
                        break;
                    case JTokenType.Null:
                        attributes[keyValuePair.Key] = null;
                        break;
                    case JTokenType.Integer:
                        attributes[keyValuePair.Key] = keyValuePair.Value.ToObject<int>();
                        break;
                    default:
                        attributes[keyValuePair.Key] = (dynamic)keyValuePair.Value;
                        break;
                }
            }

            return cloudEvent;
        }

        public byte[] EncodeStructuredEvent(CloudEvent cloudEvent, out ContentType contentType)
        {
            contentType = new ContentType(CloudEvent.MediaType + "+json")
            {
                CharSet = Encoding.UTF8.WebName
            };

            JObject jObject = new JObject();
            var attributes = cloudEvent.GetAttributes();
            foreach (var keyValuePair in attributes)
            {
                if (keyValuePair.Value is ContentType)
                {
                    jObject[keyValuePair.Key] = JToken.FromObject(((ContentType)keyValuePair.Value).ToString());
                }
                else
                {
                    jObject[keyValuePair.Key] = JToken.FromObject(keyValuePair.Value);
                }
            }

            return Encoding.UTF8.GetBytes(jObject.ToString());
        }

        public CloudEventBatch DecodeStructuredEventBatch(Stream data, IEnumerable<ICloudEventExtension> extensions)
        {
            var batch = new CloudEventBatch();
            var jsonReader = new JsonTextReader(new StreamReader(data, Encoding.UTF8, true, 8192, true));
            var jArray = JArray.Load(jsonReader);
            foreach (var element in jArray)
            {
                batch.Add(DecodeJObject(element as JObject, extensions));
            }                                                            
            return batch;
        }

        public CloudEventBatch DecodeStructuredEventBatch(byte[] data, IEnumerable<ICloudEventExtension> extensions)
        {
            var batch = new CloudEventBatch();
            var jsonText = Encoding.UTF8.GetString(data);
            var jArray = JArray.Parse(jsonText);
            foreach (var element in jArray)
            {
                batch.Add(DecodeJObject(element as JObject, extensions));
            }
            return batch;
        }

        public byte[] EncodeStructuredEventBatch(CloudEventBatch cloudEventBatch, out ContentType contentType)
        {
            contentType = new ContentType(CloudEventBatch.MediaType + "+json")
            {
                CharSet = Encoding.UTF8.WebName
            };

            JArray jArray = new JArray();
            foreach (var cloudEvent in cloudEventBatch)
            {
                JObject jObject = new JObject();
                var attributes = cloudEvent.GetAttributes();
                foreach (var keyValuePair in attributes)
                {
                    if (keyValuePair.Value is ContentType)
                    {
                        jObject[keyValuePair.Key] = JToken.FromObject(((ContentType)keyValuePair.Value).ToString());
                    }
                    else
                    {
                        jObject[keyValuePair.Key] = JToken.FromObject(keyValuePair.Value);
                    }
                }
                jArray.Add(jObject);
            }

            return Encoding.UTF8.GetBytes(jArray.ToString());
        }

        public object DecodeAttribute(CloudEventsSpecVersion specVersion, string name, byte[] data, IEnumerable<ICloudEventExtension> extensions = null)
        {
            if (name.Equals(CloudEventAttributes.IdAttributeName(specVersion)) ||
                name.Equals(CloudEventAttributes.TypeAttributeName(specVersion)))
            {
                return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), typeof(string));
            }

            if (name.Equals(CloudEventAttributes.TimeAttributeName(specVersion)))
            {
                return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), typeof(DateTime));
            }

            if (name.Equals(CloudEventAttributes.SourceAttributeName(specVersion)) ||
                name.Equals(CloudEventAttributes.SchemaUrlAttributeName(specVersion)))
            {
                var uri = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), typeof(string)) as string;
                return new Uri(uri);
            }

            if (name.Equals(CloudEventAttributes.ContentTypeAttributeName(specVersion)))
            {
                var s = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), typeof(string)) as string;
                return new ContentType(s);
            }

            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    Type type = extension.GetAttributeType(name);
                    if (type != null)
                    {
                        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type);
                    }
                }
            }
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data));
        }

        public byte[] EncodeAttribute(CloudEventsSpecVersion specVersion, string name, object value, IEnumerable<ICloudEventExtension> extensions = null)
        {
            if (name.Equals(CloudEventAttributes.DataAttributeName(specVersion)))
            {
                if (value is Stream)
                {
                    using (var buffer = new MemoryStream())
                    {
                        ((Stream)value).CopyTo(buffer);
                        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(buffer.ToArray()));
                    }
                }
            }

            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    Type type = extension.GetAttributeType(name);
                    if (type != null)
                    {
                        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Convert.ChangeType(value, type)));
                    }
                }
            }

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
        }
    }
}