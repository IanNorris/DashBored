function insertCalendar(elementRef, newEvents, startDateIn, endDateIn) {
    const calendar = new FullCalendar.Calendar(elementRef, {
        editable: false,
        droppable: false, // this allows things to be dropped onto the calendar
        dayMaxEvents: false, // allow "more" link when too many events
        headerToolbar: {
            left: "",
            center: "",
            right: ""
        },
        initialView: 'timeGrid',
        slotMinTime: '08:00:00',
        slotMaxTime: '21:00:00',
        nowIndicator: true,
        slotEventOverlap: false,
        displayEventTime: false,
        eventMinHeight: 25,
        expandRows: true,

        visibleRange: {
            start: startDateIn,
            end: endDateIn
        },

        themeSystem: "bootstrap",

        bootstrapFontAwesome: {
            close: " demo-psi-cross",
            prev: " demo-psi-arrow-left",
            next: " demo-psi-arrow-right",
            prevYear: " demo-psi-arrow-left-2",
            nextYear: " demo-psi-arrow-right-2"
        },

        events: newEvents
    });

    calendar.render();

    return calendar;
};