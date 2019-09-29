using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Gtk;

namespace Photobooth
{
    public class FilterFactory
    {
        Dictionary<string, Filter.CreateFn> _filters = new Dictionary<string, Filter.CreateFn>();

        public FilterFactory()
        {
            RegisterFilter("NoneFilter", NoneFilter.Create);
            RegisterFilter("GrayscaleFilter", GrayscaleFilter.Create);
            RegisterFilter("LightenFilter", LightenFilter.Create);
            RegisterFilter("DarkenFilter", DarkenFilter.Create);
            RegisterFilter("JitterFilter", JitterFilter.Create);
        }

        // effects: associates the filter creator function with name 
        //      replaces the existing creator if one exists
        public void RegisterFilter(string name, Filter.CreateFn fn)
        {
            if (_filters.ContainsKey(name))
            {
                throw new Exception($"{name} already registered");
            }
            _filters[name] = fn;
        }

        // effects: removes the CreatorFn corresponding to name, if one exists; 
        //      otherwise, does nothing
        public void DeregisterFilter(string name)
        {
            if (_filters.ContainsKey(name))
            {
                _filters.Remove(name);
            }
        }

        // effects: returns a new filter corresponding to name or null if none exists
        public Filter Create(string name)
        {
            return _filters[name]();
        }


        // effects: returns the list of names corresponding to each filter
        public List<string> GetFilterNames()
        {
            return _filters.Keys.ToList();
        }
    }

}
