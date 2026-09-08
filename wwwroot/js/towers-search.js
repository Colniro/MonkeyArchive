// Progressive enhancement only: category filtering already works server-side
// (the filter pills are plain links), this just narrows the already-rendered
// cards live as the user types, instead of requiring a form submit per
// keystroke.
document.addEventListener('DOMContentLoaded', function () {
    var searchInput = document.getElementById('towerSearch');
    var noResults = document.getElementById('noResults');
    var items = document.querySelectorAll('.tower-item');

    if (!searchInput) {
        return;
    }

    searchInput.addEventListener('input', function () {
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
    });
});
