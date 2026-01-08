/*
* Copyright 2025 Alastair Wyse (https://github.com/alastairwyse/ApplicationMetrics.MetricLoggers.OpenTelemetry/)
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Exporter;
using StandardAbstraction;

namespace ApplicationMetrics.MetricLoggers.OpenTelemetry
{
    /// <summary>
    /// Writes metric events using the <see href="https://opentelemetry.io/docs/specs/otel/protocol/">OpenTelemetry protocol</see>.
    /// </summary>
    public class OpenTelemetryMetricLogger : MetricLoggerBase, IMetricLogger, IDisposable
    {
        /// <summary>The type of OpenTelemetry metric that <see cref="AmountMetric">AmountMetrics</see> should be mapped to.</summary>
        protected OpenTelemetryMetricType amountMetricMappedType;
        /// <summary>The type of OpenTelemetry metric that <see cref="IntervalMetric">IntervalMetrics</see> should be mapped to.</summary>
        protected OpenTelemetryMetricType intervalMetricMappedType;
        /// <summary>Maps types of <see cref="CountMetric">CountMetrics</see> to OpenTelemetry <see cref="Counter{T}">Counters</see>.</summary>
        protected ConcurrentDictionary<Type, Counter<Int64>> countMetricsMap;
        /// <summary>Maps types of <see cref="AmountMetric">AmountMetrics</see> to OpenTelemetry <see cref="Counter{T}">Counters</see>.</summary>
        protected ConcurrentDictionary<Type, Counter<Int64>> amountMetricsCounterMap;
        /// <summary>Maps types of <see cref="AmountMetric">AmountMetrics</see> to OpenTelemetry <see cref="Histogram{T}">Histograms</see>.</summary>
        protected ConcurrentDictionary<Type, Histogram<Int64>> amountMetricsHistogramMap;
        /// <summary>Maps types of <see cref="StatusMetric">StatusMetrics</see> to OpenTelemetry <see cref="Gauge{T}">Gauges</see>.</summary>
        protected ConcurrentDictionary<Type, Gauge<Int64>> statusMetricsMap;
        /// <summary>Maps types of <see cref="IntervalMetric">IntervalMetrics</see> to OpenTelemetry <see cref="Counter{T}">Counters</see>.</summary>
        protected ConcurrentDictionary<Type, Counter<Int64>> intervalMetricsCounterMap;
        /// <summary>Maps types of <see cref="IntervalMetric">IntervalMetrics</see> to OpenTelemetry <see cref="Histogram{T}">Histograms</see>.</summary>
        protected ConcurrentDictionary<Type, Histogram<Int64>> intervalMetricsHistogramMap;
        /// <summary>Acts as a <see href="https://en.wikipedia.org/wiki/Shim_(computing)">shim</see> to OpenTelemetry metric logging methods.</summary>
        protected IOpenTelemetryMetricLoggingShim metricLoggingShim;
        /// <summary>The underlying <see cref="Meter"/> object to use to create the OpenTelemetry metric type.</summary>
        protected Meter meter;
        /// <summary>The underlying <see cref="MeterProvider"/> to use to log the metrics.</summary>
        protected MeterProvider meterProvider;
        /// <summary>Object which provides the current date and time.</summary>
        protected IDateTime dateTime;
        /// <summary>Indicates whether the object has been disposed.</summary>
        protected bool disposed;

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.OpenTelemetry.OpenTelemetryMetricLogger class.
        /// </summary>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        /// <param name="meterName">The name set on the underlying <see cref="Meter"/> class.</param>
        /// <param name="otlpExporterConfigurationAction">An action which configures the underlying OpenTelemetry exporter.</param>
        /// <remarks>
        ///   <para>The constructor maps <see cref="AmountMetric">AmountMetrics</see> to <see cref="Counter{T}">Counters</see> and <see cref="IntervalMetric">IntervalMetrics</see> to <see cref="Histogram{T}">Histograms</see>.</para>
        ///   <para>The class uses a <see cref="System.Diagnostics.Stopwatch"/> to calculate and log interval metrics.  Since the smallest unit of time supported by Stopwatch is a tick (100 nanoseconds), the smallest level of granularity supported when parameter <paramref name="intervalMetricBaseTimeUnit"/> is set to <see cref="IntervalMetricBaseTimeUnit.Nanosecond"/> is 100 nanoseconds.</para>
        /// </remarks>
        public OpenTelemetryMetricLogger(IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit, Boolean intervalMetricChecking, String meterName, Action<OtlpExporterOptions> otlpExporterConfigurationAction)
            : this(intervalMetricBaseTimeUnit, intervalMetricChecking, new MeterOptions(meterName), otlpExporterConfigurationAction, OpenTelemetryMetricType.Counter, OpenTelemetryMetricType.Historgram)
        {
        }

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.OpenTelemetry.OpenTelemetryMetricLogger class.
        /// </summary>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        /// <param name="meterOptions">The options to set on the underlying <see cref="Meter"/> class.</param>
        /// <param name="otlpExporterConfigurationAction">An action which configures the underlying OpenTelemetry exporter.</param>
        /// <remarks>
        ///   <para>The constructor maps <see cref="AmountMetric">AmountMetrics</see> to <see cref="Counter{T}">Counters</see> and <see cref="IntervalMetric">IntervalMetrics</see> to <see cref="Histogram{T}">Histograms</see>.</para>
        ///   <para>The class uses a <see cref="System.Diagnostics.Stopwatch"/> to calculate and log interval metrics.  Since the smallest unit of time supported by Stopwatch is a tick (100 nanoseconds), the smallest level of granularity supported when parameter <paramref name="intervalMetricBaseTimeUnit"/> is set to <see cref="IntervalMetricBaseTimeUnit.Nanosecond"/> is 100 nanoseconds.</para>
        /// </remarks>
        public OpenTelemetryMetricLogger(IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit, Boolean intervalMetricChecking, MeterOptions meterOptions, Action<OtlpExporterOptions> otlpExporterConfigurationAction)
            : this(intervalMetricBaseTimeUnit, intervalMetricChecking, meterOptions, otlpExporterConfigurationAction, OpenTelemetryMetricType.Counter, OpenTelemetryMetricType.Historgram)
        {
        }

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.OpenTelemetry.OpenTelemetryMetricLogger class.
        /// </summary>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        /// <param name="meterOptions">The options to set on the underlying <see cref="Meter"/> class.</param>
        /// <param name="otlpExporterConfigurationAction">An action which configures the underlying OpenTelemetry exporter.</param>
        /// <param name="amountMetricMappedType">The type of OpenTelemetry metric that <see cref="AmountMetric">AmountMetrics</see> should be mapped to.</param>
        /// <param name="intervalMetricMappedType">The type of OpenTelemetry metric that <see cref="IntervalMetric">IntervalMetrics</see> should be mapped to.</param>
        /// <remarks>The class uses a <see cref="System.Diagnostics.Stopwatch"/> to calculate and log interval metrics.  Since the smallest unit of time supported by Stopwatch is a tick (100 nanoseconds), the smallest level of granularity supported when parameter <paramref name="intervalMetricBaseTimeUnit"/> is set to <see cref="IntervalMetricBaseTimeUnit.Nanosecond"/> is 100 nanoseconds.</remarks>
        public OpenTelemetryMetricLogger
        (
            IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit, 
            Boolean intervalMetricChecking, 
            MeterOptions meterOptions, 
            Action<OtlpExporterOptions> otlpExporterConfigurationAction, 
            OpenTelemetryMetricType amountMetricMappedType, 
            OpenTelemetryMetricType intervalMetricMappedType
        )
            : base(intervalMetricBaseTimeUnit, intervalMetricChecking, true, new StandardAbstraction.Stopwatch(), new DefaultGuidProvider())
        {
            if (String.IsNullOrWhiteSpace(meterOptions.Name) == true)
                throw new ArgumentException($"The meter's '{nameof(meterOptions.Name)}' parmaeter must contain a value.");

            this.amountMetricMappedType = amountMetricMappedType;
            this.intervalMetricMappedType = intervalMetricMappedType;
            countMetricsMap = new ConcurrentDictionary<Type, Counter<Int64>>();
            amountMetricsCounterMap = new ConcurrentDictionary<Type, Counter<Int64>>();
            amountMetricsHistogramMap = new ConcurrentDictionary<Type, Histogram<Int64>>();
            statusMetricsMap = new ConcurrentDictionary<Type, Gauge<Int64>>();
            intervalMetricsCounterMap = new ConcurrentDictionary<Type, Counter<Int64>>();
            intervalMetricsHistogramMap = new ConcurrentDictionary<Type, Histogram<Int64>>();
            metricLoggingShim = new DefaultOpenTelemetryMetricLoggngShim();
            meter = new Meter(meterOptions); 
            meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(meterOptions.Name)
                .AddOtlpExporter(options =>
                {
                    otlpExporterConfigurationAction(options);
                })
                .Build();
            dateTime = new StandardAbstraction.DateTime();
            stopWatch.Start();
            startTime = dateTime.UtcNow;
            disposed = false;
        }

        /// <summary>
        /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.OpenTelemetry.OpenTelemetryMetricLogger class.
        /// </summary>
        /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
        /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
        /// <param name="meterOptions">The options to set on the underlying <see cref="Meter"/> class.</param>
        /// <param name="otlpExporterConfigurationAction">An action which configures the underlying OpenTelemetry exporter.</param>
        /// <param name="amountMetricMappedType">The type of OpenTelemetry metric that <see cref="AmountMetric">AmountMetrics</see> should be mapped to.</param>
        /// <param name="intervalMetricMappedType">The type of OpenTelemetry metric that <see cref="IntervalMetric">IntervalMetrics</see> should be mapped to.</param>
        /// <param name="stopWatch">A test (mock) <see cref="IStopwatch"/> object.</param>
        /// <param name="guidProvider">A test (mock) <see cref="IGuidProvider"/> object.</param>
        /// <param name="dateTime">A test (mock) <see cref="IDateTime"/> object.</param>
        /// <param name="metricLoggingShim">A test (mock) <see cref="IOpenTelemetryMetricLoggingShim"/> object.</param>
        /// <remarks>This constructor is included to facilitate unit testing.</remarks>
        protected OpenTelemetryMetricLogger
        (
            IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit,
            Boolean intervalMetricChecking,
            MeterOptions meterOptions,
            Action<OtlpExporterOptions> otlpExporterConfigurationAction,
            OpenTelemetryMetricType amountMetricMappedType,
            OpenTelemetryMetricType intervalMetricMappedType, 
            IStopwatch stopWatch, 
            IGuidProvider guidProvider,
            IDateTime dateTime, 
            IOpenTelemetryMetricLoggingShim metricLoggingShim
        )
            : this(intervalMetricBaseTimeUnit, intervalMetricChecking, meterOptions, otlpExporterConfigurationAction, amountMetricMappedType, intervalMetricMappedType)
        {
            base.stopWatch = stopWatch;
            base.guidProvider = guidProvider;
            stopWatchFrequency = stopWatch.Frequency;
            this.dateTime = dateTime;
            startTime = dateTime.UtcNow;
            this.metricLoggingShim = metricLoggingShim;
        }

        /// <inheritdoc/>
        public void Increment(CountMetric countMetric)
        {
            Type metricType = countMetric.GetType();
            if (countMetricsMap.ContainsKey(metricType) == false)
            {
                lock (countMetricsMap)
                {
                    if (countMetricsMap.ContainsKey(metricType) == false)
                    {
                        var counter = meter.CreateCounter<Int64>(RemoveWhitespaceFromString(countMetric.Name), null, countMetric.Description);
                        countMetricsMap.TryAdd(metricType, counter);
                    }
                }
            }

            metricLoggingShim.AddCounter(countMetricsMap[metricType], 1);
        }

        /// <inheritdoc/>
        public void Add(AmountMetric amountMetric, Int64 amount)
        {
            Type metricType = amountMetric.GetType();
            if (amountMetricMappedType == OpenTelemetryMetricType.Counter)
            {
                if (amountMetricsCounterMap.ContainsKey(metricType) == false)
                {
                    lock (amountMetricsCounterMap)
                    {
                        if (amountMetricsCounterMap.ContainsKey(metricType) == false)
                        {
                            var counter = meter.CreateCounter<Int64>(RemoveWhitespaceFromString(amountMetric.Name), null, amountMetric.Description);
                            amountMetricsCounterMap.TryAdd(metricType, counter);
                        }
                    }
                }

                metricLoggingShim.AddCounter(amountMetricsCounterMap[metricType], amount);
            }
            else
            {
                if (amountMetricsHistogramMap.ContainsKey(metricType) == false)
                {
                    lock (amountMetricsHistogramMap)
                    {
                        if (amountMetricsHistogramMap.ContainsKey(metricType) == false)
                        {
                            var counter = meter.CreateHistogram<Int64>(RemoveWhitespaceFromString(amountMetric.Name), null, amountMetric.Description);
                            amountMetricsHistogramMap.TryAdd(metricType, counter);
                        }
                    }
                }

                metricLoggingShim.RecordHistogram(amountMetricsHistogramMap[metricType], amount);
            }
        }

        /// <inheritdoc/>
        public void Set(StatusMetric statusMetric, Int64 value)
        {
            Type metricType = statusMetric.GetType();
            if (statusMetricsMap.ContainsKey(metricType) == false)
            {
                lock (statusMetricsMap)
                {
                    if (statusMetricsMap.ContainsKey(metricType) == false)
                    {
                        var gauge = meter.CreateGauge<Int64>(RemoveWhitespaceFromString(statusMetric.Name), null, statusMetric.Description);
                        statusMetricsMap.TryAdd(metricType, gauge);
                    }
                }
            }

            metricLoggingShim.RecordGauge(statusMetricsMap[metricType], value);
        }

        /// <inheritdoc/>
        public Guid Begin(IntervalMetric intervalMetric)
        {
            var beginId = guidProvider.NewGuid();
            var intervalMetricEventInstance = new UniqueIntervalMetricEventInstance(beginId, intervalMetric, IntervalMetricEventTimePoint.Start, GetStopWatchUtcNow());
            ProcessStartIntervalMetricEvent(intervalMetricEventInstance);

            return beginId;
        }

        /// <inheritdoc/>
        void IMetricLogger.End(IntervalMetric intervalMetric)
        {
            if (interleavedIntervalMetricsMode.HasValue == false)
            {
                interleavedIntervalMetricsMode = false;
            }
            else
            {
                if (interleavedIntervalMetricsMode == true)
                    throw new InvalidOperationException($"The overload of the {nameof(IMetricLogger.End)}() method without a {nameof(Guid)} parameter cannot be called when the metric logger is running in interleaved mode.");
            }

            var intervalMetricEventInstance = new UniqueIntervalMetricEventInstance(Guid.Empty, intervalMetric, IntervalMetricEventTimePoint.End, GetStopWatchUtcNow());
            ImplementEnd(intervalMetricEventInstance);
        }

        /// <inheritdoc/>
        public void End(Guid beginId, IntervalMetric intervalMetric)
        {
            if (interleavedIntervalMetricsMode.HasValue == false)
            {
                interleavedIntervalMetricsMode = true;
            }
            else
            {
                if (interleavedIntervalMetricsMode == false)
                    throw new InvalidOperationException($"The overload of the {nameof(IMetricLogger.End)}() method with a {nameof(Guid)} parameter cannot be called when the metric logger is running in non-interleaved mode.");
            }

            var intervalMetricEventInstance = new UniqueIntervalMetricEventInstance(beginId, intervalMetric, IntervalMetricEventTimePoint.End, GetStopWatchUtcNow());
            ImplementEnd(intervalMetricEventInstance);
        }

        /// <inheritdoc/>
        void IMetricLogger.CancelBegin(IntervalMetric intervalMetric)
        {
            if (interleavedIntervalMetricsMode.HasValue == false)
            {
                interleavedIntervalMetricsMode = false;
            }
            else
            {
                if (interleavedIntervalMetricsMode == true)
                    throw new InvalidOperationException($"The overload of the {nameof(IMetricLogger.CancelBegin)}() method without a {nameof(Guid)} parameter cannot be called when the metric logger is running in interleaved mode.");
            }

            var intervalMetricEventInstance = new UniqueIntervalMetricEventInstance(Guid.Empty, intervalMetric, IntervalMetricEventTimePoint.Cancel, GetStopWatchUtcNow());
            ProcessCancelIntervalMetricEvent(intervalMetricEventInstance);
        }

        /// <inheritdoc/>
        public void CancelBegin(Guid beginId, IntervalMetric intervalMetric)
        {
            if (interleavedIntervalMetricsMode.HasValue == false)
            {
                interleavedIntervalMetricsMode = true;
            }
            else
            {
                if (interleavedIntervalMetricsMode == false)
                    throw new InvalidOperationException($"The overload of the {nameof(IMetricLogger.CancelBegin)}() method with a {nameof(Guid)} parameter cannot be called when the metric logger is running in non-interleaved mode.");
            }

            var intervalMetricEventInstance = new UniqueIntervalMetricEventInstance(beginId, intervalMetric, IntervalMetricEventTimePoint.Cancel, GetStopWatchUtcNow());
            ProcessCancelIntervalMetricEvent(intervalMetricEventInstance);
        }

        #region Private/Protected Methods

        /// <summary>
        /// Common implementation for the public End() method which handles processing interval metric 'End' timepoints when interval metrics are mapped to either OpenTelemetry Counters or Gauges.
        /// </summary>
        /// <param name="intervalMetricEventInstance">An interval metric event instance representing the end of the interval metric.</param>
        protected void ImplementEnd(UniqueIntervalMetricEventInstance intervalMetricEventInstance)
        {
            Tuple<IntervalMetricEventInstance, Int64> generatedInstance = ProcessEndIntervalMetricEvent(intervalMetricEventInstance);
            Type metricType = generatedInstance.Item1.Metric.GetType();
            if (intervalMetricMappedType == OpenTelemetryMetricType.Counter)
            {
                if (intervalMetricsCounterMap.ContainsKey(metricType) == false)
                {
                    lock (intervalMetricsCounterMap)
                    {
                        if (intervalMetricsCounterMap.ContainsKey(metricType) == false)
                        {
                            var counter = meter.CreateCounter<Int64>(RemoveWhitespaceFromString(generatedInstance.Item1.Metric.Name), intervalMetricBaseTimeUnit.ToString(), generatedInstance.Item1.Metric.Description);
                            intervalMetricsCounterMap.TryAdd(metricType, counter);
                        }
                    }
                }

                metricLoggingShim.AddCounter(intervalMetricsCounterMap[metricType], generatedInstance.Item2);
            }
            else
            {
                if (intervalMetricsHistogramMap.ContainsKey(metricType) == false)
                {
                    lock (intervalMetricsHistogramMap)
                    {
                        if (intervalMetricsHistogramMap.ContainsKey(metricType) == false)
                        {
                            var counter = meter.CreateHistogram<Int64>(RemoveWhitespaceFromString(generatedInstance.Item1.Metric.Name), intervalMetricBaseTimeUnit.ToString(), generatedInstance.Item1.Metric.Description);
                            intervalMetricsHistogramMap.TryAdd(metricType, counter);
                        }
                    }
                }

                metricLoggingShim.RecordHistogram(intervalMetricsHistogramMap[metricType], generatedInstance.Item2);
            }
        }

        /// <summary>
        /// Remove any whitespace from the specified string.
        /// </summary>
        /// <remarks>OpenTelemetry ignores any metrics which have whitespace in their name.</remarks>
        protected String RemoveWhitespaceFromString(String inputString)
        {
            return new String(inputString.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        #endregion

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the MetricLoggerBuffer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #pragma warning disable 1591
        ~OpenTelemetryMetricLogger()
        {
            Dispose(false);
        }
        #pragma warning restore 1591

        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    meterProvider.Shutdown();
                    meterProvider.Dispose();
                    meter.Dispose();
                }
                // Free your own state (unmanaged objects).

                // Set large fields to null.

                disposed = true;
            }
        }

        #endregion
    }
}
