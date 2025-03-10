﻿/*
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
    /// Types of metrics defined in OpenTelemetry.
    /// </summary>
    public enum OpenTelemetryMetricType
    {
        /// <summary>An OpenTelemetry <see cref="Counter{T}"/>.</summary>
        Counter,
        /// <summary>An OpenTelemetry <see cref="Histogram{T}"/>.</summary>
        Historgram
    }
}
