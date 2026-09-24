using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmaciaApp.Core.Models;
using FarmaciaApp.Core.Services;
using System.Windows;

namespace FarmaciaApp.UI.ViewModels
{
    public partial class AgregarEditarPromocionViewModel : ObservableObject
    {
        private readonly PromocionService _service;
        private readonly Window _window;
        private int _promocionId;

        [ObservableProperty]
        private string titulo = "Agregar Promoción";

        [ObservableProperty]
        private string descripcion;

        [ObservableProperty]
        private decimal descuento;

        [ObservableProperty]
        private DateTime fechaInicio = DateTime.Now;

        [ObservableProperty]
        private DateTime fechaFin = DateTime.Now.AddDays(30);

        public IRelayCommand GuardarCommand { get; }
        public IRelayCommand CancelarCommand { get; }

        public AgregarEditarPromocionViewModel(Window window)
        {
            _window = window;
            _service = new PromocionService();

            GuardarCommand = new RelayCommand(Guardar);
            CancelarCommand = new RelayCommand(Cancelar);
        }

        public void LoadFromModel(Promocion p)
        {
            _promocionId = p.PrmId;
            Titulo = "Editar Promoción";
            Descripcion = p.PrmDescripcion;
            Descuento = p.PrmDescuento;
            FechaInicio = p.PrmFechaIni;
            FechaFin = p.PrmFechaFin;
        }

        private void Guardar()
        {
            try
            {
                var promocion = new Promocion
                {
                    PrmId = _promocionId,
                    PrmDescripcion = Descripcion,
                    PrmDescuento = Descuento,
                    PrmFechaIni = FechaInicio,
                    PrmFechaFin = FechaFin
                };

                if (_promocionId == 0)
                {
                    _service.CrearPromocion(promocion);
                    MessageBox.Show("Promoción creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _service.ActualizarPromocion(promocion);
                    MessageBox.Show("Promoción actualizada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _window.DialogResult = true;
                _window.Close();
            }
            catch (Exception ex)
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