const SEARCH_BAR_HEIGHT_PX = 104 + 48; // search box + app bar
const FEAT_DOM_OBSERVER = "dom";

/**
 * @module QuickAccessContainer
 * @description This module provides functionality to manage a container of elements for quick visibility-based access.
 */
export class QuickAccessContainer {
    /** @type {DotNet.DotNetObject} */
    componentRef;

    /** @type {HTMLElement} */
    containerElement;

    /** @type {HTMLElement} */
    scrollableParent;

    /** @type {String} */
    childElementSelector;

    /** @type {Array<HTMLElement>} */
    visibleElements;

    /** @type {int} */
    lastUpdateWindowTopScroll;

    /** @type {number | undefined} */
    scrollDebounce;

    /** @type {MutationObserver} */
    domObserver;

    /** @type {boolean} */
    disposed = false;

    /** @type {function} */
    scrollEventHandler;

    /**
     * Initializes the QuickAccessContainer and links it to the .NET component (via DotNet ObjectReference).
     *
     * @param {DotNet.DotNetObject} componentRef - The reference to the .NET component.
     * @param {HTMLElement} containerElement - The HTML element to be associated with the QuickAccessContainer.
     * @param {String} childElementSelector
     * @param {Array<String>} features
     */
    constructor(componentRef, containerElement, childElementSelector, features) {
        this.componentRef = componentRef;
        this.containerElement = containerElement;
        this.childElementSelector = childElementSelector;

        this.visibleElements = [];
        this.lastUpdateWindowTopScroll = 0;

        this.domObserver = new MutationObserver(this.handleDomChange.bind(this));
        if (features && features.includes(FEAT_DOM_OBSERVER)) {
            console.debug("DOM change tracking enabled for", this.childElementSelector);
            this.domObserver.observe(containerElement, {
                attributes: false,
                characterData: false,
                childList: true,
                subtree: true,
            })
        }

        this.scrollEventHandler = this.handleScroll.bind(this);
        this.scrollableParent = QuickAccessContainer.getClosestScrollable(containerElement);
        if (this.scrollableParent !== null) {
            this.scrollableParent.addEventListener('scroll', this.scrollEventHandler);
        } else {
            console.debug("targeted quick access container is not scrollable and does not have any scrollable parent")
        }

        this.updateDebounced();
    }

    /**
     * Returns the first scrollable parent of the given HTML element or null if no such parent is found.
     *
     * @param {HTMLElement} element
     *
     * @returns {HTMLElement | null} The first scrollable parent element.
     */
    static getClosestScrollable(element) {
        if (element == null) {
            return null;
        }

        if (element.scrollHeight > element.clientHeight) {
            return element;
        }

        return QuickAccessContainer.getClosestScrollable(element.parentElement);
    }

    /**
     * Creates new instance of QuickAccessContainer for the provider .NET component.
     *
     * @remarks This method is called from .NET code.
     *
     * @param {DotNet.DotNetObject} componentRef - The reference to the .NET component.
     * @param {HTMLElement} containerElement - The HTML element to be associated with the QuickAccessContainer.
     * @param {String} childElementSelector
     * @param {Array<String>} features
     *
     * @returns {QuickAccessContainer} A new instance of QuickAccessContainer.
     */
    static createFor(componentRef, containerElement, childElementSelector, features) {
        console.debug("Creating new QuickAccessContainer instance for", componentRef, containerElement);
        return new QuickAccessContainer(componentRef, containerElement, childElementSelector, features);
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
        if (this.scrollableParent !== null) {
            this.scrollableParent.removeEventListener('scroll', this.scrollEventHandler);
        }

        this.domObserver.disconnect()
        this.disposed = true;
    }

    /**
     * Performs the internal update process based on DOM change.
     *
     * @param {Array<MutationRecord>} changes
     * @param {MutationObserver} observer
     */
    async handleDomChange(changes, observer) {
        console.debug("DOM change detected, updating visible elements");
        this.updateDebounced(250);
    }

    updateScroll() {
        this.lastUpdateWindowTopScroll = this.scrollableParent.scrollTop;
    }

    getScrollChange() {
        return Math.abs(this.lastUpdateWindowTopScroll - this.scrollableParent.scrollTop);
    }

    /**
     * Performs the internal update process based on window scroll change.
     */
    async handleScroll() {
        if (this.getScrollChange() > 25) {
            this.updateScroll();
            this.updateDebounced();
        }
    }

    /**
     * Schedules debounced collection of visible elements and update propagation.
     *
     * @remarks This method is called from .NET code.
     *
     * @param {int?} timeout
     */
    updateDebounced(timeout) {
        clearTimeout(this.scrollDebounce);
        this.scrollDebounce = setTimeout(async () => {
            this.scrollDebounce = undefined;
            this.updateVisibleElements();
            await this.pushUpdateToDotNet();
        }, timeout || 120);
    }

    /**
     * Optimizes the collection of visible elements.
     */
    async pushUpdateToDotNet() {
        if (this.disposed) {
            console.debug("parent .NET component has disposed this interop instance, preventing update");
            return;
        }

        console.debug("sending notification to parent .NET component");
        await this.componentRef.invokeMethodAsync("OnJsUpdateAsync");
    }

    /**
     * Updates the collection of visible elements.
     */
    updateVisibleElements() {
        const matchingElements = this.containerElement.querySelectorAll(this.childElementSelector)
        this.visibleElements = Array.prototype.filter.call(matchingElements, this.isFullyVisible.bind(this));
        console.debug(
            "updated visible elements, there are",
            this.visibleElements.length,
            "matching",
            this.childElementSelector,
            "within",
            this.containerElement
        );
    }

    /**
     * Checks if the element is fully visible within the viewport.
     *
     * @param {HTMLElement} element
     */
    isFullyVisible(element) {
        const bounds = element.getBoundingClientRect();
        return bounds.bottom - bounds.height - SEARCH_BAR_HEIGHT_PX >= 0
            && bounds.top + bounds.height <= window.innerHeight;
    }

    /**
     * Checks if the element is visible within the viewport and returns its visibility information.
     *
     * @remarks This method is called from .NET code.
     *
     * @param {HTMLElement} element
     *
     * @returns {{
     *     order: number?,
     *     isVisible: boolean,
     * }}
     */
    getVisibilityInfo(element) {
        const indexOfElement = this.visibleElements.indexOf(element);
        return {
            index: indexOfElement,
            isVisible: indexOfElement >= 0,
        }
    }

}

