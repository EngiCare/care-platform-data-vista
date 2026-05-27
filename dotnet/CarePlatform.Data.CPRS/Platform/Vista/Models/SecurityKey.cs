// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarePlatform.Data.VistA
{
    public class SecurityKey 
    {
        public SecurityKey()  { }
        public SecurityKey(string keyId, string name) {} //: base(keyId, name) { }
        public SecurityKey(string keyId, string name, string recordId) { } //: base(keyId, name, recordId) { }

        public PermissionType Type
        {
            get { return PermissionType.SecurityKey; }
        }
    }
}
