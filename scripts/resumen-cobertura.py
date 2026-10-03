#!/usr/bin/env python3
"""
@file resumen-cobertura.py
@brief Resume la cobertura de líneas por proyecto a partir de los informes Cobertura.
@author Santiago Caicedo

Uso: resumen-cobertura.py <carpeta con los *.cobertura.xml>
Imprime una tabla en Markdown; pensada para el resumen de un paso de GitHub Actions.
"""
import collections
import pathlib
import sys
import xml.etree.ElementTree as ET


def leer(carpeta):
    """Une los informes tomando, por línea, el mayor número de ejecuciones."""
    lineas = collections.defaultdict(dict)
    for informe in pathlib.Path(carpeta).rglob("*.cobertura.xml"):
        for clase in ET.parse(informe).getroot().iter("class"):
            archivo = clase.get("filename").replace("\\", "/")
            for linea in clase.iter("line"):
                numero = linea.get("number")
                lineas[archivo][numero] = max(lineas[archivo].get(numero, 0), int(linea.get("hits")))
    return lineas


def por_proyecto(lineas):
    """Agrupa por el primer segmento de la ruta que empieza por FarmaciaApp. y no es de pruebas."""
    totales = collections.defaultdict(lambda: [0, 0])
    for archivo, datos in lineas.items():
        proyecto = next((p for p in archivo.split("/") if p.startswith("FarmaciaApp.")), None)
        if proyecto is None or proyecto.endswith(".Tests"):
            continue
        totales[proyecto][0] += sum(1 for h in datos.values() if h > 0)
        totales[proyecto][1] += len(datos)
    return totales


def main():
    totales = por_proyecto(leer(sys.argv[1] if len(sys.argv) > 1 else "."))
    print("### Cobertura de líneas\n")
    if not totales:
        print("No se encontraron informes de cobertura.")
        return
    print("| Proyecto | Líneas cubiertas | Total | Cobertura |")
    print("|---|---:|---:|---:|")
    for proyecto, (cubiertas, total) in sorted(totales.items()):
        print(f"| {proyecto} | {cubiertas} | {total} | {100 * cubiertas / total:.1f} % |")


if __name__ == "__main__":
    main()
