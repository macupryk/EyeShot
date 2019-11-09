using System.Collections.Generic;

namespace WpfApplication1
{
    public interface ISelect
    {
        // Gets or sets the list of selected triangles 
        List<int> SelectedSubItems { get; set; }
        
        bool DrawSubItemsForSelection{ get; set; }
        
        void SelectSubItems(int[] indices);
    }
}
