// Drag and drop interop for widget reordering
window.dragDropInterop = {
    draggedWidgetId: null,
    dragHandleActive: false,

    // Called when mousedown on a drag handle
    activateDragHandle: function () {
        window.dragDropInterop.dragHandleActive = true;
    },

    // Called when mouseup or mouseleave on a drag handle
    deactivateDragHandle: function () {
        window.dragDropInterop.dragHandleActive = false;
    },

    // Check if drag should be allowed (started from handle)
    isDragAllowed: function () {
        return window.dragDropInterop.dragHandleActive;
    },

    onDragStart: function (widgetId) {
        if (!window.dragDropInterop.dragHandleActive) {
            return false; // Signal that drag should be cancelled
        }
        window.dragDropInterop.draggedWidgetId = widgetId;
        return true;
    },

    getDraggedWidgetId: function () {
        return window.dragDropInterop.draggedWidgetId;
    },

    clearDragState: function () {
        window.dragDropInterop.draggedWidgetId = null;
        window.dragDropInterop.dragHandleActive = false;
    },

    getBoundingRect: function (element) {
        if (!element) return { left: 0, top: 0, width: 0, height: 0 };
        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height
        };
    }
};
