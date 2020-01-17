// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

namespace CloudNative.CloudEvents.NATS
{
    using global::NATS.Client;

    public static class NATSClientExtensions
    {
        static JsonEventFormatter jsonEventFormatter = new JsonEventFormatter();

        public static CloudEvent ToCloudEvent(this Msg message, params ICloudEventExtension[] extensions)
        {
            return jsonEventFormatter.DecodeStructuredEvent(message.Data, extensions);
        }
    }
}