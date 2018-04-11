using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// A base class for controlling Cpu limits.
    /// </summary>
    public abstract class CpuLimit
    {
        internal CpuLimit ()
        {
        }

        internal virtual void GetLimits ( out Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuLimits )
        {
            cpuLimits = default;
            cpuLimits.ControlFlags = Interop.Kernel32.JobObjectCpuControl.Enable;
        }
    }

    /// <summary>
    /// A base class for CPU limits that can be hard capped.
    /// </summary>
    public abstract class CpuCappable : CpuLimit
    {
        internal CpuCappable ( bool hardLimit )
        {
            HardLimit = hardLimit;
        }

        internal CpuCappable ( in Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuInfo )
        {
            HardLimit = (cpuInfo.ControlFlags & Interop.Kernel32.JobObjectCpuControl.HardCap) != 0;
        }

        internal override void GetLimits ( out Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuLimits )
        {
            base.GetLimits(out cpuLimits);
            if (HardLimit) cpuLimits.ControlFlags |= Interop.Kernel32.JobObjectCpuControl.HardCap;
        }

        /// <summary>
        /// Gets a value that indicates whether the CPU rate is forcibily capped.
        /// </summary>
        public bool HardLimit { get; }
    }

    /// <summary>
    /// Limits the CPU rate based on a fixed percentage.
    /// </summary>
    public sealed class CpuRateLimit : CpuCappable
    {
        private readonly int _rateLimit;

        /// <summary>
        /// Creates and instance of <see cref="CpuRateLimit"/>.
        /// </summary>
        /// <param name="percentageRate">A percentage (that is greater than <c>0</c> and less than or equal to <c>100</c>) of the overall system 
        /// CPU that the job is allowed to use.</param>
        /// <param name="isHardLimit">When <c>true</c>, threads in the job are not allowed to run until the next measurement interval when the
        /// <paramref name="percentageRate"/> is exceeded.</param>
        public CpuRateLimit ( decimal percentageRate, bool isHardLimit )
            : base(isHardLimit)
        {
            int scaledRate = (int)decimal.Truncate(percentageRate * 100m);
            if ((scaledRate <= 0) || (scaledRate > 10000))
                throw new ArgumentOutOfRangeException(nameof(percentageRate));

            _rateLimit = scaledRate;
        }

        internal CpuRateLimit ( in Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuInfo )
            : base(cpuInfo)
        {
            _rateLimit = (int)cpuInfo.CpuRate;
        }

        internal override void GetLimits ( out Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuLimits )
        {
            base.GetLimits(out cpuLimits);
            cpuLimits.CpuRate = (uint)_rateLimit;
        }

        /// <summary>
        /// Gets the percentage of the overall system CPU time the job is allowed to use.
        /// </summary>
        public decimal Rate { get { return (decimal)_rateLimit / 100m; } }
    }

    /// <summary>
    /// Defines a weighted CPU rate.
    /// </summary>
    public sealed class CpuWeightedRateLimit : CpuCappable
    {
        /// <summary>
        /// Creates an instance of <see cref="CpuWeightedRateLimit"/>.
        /// </summary>
        /// <param name="weight">The weight associated with the job.</param>
        /// <param name="isHardLimit">When <c>true</c>, the threads in the job are prevented from running until the next
        /// measurement cycle when the CPU usage exceeds the job's weighted CPU rate.</param>
        public CpuWeightedRateLimit ( int weight, bool isHardLimit )
            : base(isHardLimit)
        {
            if ((weight <= 0) || (weight > 9))
                throw new ArgumentOutOfRangeException(nameof(weight));

            Weight = weight;
        }

        internal CpuWeightedRateLimit ( in Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuInfo )
            : base(cpuInfo)
        {
            Weight = (int)cpuInfo.Weight;
        }

        internal override void GetLimits ( out Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuLimits )
        {
            base.GetLimits(out cpuLimits);
            cpuLimits.ControlFlags |= Interop.Kernel32.JobObjectCpuControl.WeightBased;
            cpuLimits.Weight = (ushort)Weight;
        }

        /// <summary>
        /// Gets the weighted value of the CPU rate.
        /// </summary>
        public int Weight { get; }
    }

    /// <summary>
    /// A CPU rate limit based on a minimum and maximum percentage rate.
    /// </summary>
    public sealed class CpuMinMaxRateLimit : CpuLimit
    {
        private readonly int _minRateLimit;
        private readonly int _maxRateLimit;

        /// <summary>
        /// Creates an instance of <see cref="CpuMinMaxRateLimit"/>.
        /// </summary>
        /// <param name="minPercentageRate">The minimum percentage of system CPU time reserved for this job.</param>
        /// <param name="maxPercentageRate">The maximum percentage of system CPU time reserved for this job.</param>
        public CpuMinMaxRateLimit ( decimal minPercentageRate, decimal maxPercentageRate )
        {
            _minRateLimit = (int)decimal.Truncate(minPercentageRate * 100m);
            _maxRateLimit = (int)decimal.Truncate(maxPercentageRate * 100m);

            if ((_minRateLimit < 0) || (_minRateLimit > 10000))
                throw new ArgumentOutOfRangeException(nameof(minPercentageRate));

            if ((_maxRateLimit < 0) || (_maxRateLimit > 10000))
                throw new ArgumentOutOfRangeException(nameof(maxPercentageRate));
        }

        internal CpuMinMaxRateLimit ( in Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuInfo )
        {
            _minRateLimit = cpuInfo.MinRate;
            _maxRateLimit = cpuInfo.MaxRate;
        }

        internal override void GetLimits ( out Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuLimits )
        {
            base.GetLimits(out cpuLimits);
            cpuLimits.ControlFlags |= Interop.Kernel32.JobObjectCpuControl.MinMaxRate;
            cpuLimits.MinRate = (ushort)_minRateLimit;
            cpuLimits.MaxRate = (ushort)_maxRateLimit;
        }

        /// <summary>
        /// Gets the minimum percentage of CPU time reserved for this job.
        /// </summary>
        public decimal MinimumRate { get { return (decimal)_minRateLimit / 100m; } }

        /// <summary>
        /// Gets the maximum percentage of CPU time reserved for this job.
        /// </summary>
        public decimal MaximumRate { get { return (decimal)_maxRateLimit / 100m; } }
    }
}
