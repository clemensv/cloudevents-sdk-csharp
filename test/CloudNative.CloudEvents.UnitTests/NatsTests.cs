// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

namespace CloudNative.CloudEvents.UnitTests
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Xunit;
    using CloudNative.CloudEvents;
    using CloudNative.CloudEvents.NATS;
    using global::NATS.Client;
    
    public class NatsTest : IDisposable
    {
        public NatsTest()
        {
            
        }

        public void Dispose()
        {
            
        }

        // docker run -p 4222:4222 -ti nats:latest

        [Fact]
        public async Task NatsSendTest()
        {

            var jsonEventFormatter = new JsonEventFormatter();
            var cloudEvent = new CloudEvent("com.github.pull.create",
                new Uri("https://github.com/cloudevents/spec/pull/123"))
            {
                Id = "A234-1234-1234",
                Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                DataContentType = new ContentType(MediaTypeNames.Text.Xml),
                Data = "<much wow=\"xml\"/>"
            };

            var attrs = cloudEvent.GetAttributes();
            attrs["comexampleextension1"] = "value";


            // Create a new connection factory to create
            // a connection.
            ConnectionFactory cf = new ConnectionFactory();

            // Creates a live connection to the default
            // NATS Server running locally
            IConnection c = cf.CreateConnection();



            //TaskCompletionSource<CloudEvent> tcs = new TaskCompletionSource<CloudEvent>();
            //await client.ConnectAsync(options);
            //client.ApplicationMessageReceived += (sender, args) =>
            //    {
            //        tcs.SetResult(args.ApplicationMessage.ToCloudEvent(jsonEventFormatter));
            //    };

            //var result = await client.SubscribeAsync("abc");
            //await client.PublishAsync(new NatsCloudEventMessage(cloudEvent, new JsonEventFormatter()) { Topic = "abc" });
            //var receivedCloudEvent = await tcs.Task;

            //Assert.Equal(CloudEventsSpecVersion.Default, receivedCloudEvent.SpecVersion);
            //Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
            //Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"), receivedCloudEvent.Source);
            //Assert.Equal("A234-1234-1234", receivedCloudEvent.Id);
            //Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
            //    receivedCloudEvent.Time.Value.ToUniversalTime());
            //Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.DataContentType);
            //Assert.Equal("<much wow=\"xml\"/>", receivedCloudEvent.Data);

            //var attr = receivedCloudEvent.GetAttributes();
            //Assert.Equal("value", (string)attr["comexampleextension1"]);



        }

    }
}