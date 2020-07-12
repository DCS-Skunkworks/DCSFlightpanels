using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonVisuals
{
    public class GenericBinding
    {
        private string _hidInstance;
        private string _bindingHash;
        private string _settings;

        public GenericBinding()
        {}

        public GenericBinding(string hidInstance, string bindingHash)
        {
            _hidInstance = hidInstance;
            _bindingHash = bindingHash;
        }

        public string HIDInstance
        {
            get => _hidInstance;
            set => _hidInstance = value;
        }

        public string BindingHash
        {
            get => _bindingHash;
            set => _bindingHash = value;
        }

        public string Settings
        {
            get => _settings;
            set => _settings = value;
        }
    }
}
