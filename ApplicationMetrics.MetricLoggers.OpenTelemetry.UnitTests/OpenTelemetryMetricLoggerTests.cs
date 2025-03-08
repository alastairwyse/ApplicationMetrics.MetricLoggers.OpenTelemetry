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
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using StandardAbstraction;
using OpenTelemetry.Exporter;
using NUnit.Framework;
using NSubstitute;

namespace ApplicationMetrics.MetricLoggers.OpenTelemetry.UnitTests
{
    /// <summary>
    /// Unit tests for the ApplicationMetrics.MetricLoggers.OpenTelemetry.OpenTelemetryMetricLogger class.
    /// </summary>
    public class OpenTelemetryMetricLoggerTests
    {
        private String testMeterName;
        private System.DateTime testStartTime;
        private MeterOptions testMeterOptions;
        private Action<OtlpExporterOptions> testOtlpExporterConfigurationAction;
        private IStopwatch mockStopWatch;
        private IGuidProvider mockGuidProvider;
        private IDateTime mockDateTime;
        private IOpenTelemetryMetricLoggingShim mockMetricLoggingShim;
        private TestOpenTelemetryMetricLogger testOpenTelemetryMetricLogger;

        [SetUp]
        protected void SetUp()
        {
            testMeterName = "OpenTelemetryUnitTests";
            testStartTime = GenerateUtcDateTime("2025-03-02 11:34:10.000");
            mockStopWatch = Substitute.For<IStopwatch>();
            mockStopWatch.Frequency.Returns<Int64>(10000000);
            mockGuidProvider = Substitute.For<IGuidProvider>();
            mockDateTime = Substitute.For<IDateTime>();
            mockDateTime.UtcNow.Returns(testStartTime);
            mockMetricLoggingShim = Substitute.For<IOpenTelemetryMetricLoggingShim>();
            testMeterOptions = new MeterOptions(testMeterName);
            testOtlpExporterConfigurationAction = (exporterOptions) => { };
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond, 
                true, 
                testMeterOptions, 
                testOtlpExporterConfigurationAction, 
                OpenTelemetryMetricType.Counter, 
                OpenTelemetryMetricType.Historgram, 
                mockStopWatch, 
                mockGuidProvider,
                mockDateTime,
                mockMetricLoggingShim
            );
        }

        [TearDown]
        protected void TearDown()
        {
            testOpenTelemetryMetricLogger.Dispose();
        }

        [Test]
        public void Constructor_MeterNameParameterWhitespace()
        {
            testOpenTelemetryMetricLogger.Dispose();

            var e = Assert.Throws<ArgumentException>(delegate
            {
                testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
                (
                    IntervalMetricBaseTimeUnit.Millisecond,
                    true,
                    " ",
                    testOtlpExporterConfigurationAction
                );
            });

            Assert.That(e.Message, Does.StartWith($"The meter's 'Name' parmaeter must contain a value."));
        }

        [Test]
        public void Constructor_MetricMappedTypeFieldsSetCorrectly()
        {
            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction
            );

            NonPublicFieldAssert.HasValue(new List<String> { "amountMetricMappedType" }, OpenTelemetryMetricType.Counter, testOpenTelemetryMetricLogger);
            NonPublicFieldAssert.HasValue(new List<String> { "intervalMetricMappedType" }, OpenTelemetryMetricType.Historgram, testOpenTelemetryMetricLogger);


            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterName,
                testOtlpExporterConfigurationAction
            );

            NonPublicFieldAssert.HasValue(new List<String> { "amountMetricMappedType" }, OpenTelemetryMetricType.Counter, testOpenTelemetryMetricLogger);
            NonPublicFieldAssert.HasValue(new List<String> { "intervalMetricMappedType" }, OpenTelemetryMetricType.Historgram, testOpenTelemetryMetricLogger);
        }

        [Test]
        public void Constructor_OtlpExporterConfigurationActionIsInvoked()
        {
            Boolean actionWasInvoked = false;
            testOtlpExporterConfigurationAction = (exporterOptions) => { actionWasInvoked = true; };

            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction
            );

            Assert.IsTrue(actionWasInvoked);


            actionWasInvoked = false;

            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterName,
                testOtlpExporterConfigurationAction
            );

            Assert.IsTrue(actionWasInvoked);
        }

        [Test]
        public void Constructor_IntervalRelatedFieldsSetCorrectly()
        {
            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction
            );

            NonPublicFieldAssert.HasValue(new List<String> { "intervalMetricBaseTimeUnit" }, IntervalMetricBaseTimeUnit.Millisecond, testOpenTelemetryMetricLogger);
            NonPublicFieldAssert.HasValue(new List<String> { "intervalMetricChecking" }, true, testOpenTelemetryMetricLogger);
            Assert.IsNotNull(testOpenTelemetryMetricLogger.StartTime);
            Assert.AreNotEqual(System.DateTime.MinValue, testOpenTelemetryMetricLogger.StartTime);


            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Nanosecond,
                false,
                testMeterOptions,
                testOtlpExporterConfigurationAction
            );

            NonPublicFieldAssert.HasValue(new List<String> { "intervalMetricBaseTimeUnit" }, IntervalMetricBaseTimeUnit.Nanosecond, testOpenTelemetryMetricLogger);
            NonPublicFieldAssert.HasValue(new List<String> { "intervalMetricChecking" }, false, testOpenTelemetryMetricLogger);
            Assert.IsNotNull(testOpenTelemetryMetricLogger.StartTime);
            Assert.AreNotEqual(System.DateTime.MinValue, testOpenTelemetryMetricLogger.StartTime);
        }

        [Test]
        public void Constructor_ProviderFieldsSetCorrectly()
        {
            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction
            );

            NonPublicFieldAssert.IsOfType<StandardAbstraction.DateTime>(new List<String> { "dateTime" }, testOpenTelemetryMetricLogger);
            NonPublicFieldAssert.IsOfType<StandardAbstraction.Stopwatch>(new List<String> { "stopWatch" }, testOpenTelemetryMetricLogger);
            NonPublicFieldAssert.IsOfType<DefaultGuidProvider>(new List<String> { "guidProvider" }, testOpenTelemetryMetricLogger);
        }

        [Test]
        public void Increment()
        {
            var loggedCounters = new List<Counter<Int64>>();
            mockMetricLoggingShim.AddCounter(Arg.Do<Counter<Int64>>((counter) => { loggedCounters.Add(counter); }), 1);

            testOpenTelemetryMetricLogger.Increment(new DiskReadOperation());
            testOpenTelemetryMetricLogger.Increment(new MessageReceived());
            testOpenTelemetryMetricLogger.Increment(new DiskReadOperation());

            Assert.AreEqual(3, loggedCounters.Count);
            Assert.AreEqual(new DiskReadOperation().Name, loggedCounters[0].Name);
            Assert.AreEqual(new DiskReadOperation().Description, loggedCounters[0].Description);
            Assert.AreEqual(testMeterName, loggedCounters[0].Meter.Name);
            Assert.AreEqual(new MessageReceived().Name, loggedCounters[1].Name);
            Assert.AreEqual(new MessageReceived().Description, loggedCounters[1].Description);
            Assert.AreEqual(testMeterName, loggedCounters[1].Meter.Name);
            Assert.AreEqual(new DiskReadOperation().Name, loggedCounters[2].Name);
            Assert.AreEqual(new DiskReadOperation().Description, loggedCounters[2].Description);
            Assert.AreEqual(testMeterName, loggedCounters[2].Meter.Name);
        }

        [Test]
        public void Add_AmountMetricsMappedToCounters()
        {
            var loggedCounters = new List<Counter<Int64>>();
            var loggedValues = new List<Int64>();
            mockMetricLoggingShim.AddCounter
            (
                Arg.Do<Counter<Int64>>((counter) => { loggedCounters.Add(counter); }),
                Arg.Do<Int64>((value) => { loggedValues.Add(value); })
            );

            testOpenTelemetryMetricLogger.Add(new DiskBytesRead(), 100);
            testOpenTelemetryMetricLogger.Add(new MessageBytesReceived(), 200);
            testOpenTelemetryMetricLogger.Add(new DiskBytesRead(), 300);

            Assert.AreEqual(3, loggedCounters.Count);
            Assert.AreEqual(3, loggedValues.Count);
            Assert.AreEqual(new DiskBytesRead().Name, loggedCounters[0].Name);
            Assert.AreEqual(new DiskBytesRead().Description, loggedCounters[0].Description);
            Assert.AreEqual(100, loggedValues[0]);
            Assert.AreEqual(testMeterName, loggedCounters[0].Meter.Name);
            Assert.AreEqual(new MessageBytesReceived().Name, loggedCounters[1].Name);
            Assert.AreEqual(new MessageBytesReceived().Description, loggedCounters[1].Description);
            Assert.AreEqual(200, loggedValues[1]);
            Assert.AreEqual(testMeterName, loggedCounters[1].Meter.Name);
            Assert.AreEqual(new DiskBytesRead().Name, loggedCounters[2].Name);
            Assert.AreEqual(new DiskBytesRead().Description, loggedCounters[2].Description);
            Assert.AreEqual(300, loggedValues[2]);
            Assert.AreEqual(testMeterName, loggedCounters[2].Meter.Name);
        }

        [Test]
        public void Add_AmountMetricsMappedToHistograms()
        {
            var loggedHistograms = new List<Histogram<Int64>>();
            var loggedValues = new List<Int64>();
            mockMetricLoggingShim.RecordHistogram
            (
                Arg.Do<Histogram<Int64>>((histogram) => { loggedHistograms.Add(histogram); }),
                Arg.Do<Int64>((value) => { loggedValues.Add(value); })
            );
            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction,
                OpenTelemetryMetricType.Historgram,
                OpenTelemetryMetricType.Historgram,
                mockStopWatch,
                mockGuidProvider,
                mockDateTime,
                mockMetricLoggingShim
            );

            testOpenTelemetryMetricLogger.Add(new DiskBytesRead(), 100);
            testOpenTelemetryMetricLogger.Add(new MessageBytesReceived(), 200);
            testOpenTelemetryMetricLogger.Add(new DiskBytesRead(), 300);

            Assert.AreEqual(3, loggedHistograms.Count);
            Assert.AreEqual(3, loggedValues.Count);
            Assert.AreEqual(new DiskBytesRead().Name, loggedHistograms[0].Name);
            Assert.AreEqual(new DiskBytesRead().Description, loggedHistograms[0].Description);
            Assert.AreEqual(100, loggedValues[0]);
            Assert.AreEqual(testMeterName, loggedHistograms[0].Meter.Name);
            Assert.AreEqual(new MessageBytesReceived().Name, loggedHistograms[1].Name);
            Assert.AreEqual(new MessageBytesReceived().Description, loggedHistograms[1].Description);
            Assert.AreEqual(200, loggedValues[1]);
            Assert.AreEqual(testMeterName, loggedHistograms[1].Meter.Name);
            Assert.AreEqual(new DiskBytesRead().Name, loggedHistograms[2].Name);
            Assert.AreEqual(new DiskBytesRead().Description, loggedHistograms[2].Description);
            Assert.AreEqual(300, loggedValues[2]);
            Assert.AreEqual(testMeterName, loggedHistograms[2].Meter.Name);
        }

        [Test]
        public void Set()
        {
            var loggedGauges = new List<Gauge<Int64>>();
            var loggedValues = new List<Int64>();
            mockMetricLoggingShim.RecordGauge
            (
                Arg.Do<Gauge<Int64>>((gauge) => { loggedGauges.Add(gauge); }),
                Arg.Do<Int64>((value) => { loggedValues.Add(value); })
            );

            testOpenTelemetryMetricLogger.Set(new AvailableMemory(), 1_000_000);
            testOpenTelemetryMetricLogger.Set(new FreeWorkerThreads(), 15);
            testOpenTelemetryMetricLogger.Set(new AvailableMemory(), 2_000_000);

            Assert.AreEqual(3, loggedGauges.Count);
            Assert.AreEqual(3, loggedValues.Count);
            Assert.AreEqual(new AvailableMemory().Name, loggedGauges[0].Name);
            Assert.AreEqual(new AvailableMemory().Description, loggedGauges[0].Description);
            Assert.AreEqual(1_000_000, loggedValues[0]);
            Assert.AreEqual(testMeterName, loggedGauges[0].Meter.Name);
            Assert.AreEqual(new FreeWorkerThreads().Name, loggedGauges[1].Name);
            Assert.AreEqual(new FreeWorkerThreads().Description, loggedGauges[1].Description);
            Assert.AreEqual(15, loggedValues[1]);
            Assert.AreEqual(testMeterName, loggedGauges[1].Meter.Name);
            Assert.AreEqual(new AvailableMemory().Name, loggedGauges[2].Name);
            Assert.AreEqual(new AvailableMemory().Description, loggedGauges[2].Description);
            Assert.AreEqual(2_000_000, loggedValues[2]);
            Assert.AreEqual(testMeterName, loggedGauges[2].Meter.Name);
        }

        [Test]
        public void End_IntervalMetricsMappedToHistograms_NoBeginIdPassedInInterleavedMode()
        {
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000, // Return value for first call to Begin()
                60_000, // Return value for first call to End()
                70_000  // Return value for second call to Begin()
            );
            Guid beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskReadTime()); 
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());

            var e = Assert.Throws<InvalidOperationException>(delegate
            {
                testOpenTelemetryMetricLogger.End(new DiskReadTime());
            });

            Assert.That(e.Message, Does.StartWith($"The overload of the End() method without a Guid parameter cannot be called when the metric logger is running in interleaved mode."));
        }

        [Test]
        public void End_IntervalMetricsMappedToHistograms_BeginIdPassedInNonInterleavedMode()
        {
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000, // Return value for first call to Begin()
                60_000, // Return value for first call to End()
                70_000  // Return value for second call to Begin()
            );
            testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(new DiskReadTime());
            testOpenTelemetryMetricLogger.Begin(new DiskReadTime());

            var e = Assert.Throws<InvalidOperationException>(delegate
            {
                testOpenTelemetryMetricLogger.End(Guid.NewGuid(), new DiskReadTime());
            });

            Assert.That(e.Message, Does.StartWith($"The overload of the End() method with a Guid parameter cannot be called when the metric logger is running in non-interleaved mode."));
        }

        [Test]
        public void CancelBegin_IntervalMetricsMappedToHistograms_NoBeginIdPassedInInterleavedMode()
        {
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000, // Return value for first call to Begin()
                60_000, // Return value for first call to End()
                70_000  // Return value for second call to Begin()
            );
            Guid beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.CancelBegin(beginId, new DiskReadTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());

            var e = Assert.Throws<InvalidOperationException>(delegate
            {
                testOpenTelemetryMetricLogger.CancelBegin(new DiskReadTime());
            });

            Assert.That(e.Message, Does.StartWith($"The overload of the CancelBegin() method without a Guid parameter cannot be called when the metric logger is running in interleaved mode."));
        }

        [Test]
        public void CancelBegin_IntervalMetricsMappedToHistograms_BeginIdPassedInNonInterleavedMode()
        {
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000, // Return value for first call to Begin()
                60_000, // Return value for first call to End()
                70_000  // Return value for second call to Begin()
            );
            testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.CancelBegin(new DiskReadTime());
            testOpenTelemetryMetricLogger.Begin(new DiskReadTime());

            var e = Assert.Throws<InvalidOperationException>(delegate
            {
                testOpenTelemetryMetricLogger.CancelBegin(Guid.NewGuid(), new DiskReadTime());
            });

            Assert.That(e.Message, Does.StartWith($"The overload of the CancelBegin() method with a Guid parameter cannot be called when the metric logger is running in non-interleaved mode."));
        }

        [Test]
        public void Begin_CancelBegin_IntervalMetricsMappedToHistograms()
        {
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000, // Return value for first call to Begin()
                60_000, // Return value for first call to End()
                70_000,  // Return value for second call to Begin()
                130_000  // Return value for second call to End()
            );

            Guid beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.CancelBegin(beginId, new DiskReadTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.CancelBegin(beginId, new DiskReadTime());

            mockMetricLoggingShim.DidNotReceive().RecordHistogram<Int64>(Arg.Any<Histogram<Int64>>(), (Arg.Any<Int64>()));
            mockMetricLoggingShim.DidNotReceive().AddCounter<Int64>(Arg.Any<Counter<Int64>>(), (Arg.Any<Int64>()));
        }

        [Test]
        public void Begin_CancelBegin_IntervalMetricsMappedToCounters()
        {
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000, // Return value for first call to Begin()
                60_000, // Return value for first call to End()
                70_000,  // Return value for second call to Begin()
                130_000  // Return value for second call to End()
            );
            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction,
                OpenTelemetryMetricType.Counter,
                OpenTelemetryMetricType.Counter,
                mockStopWatch,
                mockGuidProvider,
                mockDateTime,
                mockMetricLoggingShim
            );

            Guid beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.CancelBegin(beginId, new DiskReadTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.CancelBegin(beginId, new DiskReadTime());

            mockMetricLoggingShim.DidNotReceive().RecordHistogram<Int64>(Arg.Any<Histogram<Int64>>(), (Arg.Any<Int64>()));
            mockMetricLoggingShim.DidNotReceive().AddCounter<Int64>(Arg.Any<Counter<Int64>>(), (Arg.Any<Int64>()));
        }

        [Test]
        public void Begin_End_IntervalMetricsMappedToHistograms()
        {
            var loggedHistograms = new List<Histogram<Int64>>();
            var loggedValues = new List<Int64>();
            mockMetricLoggingShim.RecordHistogram
            (
                Arg.Do<Histogram<Int64>>((histogram) => { loggedHistograms.Add(histogram); }),
                Arg.Do<Int64>((value) => { loggedValues.Add(value); })
            );
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000,   // Return value for first call to Begin()
                60_000,   // Return value for first call to End()
                70_000,   // Return value for second call to Begin()
                130_000,  // Return value for second call to End()
                140_000,  // Return value for third call to Begin()
                210_000   // Return value for third call to End()
            );

            Guid beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskReadTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskWriteTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskWriteTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskReadTime());

            Assert.AreEqual(3, loggedHistograms.Count);
            Assert.AreEqual(3, loggedValues.Count);
            Assert.AreEqual(new DiskReadTime().Name, loggedHistograms[0].Name);
            Assert.AreEqual(new DiskReadTime().Description, loggedHistograms[0].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Millisecond.ToString(), loggedHistograms[0].Unit);
            Assert.AreEqual(5, loggedValues[0]);
            Assert.AreEqual(testMeterName, loggedHistograms[0].Meter.Name);
            Assert.AreEqual(new DiskWriteTime().Name, loggedHistograms[1].Name);
            Assert.AreEqual(new DiskWriteTime().Description, loggedHistograms[1].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Millisecond.ToString(), loggedHistograms[1].Unit);
            Assert.AreEqual(6, loggedValues[1]);
            Assert.AreEqual(testMeterName, loggedHistograms[1].Meter.Name);
            Assert.AreEqual(new DiskReadTime().Name, loggedHistograms[2].Name);
            Assert.AreEqual(new DiskReadTime().Description, loggedHistograms[2].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Millisecond.ToString(), loggedHistograms[2].Unit);
            Assert.AreEqual(7, loggedValues[2]);
            Assert.AreEqual(testMeterName, loggedHistograms[2].Meter.Name);
        }

        [Test]
        public void Begin_End_IntervalMetricsMappedToHistogramsBaseTimeUnitNanoseconds()
        {
            var loggedHistograms = new List<Histogram<Int64>>();
            var loggedValues = new List<Int64>();
            mockMetricLoggingShim.RecordHistogram
            (
                Arg.Do<Histogram<Int64>>((histogram) => { loggedHistograms.Add(histogram); }),
                Arg.Do<Int64>((value) => { loggedValues.Add(value); })
            );
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000,   // Return value for first call to Begin()
                60_000,   // Return value for first call to End()
                70_000,   // Return value for second call to Begin()
                130_000,  // Return value for second call to End()
                140_000,  // Return value for third call to Begin()
                210_000   // Return value for third call to End()
            );
            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Nanosecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction,
                OpenTelemetryMetricType.Counter,
                OpenTelemetryMetricType.Historgram,
                mockStopWatch,
                mockGuidProvider,
                mockDateTime,
                mockMetricLoggingShim
            );

            Guid beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskReadTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskWriteTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskWriteTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskReadTime());

            Assert.AreEqual(3, loggedHistograms.Count);
            Assert.AreEqual(3, loggedValues.Count);
            Assert.AreEqual(new DiskReadTime().Name, loggedHistograms[0].Name);
            Assert.AreEqual(new DiskReadTime().Description, loggedHistograms[0].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Nanosecond.ToString(), loggedHistograms[0].Unit);
            Assert.AreEqual(5000000, loggedValues[0]);
            Assert.AreEqual(testMeterName, loggedHistograms[0].Meter.Name);
            Assert.AreEqual(new DiskWriteTime().Name, loggedHistograms[1].Name);
            Assert.AreEqual(new DiskWriteTime().Description, loggedHistograms[1].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Nanosecond.ToString(), loggedHistograms[1].Unit);
            Assert.AreEqual(6000000, loggedValues[1]);
            Assert.AreEqual(testMeterName, loggedHistograms[1].Meter.Name);
            Assert.AreEqual(new DiskReadTime().Name, loggedHistograms[2].Name);
            Assert.AreEqual(new DiskReadTime().Description, loggedHistograms[2].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Nanosecond.ToString(), loggedHistograms[2].Unit);
            Assert.AreEqual(7000000, loggedValues[2]);
            Assert.AreEqual(testMeterName, loggedHistograms[2].Meter.Name);
        }

        [Test]
        public void Begin_End_IntervalMetricsMappedToCounters()
        {
            var loggedCounters = new List<Counter<Int64>>();
            var loggedValues = new List<Int64>();
            mockMetricLoggingShim.AddCounter
            (
                Arg.Do<Counter<Int64>>((counter) => { loggedCounters.Add(counter); }),
                Arg.Do<Int64>((value) => { loggedValues.Add(value); })
            );
            mockGuidProvider.NewGuid().Returns
            (
                Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002")
            );
            mockStopWatch.ElapsedTicks.Returns<Int64>
            (
                10_000,   // Return value for first call to Begin()
                60_000,   // Return value for first call to End()
                70_000,   // Return value for second call to Begin()
                130_000,  // Return value for second call to End()
                140_000,  // Return value for third call to Begin()
                210_000   // Return value for third call to End()
            );
            testOpenTelemetryMetricLogger.Dispose();
            testOpenTelemetryMetricLogger = new TestOpenTelemetryMetricLogger
            (
                IntervalMetricBaseTimeUnit.Millisecond,
                true,
                testMeterOptions,
                testOtlpExporterConfigurationAction,
                OpenTelemetryMetricType.Counter,
                OpenTelemetryMetricType.Counter,
                mockStopWatch,
                mockGuidProvider,
                mockDateTime,
                mockMetricLoggingShim
            );

            Guid beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskReadTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskWriteTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskWriteTime());
            beginId = testOpenTelemetryMetricLogger.Begin(new DiskReadTime());
            testOpenTelemetryMetricLogger.End(beginId, new DiskReadTime());

            Assert.AreEqual(3, loggedCounters.Count);
            Assert.AreEqual(3, loggedValues.Count);
            Assert.AreEqual(new DiskReadTime().Name, loggedCounters[0].Name);
            Assert.AreEqual(new DiskReadTime().Description, loggedCounters[0].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Millisecond.ToString(), loggedCounters[0].Unit);
            Assert.AreEqual(5, loggedValues[0]);
            Assert.AreEqual(testMeterName, loggedCounters[0].Meter.Name);
            Assert.AreEqual(new DiskWriteTime().Name, loggedCounters[1].Name);
            Assert.AreEqual(new DiskWriteTime().Description, loggedCounters[1].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Millisecond.ToString(), loggedCounters[1].Unit);
            Assert.AreEqual(6, loggedValues[1]);
            Assert.AreEqual(testMeterName, loggedCounters[1].Meter.Name);
            Assert.AreEqual(new DiskReadTime().Name, loggedCounters[2].Name);
            Assert.AreEqual(new DiskReadTime().Description, loggedCounters[2].Description);
            Assert.AreEqual(IntervalMetricBaseTimeUnit.Millisecond.ToString(), loggedCounters[1].Unit);
            Assert.AreEqual(7, loggedValues[2]);
            Assert.AreEqual(testMeterName, loggedCounters[2].Meter.Name);
        }

        #region Private/Protected Methods

        /// <summary>
        /// Generates as UTC <see cref="System.DateTime"/> from the specified string containing a date in ISO format.
        /// </summary>
        /// <param name="isoFormattedDateString">The date string.</param>
        /// <returns>the DateTime.</returns>
        private System.DateTime GenerateUtcDateTime(String isoFormattedDateString)
        {
            var returnDateTime = System.DateTime.ParseExact(isoFormattedDateString, "yyyy-MM-dd HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
            return System.DateTime.SpecifyKind(returnDateTime, DateTimeKind.Utc);
        }

        #endregion

        #region Inner Classes

        private class TestOpenTelemetryMetricLogger : OpenTelemetryMetricLogger
        {
            /// <summary>The timestamp at which the stopwatch was started.</summary>
            public System.DateTime StartTime
            {
                get
                {
                    return startTime;
                }
            }

            /// <summary>
            /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.OpenTelemetry.UnitTests+TestOpenTelemetryMetricLogger class.
            /// </summary>
            /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
            /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
            /// <param name="meterOptions">The options to set on the underlying <see cref="Meter"/> class.</param>
            /// <param name="otlpExporterConfigurationAction">An action which configures the underlying OpenTelemetry exporter.</param>
            /// <param name="amountMetricMappedType">The type of OpenTelemetry metric that <see cref="AmountMetric">AmountMetrics</see> should be mapped to.</param>
            /// <param name="intervalMetricMappedType">The type of OpenTelemetry metric that <see cref="IntervalMetric">IntervalMetrics</see> should be mapped to.</param>
            /// <param name="stopWatch">A test (mock) <see cref="IStopwatch"/> object.</param>
            /// <param name="guidProvider">A test (mock) <see cref="IGuidProvider"/> object.</param>
            /// <param name="metricLoggingShim">A test (mock) <see cref="IOpenTelemetryMetricLoggingShim"/> object.</param>
            /// <param name="dateTime">A test (mock) <see cref="IDateTime"/> object.</param>
            /// <param name="metricLoggingShim">A test (mock) <see cref="IOpenTelemetryMetricLoggingShim"/> object.</param>
            public TestOpenTelemetryMetricLogger
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
                : base(intervalMetricBaseTimeUnit, intervalMetricChecking, meterOptions, otlpExporterConfigurationAction, amountMetricMappedType, intervalMetricMappedType, stopWatch, guidProvider, dateTime, metricLoggingShim)
            {
            }

            /// <summary>
            /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.OpenTelemetry.UnitTests+TestOpenTelemetryMetricLogger class.
            /// </summary>
            /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
            /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
            /// <param name="meterName">The name set on the underlying <see cref="Meter"/> class.</param>
            /// <param name="otlpExporterConfigurationAction">An action which configures the underlying OpenTelemetry exporter.</param>
            public TestOpenTelemetryMetricLogger(IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit, Boolean intervalMetricChecking, String meterName, Action<OtlpExporterOptions> otlpExporterConfigurationAction)
                : base(intervalMetricBaseTimeUnit, intervalMetricChecking, meterName, otlpExporterConfigurationAction)
            {
            }

            /// <summary>
            /// Initialises a new instance of the ApplicationMetrics.MetricLoggers.OpenTelemetry.UnitTests+TestOpenTelemetryMetricLogger class.
            /// </summary>
            /// <param name="intervalMetricBaseTimeUnit">The base time unit to use to log interval metrics.</param>
            /// <param name="intervalMetricChecking">Specifies whether an exception should be thrown if the correct order of interval metric logging is not followed (e.g. End() method called before Begin()).  Note that this parameter only has an effect when running in 'non-interleaved' mode.</param>
            /// <param name="meterOptions">The options to set on the underlying <see cref="Meter"/> class.</param>
            /// <param name="otlpExporterConfigurationAction">An action which configures the underlying OpenTelemetry exporter.</param>
            public TestOpenTelemetryMetricLogger(IntervalMetricBaseTimeUnit intervalMetricBaseTimeUnit, Boolean intervalMetricChecking, MeterOptions meterOptions, Action<OtlpExporterOptions> otlpExporterConfigurationAction)
                : base(intervalMetricBaseTimeUnit, intervalMetricChecking, meterOptions, otlpExporterConfigurationAction)
            {
            }
        }
        #endregion
    }
}
