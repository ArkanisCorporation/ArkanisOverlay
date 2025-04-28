/**
 * @param {String} eventName
 * @param {DotNet.DotNetObject} dotNetObjectRef
 */
function addWindowEventListener(eventName, dotNetObjectRef) {
    window.addEventListener(eventName, async (event) => {
        await dotNetObjectRef.invokeMethodAsync('HandleEvent', event)
    });
}

/**
 * @param {String} eventName
 * @param {DotNet.DotNetObject} dotNetObjectRef
 */
function addDocumentEventListener(eventName, dotNetObjectRef) {
    window.addEventListener(eventName, async (event) => {
        await dotNetObjectRef.invokeMethodAsync('HandleEvent', event)
    });
}

/**
 * @param {HTMLElement} element
 * @param {String} eventName
 * @param {DotNet.DotNetObject} dotNetObjectRef
 */
function addElementEventListener(element, eventName, dotNetObjectRef) {
    element.addEventListener(eventName, async (event) => {
        await dotNetObjectRef.invokeMethodAsync('HandleEvent', event)
    });
}
