/**
 * @file facturas.js
 * @brief Listado de facturas y su detalle.
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aviso, dialogoInfo, esperar, fechaHora, h, moneda, numero, pintar, tabla } from '../ui.js';

/** Abre el detalle de una factura con sus líneas y el desglose del IVA. */
export async function mostrarFactura(numeroFactura) {
  try {
    const f = await api.get(`/api/facturas/${numeroFactura}`);
    dialogoInfo(`Factura N° ${f.numero}`, h('div', { class: 'contenido' },
      h('p', { class: 'suave' }, `${fechaHora(f.fecha)} · ${f.cliente} · vendedor ${f.vendedor} · ${f.metodoPago}`),
      tabla({
        filas: f.items,
        columnas: [
          { titulo: 'Producto', valor: (i) => i.producto },
          { titulo: 'Cant.', numerica: true, valor: (i) => numero(i.cantidad) },
          { titulo: 'Precio', numerica: true, valor: (i) => moneda(i.precioUnitario) },
          { titulo: 'Subtotal', numerica: true, valor: (i) => moneda(i.subtotal) },
        ],
      }),
      h('div', { class: 'totales' },
        h('span', null, `Subtotal ${moneda(f.subtotal)}`),
        h('span', null, `IVA 19 % ${moneda(f.iva)}`),
        h('span', { class: 'total' }, `Total ${moneda(f.total)}`))));
  } catch (error) {
    aviso(error.message, 'error');
  }
}

export async function vistaFacturas(contenedor) {
  const resultado = h('div', { class: 'tarjeta' });
  const buscador = h('input', { type: 'search', placeholder: 'Cliente, vendedor o número', 'aria-label': 'Buscar factura' });

  async function cargar() {
    try {
      const termino = buscador.value.trim();
      const facturas = await api.get(`/api/facturas${termino ? `?buscar=${encodeURIComponent(termino)}` : ''}`);
      pintar(resultado, tabla({
        vacio: 'No se encontraron facturas.',
        filas: facturas,
        alHacerClic: (f) => mostrarFactura(f.numero),
        columnas: [
          { titulo: 'N°', valor: (f) => f.numero },
          { titulo: 'Fecha', valor: (f) => fechaHora(f.fecha) },
          { titulo: 'Cliente', valor: (f) => f.cliente },
          { titulo: 'Vendedor', valor: (f) => f.vendedor },
          { titulo: 'Pago', valor: (f) => f.metodoPago },
          { titulo: 'Total', numerica: true, valor: (f) => moneda(f.total) },
        ],
      }));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  buscador.addEventListener('input', esperar(cargar));
  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Facturas'), h('div', { class: 'filtros' }, buscador)),
    h('p', { class: 'suave' }, 'Pulsa una factura para ver su detalle.'),
    resultado);
  await cargar();
}
