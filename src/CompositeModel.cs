using System;
using System.Threading;
using System.Collections.Generic;
using Gtk;

namespace Photobooth
{
    class CompositeModel
    {
        List<Layer> _layers = new List<Layer>();
        Gdk.Pixbuf _baseImage, _compImage;
        List<CallbackFn> _compImageCbs = new List<CallbackFn>();
        List<CallbackFn> _layerCbs = new List<CallbackFn>();
        FilterFactory _filters = new FilterFactory();

        public delegate void CallbackFn();

        public CompositeModel() {}

        public Gdk.Pixbuf CompositeImage
        {
            get { return _compImage; }
        }

        public int NumLayers
        {
            get { return _layers.Count; }
        }

        public List<string> FilterNames
        {
            get { return _filters.GetFilterNames(); }
        }

        // requires: non-null callback method or listener class
        // effects: removes the callback from the list of layer cbs; noop if not found
        public void RemoveLayerChangedCallback(CallbackFn cb)
        {
            if (_layerCbs.Contains(cb)) _layerCbs.Remove(cb);
        }

        // requires: non-null callback method or listener class
        // effects: adds the callback to the list of layer cbs
        public void AddLayerChangedCallback(CallbackFn cb)
        {
            _layerCbs.Add(cb);
        }

        // requires: non-null callback method or listener class
        // effects: removes the callback from the list of composite image cbs; noop if not found
        public void RemoveCompositeChangedCallback(CallbackFn cb)
        {
            if (_compImageCbs.Contains(cb)) _compImageCbs.Remove(cb);
        }

        // requires: non-null callback method or listener class
        // effects: adds the callback to the list of composite cbs
        public void AddCompositeChangedCallback(CallbackFn cb)
        {
            _compImageCbs.Add(cb);
        }

        private void ComputeCompImage()
        {
            _compImage = _baseImage.Copy();
            foreach (Layer l in _layers) { l.Apply(_compImage); }
            foreach (CallbackFn cb in _compImageCbs) { cb(); }
        }

        // requires: 0 <= id < NumLayers
        // effects: removes layer with id from our list; 
        //     updates the composite image; invokes composite and layer cbs
        public void DeleteLayer(int id)
        {
            _layers.RemoveAt(id);
            ComputeCompImage();
            foreach (CallbackFn cb in _layerCbs) { cb(); }
        }

        // requires: 0 <= id < NumLayers
        // effects: scales the layer with id uniformly by size; 
        //     updates the composite image; invokes composite image cbs
        public void ScaleLayer(int id, double size)
        {
            double currSize = _layers[id].Size;
            _layers[id].Size = currSize * size;
            ComputeCompImage();
        }

        // requires: 0 <= id < NumLayers
        // effects: moves layer with id to position X (horizontal) and Y (vertical) 
        //     relative to the base image's top left corner; 
        //     updates the composite image; invokes composite image cbs
        public void MoveLayer(int id, double x, double y)
        {
            _layers[id].X = x;
            _layers[id].Y = y;
            ComputeCompImage();
        }

        // requires: 0 <= id < NumLayers
        // effects: returns the name of layer with id (note: can be "")
        public string GetLayerName(int id)
        {
            return _layers[id].Name;
        }

        // requires: non-empty pixels
        // effects: adds a new layer having the given image and name; invokes layer cbs
        public void AddLayer(Gdk.Pixbuf pixels, string name)
        {
            Layer newLayer = new Layer(pixels, name);
            _layers.Add(newLayer);
            ComputeCompImage();
            foreach (CallbackFn cb in _layerCbs) { cb(); }
        }

        // requires: nothing
        // effects: saves the image with the given filename;
        //     returns true if successful, false otherwise
        public bool SaveCompositeImage(string filename)
        {
            return _compImage.Save(filename, "jpeg");
        }

        // requires: nothing
        // effects: sets the base image with the given filename;
        //     initializes the composite image;
        //     returns true if successful, false otherwise
        public bool LoadBaseImage(string filename)
        {
            try
            {
                _layers = new List<Layer>();
                _baseImage = new Gdk.Pixbuf(filename);
                ComputeCompImage();
                foreach (CallbackFn cb in _layerCbs) { cb(); }
            }
            catch (Exception e)
            {
                return false;
            }
            
            return true;
        }

        // requires: nothing
        // effects: applies the filter corresponding to name to the composite image;
        //      invokes callbacks to notify that the composite image has changed;
        //      does nothing if name does not correspond to a valid filter
        public void RunFilter(string name)
        {
            try
            {
                Filter filter = _filters.Create(name); // this is equivalent to, say, DarkenFilter.Create()
                Gdk.Pixbuf newImage = filter.Run(_compImage);
                _compImage = newImage;
                foreach (CallbackFn cb in _compImageCbs) { cb(); }
            }
            catch (Exception e) {}
        }

        // wrapper function for FilterFactory.RegisterFilter
        public void RegisterFilter(string name, Filter.CreateFn fn)
        {
            _filters.RegisterFilter(name, fn);
        }

        // wrapper function for FilterFactory.DeregisterFilter
        public void DeregisterFilter(string name)
        {
            _filters.DeregisterFilter(name);
        }
    }
}
