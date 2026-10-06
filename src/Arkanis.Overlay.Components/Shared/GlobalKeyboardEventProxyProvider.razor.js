export class KeyboardEventHelper {
    /** @type {DotNet.DotNetObject} */
    static componentRef;

    /** @type {RegExp} */
    static functionKeysRegex = /^F\d+$/;

    /**
     * @param {DotNet.DotNetObject} componentRef
     */
    static init(componentRef) {
        KeyboardEventHelper.componentRef = componentRef;
        window.addEventListener("keydown", this.processKeyDown);
        window.addEventListener("keyup", this.processKeyUp);
    }

    /**
     * Determines whether the given event should be consumed and default behaviour prevented.
     *
     * @param {KeyboardEvent} event
     * @returns {boolean}
     */
    static shouldConsumeEvent(event) {
        return event.key.match(KeyboardEventHelper.functionKeysRegex) // consume all function keys
            || event.altKey;
    }

    /**
     * Called when a `keydown` event is fired.
     *
     * @param {KeyboardEvent} event The event to process
     *
     * @returns {Promise<void>}
     */
    static async processKeyDown(event) {
        if (KeyboardEventHelper.shouldConsumeEvent(event)) {
            event.stopPropagation();
            event.preventDefault();
        }

        const eventObj = KeyboardEventHelper.simplify_object(event);
        await KeyboardEventHelper.componentRef.invokeMethodAsync("OnKeyDown", eventObj);
    }

    /**
     * Called when a `keyup` event is fired.
     *
     * @param {KeyboardEvent} event The event to process
     *
     * @returns {Promise<void>}
     */
    static async processKeyUp(event) {
        if (KeyboardEventHelper.shouldConsumeEvent(event)) {
            event.stopPropagation();
            event.preventDefault();
        }

        const eventObj = KeyboardEventHelper.simplify_object(event);
        await KeyboardEventHelper.componentRef.invokeMethodAsync("OnKeyUp", eventObj);
    }

    /**
     * @see https://stackoverflow.com/a/58416333/3078351
     *
     * @param {Object} object
     * @param {Number} depth
     * @param {Number} max_depth
     *
     * @returns {Object}
     */
    static simplify_object(object, depth = 0, max_depth = 2) {
        // change max_depth to see more levels, for a touch event, 2 is good
        if (depth > max_depth) {
            return {};
        }

        const obj = {};
        for (let key in object) {
            let value = object[key];
            if (value instanceof Node) {
                // specify which properties you want to see from the node
                value = {id: value.id};
            } else if (value instanceof Window) {
                value = 'Window';
            } else if (value instanceof Object) {
                value = this.simplify_object(value, depth + 1, max_depth);
            }
            obj[key] = value;
        }

        return obj;
    }
}
