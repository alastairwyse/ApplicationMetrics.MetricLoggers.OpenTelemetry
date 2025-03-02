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
    /// Default implementation of <see cref="IOpenTelemetryMetricLoggngShim"/>.
    /// </summary>
    class DefaultOpenTelemetryMetricLoggngShim : IOpenTelemetryMetricLoggingShim
    {
        /// <inheritdoc/>
        public void AddCounter<T>(Counter<T> counter, T value) where T : struct
        {
            counter.Add(value);
        }

        /// <inheritdoc/>
        public void RecordGauge<T>(Gauge<T> gauge, T value) where T : struct
        {
            gauge.Record(value);
        }

        /// <inheritdoc/>
        public void RecordHistogram<T>(Histogram<T> histogram, T value) where T : struct
        {
            histogram.Record(value);
        }
    }
}
