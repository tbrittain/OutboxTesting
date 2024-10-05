using HashidsNet;

namespace OutboxTesting.MassTransit.Models;

public record HashedId
{
    public HashedId(int value)
    {
        Value = value;
        EncodedValue = new Hashids().Encode(value);
    }

    public HashedId(string encodedValue)
    {
        EncodedValue = encodedValue;
        Value = new Hashids().Decode(encodedValue)[0];
    }

    public int Value { get; init; }
    public string EncodedValue { get; init; }
}