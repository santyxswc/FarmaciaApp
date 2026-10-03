/**
 * @file promociones.js
 * @brief Promociones: consulta para todos, mantenimiento para el administrador.
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aIso, aviso, confirmar, esperar, formulario, h, numero, pintar, tabla } from '../ui.js';

const CAMPOS = [
  { nombre: 'descripcion', etiqueta: 'Descripción', requerido: true },
  { nombre: 'descuento', etiqueta: 'Descuento (%)', tipo: 'number', requerido: true, min: 1, max: 100, step: 1 },
  { nombre: 'fechaInicio', etiqueta: 'Desde', tipo: 'date', requerido: true },
  { nombre: 'fechaFin', etiqueta: 'Hasta', tipo: 'date', requerido: true },
];

const aPeticion = (d) => ({
  descripcion: d.descripcion, descuento: Number(d.descuento),
  fechaInicio: `${d.fechaInicio}T00:00:00`, fechaFin: `${d.fechaFin}T00:00:00`,
});

const fecha = (valor) => new Date(valor).toLocaleDateString('es-CO');
const aValores = (p) => ({ ...p, fechaInicio: aIso(new Date(p.fechaInicio)), fechaFin: aIso(new Date(p.fechaFin)) });

export async function vistaPromociones(contenedor, { esAdmin }) {
  const resultado = h('div', { class: 'tarjeta' });
  const buscador = h('input', { type: 'search', placeholder: 'Buscar por descripción', 'aria-label': 'Buscar promoción' });

  async function cargar() {
    try {
      const termino = buscador.value.trim();
      const promociones = await api.get(`/api/promociones${termino ? `?buscar=${encodeURIComponent(termino)}` : ''}`);
      pintar(resultado, tabla({
        vacio: 'No se encontraron promociones.',
        filas: promociones,
        columnas: [
          { titulo: 'Promoción', valor: (p) => p.descripcion },
          { titulo: 'Descuento', numerica: true, valor: (p) => `${numero(p.descuento)} %` },
          { titulo: 'Vigencia', valor: (p) => `${fecha(p.fechaInicio)} – ${fecha(p.fechaFin)}` },
          { titulo: 'Estado', valor: (p) => h('span', { class: p.activa ? 'insignia ok' : 'insignia' }, p.activa ? 'Activa' : 'Inactiva') },
          esAdmin && { titulo: '', valor: (p) => h('div', { class: 'acciones' },
            h('button', { class: 'enlace', onclick: () => editar(p) }, 'Editar'),
            h('button', { class: 'enlace peligro', onclick: () => eliminar(p) }, 'Eliminar')) },
        ].filter(Boolean),
      }));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  async function crear() {
    const guardado = await formulario({
      titulo: 'Nueva promoción', campos: CAMPOS,
      valores: { fechaInicio: aIso(new Date()) },
      guardar: (datos) => api.post('/api/promociones', aPeticion(datos)),
    });
    if (guardado) { aviso('Promoción creada.'); cargar(); }
  }

  async function editar(promocion) {
    const guardado = await formulario({
      titulo: 'Editar promoción', campos: CAMPOS, valores: aValores(promocion),
      guardar: (datos) => api.put(`/api/promociones/${promocion.id}`, aPeticion(datos)),
    });
    if (guardado) { aviso('Promoción actualizada.'); cargar(); }
  }

  async function eliminar(promocion) {
    if (!await confirmar('Eliminar promoción', `¿Eliminar "${promocion.descripcion}"?`, 'Eliminar')) return;
    try {
      await api.delete(`/api/promociones/${promocion.id}`);
      aviso('Promoción eliminada.');
      cargar();
    } catch (error) {
      aviso(error.message, 'error');
    }
  }

  buscador.addEventListener('input', esperar(cargar));
  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Promociones'),
      h('div', { class: 'filtros' }, buscador, esAdmin && h('button', { class: 'principal', onclick: crear }, 'Nueva promoción'))),
    resultado);
  await cargar();
}
