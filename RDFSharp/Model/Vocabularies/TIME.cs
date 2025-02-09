
/*
   Copyright 2012-2025 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies
    /// </summary>
    public static partial class RDFVocabulary
    {
        #region TIME
        /// <summary>
        /// TIME represents the OWL-Time vocabulary
        /// </summary>
        public static class TIME
        {
            #region Properties
            /// <summary>
            /// time
            /// </summary>
            public const string PREFIX = "time";

            /// <summary>
            /// http://www.w3.org/2006/time#
            /// </summary>
            public const string BASE_URI = "http://www.w3.org/2006/time#";

            /// <summary>
            /// http://www.w3.org/2006/time#
            /// </summary>
            public const string DEREFERENCE_URI = "http://www.w3.org/2006/time#";

            /// <summary>
            /// time:DateTimeDescription
            /// </summary>
            public static readonly RDFResource DATETIME_DESCRIPTION = new RDFResource(string.Concat(BASE_URI, "DateTimeDescription"));

            /// <summary>
            /// time:DateTimeInterval
            /// </summary>
            public static readonly RDFResource DATETIME_INTERVAL = new RDFResource(string.Concat(BASE_URI, "DateTimeInterval"));

            /// <summary>
            /// time:DayOfWeek
            /// </summary>
            public static readonly RDFResource DAY_OF_WEEK_CLASS = new RDFResource(string.Concat(BASE_URI, "DayOfWeek"));

            /// <summary>
            /// time:Duration
            /// </summary>
            public static readonly RDFResource DURATION = new RDFResource(string.Concat(BASE_URI, "Duration"));

            /// <summary>
            /// time:DurationDescription
            /// </summary>
            public static readonly RDFResource DURATION_DESCRIPTION = new RDFResource(string.Concat(BASE_URI, "DurationDescription"));

            /// <summary>
            /// time:GeneralDateTimeDescription
            /// </summary>
            public static readonly RDFResource GENERAL_DATETIME_DESCRIPTION = new RDFResource(string.Concat(BASE_URI, "GeneralDateTimeDescription"));

            /// <summary>
            /// time:GeneralDurationDescription
            /// </summary>
            public static readonly RDFResource GENERAL_DURATION_DESCRIPTION = new RDFResource(string.Concat(BASE_URI, "GeneralDurationDescription"));

            /// <summary>
            /// time:Instant
            /// </summary>
            public static readonly RDFResource INSTANT = new RDFResource(string.Concat(BASE_URI, "Instant"));

            /// <summary>
            /// time:Interval
            /// </summary>
            public static readonly RDFResource INTERVAL = new RDFResource(string.Concat(BASE_URI, "Interval"));

            /// <summary>
            /// time:MonthOfYear
            /// </summary>
            public static readonly RDFResource MONTH_OF_YEAR_CLASS = new RDFResource(string.Concat(BASE_URI, "MonthOfYear"));

            /// <summary>
            /// time:ProperInterval
            /// </summary>
            public static readonly RDFResource PROPER_INTERVAL = new RDFResource(string.Concat(BASE_URI, "ProperInterval"));

            /// <summary>
            /// time:TemporalDuration
            /// </summary>
            public static readonly RDFResource TEMPORAL_DURATION = new RDFResource(string.Concat(BASE_URI, "TemporalDuration"));

            /// <summary>
            /// time:TemporalEntity
            /// </summary>
            public static readonly RDFResource TEMPORAL_ENTITY = new RDFResource(string.Concat(BASE_URI, "TemporalEntity"));

            /// <summary>
            /// time:TemporalPosition
            /// </summary>
            public static readonly RDFResource TEMPORAL_POSITION = new RDFResource(string.Concat(BASE_URI, "TemporalPosition"));

            /// <summary>
            /// time:TemporalUnit
            /// </summary>
            public static readonly RDFResource TEMPORAL_UNIT = new RDFResource(string.Concat(BASE_URI, "TemporalUnit"));

            /// <summary>
            /// time:TimePosition
            /// </summary>
            public static readonly RDFResource TIME_POSITION = new RDFResource(string.Concat(BASE_URI, "TimePosition"));

            /// <summary>
            /// time:TimeZone
            /// </summary>
            public static readonly RDFResource TIMEZONE_CLASS = new RDFResource(string.Concat(BASE_URI, "TimeZone"));

            /// <summary>
            /// time:TRS
            /// </summary>
            public static readonly RDFResource TRS = new RDFResource(string.Concat(BASE_URI, "TRS"));

            /// <summary>
            /// time:after
            /// </summary>
            public static readonly RDFResource AFTER = new RDFResource(string.Concat(BASE_URI, "after"));

            /// <summary>
            /// time:before
            /// </summary>
            public static readonly RDFResource BEFORE = new RDFResource(string.Concat(BASE_URI, "before"));

            /// <summary>
            /// time:day
            /// </summary>
            public static readonly RDFResource DAY = new RDFResource(string.Concat(BASE_URI, "day"));

            /// <summary>
            /// time:dayOfWeek
            /// </summary>
            public static readonly RDFResource DAY_OF_WEEK = new RDFResource(string.Concat(BASE_URI, "dayOfWeek"));

            /// <summary>
            /// time:dayOfYear
            /// </summary>
            public static readonly RDFResource DAY_OF_YEAR = new RDFResource(string.Concat(BASE_URI, "dayOfYear"));

            /// <summary>
            /// time:days
            /// </summary>
            public static readonly RDFResource DAYS = new RDFResource(string.Concat(BASE_URI, "days"));

            /// <summary>
            /// time:disjoint
            /// </summary>
            public static readonly RDFResource DISJOINT = new RDFResource(string.Concat(BASE_URI, "disjoint"));

            /// <summary>
            /// time:equals
            /// </summary>
            public static readonly RDFResource EQUALS = new RDFResource(string.Concat(BASE_URI, "equals"));

            /// <summary>
            /// time:hasBeginning
            /// </summary>
            public static readonly RDFResource HAS_BEGINNING = new RDFResource(string.Concat(BASE_URI, "hasBeginning"));

            /// <summary>
            /// time:hasDateTimeDescription
            /// </summary>
            public static readonly RDFResource HAS_DATETIME_DESCRIPTION = new RDFResource(string.Concat(BASE_URI, "hasDateTimeDescription"));

            /// <summary>
            /// time:hasDuration
            /// </summary>
            public static readonly RDFResource HAS_DURATION = new RDFResource(string.Concat(BASE_URI, "hasDuration"));

            /// <summary>
            /// time:hasDurationDescription
            /// </summary>
            public static readonly RDFResource HAS_DURATION_DESCRIPTION = new RDFResource(string.Concat(BASE_URI, "hasDurationDescription"));

            /// <summary>
            /// time:hasEnd
            /// </summary>
            public static readonly RDFResource HAS_END = new RDFResource(string.Concat(BASE_URI, "hasEnd"));

            /// <summary>
            /// time:hasInside
            /// </summary>
            public static readonly RDFResource HAS_INSIDE = new RDFResource(string.Concat(BASE_URI, "hasInside"));

            /// <summary>
            /// time:hasTemporalDuration
            /// </summary>
            public static readonly RDFResource HAS_TEMPORAL_DURATION = new RDFResource(string.Concat(BASE_URI, "hasTemporalDuration"));

            /// <summary>
            /// time:hasTime
            /// </summary>
            public static readonly RDFResource HAS_TIME = new RDFResource(string.Concat(BASE_URI, "hasTime"));

            /// <summary>
            /// time:hasTRS
            /// </summary>
            public static readonly RDFResource HAS_TRS = new RDFResource(string.Concat(BASE_URI, "hasTRS"));

            /// <summary>
            /// time:hasXSDDuration
            /// </summary>
            public static readonly RDFResource HAS_XSD_DURATION = new RDFResource(string.Concat(BASE_URI, "hasXSDDuration"));

            /// <summary>
            /// time:hour
            /// </summary>
            public static readonly RDFResource HOUR = new RDFResource(string.Concat(BASE_URI, "hour"));

            /// <summary>
            /// time:hours
            /// </summary>
            public static readonly RDFResource HOURS = new RDFResource(string.Concat(BASE_URI, "hours"));

            /// <summary>
            /// time:inDateTime
            /// </summary>
            public static readonly RDFResource IN_DATETIME = new RDFResource(string.Concat(BASE_URI, "inDateTime"));

            /// <summary>
            /// time:inside
            /// </summary>
            public static readonly RDFResource INSIDE = new RDFResource(string.Concat(BASE_URI, "inside"));

            /// <summary>
            /// time:inTemporalPosition
            /// </summary>
            public static readonly RDFResource IN_TEMPORAL_POSITION = new RDFResource(string.Concat(BASE_URI, "inTemporalPosition"));

            /// <summary>
            /// time:intervalAfter
            /// </summary>
            public static readonly RDFResource INTERVAL_AFTER = new RDFResource(string.Concat(BASE_URI, "intervalAfter"));

            /// <summary>
            /// time:intervalBefore
            /// </summary>
            public static readonly RDFResource INTERVAL_BEFORE = new RDFResource(string.Concat(BASE_URI, "intervalBefore"));

            /// <summary>
            /// time:intervalContains
            /// </summary>
            public static readonly RDFResource INTERVAL_CONTAINS = new RDFResource(string.Concat(BASE_URI, "intervalContains"));

            /// <summary>
            /// time:intervalDisjoint
            /// </summary>
            public static readonly RDFResource INTERVAL_DISJOINT = new RDFResource(string.Concat(BASE_URI, "intervalDisjoint"));

            /// <summary>
            /// time:intervalDuring
            /// </summary>
            public static readonly RDFResource INTERVAL_DURING = new RDFResource(string.Concat(BASE_URI, "intervalDuring"));

            /// <summary>
            /// time:intervalEquals
            /// </summary>
            public static readonly RDFResource INTERVAL_EQUALS = new RDFResource(string.Concat(BASE_URI, "intervalEquals"));

            /// <summary>
            /// time:intervalFinishedBy
            /// </summary>
            public static readonly RDFResource INTERVAL_FINISHED_BY = new RDFResource(string.Concat(BASE_URI, "intervalFinishedBy"));

            /// <summary>
            /// time:intervalFinishes
            /// </summary>
            public static readonly RDFResource INTERVAL_FINISHES = new RDFResource(string.Concat(BASE_URI, "intervalFinishes"));

            /// <summary>
            /// time:intervalIn
            /// </summary>
            public static readonly RDFResource INTERVAL_IN = new RDFResource(string.Concat(BASE_URI, "intervalIn"));

            /// <summary>
            /// time:intervalMeets
            /// </summary>
            public static readonly RDFResource INTERVAL_MEETS = new RDFResource(string.Concat(BASE_URI, "intervalMeets"));

            /// <summary>
            /// time:intervalMetBy
            /// </summary>
            public static readonly RDFResource INTERVAL_MET_BY = new RDFResource(string.Concat(BASE_URI, "intervalMetBy"));

            /// <summary>
            /// time:intervalOverlappedBy
            /// </summary>
            public static readonly RDFResource INTERVAL_OVERLAPPED_BY = new RDFResource(string.Concat(BASE_URI, "intervalOverlappedBy"));

            /// <summary>
            /// time:intervalOverlaps
            /// </summary>
            public static readonly RDFResource INTERVAL_OVERLAPS = new RDFResource(string.Concat(BASE_URI, "intervalOverlaps"));

            /// <summary>
            /// time:intervalStartedBy
            /// </summary>
            public static readonly RDFResource INTERVAL_STARTED_BY = new RDFResource(string.Concat(BASE_URI, "intervalStartedBy"));

            /// <summary>
            /// time:intervalStarts
            /// </summary>
            public static readonly RDFResource INTERVAL_STARTS = new RDFResource(string.Concat(BASE_URI, "intervalStarts"));

            /// <summary>
            /// time:inTimePosition
            /// </summary>
            public static readonly RDFResource IN_TIME_POSITION = new RDFResource(string.Concat(BASE_URI, "inTimePosition"));

            /// <summary>
            /// time:inXSDDate
            /// </summary>
            public static readonly RDFResource IN_XSD_DATE = new RDFResource(string.Concat(BASE_URI, "inXSDDate"));

            /// <summary>
            /// time:inXSDDateTime
            /// </summary>
            public static readonly RDFResource IN_XSD_DATETIME = new RDFResource(string.Concat(BASE_URI, "inXSDDateTime"));

            /// <summary>
            /// time:inXSDDateTimeStamp
            /// </summary>
            public static readonly RDFResource IN_XSD_DATETIMESTAMP = new RDFResource(string.Concat(BASE_URI, "inXSDDateTimeStamp"));

            /// <summary>
            /// time:inXSDgYear
            /// </summary>
            public static readonly RDFResource IN_XSD_GYEAR = new RDFResource(string.Concat(BASE_URI, "inXSDgYear"));

            /// <summary>
            /// time:inXSDgYearMonth
            /// </summary>
            public static readonly RDFResource IN_XSD_GYEARMONTH = new RDFResource(string.Concat(BASE_URI, "inXSDgYearMonth"));

            /// <summary>
            /// time:minute
            /// </summary>
            public static readonly RDFResource MINUTE = new RDFResource(string.Concat(BASE_URI, "minute"));

            /// <summary>
            /// time:minutes
            /// </summary>
            public static readonly RDFResource MINUTES = new RDFResource(string.Concat(BASE_URI, "minutes"));

            /// <summary>
            /// time:month
            /// </summary>
            public static readonly RDFResource MONTH = new RDFResource(string.Concat(BASE_URI, "month"));

            /// <summary>
            /// time:monthOfYear
            /// </summary>
            public static readonly RDFResource MONTH_OF_YEAR = new RDFResource(string.Concat(BASE_URI, "monthOfYear"));

            /// <summary>
            /// time:months
            /// </summary>
            public static readonly RDFResource MONTHS = new RDFResource(string.Concat(BASE_URI, "months"));

            /// <summary>
            /// time:nominalPosition
            /// </summary>
            public static readonly RDFResource NOMINAL_POSITION = new RDFResource(string.Concat(BASE_URI, "nominalPosition"));

            /// <summary>
            /// time:notDisjoint
            /// </summary>
            public static readonly RDFResource NOT_DISJOINT = new RDFResource(string.Concat(BASE_URI, "notDisjoint"));

            /// <summary>
            /// time:numericDuration
            /// </summary>
            public static readonly RDFResource NUMERIC_DURATION = new RDFResource(string.Concat(BASE_URI, "numericDuration"));

            /// <summary>
            /// time:numericPosition
            /// </summary>
            public static readonly RDFResource NUMERIC_POSITION = new RDFResource(string.Concat(BASE_URI, "numericPosition"));

            /// <summary>
            /// time:second
            /// </summary>
            public static readonly RDFResource SECOND = new RDFResource(string.Concat(BASE_URI, "second"));

            /// <summary>
            /// time:seconds
            /// </summary>
            public static readonly RDFResource SECONDS = new RDFResource(string.Concat(BASE_URI, "seconds"));

            /// <summary>
            /// time:timeZone
            /// </summary>
            public static readonly RDFResource TIMEZONE = new RDFResource(string.Concat(BASE_URI, "timeZone"));

            /// <summary>
            /// time:unitType
            /// </summary>
            public static readonly RDFResource UNIT_TYPE = new RDFResource(string.Concat(BASE_URI, "unitType"));

            /// <summary>
            /// time:week
            /// </summary>
            public static readonly RDFResource WEEK = new RDFResource(string.Concat(BASE_URI, "week"));

            /// <summary>
            /// time:weeks
            /// </summary>
            public static readonly RDFResource WEEKS = new RDFResource(string.Concat(BASE_URI, "weeks"));

            /// <summary>
            /// time:xsdDateTime
            /// </summary>
            public static readonly RDFResource XSD_DATETIME = new RDFResource(string.Concat(BASE_URI, "xsdDateTime"));

            /// <summary>
            /// time:year
            /// </summary>
            public static readonly RDFResource YEAR = new RDFResource(string.Concat(BASE_URI, "year"));

            /// <summary>
            /// time:years
            /// </summary>
            public static readonly RDFResource YEARS = new RDFResource(string.Concat(BASE_URI, "years"));

            /// <summary>
            /// time:generalDay
            /// </summary>
            public static readonly RDFResource GENERAL_DAY = new RDFResource(string.Concat(BASE_URI, "generalDay"));

            /// <summary>
            /// time:generalMonth
            /// </summary>
            public static readonly RDFResource GENERAL_MONTH = new RDFResource(string.Concat(BASE_URI, "generalMonth"));

            /// <summary>
            /// time:generalYear
            /// </summary>
            public static readonly RDFResource GENERAL_YEAR = new RDFResource(string.Concat(BASE_URI, "generalYear"));

            /// <summary>
            /// time:Friday
            /// </summary>
            public static readonly RDFResource FRIDAY = new RDFResource(string.Concat(BASE_URI, "Friday"));

            /// <summary>
            /// time:Monday
            /// </summary>
            public static readonly RDFResource MONDAY = new RDFResource(string.Concat(BASE_URI, "Monday"));

            /// <summary>
            /// time:Saturday
            /// </summary>
            public static readonly RDFResource SATURDAY = new RDFResource(string.Concat(BASE_URI, "Saturday"));

            /// <summary>
            /// time:Sunday
            /// </summary>
            public static readonly RDFResource SUNDAY = new RDFResource(string.Concat(BASE_URI, "Sunday"));

            /// <summary>
            /// time:Thursday
            /// </summary>
            public static readonly RDFResource THURSDAY = new RDFResource(string.Concat(BASE_URI, "Thursday"));

            /// <summary>
            /// time:Tuesday
            /// </summary>
            public static readonly RDFResource TUESDAY = new RDFResource(string.Concat(BASE_URI, "Tuesday"));

            /// <summary>
            /// time:Wednesday
            /// </summary>
            public static readonly RDFResource WEDNESDAY = new RDFResource(string.Concat(BASE_URI, "Wednesday"));

            /// <summary>
            /// time:unitCentury
            /// </summary>
            public static readonly RDFResource UNIT_CENTURY = new RDFResource(string.Concat(BASE_URI, "unitCentury"));

            /// <summary>
            /// time:unitDay
            /// </summary>
            public static readonly RDFResource UNIT_DAY = new RDFResource(string.Concat(BASE_URI, "unitDay"));

            /// <summary>
            /// time:unitDecade
            /// </summary>
            public static readonly RDFResource UNIT_DECADE = new RDFResource(string.Concat(BASE_URI, "unitDecade"));

            /// <summary>
            /// time:unitHour
            /// </summary>
            public static readonly RDFResource UNIT_HOUR = new RDFResource(string.Concat(BASE_URI, "unitHour"));

            /// <summary>
            /// time:unitMillenium
            /// </summary>
            public static readonly RDFResource UNIT_MILLENIUM = new RDFResource(string.Concat(BASE_URI, "unitMillenium"));

            /// <summary>
            /// time:unitMinute
            /// </summary>
            public static readonly RDFResource UNIT_MINUTE = new RDFResource(string.Concat(BASE_URI, "unitMinute"));

            /// <summary>
            /// time:unitMonth
            /// </summary>
            public static readonly RDFResource UNIT_MONTH = new RDFResource(string.Concat(BASE_URI, "unitMonth"));

            /// <summary>
            /// time:unitSecond
            /// </summary>
            public static readonly RDFResource UNIT_SECOND = new RDFResource(string.Concat(BASE_URI, "unitSecond"));

            /// <summary>
            /// time:unitWeek
            /// </summary>
            public static readonly RDFResource UNIT_WEEK = new RDFResource(string.Concat(BASE_URI, "unitWeek"));

            /// <summary>
            /// time:unitYear
            /// </summary>
            public static readonly RDFResource UNIT_YEAR = new RDFResource(string.Concat(BASE_URI, "unitYear"));
            #endregion

            #region Extended Properties
            /// <summary>
            /// GREG represents the TIME extension for Gregorian calendar
            /// </summary>
            public static class GREG
            {
                /// <summary>
                /// greg
                /// </summary>
                public const string PREFIX = "greg";

                /// <summary>
                /// http://www.w3.org/ns/time/gregorian#
                /// </summary>
                public const string BASE_URI = "http://www.w3.org/ns/time/gregorian#";

                /// <summary>
                /// http://www.w3.org/2006/time#
                /// </summary>
                public const string DEREFERENCE_URI = "http://www.w3.org/ns/time/gregorian#";

                /// <summary>
                /// greg:January
                /// </summary>
                public static readonly RDFResource JANUARY = new RDFResource(string.Concat(BASE_URI, "January"));

                /// <summary>
                /// greg:February
                /// </summary>
                public static readonly RDFResource FEBRUARY = new RDFResource(string.Concat(BASE_URI, "February"));

                /// <summary>
                /// greg:March
                /// </summary>
                public static readonly RDFResource MARCH = new RDFResource(string.Concat(BASE_URI, "March"));

                /// <summary>
                /// greg:April
                /// </summary>
                public static readonly RDFResource APRIL = new RDFResource(string.Concat(BASE_URI, "April"));

                /// <summary>
                /// greg:May
                /// </summary>
                public static readonly RDFResource MAY = new RDFResource(string.Concat(BASE_URI, "May"));

                /// <summary>
                /// greg:June
                /// </summary>
                public static readonly RDFResource JUNE = new RDFResource(string.Concat(BASE_URI, "June"));

                /// <summary>
                /// greg:July
                /// </summary>
                public static readonly RDFResource JULY = new RDFResource(string.Concat(BASE_URI, "July"));

                /// <summary>
                /// greg:August
                /// </summary>
                public static readonly RDFResource AUGUST = new RDFResource(string.Concat(BASE_URI, "August"));

                /// <summary>
                /// greg:September
                /// </summary>
                public static readonly RDFResource SEPTEMBER = new RDFResource(string.Concat(BASE_URI, "September"));

                /// <summary>
                /// greg:October
                /// </summary>
                public static readonly RDFResource OCTOBER = new RDFResource(string.Concat(BASE_URI, "October"));

                /// <summary>
                /// greg:November
                /// </summary>
                public static readonly RDFResource NOVEMBER = new RDFResource(string.Concat(BASE_URI, "November"));

                /// <summary>
                /// greg:December
                /// </summary>
                public static readonly RDFResource DECEMBER = new RDFResource(string.Concat(BASE_URI, "December"));
            }

            /// <summary>
            /// THORS represents the TIME extension for modeling temporal hierarchical ordinal reference systems
            /// </summary>
            public static class THORS
            {
                /// <summary>
                /// thors
                /// </summary>
                public const string PREFIX = "thors";

                /// <summary>
                /// http://resource.geosciml.org/ontology/timescale/thors#
                /// </summary>
                public const string BASE_URI = "http://resource.geosciml.org/ontology/timescale/thors#";

                /// <summary>
                /// http://resource.geosciml.org/ontology/timescale/thors
                /// </summary>
                public const string DEREFERENCE_URI = "http://resource.geosciml.org/ontology/timescale/thors#";

                /// <summary>
                /// thors:Era
                /// </summary>
                public static readonly RDFResource ERA = new RDFResource(string.Concat(BASE_URI, "Era"));

                /// <summary>
                /// thors:EraBoundary
                /// </summary>
                public static readonly RDFResource ERA_BOUNDARY = new RDFResource(string.Concat(BASE_URI, "EraBoundary"));

                /// <summary>
                /// thors:ReferenceSystem
                /// </summary>
                public static readonly RDFResource REFERENCE_SYSTEM = new RDFResource(string.Concat(BASE_URI, "ReferenceSystem"));

                /// <summary>
                /// thors:begin
                /// </summary>
                public static readonly RDFResource BEGIN = new RDFResource(string.Concat(BASE_URI, "begin"));

                /// <summary>
                /// thors:component
                /// </summary>
                public static readonly RDFResource COMPONENT = new RDFResource(string.Concat(BASE_URI, "component"));

                /// <summary>
                /// thors:end
                /// </summary>
                public static readonly RDFResource END = new RDFResource(string.Concat(BASE_URI, "end"));

                /// <summary>
                /// thors:member
                /// </summary>
                public static readonly RDFResource MEMBER = new RDFResource(string.Concat(BASE_URI, "member"));

                /// <summary>
                /// thors:nextEra
                /// </summary>
                public static readonly RDFResource NEXT_ERA = new RDFResource(string.Concat(BASE_URI, "nextEra"));

                /// <summary>
                /// thors:previousEra
                /// </summary>
                public static readonly RDFResource PREVIOUS_ERA = new RDFResource(string.Concat(BASE_URI, "previousEra"));

                /// <summary>
                /// thors:referencePoint
                /// </summary>
                public static readonly RDFResource REFERENCE_POINT = new RDFResource(string.Concat(BASE_URI, "referencePoint"));

                /// <summary>
                /// thors:system
                /// </summary>
                public static readonly RDFResource SYSTEM = new RDFResource(string.Concat(BASE_URI, "system"));

                /// <summary>
                /// thors:positionalUncertainty
                /// </summary>
                public static readonly RDFResource POSITIONAL_UNCERTAINTY = new RDFResource(string.Concat(BASE_URI, "positionalUncertainty"));
            }
            #endregion
        }
        #endregion
    }
}