// Zeigt einen Platzhalter mit Schimmer-Effekt, bis ein Bild geladen ist.
// Die Seiten selbst kommen fertig gerendert vom Server, nur die Bilder laden
// nach. Darum hängt der Platzhalter am Bild und nicht an der ganzen Karte.
//
// Die Klasse is-loading wird erst hier gesetzt und nicht schon im HTML.
// Ohne JavaScript sieht man so einfach das normale Bild und nie einen
// Platzhalter, der für immer hängen bleibt.
//
// Es sucht die Elemente über ein data Attribut statt einer festen Klasse.
// So funktioniert das gleiche Script auf jeder Seite, die ihr HTML so aufbaut.
document.addEventListener('DOMContentLoaded', function () {
    var boxes = document.querySelectorAll('[data-skeleton]');

    boxes.forEach(function (box) {
        var image = box.querySelector('img');

        if (!image) {
            return;
        }

        image.classList.add('skeleton-img');
        box.classList.add('is-loading');

        function done() {
            box.classList.remove('is-loading');
        }

        // Erst die Listener anhängen, dann auf complete prüfen.
        // Andersrum gäbe es ein Zeitfenster: wird das Bild genau dazwischen
        // fertig, kommt das load Ereignis nie an und der Platzhalter bliebe.
        image.addEventListener('load', done);
        image.addEventListener('error', done);

        if (image.complete) {
            done();
        }
    });
});
