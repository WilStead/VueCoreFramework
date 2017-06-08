import { abstractField } from 'vue-form-generator';
import * as moment from 'moment';

export default {
    mixins: [abstractField],
    data() {
        let formats: Array<string> = this.schema.inputType.split(':');
        let showYear = formats.indexOf('y') !== -1;
        let showMonth = formats.indexOf('M') !== -1;
        let showDay = formats.indexOf('d') !== -1;
        let showHour = formats.indexOf('h') !== -1;
        let showMinute = formats.indexOf('m') !== -1;
        let showSecond = formats.indexOf('s') !== -1;

        let max = moment.duration(this.schema.max);
        let min = moment.duration(this.schema.min);

        let yearMax = this.floorYearAbs(max);
        let yearMin = this.floorYearAbs(min);
        // Even if specified as shown, do not show the field if
        // min and max are both 0, since the field would be useless.
        if (yearMax === 0 && yearMin === 0) showYear = false;

        let monthMax = showYear
            ? 12 : this.floorMonthAbs(max);
        let monthMin = showYear
            ? -1 : this.floorMonthAbs(min);
        // Min and max will only be 0 here if year is also hidden,
        // since a year field will cause the min/max to be overridden
        // with the overflow range: -1-12.
        if (monthMax === 0 && monthMin === 0) showMonth = false;

        let dayMax = showYear || showMonth
            ? 31 : this.floorDayAbs(max);
        let dayMin = showYear || showMonth
            ? -1 : this.floorDayAbs(min);
        if (dayMax === 0 && dayMin === 0) showDay = false;

        let hourMax = showYear || showMonth || showDay
            ? 24 : this.floorHourAbs(max);
        let hourMin = showYear || showMonth || showDay
            ? -1 : this.floorHourAbs(min);
        if (hourMax === 0 && hourMin === 0) showHour = false;

        let minuteMax = showYear || showMonth || showDay || showHour
            ? 60 : this.floorMinuteAbs(max);
        let minuteMin = showYear || showMonth || showDay || showHour
            ? -1 : this.floorMinuteAbs(min);
        if (minuteMax === 0 && minuteMin === 0) showMinute = false;

        let secondMax = showYear || showMonth || showDay || showHour || showMinute
            ? 60 : moment.duration(max).asSeconds();
        let secondMin = showYear || showMonth || showDay || showHour || showMinute
            ? -1 : moment.duration(min).asSeconds();

        // Ensure all units in between specified units are shown,
        // even if not specified, to avoid overflow from a lower unit
        // not being reflected in the next-higher unit.
        let lowest = 7;
        let highest = 0;
        if (showYear) {
            if (lowest > 6) lowest = 6;
            if (highest < 6) highest = 6;
        }
        if (showMonth) {
            if (lowest > 5) lowest = 5;
            if (highest < 5) highest = 5;
        }
        if (showDay) {
            if (lowest > 4) lowest = 4;
            if (highest < 4) highest = 4;
        }
        if (showHour) {
            if (lowest > 3) lowest = 3;
            if (highest < 3) highest = 3;
        }
        if (showMinute) {
            if (lowest > 2) lowest = 2;
            if (highest < 2) highest = 2;
        }
        if (showSecond) {
            if (lowest > 1) lowest = 1;
            if (highest < 1) highest = 1;
        }
        if (!showMonth && lowest < 5 && highest > 5) showMonth = true;
        if (!showDay && lowest < 4 && highest > 4) showDay = true;
        if (!showHour && lowest < 3 && highest > 3) showHour = true;
        if (!showMinute && lowest < 2 && highest > 2) showMinute = true;

        let duration = moment.duration(this.value);
        let yearValue = duration.years();
        let monthValue = showYear ? duration.months() : this.floorMonthAbs(duration);
        let dayValue = showYear || showMonth
            ? duration.days() : this.floorDayAbs(duration);
        let hourValue = showYear || showMonth || showDay
            ? duration.hours() : this.floorHourAbs(duration);
        let minuteValue = showYear || showMonth || showDay || showHour
            ? duration.minutes() : this.floorMinuteAbs(duration);
        let secondValue = showYear || showMonth || showDay || showHour || showMinute
            ? duration.seconds() + duration.milliseconds() / 1000
            : duration.asSeconds();

        return {
            duration,
            showYear, showMonth, showDay, showHour, showMinute, showSecond,
            yearMax, yearMin,
            monthMax, monthMin,
            dayMax, dayMin,
            hourMax, hourMin,
            minuteMax, minuteMin,
            secondMax, secondMin,
            yearValue, monthValue, dayValue, hourValue, minuteValue, secondValue
        };
    },
    methods: {
        floorYearAbs(value) {
            let d = value.asYears();
            return (d >= 0 ? 1 : -1) * Math.floor(Math.abs(d));
        },
        floorMonthAbs(value) {
            let d = value.asMonths();
            return (d >= 0 ? 1 : -1) * Math.floor(Math.abs(d));
        },
        floorDayAbs(value) {
            let d = value.asDays();
            return (d >= 0 ? 1 : -1) * Math.floor(Math.abs(d));
        },
        floorHourAbs(value) {
            let d = value.asHours();
            return (d >= 0 ? 1 : -1) * Math.floor(Math.abs(d));
        },
        floorMinuteAbs(value) {
            let d = value.asMinutes();
            return (d >= 0 ? 1 : -1) * Math.floor(Math.abs(d));
        },
        formatValueToModel(value) {
            return value;
        },
        onYearChange(newValue) {
            this.duration.subtract(this.duration.years(), 'y');
            this.duration.add(parseInt(newValue), 'y');
            this.updateValue();
        },
        onMonthChange(newValue) {
            this.duration.subtract(this.duration.months(), 'M');
            this.duration.add(parseInt(newValue), 'M');
            this.updateValue();
        },
        onDayChange(newValue) {
            this.duration.subtract(this.duration.days(), 'd');
            this.duration.add(parseInt(newValue), 'd');
            this.updateValue();
        },
        onHourChange(newValue) {
            this.duration.subtract(this.duration.hours(), 'h');
            this.duration.add(parseInt(newValue), 'h');
            this.updateValue();
        },
        onMinuteChange(newValue) {
            this.duration.subtract(this.duration.minutes(), 'm');
            this.duration.add(parseInt(newValue), 'm');
            this.updateValue();
        },
        onSecondChange(newValue) {
            this.duration.subtract(this.duration.milliseconds(), 'ms');
            this.duration.subtract(this.duration.seconds(), 's');
            this.duration.add(parseFloat(newValue), 's');
            this.updateValue();
        },
        updateValue() {
            this.yearValue = this.duration.years();
            this.monthValue = this.showYear ? this.duration.months() : this.floorMonthAbs(this.duration);
            this.dayValue = this.showYear || this.showMonth
                ? this.duration.days() : this.floorDayAbs(this.duration);
            this.hourValue = this.showYear || this.showMonth || this.showDay
                ? this.duration.hours() : this.floorHourAbs(this.duration);
            this.minuteValue = this.showYear || this.showMonth || this.showDay || this.showHour
                ? this.duration.minutes() : this.floorMinuteAbs(this.duration);
            this.secondValue = this.showYear || this.showMonth || this.showDay || this.showHour || this.showMinute
                ? this.duration.seconds() + this.duration.milliseconds() / 1000
                : this.duration.asSeconds();
            let s = "";
            let ms = this.duration.asMilliseconds();
            if (ms < 0) {
                s += "-";
                ms *= -1;
            }
            let d = this.duration.asDays();
            d = (d >= 0 ? 1 : -1) * Math.floor(Math.abs(d));
            if (d !== 0) {
                s += d;
                s += ".";
            }
            s += moment.utc(ms).format("HH:mm:ss.SSS");
            this.value = s;
        }
    }
};