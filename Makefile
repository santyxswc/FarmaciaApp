.DEFAULT_GOAL := help

include $(wildcard .env)
APP_USER ?= farmacia
APP_USER_PASSWORD ?= farmacia123
SQLPLUS = docker exec -i farmacia-oracle sqlplus -s $(APP_USER)/$(APP_USER_PASSWORD)@//localhost/FREEPDB1
PUBLISH = dotnet publish src/FarmaciaApp.Desktop -c Release --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

.PHONY: help build test run api api-up api-down db-up db-init db-down db-reset publish-linux publish-windows clean

help: ## Muestra esta ayuda
	@grep -E '^[a-z-]+:.*## ' $(MAKEFILE_LIST) | awk -F':.*## ' '{printf "  %-17s %s\n", $$1, $$2}'

build: ## Compila la solución
	dotnet build

test: ## Ejecuta las pruebas unitarias
	dotnet test

run: ## Abre la aplicación (requiere la base de datos encendida)
	dotnet run --project src/FarmaciaApp.Desktop

api: ## Abre la API con dotnet run (requiere su appsettings.json y la base de datos)
	dotnet run --project src/FarmaciaApp.Api

api-up: ## Levanta Oracle, el esquema y la API en Docker
	docker compose up -d --build

api-down: ## Apaga la API y Oracle conservando los datos
	docker compose stop

db-up: ## Enciende solo Oracle y espera a que esté listo
	docker compose up -d --wait oracle

db-init: ## Carga schema.sql (BORRA las tablas y los datos)
	printf '@/database/schema.sql\nEXIT\n' | $(SQLPLUS)

db-down: ## Apaga Oracle conservando los datos
	docker compose stop

db-reset: ## Elimina el contenedor y el volumen de datos
	docker compose down -v

publish-linux: ## Ejecutable de Linux en publicado/linux
	$(PUBLISH) -r linux-x64 -o publicado/linux

publish-windows: ## Ejecutable de Windows en publicado/windows
	$(PUBLISH) -r win-x64 -o publicado/windows

clean: ## Borra los resultados de compilación y publicación
	dotnet clean
	rm -rf publicado TestResults
