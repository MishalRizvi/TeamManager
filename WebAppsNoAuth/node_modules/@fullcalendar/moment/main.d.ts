import * as _fullcalendar_common from '@fullcalendar/common';
import { CalendarApi, Duration } from '@fullcalendar/common';
import moment from 'moment';

declare function toMoment(date: Date, calendar: CalendarApi): moment.Moment;
declare function toMomentDuration(fcDuration: Duration): moment.Duration;
declare const _default: _fullcalendar_common.PluginDef;

export default _default;
export { toMoment, toMomentDuration };
