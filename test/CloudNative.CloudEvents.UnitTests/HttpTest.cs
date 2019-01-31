// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

namespace CloudNative.CloudEvents.UnitTests
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Xunit;

    public class HttpTest : IDisposable
    {
        const string listenerAddress = "http://localhost:52671/";

        const string testContextHeader = "testcontext";

        HttpListener listener;

        ConcurrentDictionary<string, Func<HttpListenerContext, Task>> pendingRequests =
            new ConcurrentDictionary<string, Func<HttpListenerContext, Task>>();

        public HttpTest()
        {
            listener = new HttpListener()
            {
                AuthenticationSchemes = AuthenticationSchemes.Anonymous,
                Prefixes = { listenerAddress }
            };
            listener.Start();
            listener.GetContextAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    HandleContext(t.Result);
                }
            });
        }

        public void Dispose()
        {
            listener.Stop();
        }

        async Task HandleContext(HttpListenerContext requestContext)
        {
            var ctxHeaderValue = requestContext.Request.Headers[testContextHeader];

            if (requestContext.Request.IsWebHookValidationRequest())
            {
                await requestContext.HandleAsWebHookValidationRequest(null, null);
                return;
            }

            if (pendingRequests.TryRemove(ctxHeaderValue, out var pending))
            {
                await pending(requestContext);
            }
#pragma warning disable 4014
            listener.GetContextAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    HandleContext(t.Result);
                }
            });
#pragma warning restore 4014
        }

        [Fact]
        async Task HttpWebHookValidation()
        {
            var httpClient = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Options, new Uri(listenerAddress + "ep"));
            req.Headers.Add("WebHook-Request-Origin", "example.com");
            req.Headers.Add("WebHook-Request-Rate", "120");
            var result = await httpClient.SendAsync(req);
            Assert.Equal("example.com", result.Headers.GetValues("WebHook-Allowed-Origin").First());
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        async Task HttpBinaryClientReceiveTest()
        {
            string ctx = Guid.NewGuid().ToString();
            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    var cloudEvent = new CloudEvent("com.github.pull.create",
                        new Uri("https://github.com/cloudevents/spec/pull/123"))
                    {
                        Id = "A234-1234-1234",
                        Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                        ContentType = new ContentType(MediaTypeNames.Text.Xml),
                        Data = "<much wow=\"xml\"/>"
                    };

                    var attrs = cloudEvent.GetAttributes();
                    attrs["comexampleextension1"] = "value";
                    attrs["comexampleextension2"] = new { othervalue = 5 };

                    await context.Response.CopyFromAsync(cloudEvent, ContentMode.Binary, new JsonEventFormatter());
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(testContextHeader, ctx);
            var result = await httpClient.GetAsync(new Uri(listenerAddress + "ep"));

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var receivedCloudEvent = result.ToCloudEvent();

            Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
            Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
            Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"), receivedCloudEvent.Source);
            Assert.Equal("A234-1234-1234", receivedCloudEvent.Id);
            Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                receivedCloudEvent.Time.Value.ToUniversalTime());
            Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);
            using (var sr = new StreamReader((Stream)receivedCloudEvent.Data))
            {
                Assert.Equal("<much wow=\"xml\"/>", sr.ReadToEnd());
            }

            var attr = receivedCloudEvent.GetAttributes();
            Assert.Equal("value", (string)attr["comexampleextension1"]);
            Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
        }

        [Fact]
        async Task HttpBinaryClientSendTest()
        {
            var cloudEvent = new CloudEvent("com.github.pull.create",
                new Uri("https://github.com/cloudevents/spec/pull/123"))
            {
                Id = "A234-1234-1234",
                Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                ContentType = new ContentType(MediaTypeNames.Text.Xml),
                Data = "<much wow=\"xml\"/>"
            };

            var attrs = cloudEvent.GetAttributes();
            attrs["comexampleextension1"] = "value";
            attrs["comexampleextension2"] = new { othervalue = 5 };

            string ctx = Guid.NewGuid().ToString();
            var content = new CloudEventContent(cloudEvent, ContentMode.Binary, new JsonEventFormatter());
            content.Headers.Add(testContextHeader, ctx);

            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    Assert.True(context.Request.IsCloudEvent());

                    var receivedCloudEvent = context.Request.ToCloudEvent(new JsonEventFormatter());

                    Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
                    Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
                    Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"), receivedCloudEvent.Source);
                    Assert.Equal("A234-1234-1234", receivedCloudEvent.Id);
                    Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                        receivedCloudEvent.Time.Value.ToUniversalTime());
                    Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);

                    using (var sr = new StreamReader((Stream)receivedCloudEvent.Data))
                    {
                        Assert.Equal("<much wow=\"xml\"/>", sr.ReadToEnd());
                    }

                    var attr = receivedCloudEvent.GetAttributes();
                    Assert.Equal("value", (string)attr["comexampleextension1"]);
                    Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var httpClient = new HttpClient();
            var result = (await httpClient.PostAsync(new Uri(listenerAddress + "ep"), content));
            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }
        }

        [Fact]
        async Task HttpStructuredClientReceiveTest()
        {
            string ctx = Guid.NewGuid().ToString();
            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    var cloudEvent = new CloudEvent("com.github.pull.create",
                        new Uri("https://github.com/cloudevents/spec/pull/123"))
                    {
                        Id = "A234-1234-1234",
                        Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                        ContentType = new ContentType(MediaTypeNames.Text.Xml),
                        Data = "<much wow=\"xml\"/>"
                    };

                    var attrs = cloudEvent.GetAttributes();
                    attrs["comexampleextension1"] = "value";
                    attrs["comexampleextension2"] = new { othervalue = 5 };

                    await context.Response.CopyFromAsync(cloudEvent, ContentMode.Structured, new JsonEventFormatter());
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(testContextHeader, ctx);
            var result = await httpClient.GetAsync(new Uri(listenerAddress + "ep"));

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.IsCloudEvent());
            var receivedCloudEvent = result.ToCloudEvent();

            Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
            Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
            Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"), receivedCloudEvent.Source);
            Assert.Equal("A234-1234-1234", receivedCloudEvent.Id);
            Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                receivedCloudEvent.Time.Value.ToUniversalTime());
            Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);
            Assert.Equal("<much wow=\"xml\"/>", receivedCloudEvent.Data);

            var attr = receivedCloudEvent.GetAttributes();
            Assert.Equal("value", (string)attr["comexampleextension1"]);
            Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
        }

        [Fact]
        async Task HttpStructuredClientSendTest()
        {
            var cloudEvent = new CloudEvent("com.github.pull.create",
                new Uri("https://github.com/cloudevents/spec/pull/123"))
            {
                Id = "A234-1234-1234",
                Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                ContentType = new ContentType(MediaTypeNames.Text.Xml),
                Data = "<much wow=\"xml\"/>"
            };

            var attrs = cloudEvent.GetAttributes();
            attrs["comexampleextension1"] = "value";
            attrs["comexampleextension2"] = new { othervalue = 5 };

            string ctx = Guid.NewGuid().ToString();
            var content = new CloudEventContent(cloudEvent, ContentMode.Structured, new JsonEventFormatter());
            content.Headers.Add(testContextHeader, ctx);

            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    var receivedCloudEvent = context.Request.ToCloudEvent(new JsonEventFormatter());

                    Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
                    Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
                    Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"), receivedCloudEvent.Source);
                    Assert.Equal("A234-1234-1234", receivedCloudEvent.Id);
                    Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                        receivedCloudEvent.Time.Value.ToUniversalTime());
                    Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);
                    Assert.Equal("<much wow=\"xml\"/>", receivedCloudEvent.Data);

                    var attr = receivedCloudEvent.GetAttributes();
                    Assert.Equal("value", (string)attr["comexampleextension1"]);
                    Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var httpClient = new HttpClient();
            var result = (await httpClient.PostAsync(new Uri(listenerAddress + "ep"), content));
            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }
        }

        [Fact]
        async Task HttpStructuredWebRequestSendTest()
        {
            var cloudEvent = new CloudEvent("com.github.pull.create",
                new Uri("https://github.com/cloudevents/spec/pull/123"))
            {
                Id = "A234-1234-1234",
                Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                ContentType = new ContentType(MediaTypeNames.Text.Xml),
                Data = "<much wow=\"xml\"/>"
            };

            var attrs = cloudEvent.GetAttributes();
            attrs["comexampleextension1"] = "value";
            attrs["comexampleextension2"] = new { othervalue = 5 };

            string ctx = Guid.NewGuid().ToString();
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(listenerAddress + "ep");
            httpWebRequest.Method = "POST";
            await httpWebRequest.CopyFromAsync(cloudEvent, ContentMode.Structured, new JsonEventFormatter());
            httpWebRequest.Headers.Add(testContextHeader, ctx);

            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    var receivedCloudEvent = context.Request.ToCloudEvent(new JsonEventFormatter());

                    Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
                    Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
                    Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"), receivedCloudEvent.Source);
                    Assert.Equal("A234-1234-1234", receivedCloudEvent.Id);
                    Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                        receivedCloudEvent.Time.Value.ToUniversalTime());
                    Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);
                    Assert.Equal("<much wow=\"xml\"/>", receivedCloudEvent.Data);

                    var attr = receivedCloudEvent.GetAttributes();
                    Assert.Equal("value", (string)attr["comexampleextension1"]);
                    Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var result = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(result.StatusCode.ToString());
            }
        }



        [Fact]
        async Task HttpBatchClientReceiveTest()
        {
            string ctx = Guid.NewGuid().ToString();
            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    CloudEventBatch batch = new CloudEventBatch();
                    for (int i = 0; i < 10; i++)
                    {

                        var cloudEvent = new CloudEvent("com.github.pull.create",
                            new Uri("https://github.com/cloudevents/spec/pull/123"))
                        {
                            Id = "A234-1234-1234",
                            Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                            ContentType = new ContentType(MediaTypeNames.Text.Xml),
                            Data = "<much wow=\"xml\"/>"
                        };

                        var attrs = cloudEvent.GetAttributes();
                        attrs["comexampleextension1"] = "value";
                        attrs["comexampleextension2"] = new { othervalue = 5 };
                        batch.Add(cloudEvent);
                    }

                    await context.Response.CopyFromAsync(batch, new JsonEventFormatter());
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(testContextHeader, ctx);
            var result = await httpClient.GetAsync(new Uri(listenerAddress + "ep"));

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.IsCloudEventBatch());
            var receivedCloudEvents = result.ToCloudEventBatch();
            for (int i = 0; i < receivedCloudEvents.Count; i++)
            {
                var receivedCloudEvent = receivedCloudEvents[i];

               Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
                Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
                Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"), receivedCloudEvent.Source);
                Assert.StartsWith($"A234-1234-12", receivedCloudEvent.Id);
                Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                    receivedCloudEvent.Time.Value.ToUniversalTime());
                Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);
                Assert.Equal("<much wow=\"xml\"/>", receivedCloudEvent.Data);

                var attr = receivedCloudEvent.GetAttributes();
                Assert.Equal("value", (string)attr["comexampleextension1"]);
                Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
            }

        }

        [Fact]
        async Task HttpBatchClientSendTest()
        {
            CloudEventBatch batch = new CloudEventBatch();
            for (int i = 0; i < 10; i++)
            {
                var cloudEvent = new CloudEvent("com.github.pull.create",
                    new Uri("https://github.com/cloudevents/spec/pull/123"))
                {
                    Id = $"A234-1234-12{i:00}",
                    Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                    ContentType = new ContentType(MediaTypeNames.Text.Xml),
                    Data = "<much wow=\"xml\"/>"
                };

                var attrs = cloudEvent.GetAttributes();
                attrs["comexampleextension1"] = "value";
                attrs["comexampleextension2"] = new { othervalue = 5 };
                batch.Add(cloudEvent);
            }

            string ctx = Guid.NewGuid().ToString();
            var content = new CloudEventContent(batch, new JsonEventFormatter());
            content.Headers.Add(testContextHeader, ctx);

            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    var receivedCloudEvents = context.Request.ToCloudEventBatch(new JsonEventFormatter());
                    foreach (var receivedCloudEvent in receivedCloudEvents)
                    {
                        Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
                        Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
                        Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"),
                            receivedCloudEvent.Source);
                        Assert.StartsWith("A234-1234-12", receivedCloudEvent.Id);
                        Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                            receivedCloudEvent.Time.Value.ToUniversalTime());
                        Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);
                        Assert.Equal("<much wow=\"xml\"/>", receivedCloudEvent.Data);

                        var attr = receivedCloudEvent.GetAttributes();
                        Assert.Equal("value", (string)attr["comexampleextension1"]);
                        Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
                    }
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var httpClient = new HttpClient();
            var result = (await httpClient.PostAsync(new Uri(listenerAddress + "ep"), content));
            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            }
        }

        [Fact]
        async Task HttpBatchWebRequestSendTest()
        {
            CloudEventBatch batch = new CloudEventBatch();
            for (int i = 0; i < 10; i++)
            {
                var cloudEvent = new CloudEvent("com.github.pull.create",
                    new Uri("https://github.com/cloudevents/spec/pull/123"))
                {
                    Id = $"A234-1234-12{i:00}",
                    Time = new DateTime(2018, 4, 5, 17, 31, 0, DateTimeKind.Utc),
                    ContentType = new ContentType(MediaTypeNames.Text.Xml),
                    Data = "<much wow=\"xml\"/>"
                };

                var attrs = cloudEvent.GetAttributes();
                attrs["comexampleextension1"] = "value";
                attrs["comexampleextension2"] = new { othervalue = 5 };
                batch.Add(cloudEvent);
            }

            string ctx = Guid.NewGuid().ToString();
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(listenerAddress + "ep");
            httpWebRequest.Method = "POST";
            await httpWebRequest.CopyFromAsync(batch, new JsonEventFormatter());
            httpWebRequest.Headers.Add(testContextHeader, ctx);

            pendingRequests.TryAdd(ctx, async context =>
            {
                try
                {
                    var receivedCloudEvents = context.Request.ToCloudEventBatch(new JsonEventFormatter());
                    foreach (var receivedCloudEvent in receivedCloudEvents)
                    {
                        Assert.Equal(CloudEventsSpecVersion.V0_2, receivedCloudEvent.SpecVersion);
                        Assert.Equal("com.github.pull.create", receivedCloudEvent.Type);
                        Assert.Equal(new Uri("https://github.com/cloudevents/spec/pull/123"),
                            receivedCloudEvent.Source);
                        Assert.StartsWith("A234-1234-12", receivedCloudEvent.Id);
                        Assert.Equal(DateTime.Parse("2018-04-05T17:31:00Z").ToUniversalTime(),
                            receivedCloudEvent.Time.Value.ToUniversalTime());
                        Assert.Equal(new ContentType(MediaTypeNames.Text.Xml), receivedCloudEvent.ContentType);
                        Assert.Equal("<much wow=\"xml\"/>", receivedCloudEvent.Data);

                        var attr = receivedCloudEvent.GetAttributes();
                        Assert.Equal("value", (string)attr["comexampleextension1"]);
                        Assert.Equal(5, (int)((dynamic)attr["comexampleextension2"]).othervalue);
                    }
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                }
                catch (Exception e)
                {
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        sw.Write(e.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }

                context.Response.Close();
            });

            var result = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
            if (result.StatusCode != HttpStatusCode.NoContent)
            {
                throw new InvalidOperationException(result.StatusCode.ToString());
            }
        }
    }
}