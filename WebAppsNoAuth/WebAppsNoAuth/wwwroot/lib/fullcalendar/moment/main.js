/*!
FullCalendar v5.11.5
Docs & License: https://fullcalendar.io/
(c) 2022 Adam Shaw
*/
import moment from 'moment';
import { createPlugin, CalendarApi } from '@fullcalendar/common';

function toMoment(date, calendar) {
    if (!(calendar instanceof CalendarApi)) {
        throw new Error('must supply a CalendarApi instance');
    }
    var dateEnv = calendar.getCurrentData().dateEnv;
    return convertToMoment(date, dateEnv.timeZone, null, dateEnv.locale.codes[0]);
}
function toMomentDuration(fcDuration) {
    return moment.duration(fcDuration); // moment accepts all the props that fc.Duration already has!
}
function formatWithCmdStr(cmdStr, arg) {
    var cmd = parseCmdStr(cmdStr);
    if (arg.end) {
        var startMom = convertToMoment(arg.start.array, arg.timeZone, arg.start.timeZoneOffset, arg.localeCodes[0]);
        var endMom = convertToMoment(arg.end.array, arg.timeZone, arg.end.timeZoneOffset, arg.localeCodes[0]);
        return formatRange(cmd, createMomentFormatFunc(startMom), createMomentFormatFunc(endMom), arg.defaultSeparator);
    }
    return convertToMoment(arg.date.array, arg.timeZone, arg.date.timeZoneOffset, arg.localeCodes[0]).format(cmd.whole); // TODO: test for this
}
var main = createPlugin({
    cmdFormatter: formatWithCmdStr,
});
function createMomentFormatFunc(mom) {
    return function (cmdStr) { return (cmdStr ? mom.format(cmdStr) : '' // because calling with blank string results in ISO8601 :(
    ); };
}
function convertToMoment(input, timeZone, timeZoneOffset, locale) {
    var mom;
    if (timeZone === 'local') {
        mom = moment(input);
    }
    else if (timeZone === 'UTC') {
        mom = moment.utc(input);
    }
    else if (moment.tz) {
        mom = moment.tz(input, timeZone);
    }
    else {
        mom = moment.utc(input);
        if (timeZoneOffset != null) {
            mom.utcOffset(timeZoneOffset);
        }
    }
    mom.locale(locale);
    return mom;
}
function parseCmdStr(cmdStr) {
    var parts = cmdStr.match(/^(.*?)\{(.*)\}(.*)$/); // TODO: lookbehinds for escape characters
    if (parts) {
        var middle = parseCmdStr(parts[2]);
        return {
            head: parts[1],
            middle: middle,
            tail: parts[3],
            whole: parts[1] + middle.whole + parts[3],
        };
    }
    return {
        head: null,
        middle: null,
        tail: null,
        whole: cmdStr,
    };
}
function formatRange(cmd, formatStart, formatEnd, separator) {
    if (cmd.middle) {
        var startHead = formatStart(cmd.head);
        var startMiddle = formatRange(cmd.middle, formatStart, formatEnd, separator);
        var startTail = formatStart(cmd.tail);
        var endHead = formatEnd(cmd.head);
        var endMiddle = formatRange(cmd.middle, formatStart, formatEnd, separator);
        var endTail = formatEnd(cmd.tail);
        if (startHead === endHead && startTail === endTail) {
            return startHead +
                (startMiddle === endMiddle ? startMiddle : startMiddle + separator + endMiddle) +
                startTail;
        }
    }
    var startWhole = formatStart(cmd.whole);
    var endWhole = formatEnd(cmd.whole);
    if (startWhole === endWhole) {
        return startWhole;
    }
    return startWhole + separator + endWhole;
}

export default main;
export { toMoment, toMomentDuration };
//# sourceMappingURL=main.js.map
