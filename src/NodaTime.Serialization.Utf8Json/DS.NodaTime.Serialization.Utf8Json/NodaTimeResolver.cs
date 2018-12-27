using System;
using System.Collections.Generic;
using Utf8Json;

namespace NodaTime.Serialization.Utf8Json
{
    public class NodaTimeResolver : IJsonFormatterResolver
    {
        private readonly Dictionary<Type, IJsonFormatter> _resolverCache;

        public NodaTimeResolver(
            IDateTimeZoneProvider dateTimeZoneProvider,
            bool isoIntervals = false,
            bool isoDateIntervals = false,
            bool normalizedIsoPeriods = false)
        {
            NodaFormatters.CreateDateTimeZoneFormatter(dateTimeZoneProvider);

            _resolverCache = new Dictionary<Type, IJsonFormatter>
            {
                {typeof(ZonedDateTime), NodaFormatters.CreateZonedDateTimeFormatter(dateTimeZoneProvider)},
                {typeof(ZonedDateTime?), NodaFormatters.CreateNullableZonedDateTimeFormatter(dateTimeZoneProvider)},
                {typeof(DateTimeZone), NodaFormatters.CreateDateTimeZoneFormatter(dateTimeZoneProvider)},
                {typeof(Interval), isoIntervals ? NodaFormatters.IsoIntervalFormatter : NodaFormatters.IsoDateIntervalFormatter},
                {typeof(Interval?), NodaFormatters.NullableIntervalFormatter},
                {typeof(DateInterval), isoDateIntervals ? NodaFormatters.IsoDateIntervalFormatter : NodaFormatters.DateIntervalFormatter},
                {typeof(Period), normalizedIsoPeriods ? NodaFormatters.NormalizingIsoPeriodFormatter : NodaFormatters.RoundtripPeriodFormatter}
            };
        }

        public IJsonFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter ?? GetResolverFormatter<T>();
        }

        private IJsonFormatter<T> GetResolverFormatter<T>()
        {
            return _resolverCache.TryGetValue(typeof(T), out var formatter) ? (IJsonFormatter<T>) formatter : null;
        }

        private static class FormatterCache<T>
        {
            public static readonly IJsonFormatter<T> Formatter;

            static FormatterCache()
            {
                Formatter = (IJsonFormatter<T>)NodaTimeResolverHelper.GetFormatterWithVerify(typeof(T));
            }
        }
    }

    internal static class NodaTimeResolverHelper
    {
        static readonly Dictionary<Type, IJsonFormatter> DefaultFormatterMap = new Dictionary<Type, IJsonFormatter>
        {
            {typeof(Instant), NodaFormatters.InstantFormatter},
            {typeof(Instant?), NodaFormatters.NullableInstantFormatter},
            {typeof(LocalDate), NodaFormatters.LocalDateFormatter},
            {typeof(LocalDate?), NodaFormatters.NullableLocalDateFormatter},
            {typeof(LocalTime), NodaFormatters.LocalTimeFormatter},
            {typeof(LocalTime?), NodaFormatters.NullableLocalTimeFormatter},
            {typeof(LocalDateTime), NodaFormatters.LocalDateTimeFormatter},
            {typeof(LocalDateTime?), NodaFormatters.NullableLocalDateTimeFormatter},
            {typeof(Offset), NodaFormatters.OffsetFormatter},
            {typeof(Offset?), NodaFormatters.NullableOffsetFormatter},
            {typeof(OffsetDateTime), NodaFormatters.OffsetDateTimeFormatter},
            {typeof(OffsetDateTime?), NodaFormatters.NullableOffsetDateTimeFormatter},
            {typeof(Duration), NodaFormatters.DurationFormatter},
            {typeof(Duration?), NodaFormatters.DurationFormatter},
        };

        internal static object GetFormatterWithVerify(Type t)
        {
            IJsonFormatter formatter;
            return DefaultFormatterMap.TryGetValue(t, out formatter) ? formatter : null;
        }
    }
}