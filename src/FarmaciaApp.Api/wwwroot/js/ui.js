/**
 * @file ui.js
 * @brief Utilidades de interfaz: creación de elementos, formatos, avisos y diálogos.
 * @author Santiago Caicedo
 */

/**
 * Crea un elemento. Los textos se insertan como texto, nunca como HTML.
 * @param {string} etiqueta Nombre de la etiqueta
 * @param {object} [atributos] Atributos; "on*" registra un evento y "style" acepta un objeto
 * @param {...any} hijos Nodos o textos; null y false se ignoran
 */
export function h(etiqueta, atributos, ...hijos) {
  const el = document.createElement(etiqueta);
  for (const [nombre, valor] of Object.entries(atributos ?? {})) {
    if (valor === null || valor === undefined || valor === false) continue;
    if (nombre.startsWith('on')) el.addEventListener(nombre.slice(2), valor);
    else if (nombre === 'class') el.className = valor;
    else if (nombre === 'style') Object.assign(el.style, valor);
    else el.setAttribute(nombre, valor === true ? '' : valor);
  }
  for (const hijo of hijos.flat(Infinity)) {
    if (hijo === null || hijo === undefined || hijo === false) continue;
    el.append(hijo instanceof Node ? hijo : document.createTextNode(String(hijo)));
  }
  return el;
}

const formatoMoneda = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 });
const formatoNumero = new Intl.NumberFormat('es-CO');

export const moneda = (valor) => formatoMoneda.format(valor ?? 0);
export const numero = (valor) => formatoNumero.format(valor ?? 0);
export const fechaHora = (valor) => new Date(valor).toLocaleString('es-CO', { dateStyle: 'short', timeStyle: 'short' });

/** Fecha local como yyyy-mm-dd, el formato de los filtros de la API. */
export function aIso(fecha) {
  return fecha.toLocaleDateString('sv-SE');
}

export function haceDias(dias) {
  const fecha = new Date();
  fecha.setDate(fecha.getDate() - dias);
  return fecha;
}

/** Muestra un aviso temporal en la esquina. */
export function aviso(texto, tipo = 'ok') {
  const el = h('div', { class: `aviso ${tipo === 'error' ? 'error' : ''}` }, texto);
  document.getElementById('avisos').append(el);
  setTimeout(() => el.remove(), 4500);
}

/** Retrasa una función hasta que pasen `ms` sin nuevas llamadas. */
export function esperar(funcion, ms = 300) {
  let temporizador;
  return (...args) => {
    clearTimeout(temporizador);
    temporizador = setTimeout(() => funcion(...args), ms);
  };
}

function abrirDialogo(contenido) {
  const dialogo = h('dialog', null, contenido);
  dialogo.addEventListener('close', () => dialogo.remove());
  document.body.append(dialogo);
  dialogo.showModal();
  return dialogo;
}

/** Diálogo informativo con un botón de cierre. */
export function dialogoInfo(titulo, cuerpo) {
  const dialogo = abrirDialogo(h('div', { class: 'contenido' },
    h('h2', null, titulo),
    cuerpo,
    h('div', { class: 'pie' }, h('button', { class: 'principal', onclick: () => dialogo.close() }, 'Cerrar'))));
}

/** Pregunta de confirmación. Devuelve true si el usuario acepta. */
export function confirmar(titulo, mensaje, textoBoton = 'Confirmar') {
  return new Promise((resolver) => {
    const dialogo = abrirDialogo(h('div', { class: 'contenido' },
      h('h2', null, titulo),
      h('p', null, mensaje),
      h('div', { class: 'pie' },
        h('button', { onclick: () => dialogo.close('no') }, 'Cancelar'),
        h('button', { class: 'principal', onclick: () => dialogo.close('si') }, textoBoton))));
    dialogo.addEventListener('close', () => resolver(dialogo.returnValue === 'si'));
  });
}

/**
 * Formulario en un diálogo.
 * @param {object} opciones titulo, campos [{nombre, etiqueta, tipo, requerido, min, step, filas}], valores, guardar(datos)
 * @returns {Promise<boolean>} true si se guardó
 */
export function formulario({ titulo, campos, valores = {}, textoGuardar = 'Guardar', guardar }) {
  return new Promise((resolver) => {
    const mensaje = h('p', { class: 'error', role: 'alert' });
    const boton = h('button', { class: 'principal', type: 'submit' }, textoGuardar);

    const entradas = campos.map((campo) => {
      const comun = { name: campo.nombre, required: campo.requerido, min: campo.min, step: campo.step };
      const entrada = campo.tipo === 'textarea'
        ? h('textarea', { ...comun, rows: campo.filas ?? 3 })
        : h('input', { ...comun, type: campo.tipo ?? 'text' });
      entrada.value = valores[campo.nombre] ?? '';
      return h('label', null, campo.etiqueta, entrada);
    });

    const form = h('form', { method: 'dialog' },
      h('h2', null, titulo), entradas, mensaje,
      h('div', { class: 'pie' },
        h('button', { type: 'button', onclick: () => dialogo.close('no') }, 'Cancelar'),
        boton));

    const dialogo = abrirDialogo(form);
    dialogo.addEventListener('close', () => resolver(dialogo.returnValue === 'si'));

    form.addEventListener('submit', async (evento) => {
      evento.preventDefault();
      mensaje.textContent = '';
      boton.disabled = true;
      try {
        const datos = Object.fromEntries(new FormData(form));
        await guardar(datos);
        dialogo.close('si');
      } catch (error) {
        mensaje.textContent = error.message;
        boton.disabled = false;
      }
    });
  });
}

/** Reemplaza el contenido de un contenedor. */
export function pintar(contenedor, ...hijos) {
  contenedor.replaceChildren(...hijos.flat(Infinity).filter((hijo) => hijo !== null && hijo !== false));
}

/** Tabla con encabezados y una fila por elemento; si no hay datos muestra un mensaje. */
export function tabla({ columnas, filas, vacio = 'No hay datos.', titulo, alHacerClic }) {
  const cuerpo = filas.length === 0
    ? h('tr', null, h('td', { class: 'vacio', colspan: columnas.length }, vacio))
    : filas.map((fila) => h('tr', { class: alHacerClic ? 'clic' : null, onclick: alHacerClic ? () => alHacerClic(fila) : null },
        columnas.map((c) => h('td', { class: c.numerica ? 'num' : null }, c.valor(fila)))));

  return h('div', { class: 'tabla-contenedor' },
    h('table', null,
      titulo && h('caption', null, titulo),
      h('thead', null, h('tr', null, columnas.map((c) => h('th', { class: c.numerica ? 'num' : null }, c.titulo)))),
      h('tbody', null, cuerpo)));
}
