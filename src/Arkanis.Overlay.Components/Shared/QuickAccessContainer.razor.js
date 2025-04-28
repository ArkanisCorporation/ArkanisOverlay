const SEARCH_BAR_HEIGHT_PX = 100;

/**
 * @module QuickAccessContainer
 * @description This module provides functionality to manage a container of elements for quick visibility-based access.
 */
export class QuickAccessContainer {
    /** @type {DotNet.DotNetObject} */
    componentRef;

    /** @type {HTMLElement} */
    containerElement;

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

    /**
     * Initializes the QuickAccessContainer and links it to the .NET component (via DotNet ObjectReference).
     *
     * @param {DotNet.DotNetObject} componentRef
     * @param {HTMLElement} containerElement
     * @param {String} childElementSelector
     */
    constructor(componentRef, containerElement, childElementSelector) {
        this.componentRef = componentRef;
        this.containerElement = containerElement;
        this.childElementSelector = childElementSelector;

        this.visibleElements = [];
        this.lastUpdateWindowTopScroll = 0;

        this.domObserver = new MutationObserver(this.handleDomChange.bind(this));
        // this.domObserver.observe(containerElement, {
        //     attributes: false,
        //     characterData: false,
        //     childList: true,
        //     subtree: true,
        // })
        document.addEventListener('scroll', this.handleScroll.bind(this));
        this.updateDebounced();
    }

    /**
     * Creates new instance of QuickAccessContainer for the provider .NET component.
     *
     * @remarks This method is called from .NET code.
     *
     * @param {DotNet.DotNetObject} componentRef
     * @param {HTMLElement} containerElement
     * @param {String} childElementSelector
     *
     * @returns {QuickAccessContainer}
     */
    static createFor(componentRef, containerElement, childElementSelector) {
        return new QuickAccessContainer(componentRef, containerElement, childElementSelector);
    }

    /**
     Performs the internal update process based on DOM change.
     *
     * @param {Array<MutationRecord>} changes
     * @param {MutationObserver} observer
     */
    async handleDomChange(changes, observer) {
        console.debug("DOM change detected, updating visible elements");
        this.updateDebounced();
    }

    /**
     * Performs the internal update process based on window scroll change.
     */
    async handleScroll() {
        if (Math.abs(this.lastUpdateWindowTopScroll - window.scrollY) > 25) {
            this.lastUpdateWindowTopScroll = window.scrollY;
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

