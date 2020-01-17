// Copyright 2015-2018 The NATS Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CloudNative.CloudEvents.NATS;
using IntegrationTests;
using NATS.Client;
using System.Text;
using Xunit;

namespace CloudNative.CloudEvents.IntegrationTests.NATS
{
    /// <summary>
    /// Run these tests with the gnatsd auth.conf configuration file.
    /// </summary>
    [Collection(DefaultSuiteContext.CollectionKey)]
    public class NATSTest : TestSuite<DefaultSuiteContext>
    {
        const string jsonv10 =
           "{\n" +
           "    \"specversion\" : \"1.0\",\n" +
           "    \"type\" : \"com.github.pull.create\",\n" +
           "    \"source\" : \"https://github.com/cloudevents/spec/pull/123\",\n" +
           "    \"id\" : \"A234-1234-1234\",\n" +
           "    \"time\" : \"2018-04-05T17:31:00Z\",\n" +
           "    \"comexampleextension1\" : \"value\",\n" +
           "    \"datacontenttype\" : \"text/xml\",\n" +
           "    \"data\" : \"<much wow=\\\"xml\\\"/>\"\n" +
           "}";

        public NATSTest(DefaultSuiteContext context) : base(context) { }


        [Fact]
        public void TestPubSub()
        {
            using (NATSServer.CreateFastAndVerify())
            {
                using (IConnection c = Context.OpenConnection())
                {
                    using (ISyncSubscription s = c.SubscribeSync("foo"))
                    {
                        JsonEventFormatter formatter = new JsonEventFormatter();
                        var cloudEvent = formatter.DecodeStructuredEvent(Encoding.UTF8.GetBytes(jsonv10));
                    
                        c.Publish(new CloudEventMsg("foo", cloudEvent));
                        CloudEvent cloudEvent2 = s.NextMessage(1000)?.ToCloudEvent();

                        Assert.Equal(cloudEvent2.SpecVersion, cloudEvent.SpecVersion);
                        Assert.Equal(cloudEvent2.Type, cloudEvent.Type);
                        Assert.Equal(cloudEvent2.Source, cloudEvent.Source);
                        Assert.Equal(cloudEvent2.Id, cloudEvent.Id);
                        Assert.Equal(cloudEvent2.Time.Value.ToUniversalTime(), cloudEvent.Time.Value.ToUniversalTime());
                        Assert.Equal(cloudEvent2.DataContentType, cloudEvent.DataContentType);
                        Assert.Equal(cloudEvent2.Data, cloudEvent.Data);
                    }
                }
            }
        }              
    } // class
} // namespace