export class FocusRegion {
    /** @type {DotNet.DotNetObject} */
    componentRef;

    /** @type {HTMLElement} */
    containerElement;

    /** @type {boolean} */
    disposed = false;

    /**
     * Creates a new instance of the FocusRegion.
     *
     * @param {DotNet.DotNetObject} componentRef
     * @param {HTMLElement} containerElement
     */
    constructor(componentRef, containerElement) {
        this.componentRef = componentRef;
        this.containerElement = containerElement;

        this.intersectionObserver = new IntersectionObserver(this.handleIntersectionChange.bind(this), {
            root: document,
            threshold: [0, .25, .50, .75, 1]
        });
        if (this.containerElement) {
            this.intersectionObserver.observe(this.containerElement);
        } else {
            console.error("invalid container element provided for initialization")
        }
    }

    /**
     * Creates a new FocusRegion instance for the provided .NET component.
     *
     * @remarks This method is called from .NET code.
     *
     * @param {DotNet.DotNetObject} componentRef - The reference to the .NET component.
     * @param {HTMLElement} containerElement - The HTML element to be associated with the FocusRegion.
     *
     * @returns {FocusRegion} A new instance of FocusRegion.
     */
    static createFor(componentRef, containerElement) {
        console.debug("Creating new FocusRegion instance for", componentRef, containerElement);
        return new FocusRegion(componentRef, containerElement);
    }

    /**
     * Performs the internal update process based on viewport intersection change.
     *
     * @param {Array<IntersectionObserverEntry>} changes
     * @param {IntersectionObserver} observer
     */
    async handleIntersectionChange(changes, observer) {
        for (let change of changes) {
            this.unfocusTargetWhenOutOfViewport(change);
        }
    }

    /**
     * Unfocuses the target element when it is out of the viewport.
     *
     * @param {IntersectionObserverEntry} change - The intersection change entry for the target element.
     * If the intersection ratio is less than 1, the target element will lose focus.
     */
    unfocusTargetWhenOutOfViewport(change) {
        if (change.intersectionRatio === 0) {
            console.debug("focus region container detected out of viewport: %o", change);
            if (this.containerElement) {
                //! this region may not have any focused element
                this.containerElement.querySelector(":focus")?.blur();
                this.containerElement.blur();
            }
        }
    }

    /**
     * Disposes of resources held by the QuickAccessContainer instance.
     *
     * @remarks This method is called from .NET code.
     */
    async dispose() {
        if (this.disposed) {
            return;
        }

        console.debug("dispose requested from .NET component %o", this.componentRef);
        this.intersectionObserver.disconnect();
        this.disposed = true;
    }
}
