using System;
using System.Collections.Generic;
using System.Text;

namespace ACM.BL
{
    public abstract class EntityBase
    {
        public enum EntityStateOption
        {
            Active,
            Deleted
        }
        public EntityStateOption EntityState { get; set; }
        public bool HasChanges { get; set; }
        public bool IsNew { get; private set; }
        public bool IsValid => Validate();
        public abstract bool Validate();
       

    }
}
