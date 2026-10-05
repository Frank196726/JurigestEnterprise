const escapar = valor => String(valor ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const dinero = valor => new Intl.NumberFormat('es-CL', {style:'currency',currency:'CLP',maximumFractionDigits:0}).format(valor);
const fecha = valor => valor ? escapar(String(valor).slice(0,16).replace('T',' ')) : '—';
export function imprimirInforme(recibos, estado) {
    document.getElementById('informe-recibos-impresion')?.remove();
    const titulo = estado === 1 ? 'Informe de recibos pendientes' : estado === 2 ? 'Informe de recibos pagados' : 'Informe de recibos pagados y pendientes';
    const grupo = (codigo, nombre) => {
        const items = recibos.filter(r => r.estado === codigo).sort((a,b) => String(b.fechaEmision).localeCompare(String(a.fechaEmision)));
        const total = items.reduce((sum,r) => sum + r.monto, 0);
        return `<section><h2>${nombre}</h2><p>${items.length} recibos · Total: <strong>${dinero(total)}</strong></p>${items.length ? `<table><thead><tr><th>Recibo</th><th>ROL</th><th>Diligencia realizada</th><th>Abogado</th><th>Demandante</th><th>Emisión</th><th>Monto</th><th>Estado</th><th>Pago</th></tr></thead><tbody>${items.map(r => `<tr><td>${r.numero ? 'R'+escapar(r.numero) : '—'}</td><td>${escapar(r.rol) || '—'}</td><td>${escapar(r.diligenciaRealizada)}</td><td>${escapar(r.abogado) || 'No registrado'}</td><td>${escapar(r.demandante) || 'No registrado'}</td><td>${fecha(r.fechaEmision)}</td><td class="monto">${dinero(r.monto)}</td><td>${codigo === 1 ? 'Pendiente' : 'Pagado'}</td><td>${fecha(r.fechaPago)}</td></tr>`).join('')}</tbody><tfoot><tr><td colspan="6">Total ${nombre.toLowerCase()}</td><td class="monto">${dinero(total)}</td><td colspan="2"></td></tr></tfoot></table>` : '<p>No hay recibos en este estado.</p>'}</section>`;
    };
    const contenido = `${estado !== 2 ? grupo(1,'Pendientes de pago') : ''}${estado !== 1 ? grupo(2,'Pagados') : ''}`;
    const marco = document.createElement('iframe');
    marco.id = 'informe-recibos-impresion';
    marco.title = titulo;
    marco.style.cssText = 'position:fixed;left:-10000px;top:0;width:297mm;height:210mm;border:0';
    marco.onload = () => {
        marco.contentWindow.addEventListener('afterprint',()=>marco.remove(),{once:true});
        marco.contentWindow.requestAnimationFrame(()=>{marco.contentWindow.focus();marco.contentWindow.print();});
    };
    const ahora = new Intl.DateTimeFormat('es-CL',{timeZone:'America/Santiago',dateStyle:'short',timeStyle:'short'}).format(new Date());
    marco.srcdoc = `<!doctype html><html lang="es"><head><meta charset="utf-8"><title>${titulo}</title><style>@page{size:A4 landscape;margin:12mm}body{font:9pt Arial,sans-serif;color:#000;margin:0}h1{font-size:17pt}h2{font-size:12pt;margin-top:7mm}table{width:100%;border-collapse:collapse;table-layout:fixed}th,td{border:1px solid #aaa;padding:2mm;vertical-align:top;overflow-wrap:anywhere}th{background:#eee;text-align:left}thead{display:table-header-group}tfoot{display:table-row-group;font-weight:bold}tr{break-inside:avoid}.monto{text-align:right}p{margin:3mm 0}</style></head><body><h1>${titulo}</h1><p>Jurigest Enterprise · Generado: ${escapar(ahora)}</p>${contenido}</body></html>`;
    document.body.appendChild(marco);
}
