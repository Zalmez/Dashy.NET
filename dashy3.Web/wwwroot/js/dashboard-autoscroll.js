window.dashboardAutoScroll = (() => {
    let instance = null;

    return {
        start(pixelsPerSecond) {
            this.stop();

            // Target the main dashboard scrollable container
            const el = document.querySelector('.h-full.overflow-auto.p-6');
            if (!el) return;

            let paused = false;
            let lastTimestamp = null;
            let waitingAtBottom = false;
            let waitStartTime = null;
            let scrollingToTop = false;
            let scrollToTopStart = null;
            const bottomWaitDuration = 2000; // 2 seconds wait at bottom
            const scrollToTopDuration = 800; // milliseconds for the reset animation

            const onEnter = () => paused = true;
            const onLeave = () => paused = false;
            el.addEventListener('mouseenter', onEnter);
            el.addEventListener('mouseleave', onLeave);

            const step = (timestamp) => {
                if (!instance) return;

                if (!paused) {
                    // If we're waiting at the bottom
                    if (waitingAtBottom) {
                        if (waitStartTime === null) {
                            waitStartTime = timestamp;
                        }

                        const waitElapsed = timestamp - waitStartTime;
                        if (waitElapsed >= bottomWaitDuration) {
                            waitingAtBottom = false;
                            waitStartTime = null;
                            scrollingToTop = true;
                        }
                    }
                    // If we're scrolling back to top, animate it
                    else if (scrollingToTop) {
                        if (scrollToTopStart === null) {
                            scrollToTopStart = timestamp;
                        }

                        const elapsed = timestamp - scrollToTopStart;
                        const progress = Math.min(elapsed / scrollToTopDuration, 1);

                        // Ease-in-out function for smooth animation
                        const easeProgress = progress < 0.5
                            ? 2 * progress * progress
                            : 1 - Math.pow(-2 * progress + 2, 2) / 2;

                        const startScroll = instance.scrollStartPosition;
                        el.scrollTop = startScroll - (startScroll * easeProgress);

                        if (progress >= 1) {
                            scrollingToTop = false;
                            scrollToTopStart = null;
                            el.scrollTop = 0;
                            lastTimestamp = timestamp; // Reset to avoid jump
                        }
                    }
                    // Normal scrolling down
                    else if (lastTimestamp !== null) {
                        const delta = timestamp - lastTimestamp;
                        el.scrollTop += (pixelsPerSecond * delta) / 1000;

                        // Check if we've reached the bottom
                        if (el.scrollTop >= el.scrollHeight - el.clientHeight - 1) {
                            waitingAtBottom = true;
                            waitStartTime = null;
                            instance.scrollStartPosition = el.scrollTop;
                        }
                    }
                    lastTimestamp = timestamp;
                }
                instance.raf = requestAnimationFrame(step);
            };

            // Brief delay before starting so the user can see the top of the dashboard first
            const timeout = setTimeout(() => {
                if (!instance) return;
                instance.raf = requestAnimationFrame(step);
            }, 2500);

            instance = { raf: null, timeout, el, onEnter, onLeave, scrollStartPosition: 0 };
        },

        stop() {
            if (!instance) return;
            if (instance.raf) cancelAnimationFrame(instance.raf);
            if (instance.timeout) clearTimeout(instance.timeout);
            if (instance.el) {
                instance.el.removeEventListener('mouseenter', instance.onEnter);
                instance.el.removeEventListener('mouseleave', instance.onLeave);
                // Reset scroll position
                instance.el.scrollTop = 0;
            }
            instance = null;
        }
    };
})();
