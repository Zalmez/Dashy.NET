using System;
using System.Collections.Generic;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Dashy.Net.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class OptionsJsonSerializationBenchmarks
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private List<Dictionary<string, object>> _optionsPayloads = default!;
    private List<string> _optionsJsonStrings = default!;

    [Params(50, 200)]
    public int ItemsPerSection;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);
        _optionsPayloads = new List<Dictionary<string, object>>(ItemsPerSection);
        for (int i = 0; i < ItemsPerSection; i++)
        {
            var dict = new Dictionary<string, object>
            {
                ["title"] = $"Item {i}",
                ["url"] = $"https://example.com/{i}",
                ["icon"] = i % 2 == 0 ? "fas fa-cog" : "fas fa-star",
                ["enabled"] = i % 3 != 0,
                ["retries"] = i % 5,
                ["timeoutMs"] = 250 + (i % 10) * 25,
                ["tags"] = new [] { "a", "b", i.ToString() },
                ["meta"] = new Dictionary<string, object>
                {
                    ["createdAt"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    ["priority"] = rnd.Next(0,5),
                    ["thresholds"] = new [] { 0.1, 0.2, 0.5 },
                }
            };
            _optionsPayloads.Add(dict);
        }

        _optionsJsonStrings = new List<string>(_optionsPayloads.Count);
        foreach (var payload in _optionsPayloads)
        {
            _optionsJsonStrings.Add(JsonSerializer.Serialize(payload, _jsonOptions));
        }
    }

    [Benchmark]
    public int Serialize_DictionaryStringObject()
    {
        int total = 0;
        foreach (var payload in _optionsPayloads)
        {
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            total += json.Length;
        }
        return total;
    }

    [Benchmark]
    public int Deserialize_DictionaryStringObject()
    {
        int count = 0;
        foreach (var json in _optionsJsonStrings)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
            if (dict != null) count += dict.Count;
        }
        return count;
    }

    [Benchmark]
    public int Deserialize_JsonDocument()
    {
        int props = 0;
        foreach (var json in _optionsJsonStrings)
        {
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                props++;
            }
        }
        return props;
    }
}