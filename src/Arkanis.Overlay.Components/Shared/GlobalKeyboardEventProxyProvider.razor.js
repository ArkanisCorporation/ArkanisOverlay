export class KeyboardEventHelper {
    static componentRef;

    static init(value) {
        KeyboardEventHelper.componentRef = value;
        window.addEventListener("keydown", this.processKeyDown);
        window.addEventListener("keyup", this.processKeyUp);
    }

    static async processKeyDown(event) {
        const eventObj = KeyboardEventHelper.simplify_object(event);
        const consumed = await KeyboardEventHelper.componentRef.invokeMethodAsync("OnKeyDown", eventObj);
        if (consumed) {
            event.stopPropagation();
            event.preventDefault();
        }
    }

    static async processKeyUp(event) {
        const eventObj = KeyboardEventHelper.simplify_object(event);
        const consumed = await KeyboardEventHelper.componentRef.invokeMethodAsync("OnKeyUp", eventObj);
        if (consumed) {
            event.stopPropagation();
            event.preventDefault();
        }
    }

    // See: https://stackoverflow.com/a/58416333/3078351
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
