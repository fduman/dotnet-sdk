﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace Dapr.Client.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Grpc.Net.Client;
    using Xunit;
    using Autogenerated = Dapr.Client.Autogen.Grpc.v1;

    public class SecretApiTest
    {
        [Fact]
        public async Task GetSecretAsync_ValidateRequest()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var metadata = new Dictionary<string, string>();
            metadata.Add("key1", "value1");
            metadata.Add("key2", "value2");
            var task = daprClient.GetSecretAsync("testStore", "test_key", metadata);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.GetSecretEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test_key");
            envelope.Metadata.Count.Should().Be(2);
            envelope.Metadata.Keys.Contains("key1").Should().BeTrue();
            envelope.Metadata.Keys.Contains("key2").Should().BeTrue();
            envelope.Metadata["key1"].Should().Be("value1");
            envelope.Metadata["key2"].Should().Be("value2");
        }

        [Fact]
        public async Task GetSecretAsync_ReturnSingleSecret()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var metadata = new Dictionary<string, string>();
            metadata.Add("key1", "value1");
            metadata.Add("key2", "value2");
            var task = daprClient.GetSecretAsync("testStore", "test_key", metadata);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.GetSecretEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test_key");
            envelope.Metadata.Count.Should().Be(2);
            envelope.Metadata.Keys.Contains("key1").Should().BeTrue();
            envelope.Metadata.Keys.Contains("key2").Should().BeTrue();
            envelope.Metadata["key1"].Should().Be("value1");
            envelope.Metadata["key2"].Should().Be("value2");

            // Create Response & Respond
            var secrets = new Dictionary<string, string>();
            secrets.Add("redis_secret", "Guess_Redis");
            await SendResponseWithSecrets(secrets, entry);

            // Get response and validate
            var secretsResponse= await task;
            secretsResponse.Count.Should().Be(1);
            secretsResponse.ContainsKey("redis_secret").Should().BeTrue();
            secretsResponse["redis_secret"].Should().Be("Guess_Redis");
        }

        [Fact]
        public async Task GetSecretAsync_ReturnMultipleSecrets()
        {
            // Configure Client
            var httpClient = new TestHttpClient();
            var daprClient = new DaprClientBuilder()
                .UseGrpcChannelOptions(new GrpcChannelOptions { HttpClient = httpClient })
                .Build();

            var metadata = new Dictionary<string, string>();
            metadata.Add("key1", "value1");
            metadata.Add("key2", "value2");
            var task = daprClient.GetSecretAsync("testStore", "test_key", metadata);

            // Get Request and validate
            httpClient.Requests.TryDequeue(out var entry).Should().BeTrue();
            var envelope = await GrpcUtils.GetEnvelopeFromRequestMessageAsync<Autogenerated.GetSecretEnvelope>(entry.Request);
            envelope.StoreName.Should().Be("testStore");
            envelope.Key.Should().Be("test_key");
            envelope.Metadata.Count.Should().Be(2);
            envelope.Metadata.Keys.Contains("key1").Should().BeTrue();
            envelope.Metadata.Keys.Contains("key2").Should().BeTrue();
            envelope.Metadata["key1"].Should().Be("value1");
            envelope.Metadata["key2"].Should().Be("value2");

            // Create Response & Respond
            var secrets = new Dictionary<string, string>();
            secrets.Add("redis_secret", "Guess_Redis");
            secrets.Add("kafka_secret", "Guess_Kafka");
            await SendResponseWithSecrets(secrets, entry);

            // Get response and validate
            var secretsResponse = await task;
            secretsResponse.Count.Should().Be(2);
            secretsResponse.ContainsKey("redis_secret").Should().BeTrue();
            secretsResponse["redis_secret"].Should().Be("Guess_Redis");
            secretsResponse.ContainsKey("kafka_secret").Should().BeTrue();
            secretsResponse["kafka_secret"].Should().Be("Guess_Kafka");
        }

        private async Task SendResponseWithSecrets(Dictionary<string, string> secrets, TestHttpClient.Entry entry)
        {
            var secretResponse = new Autogenerated.GetSecretResponseEnvelope();
            secretResponse.Data.Add(secrets);

            var streamContent = await GrpcUtils.CreateResponseContent(secretResponse);
            var response = GrpcUtils.CreateResponse(HttpStatusCode.OK, streamContent);
            entry.Completion.SetResult(response);
        }
    }
}
