using Microsoft.Extensions.Logging;

namespace KinesisProducerNet
{
    public static class Logging
    {
        public static ILoggerFactory LoggerFactory { get; } = Microsoft.Extensions.Logging.LoggerFactory.Create(
            builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}