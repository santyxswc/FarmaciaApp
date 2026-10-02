/**
 * @file api.js
 * @brief Cliente HTTP de la API y almacenamiento de la sesión.
 * @author Santiago Caicedo
 */
const CLAVE = 'farmacia.sesion';

/** Sesión guardada en el navegador mientras la pestaña siga abierta. */
export const sesion = {
  /** @returns {{token: string, expira: string, usuario: object} | null} la sesión vigente, o null */
  obtener() {
    try {
      const guardada = JSON.parse(sessionStorage.getItem(CLAVE));
      return guardada && new Date(guardada.expira) > new Date() ? guardada : null;
    } catch {
      return null;
    }
  },
  guardar(datos) {
    try { sessionStorage.setItem(CLAVE, JSON.stringify(datos)); } catch { /* sin almacenamiento: la sesión dura lo que dure la página */ }
  },
  borrar() {
    try { sessionStorage.removeItem(CLAVE); } catch { /* nada que borrar */ }
  },
};

/** Error de la API con el estado HTTP y el mensaje que devolvió el servidor. */
export class ErrorApi extends Error {
  constructor(mensaje, estado) {
    super(mensaje);
    this.estado = estado;
  }
}

const MENSAJES = {
  429: 'Demasiados intentos. Espera un minuto e inténtalo de nuevo.',
  503: 'La base de datos no está disponible. Inténtalo en un momento.',
};

async function pedir(metodo, ruta, cuerpo) {
  const actual = sesion.obtener();
  const cabeceras = {};
  if (cuerpo !== undefined) cabeceras['Content-Type'] = 'application/json';
  if (actual) cabeceras.Authorization = `Bearer ${actual.token}`;

  let respuesta;
  try {
    respuesta = await fetch(ruta, { method: metodo, headers: cabeceras, body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo) });
  } catch {
    throw new ErrorApi('No se pudo conectar con el servidor.', 0);
  }

  if (respuesta.status === 401 && actual) {
    sesion.borrar();
    window.dispatchEvent(new Event('sesion-vencida'));
  }

  if (!respuesta.ok) {
    let detalle = MENSAJES[respuesta.status];
    try {
      const problema = await respuesta.json();
      detalle = problema.detail || problema.title || detalle;
    } catch { /* la respuesta no trae cuerpo */ }
    throw new ErrorApi(detalle || `Error ${respuesta.status}`, respuesta.status);
  }

  return respuesta.status === 204 ? null : respuesta.json();
}

export const api = {
  get: (ruta) => pedir('GET', ruta),
  post: (ruta, cuerpo) => pedir('POST', ruta, cuerpo),
  put: (ruta, cuerpo) => pedir('PUT', ruta, cuerpo),
  delete: (ruta) => pedir('DELETE', ruta),
};
