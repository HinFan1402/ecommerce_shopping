
    (function () {
        'use strict';

        var filterForm = document.getElementById('filterForm');
        var sortBySelect = document.getElementById('sortBySelect');
        var minRange = document.getElementById('minPriceRange');
        var maxRange = document.getElementById('maxPriceRange');
        var minDisplay = document.getElementById('minPriceDisplay');
        var maxDisplay = document.getElementById('maxPriceDisplay');
        var minInput = document.getElementById('minPriceInput');
        var maxInput = document.getElementById('maxPriceInput');
        var track = document.getElementById('sliderTrack');

        if (!filterForm) return;

        var formatVND = function (val) {
            try {
                return new Intl.NumberFormat('vi-VN').format(val) + 'đ';
            } catch (e) {
                return val + 'đ';
            }
        };

        var minGap = 1000000;
        var maxLimit = 60000000;

        function safeInt(v, fallback) {
            var n = parseInt(v, 10);
            return isNaN(n) ? fallback : n;
        }

        function updateSlider(e) {
            if (!minRange || !maxRange) return;

            var minVal = safeInt(minRange.value, 0);
            var maxVal = safeInt(maxRange.value, maxLimit);

            if (maxVal - minVal <= minGap) {
                if (e && e.target === minRange) {
                    minVal = maxVal - minGap;
                    minRange.value = minVal;
                } else {
                    maxVal = minVal + minGap;
                    maxRange.value = maxVal;
                }
            }

            minVal = safeInt(minRange.value, 0);
            maxVal = safeInt(maxRange.value, maxLimit);

            if (minDisplay) minDisplay.value = formatVND(minVal);
            if (maxDisplay) maxDisplay.value = formatVND(maxVal);
            if (minInput) minInput.value = minVal;
            if (maxInput) maxInput.value = maxVal;

            if (track) {
                var percent1 = (minVal / maxLimit) * 100;
                var percent2 = (maxVal / maxLimit) * 100;
                track.style.left = percent1 + "%";
                track.style.right = (100 - percent2) + "%";
            }
        }

        function debounce(fn, wait) {
            var t;
            return function () {
                var ctx = this, args = arguments;
                clearTimeout(t);
                t = setTimeout(function () {
                    fn.apply(ctx, args);
                }, wait);
            };
        }

        function submitForm() {
            if (filterForm && typeof filterForm.submit === 'function') {
                filterForm.submit();
            }
        }

        var submitDebounced = debounce(submitForm, 250);

        // Sort change -> submit
        if (sortBySelect) {
            sortBySelect.addEventListener('change', submitForm);
        }

        // Khi người dùng kéo thanh trượt
        function activateSlider(slider) {
            minRange.classList.remove('active');
            maxRange.classList.remove('active');
            slider.classList.add('active');
        }

        if (minRange && maxRange) {
            minRange.addEventListener('input', function (e) {
                activateSlider(minRange);
                updateSlider(e);
                submitDebounced();
            });

            maxRange.addEventListener('input', function (e) {
                activateSlider(maxRange);
                updateSlider(e);
                submitDebounced();
            });

            // Khi thả ra thì bỏ active
            minRange.addEventListener('change', function () {
                minRange.classList.remove('active');
                submitDebounced();
            });

            maxRange.addEventListener('change', function () {
                maxRange.classList.remove('active');
                submitDebounced();
            });

            updateSlider();
        }
    })();
