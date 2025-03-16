ApplicationMetrics.MetricLoggers.OpenTelemetry
---
An implementation of an [ApplicationMetrics](https://github.com/alastairwyse/ApplicationMetrics) [metric logger](https://github.com/alastairwyse/ApplicationMetrics/blob/master/ApplicationMetrics/IMetricLogger.cs) which writes metrics and instrumentation using the [OpenTelemetry protocol](https://github.com/open-telemetry/opentelemetry-proto/tree/main/docs).

#### Metric Mappings
Metric types in ApplicationMetrics are mapped to the following OpenTelemetry instruments...

| ApplicationMetrics Metric Type | OpenTelemetry Instrument |
| ------------------------------ | ----------- |
| CountMetric | [Counter](https://opentelemetry.io/docs/specs/otel/metrics/api/#counter) | 
| AmountMetric | [Counter](https://opentelemetry.io/docs/specs/otel/metrics/api/#counter) or [Histogram](https://opentelemetry.io/docs/specs/otel/metrics/api/#histogram) (configurable, defaults to Counter) |
| StatusMetric | [Gauge](https://opentelemetry.io/docs/specs/otel/metrics/api/#gauge) |
| IntervalMetric | [Counter](https://opentelemetry.io/docs/specs/otel/metrics/api/#counter) or [Histogram](https://opentelemetry.io/docs/specs/otel/metrics/api/#histogram) (configurable, defaults to Histogram) |

#### Setup
The OpenTelemetryMetricLogger class use the [OTLP Exporter for OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet/tree/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol) library, and hence can be configued via an [OtlpExporterOptions](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol/OtlpExporterOptions.cs) object.  This OtlpExporterOptions object can be set via OpenTelemetryMetricLogger constructor parameter 'otlpExporterConfigurationAction'.  The code below demonstrates the setup and use case (with fake metrics logged) of the OpenTelemetryMetricLogger class...

```C#
var meterOptions = new MeterOptions("OpenTelemetryMetricLoggerTest");
Action<OtlpExporterOptions> otlpExporterConfigurationAction = (options) =>
{
    options.Protocol = OtlpExportProtocol.HttpProtobuf;
    options.Endpoint = new Uri("http://127.0.0.1:4318/v1/metrics");
};
using (var metricLogger = new OpenTelemetryMetricLogger(IntervalMetricBaseTimeUnit.Millisecond, true, meterOptions, otlpExporterConfigurationAction))
{
    Guid beginId = metricLogger.Begin(new MessageSendTime());
    Thread.Sleep(20);
    metricLogger.Increment(new MessageSent());
    metricLogger.Add(new MessageSize(), 2661);
    metricLogger.End(beginId, new MessageSendTime());
}
```

Alternate mappings for AmountMetric and IntervalMetrics can be set via the OpenTelemetryMetricLogger constructor...

```C#
var metricLogger = new OpenTelemetryMetricLogger
(
    IntervalMetricBaseTimeUnit.Millisecond, 
    true, 
    meterOptions, 
    otlpExporterConfigurationAction,
    OpenTelemetryMetricType.Historgram, // mapping for AmountMetrics
    OpenTelemetryMetricType.Counter     // mapping for IntervalMetrics
)
```

The OpenTelemetryMetricLogger class accepts the following constructor parameters...

| Parameter Name | Description |
| -------------- | ----------- |
| intervalMetricBaseTimeUnit | The base time unit to use to log interval metrics. |
| intervalMetricChecking | Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()). This parameter is ignored when the the OpenTelemetryMetricLogger operates in ['interleaved'](https://github.com/alastairwyse/ApplicationMetrics#interleaved-interval-metrics) mode. |
| meterName | (optional) The name set on the underlying OpenTelemetry Meter class. |
| meterOptions | (optional) The options to set on the underlying OpenTelemetry Meter class. |
| otlpExporterConfigurationAction | An action which configures the underlying OpenTelemetry exporter. |
| amountMetricMappedType | (optional) The type of OpenTelemetry instruments that AmountMetrics are mapped to. |
| intervalMetricMappedType | (optional) The type of OpenTelemetry instruments that IntervalMetrics are mapped to. |

#### Non-interleaved Method Overloads
Methods which support ['non-interleaved' interval metric logging](https://github.com/alastairwyse/ApplicationMetrics#interleaved-interval-metrics) (i.e. overloads of End() and CancelBegin() methods which don't accept a Guid) have been configured as explicit implementations on interface [IMetricLogger](https://github.com/alastairwyse/ApplicationMetrics/blob/master/ApplicationMetrics/IMetricLogger.cs).  Hence they cannot be called on an instance of OpenTelemetryMetricLogger without first casting the instance to IMetricLogger.  This is to discourage the use of these methods as they will be deprecated in a future version of ApplicationMetrics.

#### Links
The documentation below was written for version 1.* of ApplicationMetrics.  Minor implementation details may have changed in versions 2.0.0 and above, however the basic principles and use cases documented are still valid.  Note also that this documentation demonstrates the older ['non-interleaved'](https://github.com/alastairwyse/ApplicationMetrics#interleaved-interval-metrics) method of logging interval metrics.

Full documentation for the project...<br />
[http://www.alastairwyse.net/methodinvocationremoting/application-metrics.html](http://www.alastairwyse.net/methodinvocationremoting/application-metrics.html)

A detailed sample implementation...<br />
[http://www.alastairwyse.net/methodinvocationremoting/sample-application-5.html](http://www.alastairwyse.net/methodinvocationremoting/sample-application-5.html)

#### Release History

| Version | Changes |
| ------- | ------- |
| 1.1.0 | Updated to ApplicationMetrics version 7.0.0. | 
| 1.0.0 | Initial release. | 
