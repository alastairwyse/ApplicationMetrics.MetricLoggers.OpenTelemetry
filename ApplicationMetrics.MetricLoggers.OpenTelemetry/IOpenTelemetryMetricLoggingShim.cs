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

using System.Diagnostics.Metrics;

namespace ApplicationMetrics.MetricLoggers.OpenTelemetry
{
    /// <summary>
    /// Defines methods which log OpenTelemetry metrics.
    /// </summary>
    /// <remarks>Acts as a <see href="https://en.wikipedia.org/wiki/Shim_(computing)">shim</see> to OpenTelemetry classes for use in unit testing.</remarks>
    public interface IOpenTelemetryMetricLoggingShim
    {
        /// <summary>
        /// Calls the <see cref="Counter{T}.Add(T)">Add()</see> method on the specified <see cref="Counter{T}"/>.
        /// </summary>
        /// <typeparam name="T">The numerical type of the measurement.</typeparam>
        /// <param name="counter">The counter to call Add() against.</param>
        /// <param name="value">The value to add.</param>
        void AddCounter<T>(Counter<T> counter, T value) where T: struct;

        /// <summary>
        /// Calls the <see cref="Gauge{T}.Record(T)">Record()</see> method on the specified <see cref="Gauge{T}"/>.
        /// </summary>
        /// <typeparam name="T">The numerical type of the measurement.</typeparam>
        /// <param name="gauge">The gauge to call Record() against.</param>
        /// <param name="value">The value to record.</param>
        void RecordGauge<T>(Gauge<T> gauge, T value) where T : struct;

        /// <summary>
        /// Calls the <see cref="Histogram{T}.Record(T)">Record()</see> method on the specified <see cref="Histogram{T}"/>.
        /// </summary>
        /// <typeparam name="T">The numerical type of the measurement.</typeparam>
        /// <param name="histogram">The histogram to call Record() against.</param>
        /// <param name="value">The value to record.</param>
        void RecordHistogram<T>(Histogram<T> histogram, T value) where T : struct;
    }
}
