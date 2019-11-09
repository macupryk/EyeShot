using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace WpfApplication1
{
    /// <summary>    
    /// This class represent the ViewModel for Layers List.
    /// </summary>    
    public class LayersListViewModel
    {
        public LayersListViewModel(string layerName)
        {
            LayerName = layerName;            
        }

        public LayersListViewModel(string layerName, int layerLineWeight, Brush foregroundColor)
            : this(layerName)
        {
            LayerLineWeight = layerLineWeight;
            ForeColor = foregroundColor;
        }

        public string LayerName { get; set; }
        public float LayerLineWeight { get; set; }        
        public bool Checked { get; set; }
        public Brush ForeColor { get; set; }                  

        public override string ToString()
        {
            return LayerName;
        }

    }
}