/**
 * @file reportes.js
 * @brief Reportes de ventas por periodo (administrador).
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aIso, h, haceDias, moneda, numero, pintar, tabla } from '../ui.js';

function indicador(etiqueta, valor) {
  return h('div', { class: 'tarjeta indicador' }, h('div', { class: 'valor' }, valor), h('div', { class: 'etiqueta' }, etiqueta));
}

function conBarra(valor, maximo) {
  return h('div', { class: 'barra-relativa', style: { width: `${maximo > 0 ? Math.max(2, (valor / maximo) * 100) : 0}%` }, 'aria-hidden': 'true' });
}

export async function vistaReportes(contenedor) {
  const desde = h('input', { type: 'date', value: aIso(haceDias(6)), 'aria-label': 'Desde' });
  const hasta = h('input', { type: 'date', value: aIso(new Date()), 'aria-label': 'Hasta' });
  const resultado = h('div', { style: { display: 'grid', gap: '1rem' } });

  async function cargar() {
    try {
      const periodo = `desde=${desde.value}&hasta=${hasta.value}`;
      const [resumen, vendedores, productos] = await Promise.all([
        api.get(`/api/reportes/resumen?${periodo}`),
        api.get(`/api/reportes/vendedores?${periodo}`),
        api.get(`/api/reportes/productos-mas-vendidos?${periodo}`),
      ]);
      const mayorVendedor = Math.max(0, ...vendedores.map((v) => v.total));
      const mayorProducto = Math.max(0, ...productos.map((p) => p.unidades));

      pintar(resultado,
        h('div', { class: 'cuadricula' },
          indicador('Total vendido', moneda(resumen.total)),
          indicador('Facturas', numero(resumen.facturas)),
          indicador('Ticket promedio', moneda(resumen.ticketPromedio)),
          indicador('Unidades', numero(resumen.unidades))),
        h('div', { class: 'dos-columnas' },
          h('div', { class: 'tarjeta' }, tabla({
            titulo: 'Ventas por empleado', vacio: 'Sin ventas en el periodo.', filas: vendedores,
            columnas: [
              { titulo: 'Empleado', valor: (v) => [v.vendedor, conBarra(v.total, mayorVendedor)] },
              { titulo: 'Facturas', numerica: true, valor: (v) => numero(v.facturas) },
              { titulo: 'Total', numerica: true, valor: (v) => moneda(v.total) },
            ],
          })),
          h('div', { class: 'tarjeta' }, tabla({
            titulo: 'Productos más vendidos', vacio: 'Sin ventas en el periodo.', filas: productos,
            columnas: [
              { titulo: 'Producto', valor: (p) => [p.producto, conBarra(p.unidades, mayorProducto)] },
              { titulo: 'Unidades', numerica: true, valor: (p) => numero(p.unidades) },
              { titulo: 'Total', numerica: true, valor: (p) => moneda(p.total) },
            ],
          }))));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  const atajo = (texto, dias) => h('button', { type: 'button', onclick: () => {
    desde.value = aIso(haceDias(dias));
    hasta.value = aIso(new Date());
    cargar();
  } }, texto);

  const inicioMes = () => { const hoy = new Date(); desde.value = aIso(new Date(hoy.getFullYear(), hoy.getMonth(), 1)); hasta.value = aIso(hoy); cargar(); };

  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Reportes de ventas'),
      h('form', { class: 'filtros', onsubmit: (e) => { e.preventDefault(); cargar(); } },
        h('label', null, 'Desde', desde), h('label', null, 'Hasta', hasta),
        h('button', { class: 'principal', type: 'submit' }, 'Ver'),
        atajo('Hoy', 0), atajo('Últimos 7 días', 6), h('button', { type: 'button', onclick: inicioMes }, 'Este mes'))),
    resultado);
  await cargar();
}
