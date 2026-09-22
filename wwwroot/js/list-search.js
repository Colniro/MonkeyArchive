// Allgemeine Live Suche für Karten Grids, zum Beispiel Towers und Bloons.
// Der Kategorie Filter funktioniert schon ohne JavaScript, über normale Links.
// Dieses Script macht nur die Live Suche beim Tippen, statt jedes Mal das Formular abzuschicken.
// Es sucht Elemente über data Attribute statt einer festen id oder Klasse.
// So funktioniert das gleiche Script auf jeder Seite, die ihr HTML so aufbaut.
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
