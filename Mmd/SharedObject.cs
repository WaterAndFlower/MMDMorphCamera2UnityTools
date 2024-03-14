using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace PowerBearEditor.SharedObject {
    public class VmdObject {
        private Object _vmd_file;
        public string vmd_path = "/Assets/null.null";
        public Object vmd_file {
            set {
                _vmd_file = value;
                vmd_path = AssetDatabase.GetAssetPath(_vmd_file);
                if (_vmd_file != null && Path.GetExtension(vmd_path) != ".vmd") {
                    Debug.LogError("Not A Vmd File");
                    vmd_path = null;
                    _vmd_file = null;
                }
                if (_vmd_file is null) {
                    vmd_path = "/Assets/null.null";
                }
            }
            get { return _vmd_file; }
        }
    }
}