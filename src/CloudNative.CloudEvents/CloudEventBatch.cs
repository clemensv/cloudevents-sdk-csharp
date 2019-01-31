// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

namespace CloudNative.CloudEvents
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class CloudEventBatch : Collection<CloudEvent>
    {
        public const string MediaType = "application/cloudevents-batch";

        public CloudEventBatch()
        {
        }

        public CloudEventBatch(IList<CloudEvent> cloudEvents) : base(cloudEvents)
        {
        }
    }
}