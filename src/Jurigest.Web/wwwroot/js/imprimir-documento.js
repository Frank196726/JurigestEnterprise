export function imprimirDocumento(titulo, contenido, elementoId, tamano) {
    document.getElementById('documento-impresion')?.remove();
    const documento = elementoId ? document.getElementById(elementoId)?.cloneNode(true) : null;
    const marco = document.createElement('iframe');
    marco.id = 'documento-impresion';
    marco.title = 'Documento para impresión';
    marco.style.cssText = 'position:fixed;width:0;height:0;border:0;';
    marco.onload = () => {
        const pagina = marco.contentDocument;
        pagina.title = titulo;
        if (tamano === 'oficio' || tamano === 'carta') {
            const estilo = pagina.createElement('style');
            estilo.textContent = '@page{size:' + (tamano === 'oficio' ? '216mm 330mm' : 'letter') + ';margin:0}h1{display:none}main{white-space:normal}.membrete-hoja{box-sizing:border-box;padding:15mm;break-after:page;font:11pt Arial,sans-serif}.membrete-hoja:last-child{break-after:auto}.membrete-texto{white-space:normal;overflow-wrap:anywhere;font:9pt Arial,sans-serif;line-height:1.15}.membrete-texto>div{margin:0;padding:0}body{margin:0}';
            pagina.head.appendChild(estilo);
        }
        pagina.querySelector('h1').textContent = titulo;
        if (documento) pagina.querySelector('main').appendChild(documento);
        else pagina.querySelector('main').textContent = contenido;
        marco.contentWindow.addEventListener('afterprint', () => marco.remove(), { once: true });
        marco.contentWindow.requestAnimationFrame(() => {
            marco.contentWindow.focus();
            marco.contentWindow.print();
        });
    };
    marco.srcdoc = '<!doctype html><html lang="es"><head><meta charset="utf-8"><style>@page{size:A4;margin:20mm}body{font:12pt Georgia,serif;color:#000}h1{font-size:16pt}main{white-space:pre-wrap;overflow-wrap:anywhere;line-height:1.5}fieldset{border:1px solid #aaa;border-radius:6px;padding:12px;margin:0 0 20px}legend{padding:0 6px;font-size:12pt}dl{display:grid;grid-template-columns:180px 1fr;gap:6px 12px;white-space:normal}dt{font-weight:bold}dd{margin:0;white-space:pre-wrap}</style></head><body><h1></h1><main></main></body></html>';
    document.body.appendChild(marco);
}
