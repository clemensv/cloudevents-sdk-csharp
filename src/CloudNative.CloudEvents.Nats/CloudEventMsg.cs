// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

using NATS.Client;

namespace CloudNative.CloudEvents.NATS
{
    public class CloudEventMsg 
    {
        Msg message;
        static JsonEventFormatter jsonEventFormatter = new JsonEventFormatter();

        public CloudEventMsg(string subject, string reply, CloudEvent cloudEvent)
        {
            message = new Msg(subject, reply, jsonEventFormatter.EncodeStructuredEvent(cloudEvent, out var contentType));
        }

        public CloudEventMsg(string subject, CloudEvent cloudEvent)
        {

            message = new Msg(subject, jsonEventFormatter.EncodeStructuredEvent(cloudEvent, out var contentType));
        }

        public static implicit operator Msg(CloudEventMsg ce)
        {
            return ce.message;
        }
    }
}
