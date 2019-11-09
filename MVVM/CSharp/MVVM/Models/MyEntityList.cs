using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;

namespace WpfApplication1.Models
{
    public class MyEntityList : ObservableCollection<Entity>
    {        
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            if (Model == null)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (!_stopInvalidate)
                    {
                        Model.Entities.Add(e.NewItems[0] as Entity);
                        Model.Invalidate();
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Model.Entities.Remove(e.OldItems[0] as Entity);                    
                    if (!_stopInvalidate)
                        Model.Invalidate();
                    break;
            }
        }


        // When I add or remove a range of entities, I want to refresh the Model only at the end.
        private bool _stopInvalidate;
        public void AddRange(IEnumerable<Entity> entities)
        {
            _stopInvalidate = true;            

            foreach (var entity in entities)
            {
                Add(entity);
            }
            Model.Entities.AddRange(entities);            
            Model.Invalidate();

            _stopInvalidate = false;
        }

        public void RemoveRange(IEnumerable<Entity> entities)
        {               
            _stopInvalidate = true;

            foreach (var entity in entities)
            {
                Remove(entity);
            }            
            Model.Invalidate();

            _stopInvalidate = false;
        }
        
        public Model Model { get; set; }
    }
}
