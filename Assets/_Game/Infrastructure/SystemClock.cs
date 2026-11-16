using System;
using OutOfOffice.Core.Services;

namespace OutOfOffice.Infrastructure
{
    /// <summary>IClock'un cihaz saati karşılığı (yerel takvim).</summary>
    public sealed class SystemClock : IClock
    {
        static readonly DateTime Epoch = new DateTime(1970, 1, 1);

        public int Today { get { return (int)(DateTime.Now.Date - Epoch).TotalDays; } }
    }
}
