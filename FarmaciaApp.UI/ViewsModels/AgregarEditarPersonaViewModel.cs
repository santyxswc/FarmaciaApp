using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class AgregarEditarPersonaViewModel : ObservableObject
    {
        private readonly PersonaService _service;
        private readonly Window _window;
        private int _personaId;

        [ObservableProperty]
        private string titulo = "Agregar Persona";

        [ObservableProperty]
        private string nombre;

        [ObservableProperty]
        private string apellido;

        [ObservableProperty]
        private string direccion;

        [ObservableProperty]
        private string telefono;

        [ObservableProperty]
        private string email;

        public IRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public AgregarEditarPersonaViewModel(Window window)
        {
            _window = window;
            _service = new PersonaService();

            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(Cancelar);
        }

        public void LoadFromModel(Persona p)
        {
            _personaId = p.PerId;
            Titulo = "Editar Persona";
            Nombre = p.PerNombre;
            Apellido = p.PerApellido;
            Direccion = p.PerDireccion;
            Telefono = p.PerTelefono;
            Email = p.PerEmail;
        }

        private void Guardar()
        {
            try
            {
                var persona = new Persona
                {
                    PerId = _personaId,
                    PerNombre = Nombre,
                    PerApellido = Apellido,
                    PerDireccion = Direccion,
                    PerTelefono = Telefono,
                    PerEmail = Email
                };

                if (_personaId == 0)
                {
                    _service.CrearPersona(persona);
                    MessageBox.Show("Persona creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _service.ActualizarPersona(persona);
                    MessageBox.Show("Persona actualizada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _window.DialogResult = true;
                _window.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancelar()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}