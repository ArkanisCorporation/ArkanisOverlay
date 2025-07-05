/**
 * @param {HTMLElement} element
 * @param {String} methodName
 * @param {Array<any>} methodArgs
 */
function invokeOnElement(element, methodName, methodArgs) {
    const elementMethod = element[methodName];
    if (elementMethod === undefined) {
        throw new Error(`Method '${methodName}' is not implemented on '${element?.tagName}'`);
    }

    elementMethod.bind(element)(...methodArgs);
}
