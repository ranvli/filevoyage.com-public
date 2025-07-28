// wwwroot/js/upload.js
(function () {
    const $form = $('#uploadForm');
    const $fileInp = $('#FileInput');
    const $fileLab = $('.fv-filename');
    const $btn = $('#submitBtn');
    const $date = $('.fv-date');
    const $overlay = $('#loadingOverlay');

    if (!$form.length) return;

    $form.validate();

    $fileInp.on('change', function () {
        const f = this.files && this.files[0];
        $fileLab.text(f ? f.name : 'Ningún archivo seleccionado');
    });

    $form.on('submit', function () {
        if (!$form.valid()) return;
        $overlay.show();
        $btn.prop('disabled', true).text('⌛ Cargando...');
        $date.prop('disabled', true);
    });
})();
