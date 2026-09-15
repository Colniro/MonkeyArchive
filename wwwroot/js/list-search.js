// Generic live-search for card grids (Towers, Bloons, ...). Progressive
// enhancement only: category filtering already works server-side via the
// filter pills (plain links); this just narrows the already-rendered cards
// live as the user types, instead of requiring a form submit per keystroke.
// Targets elements by data attribute rather than a fixed id/class, so the
// same script works on any page that marks its markup up this way.
document.addEventListener('DOMContentLoaded', function () {
    var searchInput = document.querySelector('[data-search-input]');
    var clearButton = document.querySelector('[data-search-clear]');
    var noResults = document.querySelector('[data-no-results]');
    var items = document.querySelectorAll('[data-search-item]');

    if (!searchInput) {
        return;
    }

    function applyFilter() {
        var query = searchInput.value.trim().toLowerCase();
        var visibleCount = 0;

        items.forEach(function (item) {
            var matches = item.dataset.name.indexOf(query) !== -1;
            item.style.display = matches ? '' : 'none';
            if (matches) {
                visibleCount++;
            }
        });

        if (noResults) {
            noResults.style.display = visibleCount === 0 ? 'block' : 'none';
        }
    }

    searchInput.addEventListener('input', applyFilter);

    if (clearButton) {
        clearButton.addEventListener('click', function () {
            searchInput.value = '';
            searchInput.focus();
            applyFilter();
        });
    }
});
